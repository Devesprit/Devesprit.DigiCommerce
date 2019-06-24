using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class UserGroupModel
    {
        public int? Id { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(150)]
        [DisplayNameLocalized("GroupName")]
        public LocalizedString GroupName { get; set; }


        [DisplayNameLocalized("Description")]
        [AllowHtml]
        public LocalizedString GroupDescription { get; set; }


        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("SmallIconUrl")]
        public LocalizedString GroupSmallIcon { get; set; }


        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("LargeIconUrl")]
        public LocalizedString GroupLargeIcon { get; set; }


        [MaxLengthLocalized(50)]
        [DisplayNameLocalized("BackgroundColor")]
        public LocalizedString GroupBackgroundColor { get; set; }


        [MaxLengthLocalized(50)]
        [DisplayNameLocalized("TextColor")]
        public LocalizedString GroupTextColor { get; set; }


        [DisplayNameLocalized("DisplayOrder")]
        [RequiredLocalized(AllowEmptyStrings = false)]
        public int GroupDisplayOrder { get; set; }


        [DisplayNameLocalized("Priority")]
        [RequiredLocalized(AllowEmptyStrings = false)]
        public int GroupPriority { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("NumberOfDownload")]
        public int? MaxDownloadCount { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Per")]
        public TimePeriodType? MaxDownloadPeriodType { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("ExpirationDate")]
        public int? SubscriptionExpirationTime { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        public TimePeriodType? SubscriptionExpirationPeriodType { get; set; }

        [DisplayNameLocalized("Published")]
        public bool Published { get; set; } = true;


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("SubscriptionFee")]
        public double SubscriptionFee { get; set; }
        

        [DisplayNameLocalized("DiscountPercent")]
        public double SubscriptionDiscountPercentage { get; set; }


        [DisplayNameLocalized("DiscountForRenewalBeforeExpiration")]
        public double DiscountForRenewalBeforeExpiration { get; set; }


        [DisplayNameLocalized("WhenExtendCurrentPlanBtnShown")]
        public int WhenExtendCurrentPlanBtnShown { get; set; }
    }
}