using System;
using X.PagedList;

namespace Devesprit.DigiCommerce.Models.Post
{
    public partial class PostsListModel<T1, T2> where T1 : PostCardViewModel
        where T2 : Enum
    {
        public virtual IPagedList<T1> PostsList { get; set; }
        public virtual ViewStyles ViewStyle { get; set; }
        public virtual T2 PostsListType { get; set; }
        public virtual bool ShowPager { get; set; }
        public virtual int PageIndex { get; set; }
        public virtual int? PageSize { get; set; }
        public virtual int? FilterByCategoryId { get; set; }
        public virtual DateTime? FromDate { get; set; }
        public virtual string ItemWrapperStart { get; set; }
        public virtual string ItemWrapperEnd { get; set; }
    }

    public class PostsListModel : PostsListModel<PostCardViewModel, PostsListType>
    {
    }
}