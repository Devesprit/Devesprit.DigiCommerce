namespace Devesprit.Services.PaymentGateway
{
    public partial class PaymentRequestResult
    {
        public string RedirectUrl { get; set; }
        public string Token { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}