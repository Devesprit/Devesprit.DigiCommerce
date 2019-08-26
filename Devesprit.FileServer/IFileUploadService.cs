using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Devesprit.FileServer
{
    [ServiceContract]
    public partial interface IFileUploadService
    {
        [OperationContract]
        Task<UploadFileResult> UploadFile(UploadFileRequest request);

        [OperationContract]
        Task<Stream> GetFileFromWeb(string fileUrl);
    }

    [MessageContract]
    public partial class UploadFileRequest : IDisposable
    {
        [MessageHeader(MustUnderstand = true)]
        public string UploadRequestKey { get; set; }

        [MessageBodyMember(Order = 1)]
        public System.IO.Stream FileByteStream { get; set; }

        public void Dispose()
        {
            if (FileByteStream != null)
            {
                FileByteStream.Close();
                FileByteStream.Dispose();
                FileByteStream = null;
            }
        }
    }

    [MessageContract]
    public partial class UploadFileResult
    {
        [MessageHeader(MustUnderstand = true)]
        public bool Success { get; set; }
        [MessageHeader(MustUnderstand = true)]
        public string Md5Hash { get; set; }
    }
}
