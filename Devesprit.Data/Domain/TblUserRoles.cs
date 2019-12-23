using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Devesprit.Data.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_UserRoles")]
    public partial class TblUserRoles : BaseEntity
    {
        [Required]
        public string RoleName { get; set; }
        public virtual ICollection<TblUserRolePermissions> Permissions { get; set; }
    }
}
