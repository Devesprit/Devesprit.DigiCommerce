using Devesprit.Data.Events;

namespace Devesprit.Services.Events
{
    public partial interface IEventPublisher
    {
        void Publish<T>(T eventMessage) where T : IEvent;
    }
}
