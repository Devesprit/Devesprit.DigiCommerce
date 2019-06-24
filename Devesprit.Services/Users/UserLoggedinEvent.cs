using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Users
{
    public partial class UserLoggedinEvent : IEvent
    {
        public UserLoggedinEvent(TblUsers user)
        {
            this.User = user;
        }

        public TblUsers User
        {
            get; private set;
        }
    }
}
