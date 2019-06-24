using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Localization;


namespace Devesprit.Services.Currency
{
    public static partial class CurrencyExtensions
    {
        public static string ExchangeCurrencyStr(this double value, bool dontShowFree = false)
        {
            if (value <= 0)
            {
                if (!dontShowFree)
                {
                    var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
                    return localizationService.GetResource("Free");
                }

                return "-";
            }
            var currentCurrency = DependencyResolver.Current.GetService<IWorkContext>().CurrentCurrency;
            return string.Format(currentCurrency.DisplayFormat, value.ExchangeCurrency());
        }

        public static double ExchangeCurrency(this double value)
        {
            var currentCurrency = DependencyResolver.Current.GetService<IWorkContext>().CurrentCurrency;
            if (currentCurrency.ExchangeRate != 0)
            {
                value = currentCurrency.ExchangeRate * value;
            }
            return value;
        }
    }
}
