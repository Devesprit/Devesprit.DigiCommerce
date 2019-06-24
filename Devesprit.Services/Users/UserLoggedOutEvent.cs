using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Users
{
    public partial class UserLoggedOutEvent : IEvent
    {
        public UserLoggedOutEvent(TblUsers user)
        {
            this.User = user;
        }
        
        public TblUsers User { get; private set; }
    }
}