using System.Web.Mvc;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Models.Post;
using X.PagedList;

namespace Devesprit.DigiCommerce.Factories.Interfaces
{
    public partial interface IPostModelFactory
    {
        IPagedList<PostCardViewModel> PreparePostCardViewModel(IPagedList<TblPosts> posts, TblUsers currentUser,
            UrlHelper url);

        PostModel PreparePostModel(TblPosts post, TblUsers currentUser, UrlHelper url);
    }
}