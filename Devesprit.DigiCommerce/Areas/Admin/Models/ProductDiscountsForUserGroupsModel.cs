using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

using Devesprit.Services.Users;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class ProductDiscountsForUserGroupsModel
    {
        public int? Id { get; set; }

        [Required]
        public int ProductId { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("DiscountPercent")]
        public double DiscountPercent { get; set; }



        [DisplayNameLocalized("ApplyDiscountToHigherUserGroups")]
        public bool ApplyDiscountToHigherUserGroups { get; set; }

        [DisplayNameLocalized("ApplyDiscountToProductAttributes")]
        public bool ApplyDiscountToProductAttributes { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("SelectUserGroup")]
        public int UserGroupId { get; set; }


        public List<SelectListItem> UserGroupsList
        {
            get
            {
                var userGroupsService = DependencyResolver.Current.GetService<IUserGroupsService>();
                return userGroupsService.GetAsSelectList();
            }
        }
    }
}