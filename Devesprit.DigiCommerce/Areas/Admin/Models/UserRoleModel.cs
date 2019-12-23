using Devesprit.WebFramework.Attributes;

namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class UserRoleModel
    {
        public int? Id { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("RoleName")]
        public string RoleName { get; set; }
    }
}