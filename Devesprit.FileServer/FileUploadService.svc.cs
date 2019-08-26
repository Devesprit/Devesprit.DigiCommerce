using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading.Tasks;
using System.Web;
using Devesprit.FileServer.ElmahConfig;
using Devesprit.Utilities;
using Devesprit.Utilities.Extensions;
using Elmah;

namespace Devesprit.FileServer
{
    /*
         
            Note: 
            
            THIS SERVICE REQUIRES HTTPS

            -To enable Upload File Service please set Web.config -> appSettings -> AllowToUploadFile to "true"
            -Set which file types allowed to upload from Web.config -> appSettings -> AllowedFileExtensionsToUpload (e.g., .zip;.rar;.txt) 
            -The maximum size of upload stream in 10Mb, to increase it you can edit Web.config file.
        */
    [ServiceErrorBehavior(typeof(ElmahErrorHandler))]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class FileUploadService : IFileUploadService
    {
        public virtual async Task<UploadFileResult> UploadFile(UploadFileRequest request)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["AllowToUploadFile"].ToLower()))
            {
                throw new FaultException("This service is unavailable.");
            }

            string filePath;
            DateTime expireDate;
            try
            {
                var requestObject = request.UploadRequestKey.DecryptString().JsonToObject<UploadFileRequestObject>();
                filePath = requestObject.Path;
                expireDate = requestObject.Expire;
            }
            catch
            {
                throw new FaultException("Invalid Request.");
            }

            if (expireDate < DateTime.Now)
            {
                throw new FaultException("The request has expired.");
            }

            await Task.Run(() =>
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException());
                }
            });

            try
            {
                await Task.Run(() =>
                {
                    using (
                        var targetStream = new FileStream(filePath,
                            File.Exists(filePath) ? FileMode.Append : FileMode.Create,
                            FileAccess.Write, FileShare.None))
                    {
                        //read from the input stream in 65000 byte chunks
                        const int bufferLen = 65000;
                        var buffer = new byte[bufferLen];
                        int count;
                        while ((count = request.FileByteStream.Read(buffer, 0, bufferLen)) > 0)
                        {
                            // save to output stream
                            targetStream.Write(buffer, 0, count);
                        }

                        targetStream.Close();
                    }
                });

                return new UploadFileResult()
                {
                    Success = true,
                    Md5Hash = await Task.Run(() => FileUtils.CalculateMd5Checksum(filePath))
                };
            }
            catch
            {
                return new UploadFileResult()
                {
                    Success = false
                };
            }
        }

        public virtual async Task<Stream> GetFileFromWeb(string fileUrl)
        {
            if (fileUrl.IsValidUrl() && fileUrl.IsAbsoluteUrl())
            {
                byte[] fileBytes;
                using (var client = new WebClient())
                {
                    fileBytes = await client.DownloadDataTaskAsync(fileUrl);
                }

                return new MemoryStream(fileBytes);
            }

            return null;
        }
    }

    internal partial class UploadFileRequestObject
    {
        public string Path { get; set; }
        public DateTime Expire { get; set; }
    }
}
