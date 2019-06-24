using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Users
{
    public partial interface IUserMessagingService
    {
        IQueryable<TblUserMessages> GetAsQueryable();
        Task<TblUserMessages> FindByIdAsync(int id);
        Task UpdateAsync(TblUserMessages record);
        Task<int> AddAsync(TblUserMessages record);
        Task<int> NumberOfUreadedMessages();
        Task DeleteAsync(int id);
        Task ReplyToMessage(int id, string text);
        Task SetAsReaded(int id);
        Task SetAsUnReaded(int id);
    }
}
