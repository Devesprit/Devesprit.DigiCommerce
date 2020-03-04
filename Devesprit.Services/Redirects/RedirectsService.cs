using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using Devesprit.Utilities;
using Devesprit.Utilities.Extensions;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Redirects
{
    public partial class RedirectsService : IRedirectsService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;

        public RedirectsService(AppDbContext dbContext,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblRedirects> GetAsQueryable()
        {
            return _dbContext.Redirects.OrderByDescending(p => p.Order);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.Redirects.Where(p => p.Id == id).DeleteAsync();
            QueryCacheManager.ExpireTag(CacheTags.RedirectRule);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task<TblRedirects> FindByIdAsync(int id)
        {
            var result = await _dbContext.Redirects
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.RedirectRule);
            return result;
        }

        public virtual async Task<int> AddAsync(TblRedirects record)
        {
            record.Name = string.IsNullOrWhiteSpace(record.Name) ? record.RequestedUrl : record.Name;
            _dbContext.Redirects.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.RedirectRule);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task UpdateAsync(TblRedirects record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.Redirects.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.RedirectRule);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual TblRedirects FindMatchedRuleForRequestedUrl(string url, int? fromOrder)
        {
            var rules = GetAsQueryable().FromCache(CacheTags.RedirectRule);
            if (fromOrder != null)
            {
                rules = rules.Where(p => p.Order > fromOrder.Value);
            }

            foreach (var rule in rules.Where(p => p.Active))
            {
                switch (rule.MatchType)
                {
                    case MatchType.Exact:
                        if (string.Compare(rule.RequestedUrl, url, rule.IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture) == 0)
                        {
                            return rule;
                        }
                        break;
                    case MatchType.Regex:
                        try
                        {
                            if (Regex.IsMatch(url, rule.RequestedUrl, rule.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))
                            {
                                return rule;
                            }
                        }
                        catch
                        { }
                        break;
                    case MatchType.Wildcards:
                        try
                        {
                            if (url.IsMatchWildcard(rule.RequestedUrl, rule.IgnoreCase))
                            {
                                return rule;
                            }
                        }
                        catch
                        { }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }

        public virtual string GenerateRedirectUrl(TblRedirects rule, Uri requestedUrl, bool absoluteUrl = false)
        {
            if (rule.MatchType == MatchType.Exact)
            {
                var result = rule.ResponseUrl;
                if (rule.AppendQueryString && !string.IsNullOrWhiteSpace(requestedUrl.Query))
                {
                    result = rule.ResponseUrl.TrimEnd('?');
                    var query = requestedUrl.Query.TrimStart('?').ParseQueryString();
                    var queryArray = new List<string>();
                    foreach (string key in query.Keys)
                    {
                        queryArray.Add(key + "=" + query[key]);
                    }

                    result = result.BuildQueryStringUrl(queryArray.ToArray());
                }

                if (absoluteUrl)
                {
                    result = result.GetAbsoluteUrl(requestedUrl);
                }

                return result;
            }

            var pattern = rule.RequestedUrl;
            if (rule.MatchType == MatchType.Wildcards)
            {
                pattern = pattern.WildCardToRegular();
            }

            var groups = Regex.Match(requestedUrl.AbsoluteUri.Trim(), pattern, rule.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None).Groups;
            var responseUrl = rule.ResponseUrl;
            for (int i = 0; i < groups.Count; i++)
            {
                responseUrl = Regex.Replace(responseUrl, "{R:" + i + "}", groups[i].Value,
                    RegexOptions.IgnoreCase);
            }

            if (rule.AppendQueryString && !string.IsNullOrWhiteSpace(requestedUrl.Query))
            {
                var result = responseUrl.TrimEnd('?');
                var query = requestedUrl.Query.TrimStart('?').ParseQueryString();
                var queryArray = new List<string>();
                foreach (string key in query.Keys)
                {
                    queryArray.Add(key + "=" + query[key]);
                }

                responseUrl = result.BuildQueryStringUrl(queryArray.ToArray());
            }

            if (absoluteUrl)
            {
                responseUrl = responseUrl.GetAbsoluteUrl(requestedUrl);
            }

            return responseUrl;
        }
    }
}