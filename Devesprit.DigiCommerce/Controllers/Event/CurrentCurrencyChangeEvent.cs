using Devesprit.Data.Events;

namespace Devesprit.DigiCommerce.Controllers.Event
{
    public partial class CurrentCurrencyChangeEvent: IEvent
    {
        public string Currency { get; }

        public CurrentCurrencyChangeEvent(string currency)
        {
            Currency = currency;
        }
    }
}