using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;


namespace Devesprit.Services.Currency
{
    public static partial class CurrencyExtensions
    {
        public static string ExchangeCurrencyStr(this double value, bool dontShowFree = false, TblCurrencies currency = null)
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
            if (currency == null)
            {
                currency = DependencyResolver.Current.GetService<IWorkContext>().CurrentCurrency;
            }
            return string.Format(currency.DisplayFormat, value.ExchangeCurrency(currency));
        }

        public static double ExchangeCurrency(this double value, TblCurrencies currency = null)
        {
            if (currency == null)
            {
                currency = DependencyResolver.Current.GetService<IWorkContext>().CurrentCurrency;
            }
            if (currency.ExchangeRate != 0)
            {
                value = currency.ExchangeRate * value;
            }
            return value;
        }
    }
}
