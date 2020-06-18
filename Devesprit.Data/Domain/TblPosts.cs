using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Devesprit.Data.Enums;

namespace Devesprit.Data.Domain
{
    [Table(("Tbl_Posts"))]
    public abstract partial class TblPosts : BaseEntity
    {
        [Required, MaxLength(500)]
        public string Title { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public bool Published { get; set; }
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public DateTime PublishDate { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public DateTime? LastUpDate { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public bool ShowInHotList { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public int NumberOfViews { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public bool IsFeatured { get; set; }
        public bool ShowSimilarCases { get; set; }
        public bool ShowKeywords { get; set; }
        public bool AllowCustomerReviews { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public bool PinToTop { get; set; }
        [MaxLength(500)]
        public string PageTitle { get; set; }
        [Required,
         StringLength(500),
         Index(IsClustered = false, IsUnique = true),
        Column(TypeName = "VARCHAR")]
        public string Slug { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeyWords { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public PostType? PostType { get; set; }
        

        public virtual ICollection<TblPostSlugs> AlternativeSlugs { get; set; }
        public virtual ICollection<TblPostImages> Images { get; set; }
        public virtual ICollection<TblPostDescriptions> Descriptions { get; set; }
        public virtual ICollection<TblPostTags> Tags { get; set; }
        public virtual ICollection<TblPostAttributesMapping> Attributes { get; set; }
        public virtual ICollection<TblPostComments> Comments { get; set; }
        public virtual ICollection<TblPostCategories> Categories { get; set; }
        public virtual ICollection<TblUserLikes> UserLikes { get; set; }
        public virtual ICollection<TblUserWishlist> UserWishlist { get; set; }
    }
}