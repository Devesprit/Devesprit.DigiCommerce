using Devesprit.WebFramework.Attributes;

namespace Devesprit.DigiCommerce.Models.Comment
{
    public partial class CommentEditorModel
    {
        public int PostId { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("YourName")]
        public string UserName { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("YourEmail")]
        [EmailAddressLocalized]
        public string UserEmail { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(2000)]
        [DisplayNameLocalized("YourComment")]
        public string Comment { get; set; }

        [DisplayNameLocalized("Published")]
        public bool Published { get; set; }

        [DisplayNameLocalized("NotifyWhenReply")]
        public bool NotifyWhenReply { get; set; }

        [DisplayNameLocalized("NotifyWhenNewComment")]
        public bool NotifyWhenNewComment { get; set; }
        public bool UserIsAdmin { get; set; }
        public bool GuestUser { get; set; }

        public int? ReplyToCommentId { get; set; }
    }
}