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
        public bool Published { get; set; }
        [Required]
        public DateTime PublishDate { get; set; }
        public DateTime? LastUpDate { get; set; }
        public bool ShowInHotList { get; set; }
        public int NumberOfViews { get; set; }
        public bool IsFeatured { get; set; }
        public bool ShowSimilarCases { get; set; }
        public bool AllowCustomerReviews { get; set; }
        public bool PinToTop { get; set; }
        [MaxLength(500)]
        public string PageTitle { get; set; }
        [Required,
         StringLength(500),
         Index(IsUnique = true),
        Column(TypeName = "NVARCHAR")]
        public string Slug { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeyWords { get; set; }
        public PostType? PostType { get; set; }
        

        public virtual ICollection<TblPostImages> Images { get; set; }
        public virtual ICollection<TblPostDescriptions> Descriptions { get; set; }
        public virtual ICollection<TblPostTags> Tags { get; set; }
        public virtual ICollection<TblPostAttributesMapping> Attributes { get; set; }
        public virtual ICollection<TblPostComments> Comments { get; set; }
        public virtual ICollection<TblPostCategories> Categories { get; set; }
    }
}