using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading.Tasks;
using System.Web;
using Devesprit.FileServer.ElmahConfig;
using Devesprit.Utilities;
using Devesprit.Utilities.Extensions;

namespace Devesprit.FileServer
{
    /*
         
            Note: 
            -To enable Upload File Service please set Web.config -> appSettings -> AllowToUploadFile to "true"
            -Set which file types allowed to upload from Web.config -> appSettings -> AllowedFileExtensionsToUpload (e.g., .zip;.rar;.txt) 
            -The maximum size of upload stream in 10Mb, to increase it you can edit Web.config file.
             Example:
       
              <system.webServer>
                <security>
                  <requestFiltering>
                    <!--Increase 'maxAllowedContentLength' to needed value: 11mb (value is in bytes)-->
                    <requestLimits maxAllowedContentLength="14534336"/>
                  </requestFiltering>
                </security>
              </system.webServer>

            -Also you can upload large files by chunking it. 
             Example:

              int chunkSize = 10 * 1024 * 1024; //Split files into 10MB slices
              using (FileStream fs = new FileStream("Your local file path", FileMode.Open, FileAccess.Read, FileShare.Read))
                  {
                      // Calculate total chunks to be sent to service
                      var totalChunks = (int)Math.Ceiling((double)fs.Length / chunkSize);
                      for (int i = 0; i < totalChunks; i++)
                      {
                          var startIndex = i * chunkSize;
                          var endIndex = (int)(startIndex + chunkSize > fs.Length ? fs.Length : startIndex + chunkSize);
                          var length = endIndex - startIndex;
                          var bytes = new byte[length];
                          
                          // Read bytes from file, and send upload request
                          fs.Read(bytes, 0, bytes.Length);
                          using (var stream = new MemoryStream(bytes))
                          {
                              bool success = false;
                              while (!success)
                              {
                                  var md5 = upload.UploadFile("Test.txt", "D:\\", i == 0 ? UploadMode.None : UploadMode.Append, stream, out success);
                                  if (success && i == totalChunks - 1)
                                  {
                                      if (md5 == CalculateMd5Checksum("Your local file path"))
                                      {
                                          Console.WriteLine("Uploaded successfully.");
                                      }
                                  }
                              }
                          }
                      }
                  }
             
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
    }

    internal partial class UploadFileRequestObject
    {
        public string Path { get; set; }
        public DateTime Expire { get; set; }
    }
}
