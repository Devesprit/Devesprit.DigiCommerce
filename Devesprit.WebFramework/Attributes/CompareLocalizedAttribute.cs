using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.WebFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public partial class CompareLocalizedAttribute : System.ComponentModel.DataAnnotations.CompareAttribute, IClientValidatable
    {
        private readonly string _errorMessageResourceName;

        public CompareLocalizedAttribute(string otherProperty, string errorMessageResourceName) : base(otherProperty)
        {
            _errorMessageResourceName = errorMessageResourceName;
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
                ValidationType = "compare"
            };
            mvr.ValidationParameters.Add("comparewith", OtherProperty);
            return new[] { mvr };
        }
    }
}