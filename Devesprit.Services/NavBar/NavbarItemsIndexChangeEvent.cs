using Devesprit.Data.Events;

namespace Devesprit.Services.NavBar
{
    public partial class NavbarItemsIndexChangeEvent : IEvent
    {
        public int[] ItemsOrder { get; }
        public int Id { get; }
        public int? NewParentId { get; }

        public NavbarItemsIndexChangeEvent(int[] itemsOrder, int id, int? newParentId)
        {
            ItemsOrder = itemsOrder;
            Id = id;
            NewParentId = newParentId;
        }
    }
}