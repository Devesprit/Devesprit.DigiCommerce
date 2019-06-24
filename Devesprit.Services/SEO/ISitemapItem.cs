using System;

namespace Devesprit.Services.SEO
{
    public partial interface ISitemapItem
    {
        string Url { get; }

        DateTime? LastModified { get; }

        SitemapChangeFrequency? ChangeFrequency { get; }

        double? Priority { get; }
    }
}
