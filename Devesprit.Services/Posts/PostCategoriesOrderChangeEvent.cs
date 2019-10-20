using Devesprit.Data.Events;

namespace Devesprit.Services.Posts
{
    public partial class PostCategoriesOrderChangeEvent : IEvent
    {
        public int[] ItemsOrder { get; }
        public int Id { get; }
        public int? NewParentId { get; }

        public PostCategoriesOrderChangeEvent(int[] itemsOrder, int id, int? newParentId)
        {
            ItemsOrder = itemsOrder;
            Id = id;
            NewParentId = newParentId;
        }
    }
}