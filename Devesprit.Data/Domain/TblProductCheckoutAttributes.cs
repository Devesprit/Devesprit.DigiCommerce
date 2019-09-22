using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Devesprit.Data.Enums;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_ProductCheckoutAttributes")]
    public partial class TblProductCheckoutAttributes : BaseEntity
    {
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public int ProductId { get; set; }
        public virtual TblProducts Product { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public double UnitPrice { get; set; }
        public int? MinRange { get; set; }
        public int? MaxRange { get; set; }
        public string LicenseGeneratorServiceId { get; set; }
        public int DisplayOrder { get; set; }
        public ProductCheckoutAttributeType AttributeType { get; set; }
        public virtual ICollection<TblProductCheckoutAttributeOptions> Options { get; set; }
    }
}