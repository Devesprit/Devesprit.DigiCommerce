using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.WebFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public partial class MustBeTrueLocalizedAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly string _errorMessageResourceName;

        public MustBeTrueLocalizedAttribute(string errorMessageResourceName)
        {
            _errorMessageResourceName = errorMessageResourceName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is bool && (bool)value)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(String.Format(ErrorMessageString, validationContext.DisplayName));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var errorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource(_errorMessageResourceName).FormatWith(metadata.DisplayName);

            var rule = new ModelClientValidationRule
            {
                ErrorMessage = errorMessage,
                ValidationType = "shouldbetrue"
            };

            yield return rule;
        }

        public override string FormatErrorMessage(string name)
        {
            this.ErrorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource(_errorMessageResourceName);
            return base.FormatErrorMessage(name);
        }
    }
}