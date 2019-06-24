using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_Notifications")]
    public partial class TblNotifications : BaseEntity
    {
        public string UserId { get; set; }
        public virtual TblUsers User { get; set; }
        public bool Readed { get; set; }
        public bool IsMessage { get; set; }
        [Required]
        public string MessageResourceName { get; set; }
        public string MessageArguments { get; set; }
        [Required]
        public DateTime NotificationDate { get; set; }
    }
}
