using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;

namespace Devesprit.Services.Users
{
    public partial interface IUserLikesService
    {
        IQueryable<TblUserLikes> GetAsQueryable();
        Task<TblUserLikes> FindByIdAsync(int id);
        Task DeleteAsync(int id);
        Task DeletePostLikesAsync(int postId);
        Task AddAsync(TblUserLikes like);
        Task<bool> LikePostAsync(int postId, string userId, PostType? postType);
        bool UserLikedThisPost(int postId, string userId);
        Dictionary<int, bool> UserLikedThisPost(int[] postIds, string userId);
        int GetNumberOfLikes(int postId);
        Dictionary<int, int> GetNumberOfLikes(int[] postIds);
    }
}