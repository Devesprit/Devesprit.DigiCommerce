using System;
using System.ComponentModel;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.WebFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    public partial class DisplayNameLocalizedAttribute : DisplayNameAttribute
    {
        private readonly string _resourceKey;

        public DisplayNameLocalizedAttribute(string resourceKey)
            : base(resourceKey)
        {
            _resourceKey = resourceKey;
        }

        public override string DisplayName => DependencyResolver.Current.GetService<ILocalizationService>().GetResource(_resourceKey);
    }
}