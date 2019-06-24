using Devesprit.Data.Events;

namespace Devesprit.DigiCommerce.Controllers.Event
{
    public partial class CurrentLanguageChangeEvent: IEvent
    {
        public string Language { get; }

        public CurrentLanguageChangeEvent(string language)
        {
            Language = language;
        }
    }
}