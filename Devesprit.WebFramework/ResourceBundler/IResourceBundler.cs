using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Devesprit.WebFramework.ResourceBundler
{
    public partial interface IResourceBundler
    {
        void AddScriptParts(ResourceLocation location, string part, bool excludeFromBundle, bool isAsync);
        void AppendScriptParts(ResourceLocation location, string part, bool excludeFromBundle, bool isAsync);
        void AddInlineScript(ResourceLocation location, string script, bool addToBundle = false);
        void AppendInlineScript(ResourceLocation location, string script, bool addToBundle = false);
        string GenerateScripts(UrlHelper urlHelper, ResourceLocation location, bool? bundleFiles = null);

        void AddCssFileParts(ResourceLocation location, string part, bool excludeFromBundle = false);
        void AppendCssFileParts(ResourceLocation location, string part, bool excludeFromBundle = false);
        void AddInlineCss(ResourceLocation location, string style, bool addToBundle = false);
        void AppendInlineCss(ResourceLocation location, string style, bool addToBundle = false);
        string GenerateCssFiles(UrlHelper urlHelper, ResourceLocation location, bool? bundleFiles = null);

        void AddCanonicalUrlParts(string part);
        void AppendCanonicalUrlParts(string part);
        string GenerateCanonicalUrls();

        void AddHeadCustomParts(string part);
        void AppendHeadCustomParts(string part);
        string GenerateHeadCustom();
    }
}
