using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Devesprit.FileServer.ElmahConfig;
using Devesprit.Services.MemoryCache;
using Devesprit.Utilities;
using Devesprit.Utilities.Extensions;

namespace Devesprit.FileServer
{
    /*
        NOTE:
        Before publish this service please follow bellow instructions:
        Change Admin username & password from Web.config -> appSettings -> ServiceAdminUserName & ServiceAdminPassword
        Change encryption key in Web.config -> appSettings -> EncryptionKey & EncryptionSalt
        You need run this service over SSL

        If you receive "Page Not Found 404" or "(405) Method Not Allowed." errors, make sure "HTTP-Activation" installed on your IIS
        To install "HTTP-Activation" follow below instructions:
        -Run Server Manager (on task bar and start menu)
        -Choose the server to administer (probably local server)
        -Scroll down to "Roles and Features" section.
        -Choose "Add Role or Feature" from Tasks drop down
        -On "Add Role or Feature Wizard" dialog, click down to "Features" in list of pages on the left.
        -Expand ".Net 3.5" or ".Net 4.5", depending on what you have installed. (you can go back up to "roles" screen to add if you don't have.
        -Under "WCF Services", check the box for "HTTP-Activation". You can also add non-http types if you know you need them (tcp, named pipes, etc).
        -Click "Install" Button.

        Usage example:

        var binding = new WSHttpBinding();
        binding.Security.Mode = SecurityMode.TransportWithMessageCredential;
        binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
        binding.Security.Message.EstablishSecurityContext = false;

        var endPoint = new EndpointAddress("https://my-host/FileManagerService.svc");
        var fileManager = new FileManagerService.FileManagerServiceClient(binding, endPoint);
        fileManager.ClientCredentials.UserName.UserName = "Admin";
        fileManager.ClientCredentials.UserName.Password = "Admin@123456";

        var result = fileManager.EnumerateDirectoryEntries("bin", "*.*", true, true, TimeSpan.FromDays(2), 20);
        fileManager.Close(); 
    */

    [ServiceErrorBehavior(typeof(ElmahErrorHandler))]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public partial class FileManagerService : IFileManagerService
    {
        private readonly IMemoryCache _memoryCache;

        private TimeSpan CacheExpire => TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings["CacheExpireSec"]));


        public FileManagerService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public virtual async Task<List<FileSystemEntries>> EnumerateDirectoryEntries(string path, string searchPattern, bool includeSubEntries, bool includeDownloadLink, TimeSpan downloadLinksExpire, int downloadCountLimit)
        {
            path = await ProcessPath(path, true);

            if (!await Task.Run(() => Directory.Exists(path)))
            {
                throw new FaultException("Invalid path.");
            }

            var result = new List<FileSystemEntries>();

            // Try get result from cache
            if (_memoryCache.Contains(path + searchPattern + includeSubEntries, "FileManagerService::EnumerateDirectoryEntries"))
            {
                return _memoryCache.GetObject<List<FileSystemEntries>>(path + searchPattern + includeSubEntries, "FileManagerService::EnumerateDirectoryEntries");
            }

            var notAllowedFileExtensions =
                    ConfigurationManager.AppSettings["NotAllowedFileExtensionsToDownload"].ToLower()
                        .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            //Get Files
            foreach (var entry in await Task.Run(() => Directory.EnumerateFiles(path, searchPattern)))
            {
                //Check AllowedFileExtensionsToUpload
                var fileExt = Path.GetExtension(entry);
                if (notAllowedFileExtensions.Contains(fileExt?.ToLower()))
                {
                    continue;
                }

                try
                {
                    long entrySize = await FileUtils.GetFilesSizeAsync(new[] { entry });
                    var downloadLink = includeDownloadLink
                        ? await GenerateDownloadLink(GlobalStaticClass.HostUrl, entry, downloadLinksExpire, downloadCountLimit)
                        : "";
                    result.Add(await Task.Run(() => new FileSystemEntries()
                    {
                        Path = entry,
                        Name = Path.GetFileName(entry),
                        Type = FileSystemEntryType.File,
                        CreateDateUtc = File.GetCreationTimeUtc(entry),
                        ModifiedDateUtc = File.GetLastWriteTimeUtc(entry),
                        SizeInByte = entrySize,
                        DisplaySize = entrySize.FileSizeSuffix(),
                        SubEntries = new List<FileSystemEntries>(),
                        DownloadLink = downloadLink
                    }));
                }
                catch
                { }
            }

            //Get Directories
            foreach (var entry in await Task.Run(() => Directory.EnumerateDirectories(path, searchPattern)))
            {
                try
                {
                    var entrySize = await FileUtils.GetDirectorySizeAsync(entry, searchPattern, true);
                    result.Add(new FileSystemEntries()
                    {
                        Path = entry,
                        Name = Path.GetFileName(entry),
                        Type = FileSystemEntryType.Dir,
                        CreateDateUtc = await Task.Run(() => Directory.GetCreationTimeUtc(entry)),
                        ModifiedDateUtc = await Task.Run(() => Directory.GetLastWriteTimeUtc(entry)),
                        SizeInByte = entrySize,
                        DisplaySize = entrySize.FileSizeSuffix(),
                        SubEntries = includeSubEntries ? await EnumerateDirectoryEntries(entry, searchPattern, true, includeDownloadLink, downloadLinksExpire, downloadCountLimit) : new List<FileSystemEntries>()
                    });
                }
                catch
                { }
            }

            // Store results in the cache
            _memoryCache.AddObject(path + searchPattern + includeSubEntries, result, CacheExpire, "FileManagerService::EnumerateDirectoryEntries");
            
            return result;
        }

        public virtual async Task<List<FileSystemEntries>> EnumerateDirectories(string path, string searchPattern, bool includeSubEntries)
        {
            path = await ProcessPath(path, true);

            if (!await Task.Run(() => Directory.Exists(path)))
            {
                throw new FaultException("Invalid path.");
            }

            var result = new List<FileSystemEntries>();

            // Try get result from cache
            if (_memoryCache.Contains(path + searchPattern + includeSubEntries, "FileManagerService::EnumerateDirectories"))
            {
                return _memoryCache.GetObject<List<FileSystemEntries>>(path + searchPattern + includeSubEntries, "FileManagerService::EnumerateDirectories");
            }

            foreach (var entry in await Task.Run(() => Directory.EnumerateDirectories(path, searchPattern)))
            {
                try
                {
                    var entrySize = await FileUtils.GetDirectorySizeAsync(entry, searchPattern, true);
                    result.Add(new FileSystemEntries()
                    {
                        Path = entry,
                        Name = Path.GetFileName(entry),
                        Type = FileSystemEntryType.Dir,
                        CreateDateUtc = await Task.Run(() => Directory.GetCreationTimeUtc(entry)),
                        ModifiedDateUtc = await Task.Run(() => Directory.GetLastWriteTimeUtc(entry)),
                        SizeInByte = entrySize,
                        DisplaySize = entrySize.FileSizeSuffix(),
                        SubEntries = includeSubEntries ? await EnumerateDirectories(entry, searchPattern, true) : new List<FileSystemEntries>()
                    });
                }
                catch
                { }
            }

            // Store results in the cache
            _memoryCache.AddObject(path + searchPattern + includeSubEntries, result, CacheExpire, "FileManagerService::EnumerateDirectories");
            
            return result;
        }

        public virtual async Task<List<FileSystemEntries>> EnumerateFiles(string path, string searchPattern, bool includeDownloadLink, TimeSpan downloadLinksExpire, int downloadCountLimit)
        {
            path = await ProcessPath(path, true);

            if (!await Task.Run(() => Directory.Exists(path)))
            {
                throw new FaultException("Invalid path.");
            }

            // Try get result from cache
            if (_memoryCache.Contains(path + searchPattern, "FileManagerService::EnumerateFiles"))
            {
                return _memoryCache.GetObject<List<FileSystemEntries>>(path + searchPattern, "FileManagerService::EnumerateFiles");
            }

            var notAllowedFileExtensions =
                    ConfigurationManager.AppSettings["NotAllowedFileExtensionsToDownload"].ToLower()
                        .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            var result = new List<FileSystemEntries>();
            foreach (var entry in await Task.Run(() => Directory.EnumerateFiles(path, searchPattern)))
            {
                //Check AllowedFileExtensionsToUpload
                var fileExt = Path.GetExtension(entry);
                if (notAllowedFileExtensions.Contains(fileExt?.ToLower()))
                {
                    continue;
                }

                try
                {
                    var entrySize = await FileUtils.GetFilesSizeAsync(new[] { entry });
                    var downloadLink = includeDownloadLink
                        ? await GenerateDownloadLink(GlobalStaticClass.HostUrl, entry, downloadLinksExpire, downloadCountLimit)
                        : "";
                    result.Add(await Task.Run(() => new FileSystemEntries()
                    {
                        Path = entry,
                        Name = Path.GetFileName(entry),
                        Type = FileSystemEntryType.File,
                        CreateDateUtc = File.GetCreationTimeUtc(entry),
                        ModifiedDateUtc = File.GetLastWriteTimeUtc(entry),
                        SizeInByte = entrySize,
                        DisplaySize = entrySize.FileSizeSuffix(),
                        SubEntries = new List<FileSystemEntries>(),
                        DownloadLink = downloadLink
                    }));
                }
                catch
                { }
            }

            // Store results in the cache
            _memoryCache.AddObject(path + searchPattern, result, CacheExpire, "FileManagerService::EnumerateFiles");
            
            return result;
        }

        public virtual async Task<FileSystemEntries> GetFileInfo(string path, bool includeDownloadLink, TimeSpan downloadLinksExpire, int downloadCountLimit)
        {
            path = await ProcessPath(path, true);

            if (!await Task.Run(() => File.Exists(path)))
            {
                throw new FaultException("Invalid path.");
            }

            // Try get result from cache
            if (_memoryCache.Contains(path, "FileManagerService::GetFileInfo"))
            {
                return _memoryCache.GetObject<FileSystemEntries>(path, "FileManagerService::GetFileInfo");
            }

            var fileSize = await FileUtils.GetFilesSizeAsync(new[] { path });
            var downloadLink = includeDownloadLink
                ? await GenerateDownloadLink(GlobalStaticClass.HostUrl, path, downloadLinksExpire, downloadCountLimit)
                : "";
            var result = await Task.Run(() => new FileSystemEntries()
            {
                Path = path,
                Name = Path.GetFileName(path),
                Type = FileSystemEntryType.File,
                CreateDateUtc = File.GetCreationTimeUtc(path),
                ModifiedDateUtc = File.GetLastWriteTimeUtc(path),
                SizeInByte = fileSize,
                DisplaySize = fileSize.FileSizeSuffix(),
                SubEntries = new List<FileSystemEntries>(),
                DownloadLink = downloadLink
            });

            // Store results in the cache
            _memoryCache.AddObject(path, result, CacheExpire, "FileManagerService::GetFileInfo");

            return result;
        }

        public virtual async Task<FileSystemEntries> GetDirectoryInfo(string path)
        {
            path = await ProcessPath(path, true);

            if (!await Task.Run(() => Directory.Exists(path)))
            {
                throw new FaultException("Invalid path.");
            }

            // Try get result from cache
            if (_memoryCache.Contains(path, "FileManagerService::GetDirectoryInfo"))
            {
                return _memoryCache.GetObject<FileSystemEntries>(path, "FileManagerService::GetDirectoryInfo");
            }

            var directorySize = await FileUtils.GetDirectorySizeAsync(path, "*.*", true);
            var result = await Task.Run(() => new FileSystemEntries()
            {
                Path = path,
                Name = Path.GetFileName(path),
                Type = FileSystemEntryType.Dir,
                CreateDateUtc = Directory.GetCreationTimeUtc(path),
                ModifiedDateUtc = Directory.GetLastWriteTimeUtc(path),
                SizeInByte = directorySize,
                DisplaySize = directorySize.FileSizeSuffix(),
                SubEntries = new List<FileSystemEntries>()
            });

            // Store results in the cache
            _memoryCache.AddObject(path, result, CacheExpire, "FileManagerService::GetDirectoryInfo");

            return result;
        }

        public virtual async Task<bool> DeleteFile(string path)
        {
            path = await ProcessPath(path, true, FileIOPermissionAccess.AllAccess);

            try
            {
                if (await Task.Run(() => File.Exists(path)))
                    await Task.Run(() => File.Delete(path));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual async Task<bool> DeleteDirectory(string path)
        {
            path = await ProcessPath(path, true, FileIOPermissionAccess.AllAccess);

            try
            {
                if (await Task.Run(() => Directory.Exists(path)))
                    await Task.Run(() => Directory.Delete(path, true));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual async Task<bool> CreateDirectory(string path)
        {
            path = await ProcessPath(path, true, FileIOPermissionAccess.AllAccess);

            try
            {
                if (!await Task.Run(() => Directory.Exists(path)))
                    await Task.Run(() => Directory.CreateDirectory(path));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual async Task<bool> CopyFile(string src, string dest, bool overWrite)
        {
            src = await ProcessPath(src, true);
            dest = await ProcessPath(dest, true, FileIOPermissionAccess.AllAccess);

            try
            {
                await Task.Run(() => File.Copy(src, dest, overWrite));
                return true;
            }
            catch
            {
                throw new FaultException("An error occurred while copying the file.");
            }
        }

        public virtual async Task<bool> MoveFile(string src, string dest, bool overWrite)
        {
            src = await ProcessPath(src, true, FileIOPermissionAccess.AllAccess);
            dest = await ProcessPath(dest, true, FileIOPermissionAccess.AllAccess);

            if (await Task.Run(() => File.Exists(dest)))
            {
                if (overWrite)
                {
                    await Task.Run(() => File.Delete(dest));
                }
                else
                {
                    throw new FaultException("File already exists.");
                }
            }

            try
            {
                await Task.Run(() => File.Move(src, dest));
                return true;
            }
            catch
            {
                throw new FaultException("An error occurred while copying the file.");
            }
        }

        public virtual async Task<bool> CopyDirectory(string src, string dest, bool overWrite)
        {
            src = await ProcessPath(src, true);
            dest = await ProcessPath(dest, true, FileIOPermissionAccess.AllAccess);

            try
            {
                await Task.Run(() => FileUtils.CopyDirectory(src, dest, overWrite));
                return true;
            }
            catch
            {
                throw new FaultException("An error occurred while copying the directory.");
            }
        }

        public virtual async Task<bool> MoveDirectory(string src, string dest, bool overWrite)
        {
            src = await ProcessPath(src, true, FileIOPermissionAccess.AllAccess);
            dest = await ProcessPath(dest, true, FileIOPermissionAccess.AllAccess);

            try
            {
                await Task.Run(() =>
                {
                    FileUtils.CopyDirectory(src, dest, overWrite);
                    Directory.Delete(src, true);
                });
                return true;
            }
            catch
            {
                throw new FaultException("An error occurred while copying the directory.");
            }
        }

        public virtual async Task<string> GenerateDownloadLink(string path, TimeSpan expire, int downloadCountLimit)
        {
            return await GenerateDownloadLink(GlobalStaticClass.HostUrl, path, expire, downloadCountLimit);
        }

        protected virtual async Task<string> GenerateDownloadLink(string hostUrl, string path, TimeSpan expire, int downloadCountLimit)
        {
            path = await ProcessPath(path, true);

            if (!await Task.Run(() => File.Exists(path.Trim())))
            {
                throw new FaultException("The requested file was not found.");
            }

            //Check AllowedFileExtensionsToUpload
            var notAllowedFileExtensions =
                    ConfigurationManager.AppSettings["NotAllowedFileExtensionsToDownload"].ToLower()
                        .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var fileExt = Path.GetExtension(path);
            if (notAllowedFileExtensions.Contains(fileExt?.ToLower()))
            {
                throw new FaultException("The requested file was not found.");
            }

            return hostUrl.TrimEnd('/') + "/Download.ashx?request=" +
                   HttpUtility.UrlEncode(new DownloadRequest()
                   {
                       File = path,
                       Expire = expire.TotalSeconds > 0 ? DateTime.Now.Add(expire) : DateTime.MinValue,
                       DownloadCount = downloadCountLimit
                   }.ObjectToJson().EncryptString());
        }

        public virtual async Task<bool> CompressFile(string path, string saveTo, bool overwrite, string password, int compressionLevel, bool useBZip2)
        {
            path = await ProcessPath(path, true);
            saveTo = await ProcessPath(saveTo, true, FileIOPermissionAccess.AllAccess);

            try
            {
                await FileUtils.CompressFileAsync(path, saveTo, overwrite, password, compressionLevel, useBZip2);
                return true;
            }
            catch
            {
                throw new FaultException("An error occurred while compressing the file.");
            }

        }

        public virtual async Task<bool> CompressDirectory(string path, string saveTo, bool overwrite, string password, int compressionLevel, bool useBZip2)
        {
            path = await ProcessPath(path, true);
            saveTo = await ProcessPath(saveTo, true, FileIOPermissionAccess.AllAccess);

            try
            {
                await FileUtils.CompressDirectoryAsync(path, saveTo, overwrite, password, compressionLevel, useBZip2);
                return true;
            }
            catch
            {
                throw new FaultException("An error occurred while compressing the directory.");
            }
        }

        public virtual async Task<string> CalculateMd5Checksum(string path)
        {
            path = await ProcessPath(path, true);
            return await Task.Run(() => FileUtils.CalculateMd5Checksum(path));
        }

        public virtual async Task<string> UploadFileRequest(string fileName, string saveTo, bool overWrite)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["AllowToUploadFile"].ToLower()))
            {
                throw new FaultException("This service is unavailable.");
            }

            var filePath = await ProcessPath(Path.Combine(saveTo, fileName), true, FileIOPermissionAccess.AllAccess);

            //Check AllowedFileExtensionsToUpload
            var allowedFileExtensions =
                ConfigurationManager.AppSettings["AllowedFileExtensionsToUpload"].ToLower()
                    .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var fileExt = Path.GetExtension(filePath);
            if (!allowedFileExtensions.Contains(fileExt?.ToLower()))
            {
                throw new FaultException("The filetype you are attempting to upload is not allowed.");
            }

            if (await Task.Run(() => File.Exists(filePath)))
            {
                if (overWrite)
                {
                    await Task.Run(() => File.Delete(filePath));
                }
                else
                {
                    throw new FaultException("File already exists.");
                }
            }

            return new { Path = filePath, Expire = DateTime.Now.AddDays(10) }.ObjectToJson().EncryptString();
        }

        protected virtual async Task<string> ProcessPath(string path, bool checkAccess = false, FileIOPermissionAccess access = FileIOPermissionAccess.Read)
        {
            return await Task.Run(() =>
            {
                var serverPath = (HostingEnvironment.MapPath("~") ?? "").TrimEnd('\\') + "\\";
                if (path == "\\" || path == "")
                {
                    if (checkAccess)
                    {
                        if (!FileUtils.HasAccess(serverPath, access))
                        {
                            throw new FaultException($"Access is dined to '{path}' path.");
                        }
                    }

                    return serverPath;
                }

                string result = path;
                if (!FileUtils.FileSystemPathIsAbsolute(result))
                {
                    //Convert relative path to absolute path
                    result = Path.GetFullPath(Path.Combine(serverPath, result));
                }

                bool allowToAccessToOutSideOfRootDirectory =
                    bool.Parse(ConfigurationManager.AppSettings["AllowToAccessToOutSideOfRootDirectory"].ToLower());

                if (!allowToAccessToOutSideOfRootDirectory)
                {
                    if (!result.TrimEnd('\\').StartsWith(serverPath.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    {
                        throw new FaultException($"Access is dined to '{path}' path.");
                    }
                }

                if (checkAccess)
                {
                    if (!FileUtils.HasAccess(result, access))
                    {
                        throw new FaultException($"Access is dined to '{path}' path.");
                    }
                }

                return result;
            });
        }
    }
}
