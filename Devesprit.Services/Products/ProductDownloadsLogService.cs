using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Products
{
    public partial class ProductDownloadsLogService : IProductDownloadsLogService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;

        public ProductDownloadsLogService(AppDbContext dbContext,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblProductDownloadsLog> GetAsQueryable()
        {
            return _dbContext.ProductDownloadsLog.OrderByDescending(p=> p.DownloadDate);
        }

        public virtual async Task<TblProductDownloadsLog> FindByIdAsync(int id)
        {
            return await _dbContext.ProductDownloadsLog
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.ProductDownloadLog);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.ProductDownloadsLog.Where(p => p.Id == id).DeleteAsync();
            QueryCacheManager.ExpireTag(CacheTags.ProductDownloadLog);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task AddAsync(TblProductDownloadsLog log)
        {
            _dbContext.ProductDownloadsLog.Add(log);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.ProductDownloadLog);

            _eventPublisher.EntityInserted(log);
        }

        public virtual int GetNumberOfDownloads()
        {
            return _dbContext.ProductDownloadsLog.DeferredCount().FromCache(CacheTags.ProductDownloadLog);
        }

        public virtual async Task<Dictionary<DateTime, int>> DownloadReportAsync(DateTime fromDate, DateTime toDate, TimePeriodType periodType, bool? demoVersions)
        {
            var query = _dbContext.ProductDownloadsLog.Where(p => p.DownloadDate >= fromDate && p.DownloadDate <= toDate);
            if (demoVersions != null)
            {
                query = query.Where(p => p.IsDemoVersion == demoVersions.Value);
            }

            var downloads = await query.OrderBy(p => p.DownloadDate)
                .Select(p => new { p.DownloadDate }).FromCacheAsync(CacheTags.ProductDownloadLog);

            var report = new Dictionary<DateTime, int>();
            var datetimeToStringFormat = "g";
            switch (periodType)
            {
                case TimePeriodType.Hour:
                    datetimeToStringFormat = "yyyy/MM/dd HH:mm";
                    report = downloads.GroupBy(p =>
                            new DateTime(p.DownloadDate.Year, p.DownloadDate.Month, p.DownloadDate.Day, p.DownloadDate.Hour, 0,
                                0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
                case TimePeriodType.Day:
                    datetimeToStringFormat = "yyyy/MM/dd";
                    report = downloads.GroupBy(p =>
                            new DateTime(p.DownloadDate.Year, p.DownloadDate.Month, p.DownloadDate.Day, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
                case TimePeriodType.Month:
                    datetimeToStringFormat = "yyyy/MM";
                    report = downloads.GroupBy(p =>
                            new DateTime(p.DownloadDate.Year, p.DownloadDate.Month, 1, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
                case TimePeriodType.Year:
                    datetimeToStringFormat = "yyyy";
                    report = downloads.GroupBy(p =>
                            new DateTime(p.DownloadDate.Year, 1, 1, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
            }

            //Insert Zero Values
            var dateCounter = fromDate;
            while (dateCounter < toDate)
            {
                if (report.All(p => p.Key.ToString(datetimeToStringFormat) != dateCounter.ToString(datetimeToStringFormat)))
                {
                    report.Add(dateCounter, 0);
                }
                switch (periodType)
                {
                    case TimePeriodType.Hour:
                        dateCounter = dateCounter.AddHours(1);
                        break;
                    case TimePeriodType.Day:
                        dateCounter = dateCounter.AddDays(1);
                        break;
                    case TimePeriodType.Month:
                        dateCounter = dateCounter.AddMonths(1);
                        break;
                    case TimePeriodType.Year:
                        dateCounter = dateCounter.AddYears(1);
                        break;
                }
            }

            return report.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        }
    }
}