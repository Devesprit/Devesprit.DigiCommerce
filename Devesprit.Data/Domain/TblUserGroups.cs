using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Devesprit.Data.Enums;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_UserGroups")]
    public partial class TblUserGroups: BaseEntity
    {
        [Required, MaxLength(150)]
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        [MaxLength(500)]
        public string GroupSmallIcon { get; set; }
        [MaxLength(500)]
        public string GroupLargeIcon { get; set; }
        [MaxLength(50)]
        public string GroupBackgroundColor { get; set; }
        [MaxLength(50)]
        public string GroupTextColor { get; set; }
        public virtual ICollection<TblUsers> SubscribedUsers { get; set; }
        public double SubscriptionFee { get; set; }
        public double SubscriptionDiscountPercentage { get; set; }
        public double DiscountForRenewalBeforeExpiration { get; set; }
        public int GroupDisplayOrder { get; set; }
        public int GroupPriority { get; set; }
        public int? MaxDownloadCount { get; set; }
        public TimePeriodType? MaxDownloadPeriodType { get; set; }
        public int? SubscriptionExpirationTime { get; set; }
        public TimePeriodType? SubscriptionExpirationPeriodType { get; set; }
        public bool Published { get; set; }
    }
}
