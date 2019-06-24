using System.Web;
using Devesprit.Data.Events;

namespace Devesprit.DigiCommerce.Controllers.Event
{
    public partial class ErrorEvent: IEvent
    {
        public HttpContextBase HttpContext { get; }
        public string ErrorCode { get; }

        public ErrorEvent(HttpContextBase httpContext, string errorCode)
        {
            HttpContext = httpContext;
            ErrorCode = errorCode;
        }
    }
}