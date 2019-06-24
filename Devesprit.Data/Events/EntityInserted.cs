namespace Devesprit.Data.Events
{
    public partial class EntityInserted<T> : IEvent
    {
        public EntityInserted(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; private set; }
    }
}