using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.WebFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public partial class FileExtensionsAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly string _errorMessageResourceName;
        private List<string> AllowedExtensions { get; set; }

        public FileExtensionsAttribute(string fileExtensions, string errorMessageResourceName)
        {
            _errorMessageResourceName = errorMessageResourceName;
            AllowedExtensions = fileExtensions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public override bool IsValid(object value)
        {
            if (value is HttpPostedFileBase file)
            {
                var fileName = file.FileName;

                return AllowedExtensions.Any(y => fileName.EndsWith(y));
            }

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            this.ErrorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource(_errorMessageResourceName);
            return base.FormatErrorMessage(name);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var errorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource(_errorMessageResourceName).FormatWith(metadata.DisplayName);

            ModelClientValidationRule mvr = new ModelClientValidationRule
            {
                ErrorMessage = errorMessage,
                ValidationType = "allowedextensions"
            };
            mvr.ValidationParameters.Add("extensions", string.Join(",", AllowedExtensions));
            return new[] { mvr };
        }
    }
}