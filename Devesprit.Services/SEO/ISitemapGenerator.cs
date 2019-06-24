using System.Collections.Generic;
using System.Xml.Linq;

namespace Devesprit.Services.SEO
{
    public partial interface ISitemapGenerator
    {
        XDocument GenerateSiteMap(IEnumerable<ISitemapItem> items);
    }
}
