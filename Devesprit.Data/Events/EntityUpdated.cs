namespace Devesprit.Data.Events
{
    public partial class EntityUpdated<T>: IEvent
    {
        public EntityUpdated(T entity, T oldEntity)
        {
            this.Entity = entity;
            this.OldEntity = oldEntity;
        }

        public T Entity { get; private set; }
        public T OldEntity { get; private set; }
    }
}
