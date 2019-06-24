using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Models.Profile;
using X.PagedList;

namespace Devesprit.DigiCommerce.Factories.Interfaces
{
    public partial interface IProfileModelFactory
    {
        Task<ProfileModel> PrepareProfileModelAsync(TblUsers user);
        Task<UserInfoModel> PrepareUserInfoModelAsync(TblUsers user);
        UpdateProfileModel PrepareUpdateProfileModel(TblUsers user);
        Task<IPagedList<UserLikeWishlistModel>> PrepareUserLikedEntitiesModelAsync(IPagedList<TblUserLikes> likes);
        Task<IPagedList<UserLikeWishlistModel>> PrepareUserWishlistModelAsync(IPagedList<TblUserWishlist> wishlist);
    }
}