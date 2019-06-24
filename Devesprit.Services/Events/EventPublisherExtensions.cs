using Devesprit.Data.Events;

namespace Devesprit.Services.Events
{
    public static partial class EventPublisherExtensions
    {
        public static void EntityInserted<T>(this IEventPublisher eventPublisher, T entity)
        {
            eventPublisher.Publish(new EntityInserted<T>(entity));
        }

        public static void EntityUpdated<T>(this IEventPublisher eventPublisher, T entity, T oldEntity)
        {
            eventPublisher.Publish(new EntityUpdated<T>(entity, oldEntity));
        }

        public static void EntityDeleted<T>(this IEventPublisher eventPublisher, T entity)
        {
            eventPublisher.Publish(new EntityDeleted<T>(entity));
        }
    }
}
