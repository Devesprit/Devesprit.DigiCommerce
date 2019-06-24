using Devesprit.Data.Events;

namespace Devesprit.Services.Events
{
    public partial interface IConsumer<T> where T : IEvent
    {
        void HandleEvent(T eventMessage);
    }
}
