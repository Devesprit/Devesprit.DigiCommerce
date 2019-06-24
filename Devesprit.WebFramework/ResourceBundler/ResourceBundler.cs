using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using Devesprit.Core.Settings;
using Devesprit.Services;
using Microsoft.Ajax.Utilities;

namespace Devesprit.WebFramework.ResourceBundler
{
    public partial class ResourceBundler : IResourceBundler
    {
        private static readonly object SLock = new object();
        private readonly Dictionary<ResourceLocation, List<ScriptReferenceMeta>> _scriptParts;
        private readonly Dictionary<ResourceLocation, List<ScriptReferenceMeta>> _inlineScripts;
        private readonly Dictionary<ResourceLocation, List<CssReferenceMeta>> _cssParts;
        private readonly Dictionary<ResourceLocation, List<CssReferenceMeta>> _inlineCss;
        private readonly List<string> _canonicalUrlParts;
        private readonly List<string> _headCustomParts;
        private readonly SiteSettings _siteSettings;

        public ResourceBundler(ISettingService settingService)
        {
            this._siteSettings = settingService.LoadSetting<SiteSettings>();
            this._scriptParts = new Dictionary<ResourceLocation, List<ScriptReferenceMeta>>();
            this._inlineScripts = new Dictionary<ResourceLocation, List<ScriptReferenceMeta>>();
            this._cssParts = new Dictionary<ResourceLocation, List<CssReferenceMeta>>();
            this._inlineCss = new Dictionary<ResourceLocation, List<CssReferenceMeta>>();
            this._canonicalUrlParts = new List<string>();
            this._headCustomParts = new List<string>();
            new List<string>();
        }

        #region Utilities

        protected virtual string ComputeHash(string[] parts)
        {
            if (parts == null || parts.Length == 0)
                throw new ArgumentException("parts");

            //calculate hash
            var hash = "";
            using (SHA256 sha = new SHA256Managed())
            {
                // string concatenation
                var hashInput = "";
                foreach (var part in parts)
                {
                    hashInput += string.Join("_", part.Split(new []{"\n\r"}, StringSplitOptions.RemoveEmptyEntries)).Replace(" ", "");
                    hashInput += ",";
                }

                byte[] input = sha.ComputeHash(Encoding.Unicode.GetBytes(hashInput));
                hash = HttpServerUtility.UrlTokenEncode(input);
            }

            return hash;
        }

        protected virtual IItemTransform GetCssTransform()
        {
            return new CssRewriteUrlTransform();
        }

        #endregion

        public virtual void AddScriptParts(ResourceLocation location, string part, bool excludeFromBundle, bool isAsync)
        {
            if (!_scriptParts.ContainsKey(location))
                _scriptParts.Add(location, new List<ScriptReferenceMeta>());

            if (string.IsNullOrEmpty(part))
                return;

            _scriptParts[location].Add(new ScriptReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsAsync = isAsync,
                Part = part
            });
        }
        public virtual void AppendScriptParts(ResourceLocation location, string part, bool excludeFromBundle, bool isAsync)
        {
            if (!_scriptParts.ContainsKey(location))
                _scriptParts.Add(location, new List<ScriptReferenceMeta>());

            if (string.IsNullOrEmpty(part))
                return;

            _scriptParts[location].Insert(0, new ScriptReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsAsync = isAsync,
                Part = part
            });
        }
        public void AddInlineScript(ResourceLocation location, string script, bool addToBundle)
        {
            if (!_inlineScripts.ContainsKey(location))
                _inlineScripts.Add(location, new List<ScriptReferenceMeta>());

            if (string.IsNullOrEmpty(script))
                return;

            _inlineScripts[location].Add(new ScriptReferenceMeta
            {
                Part = script,
                ExcludeFromBundle = !addToBundle
            });
        }
        public void AppendInlineScript(ResourceLocation location, string script, bool addToBundle)
        {
            if (!_inlineScripts.ContainsKey(location))
                _inlineScripts.Add(location, new List<ScriptReferenceMeta>());

            if (string.IsNullOrEmpty(script))
                return;

            _inlineScripts[location].Insert(0, new ScriptReferenceMeta
            {
                Part = script,
                ExcludeFromBundle = !addToBundle
            });
        }
        public virtual string GenerateScripts(UrlHelper urlHelper, ResourceLocation location, bool? bundleFiles = null)
        {
            if ((!_scriptParts.ContainsKey(location) || _scriptParts[location] == null) &&
                (!_inlineScripts.ContainsKey(location) || _inlineScripts[location] == null))
                return "";

            if (!_scriptParts.Any() && !_inlineScripts.Any())
                return "";

            if (!bundleFiles.HasValue)
            {
                //use setting if no value is specified
                bundleFiles = _siteSettings.EnableJsBundling && BundleTable.EnableOptimizations;
            }

            if (bundleFiles.Value)
            {
                var partsToBundle = new List<ScriptReferenceMeta>();
                var inlinePartsToBundle = new List<ScriptReferenceMeta>();
                var partsToDontBundle = new List<ScriptReferenceMeta>();
                var inlinePartsToDontBundle = new List<ScriptReferenceMeta>();

                if (_scriptParts.ContainsKey(location))
                {
                    partsToBundle.AddRange(_scriptParts[location]
                        .Where(x => !x.ExcludeFromBundle)
                        .Distinct());
                    partsToDontBundle.AddRange(_scriptParts[location]
                        .Where(x => x.ExcludeFromBundle)
                        .Distinct());
                }

                if (_inlineScripts.ContainsKey(location))
                {
                    inlinePartsToBundle.AddRange(_inlineScripts[location]
                        .Where(x => !x.ExcludeFromBundle)
                        .Distinct());
                    inlinePartsToDontBundle.AddRange(_inlineScripts[location]
                        .Where(x => x.ExcludeFromBundle)
                        .Distinct());
                }

                var result = new StringBuilder();
                var allScripts = new List<string>();
                allScripts.AddRange(partsToBundle.Select(x=> x.Part));
                allScripts.AddRange(inlinePartsToBundle.Select(x=> x.Part));
                if (allScripts.Any())
                {
                    var hash = ComputeHash(allScripts.ToArray());
                    string bundleVirtualPath = "~/bundles/scripts/" + hash;
                    //create bundle
                    lock (SLock)
                    {
                        var bundleFor = BundleTable.Bundles.GetBundleFor(bundleVirtualPath);
                        if (bundleFor == null)
                        {
                            var bundle = new ScriptBundle(bundleVirtualPath);
                            //bundle.Transforms.Clear();

                            //"As is" ordering
                            bundle.Orderer = new AsIsBundleOrderer();
                            //disable file extension replacements. renders scripts which were specified by a developer
                            bundle.EnableFileExtensionReplacements = false;
                            bundle.Include(partsToBundle.Select(x => x.Part).ToArray());

                            if (inlinePartsToBundle.Any())
                            {
                                var inlineScriptFilePath = HttpContext.Current.Server.MapPath($"~/App_Data/{hash}.js");
                                File.WriteAllText(inlineScriptFilePath,
                                    string.Join(Environment.NewLine, inlinePartsToBundle.Select(x => x.Part)));
                                bundle.Include($"~/App_Data/{hash}.js");
                            }

                            BundleTable.Bundles.Add(bundle);
                        }
                    }

                    //parts to bundle
                    result.AppendLine(Scripts.Render(bundleVirtualPath).ToString());
                    GC.Collect();
                }

                //parts to do not bundle
                foreach (var item in partsToDontBundle)
                {
                    result.AppendFormat("<script {2}src=\"{0}\" type=\"{1}\"></script>", urlHelper.Content(item.Part), "text/javascript", item.IsAsync ? "async " : "");
                    result.Append(Environment.NewLine);
                }

                //inline scripts to do not bundle
                if (inlinePartsToDontBundle.Any())
                {
                    string script = "";
                    foreach (var item in inlinePartsToDontBundle.Select(x => x.Part).Distinct())
                    {
                        script += urlHelper.Content(item);
                        script += Environment.NewLine;
                    }

                    if (_siteSettings.EnableInlineJsMinification)
                    {
                        var minifier = new Minifier();
                        script = minifier.MinifyJavaScript(script, new CodeSettings
                        {
                            EvalTreatment = EvalTreatment.MakeImmediateSafe,
                            PreserveImportantComments = false
                        });
                    }

                    result.Append("<script type=\"text/javascript\">");
                    result.Append(Environment.NewLine);
                    result.Append(script);
                    result.Append(Environment.NewLine);
                    result.Append("</script>");
                }
                
                return result.ToString();
            }
            else
            {
                //bundling is disabled
                var result = new StringBuilder();
                if (_scriptParts.ContainsKey(location))
                {
                    foreach (var item in _scriptParts[location].Select(x => new { x.Part, x.IsAsync }).Distinct())
                    {
                        result.AppendFormat("<script {2}src=\"{0}\" type=\"{1}\"></script>", urlHelper.Content(item.Part), "text/javascript", item.IsAsync ? "async " : "");
                        result.Append(Environment.NewLine);
                    }
                }

                //inline scripts
                if (_inlineScripts.ContainsKey(location))
                {
                    string script = "";
                    foreach (var item in _inlineScripts[location].Select(x => new { x.Part }).Distinct())
                    {
                        script += urlHelper.Content(item.Part);
                        script += Environment.NewLine;
                    }

                    if (_siteSettings.EnableInlineJsMinification)
                    {
                        var minifier = new Minifier();
                        script = minifier.MinifyJavaScript(script, new CodeSettings
                        {
                            EvalTreatment = EvalTreatment.MakeImmediateSafe,
                            PreserveImportantComments = false
                        });
                    }

                    result.Append("<script type=\"text/javascript\">");
                    result.Append(Environment.NewLine);
                    result.Append(script);
                    result.Append(Environment.NewLine);
                    result.Append("</script>");
                }


                return result.ToString();
            }
        }



        public virtual void AddCssFileParts(ResourceLocation location, string part, bool excludeFromBundle = false)
        {
            if (!_cssParts.ContainsKey(location))
                _cssParts.Add(location, new List<CssReferenceMeta>());

            if (string.IsNullOrEmpty(part))
                return;

            _cssParts[location].Add(new CssReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                Part = part
            });
        }
        public virtual void AppendCssFileParts(ResourceLocation location, string part, bool excludeFromBundle = false)
        {
            if (!_cssParts.ContainsKey(location))
                _cssParts.Add(location, new List<CssReferenceMeta>());

            if (string.IsNullOrEmpty(part))
                return;

            _cssParts[location].Insert(0, new CssReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                Part = part
            });
        }
        public void AddInlineCss(ResourceLocation location, string style, bool addToBundle)
        {
            if (!_inlineCss.ContainsKey(location))
                _inlineCss.Add(location, new List<CssReferenceMeta>());

            if (string.IsNullOrEmpty(style))
                return;

            _inlineCss[location].Add(new CssReferenceMeta
            {
                Part = style,
                ExcludeFromBundle = !addToBundle
            });
        }
        public void AppendInlineCss(ResourceLocation location, string style, bool addToBundle)
        {
            if (!_inlineCss.ContainsKey(location))
                _inlineCss.Add(location, new List<CssReferenceMeta>());

            if (string.IsNullOrEmpty(style))
                return;

            _inlineCss[location].Insert(0, new CssReferenceMeta
            {
                Part = style,
                ExcludeFromBundle = !addToBundle
            });
        }
        public virtual string GenerateCssFiles(UrlHelper urlHelper, ResourceLocation location, bool? bundleFiles = null)
        {
            if ((!_cssParts.ContainsKey(location) || _cssParts[location] == null) &&
                (!_inlineCss.ContainsKey(location) || _inlineCss[location] == null))
                return "";

            if (!_cssParts.Any() && !_inlineCss.Any())
                return "";

            if (!bundleFiles.HasValue)
            {
                //use setting if no value is specified
                bundleFiles = _siteSettings.EnableCssBundling && BundleTable.EnableOptimizations;
            }

            if (bundleFiles.Value)
            {
                var partsToBundle = new List<CssReferenceMeta>();
                var inlinePartsToBundle = new List<CssReferenceMeta>();
                var partsToDontBundle = new List<CssReferenceMeta>();
                var inlinePartsToDontBundle = new List<CssReferenceMeta>();

                if (_cssParts.ContainsKey(location))
                {
                    partsToBundle.AddRange(_cssParts[location]
                        .Where(x => !x.ExcludeFromBundle)
                        .Distinct());
                    partsToDontBundle.AddRange(_cssParts[location]
                        .Where(x => x.ExcludeFromBundle)
                        .Distinct());
                }

                if (_inlineCss.ContainsKey(location))
                {
                    inlinePartsToBundle.AddRange(_inlineCss[location]
                        .Where(x => !x.ExcludeFromBundle)
                        .Distinct());
                    inlinePartsToDontBundle.AddRange(_inlineCss[location]
                        .Where(x => x.ExcludeFromBundle)
                        .Distinct());
                }


                var result = new StringBuilder();
                var allStyles = new List<string>();
                allStyles.AddRange(partsToBundle.Select(x => x.Part));
                allStyles.AddRange(inlinePartsToBundle.Select(x => x.Part));
                if (allStyles.Any())
                {
                    //IMPORTANT: Do not use CSS bundling in virtual directories
                    var hash = ComputeHash(allStyles.ToArray());
                    string bundleVirtualPath = "~/bundles/styles/" + hash;

                    //create bundle
                    lock (SLock)
                    {
                        var bundleFor = BundleTable.Bundles.GetBundleFor(bundleVirtualPath);
                        if (bundleFor == null)
                        {
                            var bundle = new StyleBundle(bundleVirtualPath);
                            //bundle.Transforms.Clear();
                            
                            //"As is" ordering
                            bundle.Orderer = new AsIsBundleOrderer();
                            //disable file extension replacements. renders scripts which were specified by a developer
                            bundle.EnableFileExtensionReplacements = false;
                            foreach (var ptb in partsToBundle.Select(x => x.Part))
                            {
                                bundle.Include(ptb, GetCssTransform());
                            }

                            if (inlinePartsToBundle.Any())
                            {
                                var inlineStyleFilePath = HttpContext.Current.Server.MapPath($"~/App_Data/{hash}.css");
                                File.WriteAllText(inlineStyleFilePath,
                                    string.Join(Environment.NewLine, inlinePartsToBundle.Select(x => x.Part)));
                                bundle.Include($"~/App_Data/{hash}.css", GetCssTransform());
                            }

                            BundleTable.Bundles.Add(bundle);
                        }
                    }

                    //parts to bundle
                    result.AppendLine(Styles.Render(bundleVirtualPath).ToString());
                    GC.Collect();
                }

                //parts to do not bundle
                foreach (var item in partsToDontBundle)
                {
                    result.AppendFormat("<link href=\"{0}\" rel=\"stylesheet\" type=\"{1}\" />", urlHelper.Content(item.Part), "text/css");
                    result.Append(Environment.NewLine);
                }

                //inline styles to do not bundle
                if (inlinePartsToDontBundle.Any())
                {
                    string style = "";
                    foreach (var item in inlinePartsToDontBundle.Select(x => new { x.Part }).Distinct())
                    {
                        style += urlHelper.Content(item.Part);
                        style += Environment.NewLine;
                    }

                    if (_siteSettings.EnableInlineCssMinification)
                    {
                        var minifier = new Minifier();
                        style = minifier.MinifyStyleSheet(style, new CssSettings()
                        {
                            CommentMode = CssComment.None
                        });
                    }

                    result.Append("<style>");
                    result.Append(Environment.NewLine);
                    result.Append(style);
                    result.Append(Environment.NewLine);
                    result.Append("</style>");
                }

                return result.ToString();
            }
            else
            {
                //bundling is disabled
                var result = new StringBuilder();
                if (_cssParts.ContainsKey(location))
                {
                    foreach (var path in _cssParts[location].Select(x => x.Part).Distinct())
                    {
                        result.AppendFormat("<link href=\"{0}\" rel=\"stylesheet\" type=\"{1}\" />", urlHelper.Content(path), "text/css");
                        result.AppendLine();
                    }
                }

                //inline styles to do not bundle
                if (_inlineCss.ContainsKey(location))
                {
                    string style = "";
                    foreach (var item in _inlineCss[location].Select(x => new { x.Part }).Distinct())
                    {
                        style += urlHelper.Content(item.Part);
                        style += Environment.NewLine;
                    }

                    if (_siteSettings.EnableInlineCssMinification)
                    {
                        var minifier = new Minifier();
                        style = minifier.MinifyStyleSheet(style, new CssSettings()
                        {
                            CommentMode = CssComment.None
                        });
                    }

                    result.Append("<style>");
                    result.Append(Environment.NewLine);
                    result.Append(style);
                    result.Append(Environment.NewLine);
                    result.Append("</style>");
                }

                return result.ToString();
            }
        }


         
        public virtual void AddCanonicalUrlParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _canonicalUrlParts.Add(part);
        }
        public virtual void AppendCanonicalUrlParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _canonicalUrlParts.Insert(0, part);
        }
        public virtual string GenerateCanonicalUrls()
        {
            var result = new StringBuilder();
            foreach (var canonicalUrl in _canonicalUrlParts)
            {
                result.AppendFormat("<link rel=\"canonical\" href=\"{0}\" />", canonicalUrl);
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }



        public virtual void AddHeadCustomParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _headCustomParts.Add(part);
        }
        public virtual void AppendHeadCustomParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _headCustomParts.Insert(0, part);
        }
        public virtual string GenerateHeadCustom()
        {
            //use only distinct rows
            var distinctParts = _headCustomParts.Distinct().ToList();
            if (!distinctParts.Any())
                return "";

            var result = new StringBuilder();
            foreach (var path in distinctParts)
            {
                result.Append(path);
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }


        #region Nested classes

        private class ScriptReferenceMeta
        {
            public bool ExcludeFromBundle { get; set; }

            public bool IsAsync { get; set; }

            public string Part { get; set; }
        }

        private class CssReferenceMeta
        {
            public bool ExcludeFromBundle { get; set; }

            public string Part { get; set; }
        }

        private class AsIsBundleOrderer : IBundleOrderer
        {
            public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
            {
                return files;
            }
        }
        #endregion

    }
}
