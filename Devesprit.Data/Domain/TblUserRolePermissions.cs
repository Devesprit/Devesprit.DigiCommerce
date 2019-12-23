using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_UserRolePermissions")]
    public partial class TblUserRolePermissions : BaseEntity
    {
        [Required]
        public int RoleId { get; set; }
        public virtual TblUserRoles Role { get; set; }
        [Required]
        public bool HaveAccess { get; set; }
        [Required]
        public string AreaName { get; set; }
    }
}