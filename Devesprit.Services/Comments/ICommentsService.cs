using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using X.PagedList;

namespace Devesprit.Services.Comments
{
    public partial interface ICommentsService
    {
        Task<IPagedList<TblPostComments>> GetAsPagedListAsync(bool onlyPublished, int? productId, int pageIndex = 1,
            int pageSize = int.MaxValue);
        Task<IPagedList<TblPostComments>> GetUserCommentsAsPagedListAsync(string userId, int pageIndex = 1,
            int pageSize = int.MaxValue);
        Task<IPagedList<TblPostComments>> FindCommentInListAsync(bool onlyPublished, int? productId, int commentId,
            int pageSize = int.MaxValue);
        IQueryable<TblPostComments> GetAsQueryable();
        Task<TblPostComments> FindByIdAsync(int id);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblPostComments record);
        Task<int> AddAsync(TblPostComments record, bool sendNotifications);
        Task<TblPostComments> SetCommentPublished(int commentId, bool published);
    }
}
