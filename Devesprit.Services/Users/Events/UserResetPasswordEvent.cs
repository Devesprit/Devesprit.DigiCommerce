using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Users.Events
{
    public partial class UserResetPasswordEvent : IEvent
    {
        public UserResetPasswordEvent(TblUsers user)
        {
            this.User = user;
        }

        public TblUsers User
        {
            get; private set;
        }
    }
}