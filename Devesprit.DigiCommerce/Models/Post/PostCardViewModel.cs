using System;
using System.Collections.Generic;

namespace Devesprit.DigiCommerce.Models.Post
{
    public partial class PostCardViewModel
    {
        public virtual int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string IconImageUrl { get; set; }
        public virtual int NumberOfViews { get; set; }
        public virtual int NumberOfLikes { get; set; }
        public virtual DateTime LastUpDate { get; set; }
        public virtual string MainImageUrl { get; set; }
        public virtual string DescriptionTruncated { get; set; }
        public virtual bool IsFeatured { get; set; }
        public virtual LikeWishlistButtonsModel LikeWishlistButtonsModel { get; set; }
        public virtual string PostUrl { get; set; }
        public virtual List<PostCategoriesModel> Categories { get; set; }
    }
}