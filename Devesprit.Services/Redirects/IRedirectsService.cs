using System;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Redirects
{
    public partial interface IRedirectsService
    {
        IQueryable<TblRedirects> GetAsQueryable();
        Task DeleteAsync(int id);
        Task<TblRedirects> FindByIdAsync(int id);
        Task<int> AddAsync(TblRedirects record);
        Task UpdateAsync(TblRedirects record);
        TblRedirects FindMatchedRuleForRequestedUrl(string url);
        string GenerateRedirectUrl(TblRedirects rule, Uri requestedUrl, bool absoluteUrl = false);
    }
}
