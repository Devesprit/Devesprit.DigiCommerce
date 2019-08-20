using System;
using System.Collections.Generic;
using System.Linq;
using Devesprit.Core.Plugin;

namespace Devesprit.Services.PaymentGateway
{
    public partial class PaymentGatewayManager : IPaymentGatewayManager
    {
        private readonly IPluginFinder _pluginFinder;

        public PaymentGatewayManager(IPluginFinder pluginFinder)
        {
            _pluginFinder = pluginFinder;
        }

        public virtual List<IPaymentMethod> GetAvailablePaymentGatewaysForCurrency(string currencyIso)
        {
            var paymentMethods = _pluginFinder.GetPlugins<IPaymentMethod>();
            return paymentMethods.Where(p =>
                    p.AcceptedCurrenciesIso.Contains(currencyIso, StringComparer.OrdinalIgnoreCase))
                .OrderBy(p => p.PluginDescriptor.DisplayOrder).ToList();
        }

        public virtual IPaymentMethod FindPaymentMethodBySystemName(string name)
        {
            var paymentMethods = _pluginFinder.GetPlugins<IPaymentMethod>();
            return paymentMethods.FirstOrDefault(p =>
                string.Compare(p.PaymentGatewaySystemName, name, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
