using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.WebFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public partial class EmailAddressLocalizedAttribute : DataTypeAttribute, IClientValidatable
    {
        public EmailAddressLocalizedAttribute()
          : base(DataType.EmailAddress)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            this.ErrorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource("FieldInvalidEmail");
            return base.FormatErrorMessage(name);
        }

        public override bool IsValid(object value)
        {
            if (value is LocalizedString localizedString)
            {
                foreach (var str in localizedString.Values)
                {
                    if (!string.IsNullOrWhiteSpace(str) && !IsValidEmail(str))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return true;
            }

            return IsValidEmail(value.ToString());
        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var errorMessage = DependencyResolver.Current.GetService<ILocalizationService>()
                .GetResource("FieldInvalidEmail").FormatWith(metadata.DisplayName);

            ModelClientValidationRule mvr = new ModelClientValidationRule
            {
                ErrorMessage = errorMessage,
                ValidationType = "emailaddress"
            };
            return new[] { mvr };
        }
    }
}