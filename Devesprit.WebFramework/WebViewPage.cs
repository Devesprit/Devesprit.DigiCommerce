using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.WebFramework.Localization;

namespace Devesprit.WebFramework
{

    public abstract partial class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>
    {
        private ILocalizationService _localizationService;
        private Localizer _localizer;

        public Localizer T
        {
            get
            {
                return _localizer ?? (_localizer = (format, args) =>
                {
                    var resFormat = _localizationService.GetResource(format);
                    if (string.IsNullOrEmpty(resFormat))
                    {
                        return new LocalizedMVCString(format);
                    }

                    return
                        new LocalizedMVCString((args == null || args.Length == 0)
                            ? resFormat
                            : string.Format(resFormat, args));
                });
            }
        }
        public override void InitHelpers()
        {
            base.InitHelpers();
            _localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
        }

    }

    public abstract partial class WebViewPage : WebViewPage<dynamic>
    {
    }
}
