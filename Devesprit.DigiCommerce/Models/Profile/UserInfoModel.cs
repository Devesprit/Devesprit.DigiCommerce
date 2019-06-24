using System;

namespace Devesprit.DigiCommerce.Models.Profile
{
    public partial class UserInfoModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EMail { get; set; }
        public string Country { get; set; }
        public string Avatar { get; set; }
        public DateTime RegisterDate { get; set; }
        public string UserGroup { get; set; }
        public string UserGroupDownloadLimit { get; set; }
        public string DownloadLimit { get; set; }
        public DateTime? SubscriptionDate { get; set; }
        public DateTime? SubscriptionExpireDate { get; set; }
        public bool UserSubscribedToHighestPlan { get; set; }
        public bool ShowUserSubscriptionInfo { get; set; }
    }
}