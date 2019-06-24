using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.WebFramework.Attributes
{
    public partial class MaxFileSizeAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        public override bool IsValid(object value)
        {
            var file = value as HttpPostedFileBase;
            return (file?.ContentLength ?? 0) <= _maxFileSize;
        }

        public override string FormatErrorMessage(string name)
        {
            this.ErrorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource("MaxAllowedFileSize");
            return base.FormatErrorMessage(_maxFileSize.ToString());
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var errorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource("MaxAllowedFileSize").FormatWith(_maxFileSize);

            ModelClientValidationRule mvr = new ModelClientValidationRule
            {
                ErrorMessage = errorMessage,
                ValidationType = "maxfilesize"
            };
            mvr.ValidationParameters.Add("maxsize", _maxFileSize);
            return new[] { mvr };
        }
    }
}