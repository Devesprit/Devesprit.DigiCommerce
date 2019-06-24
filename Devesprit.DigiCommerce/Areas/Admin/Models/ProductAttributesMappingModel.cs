using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Services.Posts;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class PostAttributeMappingModel
    {
        public int? Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Attribute")]
        public int PostAttributeId { get; set; }

        [DisplayNameLocalized("Option")]
        public int? AttributeOptionId { get; set; }

        [DisplayNameLocalized("Value")]
        [AllowHtml]
        public LocalizedString Value { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("DisplayOrder")]
        public int DisplayOrder { get; set; }

        public List<SelectListItem> PostAttributesList
        {
            get
            {
                var postAttributesService = DependencyResolver.Current.GetService<IPostAttributesService>();
                return postAttributesService.GetAsSelectList();
            }
        }

        public List<SelectListItem> PostAttributeOptionsList
        {
            get
            {
                var postAttributesService = DependencyResolver.Current.GetService<IPostAttributesService>();
                var result = postAttributesService.GetOptionsAsSelectList(PostAttributeId);
                return result;
            }
        }
    }
}