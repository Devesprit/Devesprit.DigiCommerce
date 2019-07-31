using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;

namespace Devesprit.Utilities
{
    public static partial class FileUtils
    {
        public static string DetectFileType(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLower().TrimStart('.').Trim() ?? "";
            if (ext == "png" ||
                ext == "jpg" ||
                ext == "jpeg" ||
                ext == "bmp" ||
                ext == "gif")
            {
                return "Image";
            }
            return "UNKNOWN";
        }

        /// <summary>
        /// Return file(s) size in byte
        /// </summary>
        /// <param name="pathList"></param>
        /// <returns></returns>
        public static long GetFilesSize(string[] pathList)
        {
            return pathList.Where(File.Exists).Sum(path => new FileInfo(path).Length);
        }

        /// <summary>
        /// Return file(s) size in byte asynchronously
        /// </summary>
        /// <param name="pathList"></param>
        /// <returns></returns>
        public static async Task<long> GetFilesSizeAsync(string[] pathList)
        {
            return await Task.Run(() => GetFilesSize(pathList));
        }

        /// <summary>
        /// Return directory files size in byte
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="includeSubDirectories"></param>
        /// <returns></returns>
        public static long GetDirectorySize(string path, string searchPattern, bool includeSubDirectories)
        {
            if (!Directory.Exists(path))
            {
                return 0;
            }

            var files = Directory.GetFiles(path, searchPattern,
                includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return GetFilesSize(files);
        }

        /// <summary>
        /// Return directory files size in byte asynchronously
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="includeSubDirectories"></param>
        /// <returns></returns>
        public static async Task<long> GetDirectorySizeAsync(string path, string searchPattern, bool includeSubDirectories)
        {
            return await Task.Run(() => GetDirectorySize(path, searchPattern, includeSubDirectories));
        }

        /// <summary>
        /// Copy the entire contents of a directory
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="overWrite"></param>
        public static void CopyDirectory(string src, string dest, bool overWrite)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(src, "*",
            SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(src, dest));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(src, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(src, dest), overWrite);
        }

        /// <summary>
        /// Copy the entire contents of a directory asynchronously
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="overWrite"></param>
        public static async void CopyDirectoryAsync(string src, string dest, bool overWrite)
        {
            await Task.Run(() => CopyDirectory(src, dest, overWrite));
        }

        /// <summary>
        /// Compress a directory and save it in the specific path
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="saveTo"></param>
        /// <param name="overwrite"></param>
        /// <param name="password">Lock the archive file</param>
        /// <param name="compressionLevel">Compression Level from 0(No Compression) to 9(Best Compression)</param>
        /// <param name="useBZip2">BZip2 or Deflate</param>
        public static void CompressDirectory(string dirPath, string saveTo, bool overwrite = false, string password = "", int compressionLevel = 6, bool useBZip2 = false)
        {
            if (overwrite)
            {
                if (File.Exists(saveTo))
                    File.Delete(saveTo);
            }
            else
            {
                if (File.Exists(saveTo))
                    throw new FaultException($"The '{Path.GetFileName(saveTo)}' file already exists.");
            }

            compressionLevel = compressionLevel < 0 ? 0 : compressionLevel;
            compressionLevel = compressionLevel > 9 ? 9 : compressionLevel;
            using (var zip = new ZipFile(saveTo, Encoding.UTF8)
            {
                CompressionLevel = (CompressionLevel)compressionLevel,
                CompressionMethod = useBZip2 ? CompressionMethod.BZip2 : CompressionMethod.Deflate,
                Password = String.IsNullOrWhiteSpace(password) ? null : password,
            })
            {
                zip.AddDirectory(dirPath, "");
                zip.Save();
            }
        }

        /// <summary>
        /// Compress a directory asynchronously and save it in the specific path
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="saveTo"></param>
        /// <param name="overwrite"></param>
        /// <param name="password">Lock the archive file</param>
        /// <param name="compressionLevel">Compression Level from 0(No Compression) to 9(Best Compression)</param>
        /// <param name="useBZip2">BZip2 or Deflate</param>
        public static async Task CompressDirectoryAsync(string dirPath, string saveTo, bool overwrite = false, string password = "", int compressionLevel = 6, bool useBZip2 = false)
        {
            await Task.Run(() => CompressDirectory(dirPath, saveTo, overwrite, password, compressionLevel, useBZip2));
        }

        /// <summary>
        /// Compress a file and save it in the specific path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="saveTo"></param>
        /// <param name="overwrite"></param>
        /// <param name="password">Lock the archive file</param>
        /// <param name="compressionLevel">Compression Level from 0(No Compression) to 9(Best Compression)</param>
        /// <param name="useBZip2">BZip2 or Deflate</param>
        public static void CompressFile(string filePath, string saveTo, bool overwrite = false, string password = "", int compressionLevel = 6, bool useBZip2 = false)
        {
            if (overwrite)
            {
                if (File.Exists(saveTo))
                    File.Delete(saveTo);
            }
            else
            {
                if (File.Exists(saveTo))
                    throw new FaultException($"The '{Path.GetFileName(saveTo)}' file already exists.");
            }

            compressionLevel = compressionLevel < 0 ? 0 : compressionLevel;
            compressionLevel = compressionLevel > 9 ? 9 : compressionLevel;
            using (var zip = new ZipFile(saveTo, Encoding.UTF8)
            {
                CompressionLevel = (CompressionLevel)compressionLevel,
                CompressionMethod = useBZip2 ? CompressionMethod.BZip2 : CompressionMethod.Deflate,
                Password = String.IsNullOrWhiteSpace(password) ? null : password,
            })
            {
                zip.AddFile(filePath, "");
                zip.Save();
            }
        }

        /// <summary>
        /// Compress a file asynchronously and save it in the specific path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="saveTo"></param>
        /// <param name="overwrite"></param>
        /// <param name="password">Lock the archive file</param>
        /// <param name="compressionLevel">Compression Level from 0(No Compression) to 9(Best Compression)</param>
        /// <param name="useBZip2">BZip2 or Deflate</param>
        public static async Task CompressFileAsync(string filePath, string saveTo, bool overwrite = false, string password = "", int compressionLevel = 6, bool useBZip2 = false)
        {
            await Task.Run(() => CompressFile(filePath, saveTo, overwrite, password, compressionLevel, useBZip2));
        }

        /// <summary>
        /// Determine if path is relative or absolute
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool FileSystemPathIsAbsolute(string path)
        {
            Uri uri;
            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out uri))
            {
                throw new FaultException($"'{path}' is not a valid FileSystem path.");
            }
            if (!uri.IsAbsoluteUri)
            {
                return false;
            }
            if (uri.IsFile)
            {
                if (uri.IsUnc)
                {
                    return false;
                }
                return true;
            }

            throw new FaultException($"'{path}' is not a valid FileSystem path.");
        }

        /// <summary>
        /// Calculate MD5 checksum for a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string CalculateMd5Checksum(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }

        /// <summary>
        /// Calculate MD5 checksum for a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string CalculateMd5Checksum(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                return Encoding.Default.GetString(md5.ComputeHash(stream));
            }
        }

        /// <summary>
        /// Calculate MD5 checksum for a byte array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CalculateMd5Checksum(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                return Encoding.Default.GetString(md5.ComputeHash(data));
            }
        }

        /// <summary>
        /// Checking for directory and file read/write permissions
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool HasAccess(string path, FileIOPermissionAccess access)
        {
            PermissionSet permissionSet = new PermissionSet(PermissionState.None);

            FileIOPermission permission = new FileIOPermission(access, path);

            permissionSet.AddPermission(permission);

            if (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
            {
                return true;
            }
            return false;
        }

        public static void DeleteDirRecursively(DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        public static async Task DeleteDirRecursivelyAsync(DirectoryInfo directory)
        {
            await Task.Run(() => DeleteDirRecursively(directory));
        }
    }
}