using Devesprit.Data.Enums;

namespace Devesprit.DigiCommerce.Models.Post
{
    public partial class LikeWishlistButtonsModel
    {
        public int PostId { get; set; }
        public bool AlreadyAddedToWishlist { get; set; }
        public bool AlreadyLiked { get; set; }
    }
}