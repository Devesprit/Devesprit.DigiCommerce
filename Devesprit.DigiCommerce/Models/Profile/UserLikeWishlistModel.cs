using System;
using Devesprit.Data.Enums;

namespace Devesprit.DigiCommerce.Models.Profile
{
    public partial class UserLikeWishlistModel
    {
        public DateTime Date { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }
        public string PostHomePageUrl { get; set; }
        public string PostTitle { get; set; }
    }
}