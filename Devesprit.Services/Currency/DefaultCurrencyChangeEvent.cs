using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Currency
{
    public partial class DefaultCurrencyChangeEvent : IEvent
    {
        public TblCurrencies Record { get; }

        public DefaultCurrencyChangeEvent(TblCurrencies record)
        {
            Record = record;
        }
    }
}