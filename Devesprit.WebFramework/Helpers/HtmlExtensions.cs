using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;
using Devesprit.Services;
using Devesprit.Services.Languages;
using Devesprit.Utilities.Extensions;
using Devesprit.WebFramework.ResourceBundler;
using Microsoft.Ajax.Utilities;

namespace Devesprit.WebFramework.Helpers
{
    public static partial class HtmlExtensions
    {
        public static MvcHtmlString LocalizedTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string format, object htmlAttributes)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            if (modelMetadata.ModelType != typeof(LocalizedString))
            {
                throw new Exception("Model must be of type LocalizedString.");
            }
            var langService = DependencyResolver.Current.GetService<ILanguagesService>();
            var localization = DependencyResolver.Current.GetService<ILocalizationService>();
            var model = modelMetadata.Model as LocalizedString;
            var fullHtmlFieldName = ExpressionHelper.GetExpressionText(expression);
            var allLanguages = langService.GetAsEnumerable();
            var defaultLanguage = langService.GetDefaultLanguage();

            //Default tab
            var result = $@"<ul class=""nav nav-tabs border-0 justify-content-end"" style=""margin-top: -18px"" id=""{fullHtmlFieldName}-tab-list"" role=""tablist"">";
            result += @"<li class=""nav-item"">";
            result +=
                $@"<a class=""nav-link active nav-link-xs"" id=""{fullHtmlFieldName}-default-tab"" data-toggle=""tab"" href=""#{fullHtmlFieldName}-default-tab-continer"" role=""tab"" aria-controls=""{fullHtmlFieldName}-default-tab-continer"" aria-selected=""true"">{
                        localization.GetResource("Default")
                    }</a>";
            result += "</li>";
            //Tab for each language
            foreach (var language in allLanguages)
            {
                result += @"<li class=""nav-item"">";
                result +=
                    $@"<a class=""nav-link nav-link-xs"" id=""{fullHtmlFieldName}-{language.IsoCode}-tab"" data-toggle=""tab"" href=""#{fullHtmlFieldName}-{language.IsoCode}-tab-continer"" role=""tab"" aria-controls=""{fullHtmlFieldName}-{language.IsoCode}-tab-continer"" aria-selected=""false"">{
                            language.LanguageName
                        }</a>";
                result += "</li>";
            }

            result += "</ul>";

            //Tab contents
            result += $@"<div class=""tab-content"" id=""{fullHtmlFieldName}-tab-content"">";
            result += $@"<div class=""tab-pane fade show active"" id=""{fullHtmlFieldName}-default-tab-continer"" role=""tabpanel"" aria-labelledby=""{fullHtmlFieldName}-default-tab"">";

            //Default Text box
            var tagBuilder = new TagBuilder("input");
            var dir = defaultLanguage.IsRtl ? "rtl-dir " : "ltr-dir ";
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            if (attributes.ContainsKey("class"))
                attributes["class"] = dir + attributes["class"];
            else
                attributes.Add("class", dir);
            tagBuilder.MergeAttributes(attributes, false);
            tagBuilder.MergeAttribute("type", HtmlHelper.GetInputTypeString(InputType.Text));
            tagBuilder.MergeAttribute("name", fullHtmlFieldName, true);
            tagBuilder.GenerateId(fullHtmlFieldName);
            if (htmlHelper.ViewData.ModelState.TryGetValue(fullHtmlFieldName, out var modelState) &&
                modelState.Errors.Count > 0)
            {
                tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
            }

            var validationAttributes = htmlHelper.GetUnobtrusiveValidationAttributes(fullHtmlFieldName, modelMetadata);
            tagBuilder.MergeAttributes(validationAttributes);
            tagBuilder.MergeAttribute("validation-element", fullHtmlFieldName, true);
            if (model != null && model.ContainsKey(0))
            {
                string text = htmlHelper.FormatValue(model[0], format);
                tagBuilder.MergeAttribute("value", text);
            }

            result += tagBuilder.ToString(TagRenderMode.SelfClosing);
            result += "</div>";

            //Text box for each language
            foreach (var language in allLanguages)
            {
                result += $@"<div class=""tab-pane fade"" id=""{fullHtmlFieldName}-{language.IsoCode}-tab-continer"" role=""tabpanel"" aria-labelledby=""{fullHtmlFieldName}-{language.IsoCode}-tab"">";

                tagBuilder = new TagBuilder("input");
                dir = language.IsRtl ? "rtl-dir " : "ltr-dir ";
                attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                if (attributes.ContainsKey("class"))
                    attributes["class"] = dir + attributes["class"];
                else
                    attributes.Add("class", dir);
                tagBuilder.MergeAttributes(attributes, false); 
                tagBuilder.MergeAttribute("type", HtmlHelper.GetInputTypeString(InputType.Text));
                tagBuilder.MergeAttribute("name", fullHtmlFieldName + "_" + language.Id, true);
                tagBuilder.GenerateId(fullHtmlFieldName + "_" + language.Id);
                if (modelState != null && modelState.Errors.Count > 0)
                {
                    tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
                tagBuilder.MergeAttributes(validationAttributes);
                tagBuilder.MergeAttribute("validation-element", fullHtmlFieldName, true);
                if (model != null && model.ContainsKey(language.Id))
                {
                    string text = htmlHelper.FormatValue(model[language.Id], format);
                    tagBuilder.MergeAttribute("value", text);
                }

                result += tagBuilder.ToString(TagRenderMode.SelfClosing);
                result += "</div>";
            }
            result += "</div>";

            return MvcHtmlString.Create(result);
        }

        public static MvcHtmlString LocalizedTextAreaFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            var modelMetadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            if (modelMetadata.ModelType != typeof(LocalizedString))
            {
                throw new Exception("Model must be of type LocalizedString.");
            }
            var langService = DependencyResolver.Current.GetService<ILanguagesService>();
            var localization = DependencyResolver.Current.GetService<ILocalizationService>();
            var model = modelMetadata.Model as LocalizedString;
            var fullHtmlFieldName = ExpressionHelper.GetExpressionText(expression);
            var allLanguages = langService.GetAsEnumerable();
            var defaultLanguage = langService.GetDefaultLanguage();

            //Default tab
            var result = $@"<ul class=""nav nav-tabs border-0 justify-content-end"" style=""margin-top: -18px"" id=""{fullHtmlFieldName}-tab-list"" role=""tablist"">";
            result += @"<li class=""nav-item"">";
            result +=
                $@"<a class=""nav-link active nav-link-xs"" id=""{fullHtmlFieldName}-default-tab"" data-toggle=""tab"" href=""#{fullHtmlFieldName}-default-tab-continer"" role=""tab"" aria-controls=""{fullHtmlFieldName}-default-tab-continer"" aria-selected=""true"">{
                    localization.GetResource("Default")
                    }</a>";
            result += "</li>";
            //Tab for each language
            foreach (var language in allLanguages)
            {
                result += @"<li class=""nav-item"">";
                result +=
                    $@"<a class=""nav-link  nav-link-xs"" id=""{fullHtmlFieldName}-{language.IsoCode}-tab"" data-toggle=""tab"" href=""#{fullHtmlFieldName}-{language.IsoCode}-tab-continer"" role=""tab"" aria-controls=""{fullHtmlFieldName}-{language.IsoCode}-tab-continer"" aria-selected=""false"">{
                            language.LanguageName
                        }</a>";
                result += "</li>";
            }

            result += "</ul>";

            //Tab contents
            result += $@"<div class=""tab-content"" id=""{fullHtmlFieldName}-tab-content"">";
            result += $@"<div class=""tab-pane fade show active"" id=""{fullHtmlFieldName}-default-tab-continer"" role=""tabpanel"" aria-labelledby=""{fullHtmlFieldName}-default-tab"">";

            //Default Text Area
            var tagBuilder = new TagBuilder("textarea");
            var dir = defaultLanguage.IsRtl ? "rtl-dir " : "ltr-dir ";
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            if (attributes.ContainsKey("class"))
                attributes["class"] = dir + attributes["class"];
            else
                attributes.Add("class", dir);
            tagBuilder.MergeAttributes(attributes, false);
            tagBuilder.MergeAttribute("name", fullHtmlFieldName, true);
            tagBuilder.GenerateId(fullHtmlFieldName);
            if (htmlHelper.ViewData.ModelState.TryGetValue(fullHtmlFieldName, out var modelState) &&
                modelState.Errors.Count > 0)
            {
                tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
            }
            var validationAttributes = htmlHelper.GetUnobtrusiveValidationAttributes(fullHtmlFieldName, modelMetadata);
            tagBuilder.MergeAttributes(validationAttributes);
            tagBuilder.MergeAttribute("validation-element", fullHtmlFieldName, true);
            if (model != null && model.ContainsKey(0))
            {
                string text = model[0];
                tagBuilder.InnerHtml = text;
            }

            result += tagBuilder.ToString(TagRenderMode.Normal);
            result += "</div>";

            //Text Area for each language
            foreach (var language in allLanguages)
            {
                result += $@"<div class=""tab-pane fade"" id=""{fullHtmlFieldName}-{language.IsoCode}-tab-continer"" role=""tabpanel"" aria-labelledby=""{fullHtmlFieldName}-{language.IsoCode}-tab"">";

                tagBuilder = new TagBuilder("textarea");
                dir = language.IsRtl ? "rtl-dir " : "ltr-dir ";
                attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                if (attributes.ContainsKey("class"))
                    attributes["class"] = dir + attributes["class"];
                else
                    attributes.Add("class", dir);
                tagBuilder.MergeAttributes(attributes, false);
                tagBuilder.MergeAttribute("name", fullHtmlFieldName + "_" + language.Id, true);
                tagBuilder.GenerateId(fullHtmlFieldName + "_" + language.Id);
                if (modelState != null && modelState.Errors.Count > 0)
                {
                    tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
                tagBuilder.MergeAttributes(validationAttributes);
                tagBuilder.MergeAttribute("validation-element", fullHtmlFieldName, true);
                if (model != null && model.ContainsKey(language.Id))
                {
                    string text = model[language.Id];
                    tagBuilder.InnerHtml = text;
                }

                result += tagBuilder.ToString(TagRenderMode.Normal);
                result += "</div>";
            }
            result += "</div>";

            return MvcHtmlString.Create(result);
        }

        public static MvcHtmlString LocalizedEnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            var localization = DependencyResolver.Current.GetService<ILocalizationService>();
            Type enumType = typeof(TEnum);
            if (enumType.IsGenericType)
            {
                //Assume it's a nullable enum
                enumType = typeof(TEnum).GenericTypeArguments[0];
            }

            IEnumerable<TEnum> values = Enum.GetValues(enumType)
                .Cast<TEnum>();

            IEnumerable<SelectListItem> items =
                from value in values
                select new SelectListItem
                {
                    Text = string.IsNullOrWhiteSpace(value.GetCustomAttributeDescription())
                        ? value.ToString()
                        : localization.GetResource(value.GetCustomAttributeDescription()),
                    Value = value.ToString(),
                };

            return htmlHelper.DropDownListFor(expression, items, htmlAttributes);
        }

        public static MvcHtmlString LocalizedEnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, string optionLabel, object htmlAttributes)
        {
            var localization = DependencyResolver.Current.GetService<ILocalizationService>();
            Type enumType = typeof(TEnum);
            if (enumType.IsGenericType)
            {
                //Assume it's a nullable enum
                enumType = typeof(TEnum).GenericTypeArguments[0];
            }

            IEnumerable<TEnum> values = Enum.GetValues(enumType)
                .Cast<TEnum>();

            IEnumerable<SelectListItem> items =
                from value in values
                select new SelectListItem
                {
                    Text = string.IsNullOrWhiteSpace(value.GetCustomAttributeDescription())
                        ? value.ToString()
                        : localization.GetResource(value.GetCustomAttributeDescription()).ToTitleCase(),
                    Value = value.ToString(),
                };

            return htmlHelper.DropDownListFor(expression, items, optionLabel, htmlAttributes);
        }

        public static MvcHtmlString Widget(this HtmlHelper helper, string widgetZone, object additionalData = null, string area = null)
        {
            return helper.Action("WidgetsByZone", "Widget", new { widgetZone = widgetZone, additionalData = additionalData, area = area });
        }

        public static string GetCurrentThemeLayoutAddress(this HtmlHelper helper, bool baseLayout = false)
        {
            var settings = DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>();
            var currentTheme = settings.WebsiteTheme;
            currentTheme = string.IsNullOrWhiteSpace(currentTheme) ? "Default Theme" : currentTheme;
            var server = helper.ViewContext.HttpContext.Server;
            if (baseLayout)
            {
                var layoutPath = "~/Views/Shared/_BaseLayout.cshtml";
                if (File.Exists(server.MapPath("~/Themes/{0}/Views/Shared/_BaseLayout.cshtml".FormatWith(currentTheme))))
                {
                    layoutPath = "~/Themes/{0}/Views/Shared/_BaseLayout.cshtml".FormatWith(currentTheme);
                }
                return layoutPath;
            }
            else
            {
                var layoutPath = "~/Views/Shared/_Layout.cshtml";
                if (File.Exists(server.MapPath("~/Themes/{0}/Views/Shared/_Layout.cshtml".FormatWith(currentTheme))))
                {
                    layoutPath = "~/Themes/{0}/Views/Shared/_Layout.cshtml".FormatWith(currentTheme);
                }
                return layoutPath;
            }
        }

        public static string GetCurrentThemeAdminAreaLayoutAddress(this HtmlHelper helper, bool baseLayout = false)
        {
            var settings = DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>();
            var currentTheme = settings.WebsiteTheme;
            currentTheme = string.IsNullOrWhiteSpace(currentTheme) ? "Default Theme" : currentTheme;
            var server = helper.ViewContext.HttpContext.Server;
            if (baseLayout)
            {
                var layoutPath = "~/Areas/Admin/Views/Shared/_BaseLayout.cshtml";
                if (File.Exists(server.MapPath("~/Themes/{0}/Views/Shared/_BaseLayout.cshtml".FormatWith(currentTheme))))
                {
                    layoutPath = "~/Themes/{0}/Areas/Admin/Views/Shared/_BaseLayout.cshtml".FormatWith(currentTheme);
                }
                return layoutPath;
            }
            else
            {
                var layoutPath = "~/Areas/Admin/Views/Shared/_Layout.cshtml";
                if (File.Exists(server.MapPath("~/Themes/{0}/Views/Shared/_Layout.cshtml".FormatWith(currentTheme))))
                {
                    layoutPath = "~/Themes/{0}/Areas/Admin/Views/Shared/_Layout.cshtml".FormatWith(currentTheme);
                }
                return layoutPath;
            }
        }

        

        public static void AddScriptParts(this HtmlHelper html, string part, bool excludeFromBundle = false, bool isAsync = false)
        {
            AddScriptParts(html, ResourceLocation.Header, part, excludeFromBundle, isAsync);
        }
        public static void AddScriptParts(this HtmlHelper html, ResourceLocation location, string part, bool excludeFromBundle = false, bool isAsync = false)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AddScriptParts(location, part, excludeFromBundle, isAsync);
        }
        public static void AppendScriptParts(this HtmlHelper html, string part, bool excludeFromBundle = false, bool isAsync = false)
        {
            AppendScriptParts(html, ResourceLocation.Header, part, excludeFromBundle, isAsync);
        }
        public static void AppendScriptParts(this HtmlHelper html, ResourceLocation location, string part, bool excludeFromBundle = false, bool isAsync = false)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AppendScriptParts(location, part, excludeFromBundle, isAsync);
        }
        public static void AddInlineScript(this HtmlHelper html, Func<object, object> markup, bool addToBundle = false)
        {
            AddInlineScript(html, ResourceLocation.Header, markup, addToBundle);
        }
        public static void AddInlineScript(this HtmlHelper html, ResourceLocation location, Func<object, object> markup, bool addToBundle = false)
        {
            string script = (markup.Invoke(html.ViewContext)?.ToString() ?? "").TrimStart("<script>").TrimStart("<text>").TrimEnd("</script>").TrimEnd("</text>");
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AddInlineScript(location, script, addToBundle);
        }
        public static void AppendInlineScript(this HtmlHelper html, Func<object, object> markup, bool addToBundle = false)
        {
            AppendInlineScript(html, ResourceLocation.Header, markup, addToBundle);
        }
        public static void AppendInlineScript(this HtmlHelper html, ResourceLocation location, Func<object, object> markup, bool addToBundle = false)
        {
            string script = (markup.Invoke(html.ViewContext)?.ToString() ?? "").TrimStart("<script>").TrimStart("<text>").TrimEnd("</script>").TrimEnd("</text>");
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AppendInlineScript(location, script, addToBundle);
        }
        public static MvcHtmlString GenerateScripts(this HtmlHelper html, UrlHelper urlHelper,
            ResourceLocation location, bool? bundleFiles = null)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            return MvcHtmlString.Create(resourceBundler.GenerateScripts(urlHelper, location, bundleFiles));
        }



        public static void AddCssFileParts(this HtmlHelper html, string part, bool excludeFromBundle = false)
        {
            AddCssFileParts(html, ResourceLocation.Header, part, excludeFromBundle);
        }
        public static void AddCssFileParts(this HtmlHelper html, ResourceLocation location, string part, bool excludeFromBundle = false)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AddCssFileParts(location, part, excludeFromBundle);
        }
        public static void AppendCssFileParts(this HtmlHelper html, string part, bool excludeFromBundle = false)
        {
            AppendCssFileParts(html, ResourceLocation.Header, part, excludeFromBundle);
        }
        public static void AppendCssFileParts(this HtmlHelper html, ResourceLocation location, string part, bool excludeFromBundle = false)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AppendCssFileParts(location, part, excludeFromBundle);
        }
        public static void AddInlineCss(this HtmlHelper html, Func<object, object> markup, bool addToBundle = false)
        {
            AddInlineCss(html, ResourceLocation.Header, markup, addToBundle);
        }
        public static void AddInlineCss(this HtmlHelper html, ResourceLocation location, Func<object, object> markup, bool addToBundle = false)
        {
            string style = (markup.Invoke(html.ViewContext)?.ToString() ?? "").TrimStart("<style>").TrimStart("<text>").TrimEnd("</style>").TrimEnd("</text>");
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AddInlineCss(location, style, addToBundle);
        }
        public static void AppendInlineCss(this HtmlHelper html, Func<object, object> markup, bool addToBundle = false)
        {
            AppendInlineCss(html, ResourceLocation.Header, markup, addToBundle);
        }
        public static void AppendInlineCss(this HtmlHelper html, ResourceLocation location, Func<object, object> markup, bool addToBundle = false)
        {
            string style = (markup.Invoke(html.ViewContext)?.ToString() ?? "").TrimStart("<style>").TrimStart("<text>").TrimEnd("</style>").TrimEnd("</text>");
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AppendInlineCss(location, style, addToBundle);
        }
        public static MvcHtmlString GenerateCssFiles(this HtmlHelper html, UrlHelper urlHelper,
            ResourceLocation location, bool? bundleFiles = null)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            return MvcHtmlString.Create(resourceBundler.GenerateCssFiles(urlHelper, location, bundleFiles));
        }



        /// <summary>
        /// Add canonical URL element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Canonical URL part</param>
        public static void AddCanonicalUrlParts(this HtmlHelper html, string part)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AddCanonicalUrlParts(part);
        }
        /// <summary>
        /// Append canonical URL element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Canonical URL part</param>
        public static void AppendCanonicalUrlParts(this HtmlHelper html, string part)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AppendCanonicalUrlParts(part);
        }
        /// <summary>
        /// Generate all canonical URL parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Canonical URL part</param>
        /// <returns>Generated string</returns>
        public static MvcHtmlString GenerateCanonicalUrls(this HtmlHelper html, string part = "")
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            html.AppendCanonicalUrlParts(part);
            return MvcHtmlString.Create(resourceBundler.GenerateCanonicalUrls());
        }



        /// <summary>
        /// Add any custom element to the <![CDATA[<head>]]> element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">The entire element. For example, <![CDATA[<meta name="msvalidate.01" content="123121231231313123123" />]]></param>
        public static void AddHeadCustomParts(this HtmlHelper html, string part)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AddHeadCustomParts(part);
        }
        /// <summary>
        /// Append any custom element to the <![CDATA[<head>]]> element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">The entire element. For example, <![CDATA[<meta name="msvalidate.01" content="123121231231313123123" />]]></param>
        public static void AppendHeadCustomParts(this HtmlHelper html, string part)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            resourceBundler.AppendHeadCustomParts(part);
        }
        /// <summary>
        /// Generate all custom elements
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <returns>Generated string</returns>
        public static MvcHtmlString GenerateHeadCustom(this HtmlHelper html)
        {
            var resourceBundler = DependencyResolver.Current.GetService<IResourceBundler>();
            return MvcHtmlString.Create(resourceBundler.GenerateHeadCustom());
        }



        public static MvcHtmlString MinifyInlineScript(this HtmlHelper html, Func<object, object> markup)
        {
            string notMinifiedJs = (markup.Invoke(html.ViewContext)?.ToString() ?? "").TrimStart("<script>")
                .TrimStart("<text>").TrimEnd("</script>").TrimEnd("</text>");

            var minifier = new Minifier();
            var minifiedJs = minifier.MinifyJavaScript(notMinifiedJs, new CodeSettings
            {
                EvalTreatment = EvalTreatment.MakeImmediateSafe,
                PreserveImportantComments = false
            });
            return new MvcHtmlString("<script type='text/javascript'>" + minifiedJs + "</script>");
        }
        public static MvcHtmlString MinifyInlineCss(this HtmlHelper html, Func<object, object> markup)
        {
            string notMinifiedCss = (markup.Invoke(html.ViewContext)?.ToString() ?? "").TrimStart("<style>")
                .TrimStart("<text>").TrimEnd("</style>").TrimEnd("</text>");

            var minifier = new Minifier();
            var minifiedCss = minifier.MinifyStyleSheet(notMinifiedCss, new CssSettings()
            {
                CommentMode = CssComment.None
            });
            return new MvcHtmlString("<style>" + minifiedCss + "</style>");
        }
    }
}