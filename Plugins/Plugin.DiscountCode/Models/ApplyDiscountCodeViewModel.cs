using System;
using Devesprit.Data.Enums;
using Devesprit.WebFramework.Attributes;

namespace Plugin.DiscountCode.Models
{
    public partial class ApplyDiscountCodeViewModel
    {
        public Guid InvoiceId { get; set; }

        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("Plugin.DiscountCode.DiscountCode")]
        public string DiscountCode { get; set; }

        public InvoiceStatus InvoiceStatus { get; set; }
    }
}