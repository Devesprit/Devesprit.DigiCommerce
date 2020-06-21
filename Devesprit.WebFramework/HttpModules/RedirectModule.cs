using System;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Settings;
using Devesprit.Data.Enums;
using Devesprit.Services;
using Devesprit.Services.Events;
using Devesprit.Services.Redirects;
using Devesprit.Utilities.Extensions;

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
                var rule = redirectsService.FindMatchedRuleForRequestedUrl(request.Url.AbsoluteUri.Trim());

                while (rule != null)
                {
                    if (rule.ResponseType == ResponseType.JustReturnStatusCode)
                    {
                        break;
                    }

                    if (rule.StopProcessingOfSubsequentRules)
                    {
                        break;
                    }

                    var responseUrl = redirectsService.GenerateRedirectUrl(rule, requestedUrl, true);
                    var newRule = redirectsService.FindMatchedRuleForRequestedUrl(responseUrl);
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
                    if (rule.ResponseType == ResponseType.JustReturnStatusCode)
                    {
                        ctx.Response.Clear();
                        if (rule.RedirectStatus != null)
                        {
                            ctx.Response.StatusCode = rule.RedirectStatus.Value;
                        }
                        ctx.Response.Flush();
                        ctx.Response.End();
                        return;
                    }

                    var responseUrl = redirectsService.GenerateRedirectUrl(rule, requestedUrl);

                    var eventPublisher = DependencyResolver.Current.GetService<IEventPublisher>();
                    switch (rule.ResponseType)
                    {
                        case ResponseType.Redirect:
                            ctx.Response.Redirect(responseUrl, false);
                            if (rule.RedirectStatus != null)
                            {
                                ctx.Response.StatusCode = rule.RedirectStatus.Value;
                            }
                            ctx.Response.Flush();
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