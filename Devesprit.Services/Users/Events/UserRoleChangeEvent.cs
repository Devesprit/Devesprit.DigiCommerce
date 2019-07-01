using Devesprit.Data.Events;

namespace Devesprit.Services.Users.Events
{
    public partial class UserRoleChangeEvent : IEvent
    {
        public string UserId { get; }
        public bool IsAdmin { get; }

        public UserRoleChangeEvent(string userId, bool isAdmin)
        {
            UserId = userId;
            IsAdmin = isAdmin;
        }
    }
}