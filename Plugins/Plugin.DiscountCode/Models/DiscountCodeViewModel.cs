using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;

namespace Plugin.DiscountCode.Models
{
    public partial class DiscountCodeViewModel
    {
        public int? Id { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("Plugin.DiscountCode.DiscountCode")]
        public string DiscountCode { get; set; }

        [DisplayNameLocalized("Title")]
        public LocalizedString DiscountCodeTitle { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Amount")]
        public double DiscountAmount { get; set; }

        [DisplayNameLocalized("Plugin.DiscountCode.AmountIsPercentage")]
        public bool IsPercentage { get; set; }

        [DisplayNameLocalized("ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [DisplayNameLocalized("Plugin.DiscountCode.MaxNumberOfUsage")]
        public int? MaxNumberOfUsage { get; set; }
    }
}
