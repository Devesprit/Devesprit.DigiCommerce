using System;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using Devesprit.Services.Redirects;

namespace Devesprit.WebFramework.HttpModules
{
    public partial class RedirectModule : IHttpModule
    {
        public virtual void Init(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
        }

        protected virtual void Application_BeginRequest(Object source, EventArgs e)
        {
            try
            {
                var app = (HttpApplication)source;
                var ctx = app.Context;
                var request = ctx.Request;

                var requestedUrl = request.Url;

                var redirectsService = DependencyResolver.Current.GetService<IRedirectsService>();
                var eventPublisher = DependencyResolver.Current.GetService<IEventPublisher>();
                var rule = redirectsService.FindMatchedRuleForRequestedUrl(request.Url.AbsoluteUri.Trim(), null);

                while (rule != null)
                {
                    if (rule.StopProcessingOfSubsequentRules)
                    {
                        break;
                    }

                    var responseUrl = redirectsService.GenerateRedirectUrl(rule, requestedUrl, true);
                    var newRule = redirectsService.FindMatchedRuleForRequestedUrl(responseUrl, rule.Order);
                    if (newRule == null)
                    {
                        break;
                    }
                    else
                    {
                        requestedUrl = new Uri(responseUrl);
                        rule = newRule;
                    }
                }

                if (rule != null)
                {
                    var responseUrl = redirectsService.GenerateRedirectUrl(rule, requestedUrl);
                    
                    if (responseUrl.StartsWith("/")) //check language iso code added to url if redirect to local path
                    {
                        var currentLanguage = DependencyResolver.Current.GetService<IWorkContext>().CurrentLanguage;
                        var isLocaleDefined = responseUrl.TrimStart('/').StartsWith(currentLanguage.IsoCode + "/",
                                                  StringComparison.InvariantCultureIgnoreCase) ||
                                              responseUrl.TrimStart('/').StartsWith(currentLanguage.IsoCode + "?",
                                                  StringComparison.InvariantCultureIgnoreCase) ||
                                              responseUrl.TrimStart('/').StartsWith(currentLanguage.IsoCode + "#",
                                                  StringComparison.InvariantCultureIgnoreCase) ||
                                              responseUrl.TrimStart('/').Equals(currentLanguage.IsoCode,
                                                  StringComparison.InvariantCultureIgnoreCase);
                        if (!isLocaleDefined)
                        {
                            responseUrl = $"/{currentLanguage.IsoCode.ToLower()}{responseUrl}";
                        }
                    }

                    switch (rule.ResponseType)
                    {
                        case ResponseType.Redirect:
                            ctx.Response.Redirect(responseUrl, false);
                            if (rule.RedirectStatus != null)
                            {
                                ctx.Response.StatusCode = (int)rule.RedirectStatus.Value;
                            }
                            ctx.Response.End();
                            eventPublisher.Publish(new RedirectEvent()
                            {
                                RequestedUrl = request.Url.ToString(),
                                ResponseUrl = responseUrl,
                                Redirects = rule
                            });
                            break;

                        case ResponseType.Rewrite:
                            ctx.RewritePath(responseUrl);
                            eventPublisher.Publish(new RedirectEvent()
                            {
                                RequestedUrl = request.Url.ToString(),
                                ResponseUrl = responseUrl,
                                Redirects = rule
                            });
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (Exception)
            {}
        }

        public virtual void Dispose()
        {}
    }
}