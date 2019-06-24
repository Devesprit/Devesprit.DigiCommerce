namespace Devesprit.Services.PaymentGateway
{
    public partial class VerifyPaymentResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
    }
}