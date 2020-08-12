using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class FileServerModel
    {
        public int? Id { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("ServerName")]
        public string FileServerName { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("ServerUrl")]
        public string FileServerUrl { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("FileUploadServerUrl")]
        public string FileUploadServerUrl { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("LoginUsername")]
        public string ServiceUserName { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("LoginPassword")]
        public string ServicePassword { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("EncryptionKey")]
        public string EncryptionKey { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("EncryptionSalt")]
        public string EncryptionSalt { get; set; }
    }
}