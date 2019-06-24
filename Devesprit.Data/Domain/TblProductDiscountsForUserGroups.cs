using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_ProductDiscountsForUserGroups")]
    public partial class TblProductDiscountsForUserGroups : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }
        public virtual TblProducts Product { get; set; }
        [Required]
        public int UserGroupId { get; set; }
        public virtual TblUserGroups UserGroup { get; set; }

        [Required]
        public double DiscountPercent { get; set; }
        public bool ApplyDiscountToProductAttributes { get; set; }
    }
}