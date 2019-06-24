using System.Collections.Generic;
using System.Threading.Tasks;
using Devesprit.Core.Plugin;
using Devesprit.Data.Domain;

namespace Devesprit.Services.PaymentGateway
{
    public partial interface IPaymentMethod: IPlugin
    {
        Task<PaymentRequestResult> RequestForPaymentUrl(string callbackUrl, TblInvoices invoice);
        Task<VerifyPaymentResult> VerifyPayment(TblInvoices invoice);
        string PaymentGatewayName { get; }
        string PaymentGatewaySystemName { get; }
        List<string> AcceptedCurrenciesIso { get; }
        byte[] PaymentGatewayIcon { get; }
    }
}
