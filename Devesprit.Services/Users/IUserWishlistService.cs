using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;

namespace Devesprit.Services.Users
{
    public partial interface IUserWishlistService
    {
        IQueryable<TblUserWishlist> GetAsQueryable();
        Task<TblUserWishlist> FindByIdAsync(int id);
        Task DeleteAsync(int id);
        Task DeletePostFromWishlistAsync(int postId);
        Task AddAsync(TblUserWishlist record);
        Task<bool> AddPostToUserWishlistAsync(int postId, string userId, PostType? postType);
        bool UserAddedThisPostToWishlist(int postId, string userId);
        Dictionary<int, bool> UserAddedThisPostToWishlist(int[] postIds, string userId);
    }
}