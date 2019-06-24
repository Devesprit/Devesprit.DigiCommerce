using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace Devesprit.WebFramework.Helpers
{
    public static partial class AjaxExtensions
    {
        public static MvcHtmlString HtmlActionLink(this AjaxHelper ajaxHelper, string linkText, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes)
        {
            var link = ajaxHelper.ActionLink(@"[replaceme]", actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);
            return MvcHtmlString.Create(link.ToHtmlString().Replace("[replaceme]", linkText));
        }
    }
}