using System.Web;
using Devesprit.Data.Events;

namespace Devesprit.DigiCommerce.Controllers.Event
{
    public partial class PageNotFoundEvent: IEvent
    {
        public HttpContextBase HttpContext { get; }
        public string ErrorCode { get; }

        public PageNotFoundEvent(HttpContextBase httpContext, string errorCode)
        {
            HttpContext = httpContext;
            ErrorCode = errorCode;
        }
    }
}