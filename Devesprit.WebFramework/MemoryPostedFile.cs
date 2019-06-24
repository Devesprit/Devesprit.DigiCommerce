using System.IO;
using System.Web;

namespace Devesprit.WebFramework
{
    public partial class MemoryPostedFile : HttpPostedFileBase
    {
        private readonly byte[] _fileBytes;

        public MemoryPostedFile(byte[] fileBytes, string fileName = null)
        {
            this._fileBytes = fileBytes;
            this.FileName = fileName;
            if (fileBytes != null)
            {
                this.InputStream = new MemoryStream(fileBytes);
            }
        }

        public override int ContentLength => _fileBytes.Length;

        public override string FileName { get; }

        public override Stream InputStream { get; }
    }
}