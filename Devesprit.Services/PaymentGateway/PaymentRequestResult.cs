using System.Collections.Generic;
using System.IO;

namespace Devesprit.Services.PaymentGateway
{
    public partial class PaymentRequestResult
    {
        public string RedirectUrl { get; set; }
        public Dictionary<string, string> PostDate { get; set; }
        public string Token { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}