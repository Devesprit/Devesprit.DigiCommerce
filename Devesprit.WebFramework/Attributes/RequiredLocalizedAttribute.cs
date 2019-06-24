using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.WebFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public partial class RequiredLocalizedAttribute : RequiredAttribute, IClientValidatable
    {
        public override bool IsValid(object value)
        {
            if (value is LocalizedString localizedString)
            {
                return localizedString.ContainsKey(0) && !string.IsNullOrWhiteSpace(localizedString[0]);
            }

            return base.IsValid(value);
        }

        public override string FormatErrorMessage(string name)
        {
            this.ErrorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource("FieldRequired");
            return base.FormatErrorMessage(name);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var errorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource("FieldRequired").FormatWith(metadata.DisplayName);

            ModelClientValidationRule mvr = new ModelClientValidationRule
            {
                ErrorMessage = errorMessage, ValidationType = "requiredlocalized"
            };
            return new[] { mvr };
        }
    }
}