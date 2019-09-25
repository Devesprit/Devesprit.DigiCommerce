using System;

namespace Devesprit.DigiCommerce.Models.Post
{
    public partial class PostsExplorerModel<T> where T : Enum
    {
        public virtual T PostsListType { get; set; }
        public virtual int PageIndex { get; set; }
        public virtual int? PageSize { get; set; }
        public virtual int? FilterByCategoryId { get; set; }
        public virtual DateTime? FromDate { get; set; }
        public string CategoryName { get; set; }
    }

    public class PostsExplorerModel : PostsExplorerModel<PostsListType>
    {
    }
}