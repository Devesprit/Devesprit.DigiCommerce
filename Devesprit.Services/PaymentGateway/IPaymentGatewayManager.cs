using System.Collections.Generic;

namespace Devesprit.Services.PaymentGateway
{
    public partial interface IPaymentGatewayManager
    {
        IPaymentMethod FindPaymentMethodBySystemName(string name);
        List<IPaymentMethod> GetAvailablePaymentGatewaysForCurrency(string currencyIso);
    }
}
