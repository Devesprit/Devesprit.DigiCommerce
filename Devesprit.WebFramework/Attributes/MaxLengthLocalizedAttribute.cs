using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.WebFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public partial class MaxLengthLocalizedAttribute : MaxLengthAttribute, IClientValidatable
    {
        public MaxLengthLocalizedAttribute(int length): base(length)
        {
        }

        public override bool IsValid(object value)
        {
            if (value is LocalizedString localizedString)
            {
                return localizedString.Values.All(str => str.Length <= Length);
            }

            return base.IsValid(value);
        }

        public override string FormatErrorMessage(string name)
        {
            this.ErrorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource("FieldMaxLength");
            return base.FormatErrorMessage(name);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var errorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource("FieldMaxLength").FormatWith("", Length);

            ModelClientValidationRule mvr = new ModelClientValidationRule
            {
                ErrorMessage = errorMessage,
                ValidationType = "maxlength"
            };
            mvr.ValidationParameters.Add("maxallowedlength", Length);
            return new[] { mvr };
        }
    }
}