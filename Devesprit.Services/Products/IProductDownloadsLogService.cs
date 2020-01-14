using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;

namespace Devesprit.Services.Products
{
    public partial interface IProductDownloadsLogService
    {
        IQueryable<TblProductDownloadsLog> GetAsQueryable();
        Task<TblProductDownloadsLog> FindByIdAsync(int id);
        Task DeleteAsync(int id);
        Task AddAsync(TblProductDownloadsLog log);
        int GetNumberOfDownloads();

        #region Report Generators

        Task<Dictionary<DateTime, int>> DownloadReportAsync(DateTime fromDate, DateTime toDate,
            TimePeriodType periodType, bool? demoVersions);

        #endregion
    }
}