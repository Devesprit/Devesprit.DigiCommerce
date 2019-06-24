﻿using System;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web.Mvc;
using System.Xml;

namespace Devesprit.WebFramework.ActionResults
{
    public partial class RssActionResult : ActionResult
    {
        public Encoding ContentEncoding { get; set; }
        public string ContentType { get; set; }

        private readonly SyndicationFeedFormatter _feed;
        public SyndicationFeedFormatter Feed => _feed;

        public RssActionResult(SyndicationFeedFormatter feed)
        {
            this._feed = feed;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var response = context.HttpContext.Response;
            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/xml";

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (_feed != null)
            {
                using (var xmlWriter = new XmlTextWriter(response.Output))
                {
                    xmlWriter.Formatting = Formatting.Indented;
                    _feed.WriteTo(xmlWriter);
                }
            }
        }
    }
}