namespace Devesprit.Data.Events
{
    public partial class EntityDeleted<T> : IEvent
    {
        public EntityDeleted(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; private set; }
    }
}