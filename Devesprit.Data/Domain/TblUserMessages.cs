using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_UserMessages")]
    public partial class TblUserMessages : BaseEntity
    {
        public string UserId { get; set; }
        public virtual TblUsers User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(250)]
        public string Email { get; set; }

        [Required]
        [MaxLength(500)]
        public string Subject { get; set; }

        public DateTime ReceiveDate { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime? ReplyDate { get; set; }

        public string ResponseText { get; set; }

        public bool Readed { get; set; }

        public bool Replied { get; set; }
    }
}