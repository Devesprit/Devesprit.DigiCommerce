using System;
using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Users
{
    public partial class CustomerUserGroupChangeEvent : IEvent
    {
        public string UserId { get; }
        public TblUserGroups UserGroup { get; }
        public DateTime? StartDate { get; }

        public CustomerUserGroupChangeEvent(string userId, TblUserGroups userGroup, DateTime? startDate)
        {
            UserId = userId;
            UserGroup = userGroup;
            StartDate = startDate;
        }
    }
}