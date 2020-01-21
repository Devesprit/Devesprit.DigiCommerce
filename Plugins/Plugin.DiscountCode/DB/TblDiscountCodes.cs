using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Devesprit.Data;

namespace Plugin.DiscountCode.DB
{
    public class TblDiscountCodes : BaseEntity
    {
        [Required,
         Column(TypeName = "VARCHAR"),
         StringLength(250),
         MaxLength(250),
         Index(IsUnique = true)]
        public string DiscountCode { get; set; }
        public string DiscountCodeTitle { get; set; }
        [Required]
        public double DiscountAmount { get; set; }
        public bool IsPercentage { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? MaxNumberOfUsage { get; set; }
    }
}