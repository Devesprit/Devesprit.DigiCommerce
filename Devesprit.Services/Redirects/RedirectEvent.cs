using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Redirects
{
    public partial class RedirectEvent : IEvent
    {
        public string RequestedUrl { get; set; }
        public string ResponseUrl { get; set; }
        public TblRedirects Redirects { get; set; }
    }
}
