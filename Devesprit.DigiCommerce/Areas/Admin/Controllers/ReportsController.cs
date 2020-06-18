using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Autofac.Extras.DynamicProxy;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Currency;
using Devesprit.Services.Invoice;
using Devesprit.Services.Localization;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Products;
using Devesprit.Services.Users;
using Devesprit.Utilities.Extensions;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using JetBrains.Annotations;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("Reports")]
    public partial class ReportsController : BaseController
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IUsersService _usersService;
        private readonly IUserGroupsService _userGroupsService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductDownloadsLogService _productDownloadsLogService;

        public ReportsController(IInvoiceService invoiceService, 
            IUsersService usersService, 
            IUserGroupsService userGroupsService,
            ILocalizationService localizationService,
            IProductDownloadsLogService productDownloadsLogService)
        {
            _invoiceService = invoiceService;
            _usersService = usersService;
            _userGroupsService = userGroupsService;
            _localizationService = localizationService;
            _productDownloadsLogService = productDownloadsLogService;
        }

        [UserHasPermission("Reports_InvoicesChart")]
        public virtual async Task<ActionResult> InvoicesChart(DateTime? FromDate, DateTime? ToDate, TimePeriodType PeriodType = TimePeriodType.Day)
        {
            if (FromDate == null || ToDate == null || FromDate >= ToDate)
            {
                FromDate = DateTime.Now.AddDays(-6);
                FromDate = new DateTime(FromDate.Value.Year, FromDate.Value.Month, FromDate.Value.Day, 0, 0, 0);
                ToDate = DateTime.Now;
                ToDate = new DateTime(ToDate.Value.Year, ToDate.Value.Month, ToDate.Value.Day, 23, 59, 59);
            }

            var datetimeToStringFormat = "g";
            switch (PeriodType)
            {
                case TimePeriodType.Hour:
                    datetimeToStringFormat = "yyyy/MM/dd HH:mm";
                    break;
                case TimePeriodType.Day:
                    datetimeToStringFormat = "yyyy/MM/dd";
                    break;
                case TimePeriodType.Month:
                    datetimeToStringFormat = "yyyy/MM";
                    break;
                case TimePeriodType.Year:
                    datetimeToStringFormat = "yyyy";
                    break;
            }

            var allInvoices = await _invoiceService.InvoicesReportAsync(FromDate.Value, ToDate.Value, PeriodType, null);
            var paidInvoices = await _invoiceService.InvoicesReportAsync(FromDate.Value, ToDate.Value, PeriodType, InvoiceStatus.Paid);
            var allInvoicesCount = allInvoices.Sum(p => p.Value);

            var chartDatas = new List<ChartData>
            {
                new ChartData()
                {
                    ChartItems = allInvoices.Select(p => new ChartPoint() {Y = p.Value, X = p.Key.ToString(datetimeToStringFormat)}).ToList(),
                    Name = $"{_localizationService.GetResource("Total")} ({allInvoicesCount})",
                    Color = "#5e35b1"
                },
                new ChartData()
                {
                    ChartItems = paidInvoices.Select(p => new ChartPoint() {Y = p.Value, X = p.Key.ToString(datetimeToStringFormat)}).ToList(),
                    Name = $"{_localizationService.GetResource("Paid")} ({paidInvoices.Sum(p=> p.Value)} - %{(allInvoicesCount > 0 ? (paidInvoices.Sum(p=> p.Value) * 100) / allInvoicesCount : 0)})",
                    Color = "#bd30ab"
                }
            };

            return View("ChartReport", new ChartModel()
            {
                ChartDatas = chartDatas,
                ToDate = ToDate.Value,
                FromDate = FromDate.Value,
                PeriodType = PeriodType,
                XAxisTitle = _localizationService.GetResource("Date"),
                YAxisTitle = _localizationService.GetResource("NumberOfInvoices"),
                ControllerName = "Reports",
                ActionName = "InvoicesChart",
                ReportName = _localizationService.GetResource("Invoices")
            });
        }

        [UserHasPermission("Reports_SellsChart")]
        public virtual async Task<ActionResult> SellsChart(DateTime? FromDate, DateTime? ToDate, TimePeriodType PeriodType = TimePeriodType.Day)
        {
            if (FromDate == null || ToDate == null || FromDate >= ToDate)
            {
                FromDate = DateTime.Now.AddDays(-6);
                FromDate = new DateTime(FromDate.Value.Year, FromDate.Value.Month, FromDate.Value.Day, 0, 0, 0);
                ToDate = DateTime.Now;
                ToDate = new DateTime(ToDate.Value.Year, ToDate.Value.Month, ToDate.Value.Day, 23, 59, 59);
            }

            var datetimeToStringFormat = "g";
            switch (PeriodType)
            {
                case TimePeriodType.Hour:
                    datetimeToStringFormat = "yyyy/MM/dd HH:mm";
                    break;
                case TimePeriodType.Day:
                    datetimeToStringFormat = "yyyy/MM/dd";
                    break;
                case TimePeriodType.Month:
                    datetimeToStringFormat = "yyyy/MM";
                    break;
                case TimePeriodType.Year:
                    datetimeToStringFormat = "yyyy";
                    break;
            }

            var currencyList = await CurrencyService.GetAsEnumerableAsync();
            var chartDatas = new List<ChartData>();
            var random = new Random();
            foreach (var curr in currencyList.OrderBy(p=> p.DisplayOrder))
            {
                var sells = await _invoiceService.SellsReportAsync(FromDate.Value, ToDate.Value, PeriodType, curr.Id);
                chartDatas.Add(new ChartData()
                {
                    ChartItems = sells.Select(p => new ChartPoint()
                        {Y = p.Value, X = p.Key.ToString(datetimeToStringFormat)}).ToList(),
                    Name =
                        $"{curr.GetLocalized(p=> p.CurrencyName)}: {string.Format(curr.DisplayFormat, sells.Sum(p => p.Value))}",
                    Color = $"#{random.Next(0x1000000):X6}"
                });
            }

            return View("ChartReport", new ChartModel()
            {
                ChartDatas = chartDatas,
                ToDate = ToDate.Value,
                FromDate = FromDate.Value,
                PeriodType = PeriodType,
                XAxisTitle = _localizationService.GetResource("Date"),
                YAxisTitle = _localizationService.GetResource("Amount"),
                ControllerName = "Reports",
                ActionName = "SellsChart",
                ReportName = _localizationService.GetResource("Selling")
            });
        }

        [UserHasPermission("Reports_UsersChart")]
        public virtual async Task<ActionResult> UsersChart(DateTime? FromDate, DateTime? ToDate, TimePeriodType PeriodType = TimePeriodType.Day)
        {
            if (FromDate == null || ToDate == null || FromDate >= ToDate)
            {
                FromDate = DateTime.Now.AddDays(-6);
                FromDate = new DateTime(FromDate.Value.Year, FromDate.Value.Month, FromDate.Value.Day, 0, 0, 0);
                ToDate = DateTime.Now;
                ToDate = new DateTime(ToDate.Value.Year, ToDate.Value.Month, ToDate.Value.Day, 23, 59, 59);
            }

            var datetimeToStringFormat = "g";
            switch (PeriodType)
            {
                case TimePeriodType.Hour:
                    datetimeToStringFormat = "yyyy/MM/dd HH:mm";
                    break;
                case TimePeriodType.Day:
                    datetimeToStringFormat = "yyyy/MM/dd";
                    break;
                case TimePeriodType.Month:
                    datetimeToStringFormat = "yyyy/MM";
                    break;
                case TimePeriodType.Year:
                    datetimeToStringFormat = "yyyy";
                    break;
            }

            var allUsers = await _usersService.UsersReportAsync(FromDate.Value, ToDate.Value, PeriodType, false, 0);
            var verifiedUsers = await _usersService.UsersReportAsync(FromDate.Value, ToDate.Value, PeriodType, true, 0);
            var allUsersCount = allUsers.Sum(p => p.Value);

            var chartDatas = new List<ChartData>
            {
                new ChartData()
                {
                    ChartItems = allUsers.Select(p => new ChartPoint() {Y = p.Value, X = p.Key.ToString(datetimeToStringFormat)}).ToList(),
                    Name = $"{_localizationService.GetResource("Total")} ({allUsersCount})",
                    Color = "#5e35b1"
                },
                new ChartData()
                {
                    ChartItems = verifiedUsers.Select(p => new ChartPoint() {Y = p.Value, X = p.Key.ToString(datetimeToStringFormat)}).ToList(),
                    Name = $"{_localizationService.GetResource("EmailConfirmed")} ({verifiedUsers.Sum(p=> p.Value)} - %{(allUsersCount > 0 ? (verifiedUsers.Sum(p=> p.Value) * 100) / allUsersCount : 0)})",
                    Color = "#bd30ab"
                }
            };

            //By UserGroup
            foreach (var userGroup in (await _userGroupsService.GetAsEnumerableAsync()).OrderByDescending(p=> p.GroupPriority))
            {
                var userGroupReport = await _usersService.UsersReportAsync(FromDate.Value, ToDate.Value, PeriodType, false, userGroup.Id);

                chartDatas.Add(new ChartData()
                {
                    ChartItems = userGroupReport.Select(p => new ChartPoint() { Y = p.Value, X = p.Key.ToString(datetimeToStringFormat) }).ToList(),
                    Name = $"{userGroup.GetLocalized(x=> x.GroupName)} ({userGroupReport.Sum(p => p.Value)} - %{(allUsersCount > 0 ? (userGroupReport.Sum(p => p.Value) * 100) / allUsersCount : 0)})",
                    Color = userGroup.GetLocalized(x => x.GroupBackgroundColor)
                });
            }

            return View("ChartReport", new ChartModel()
            {
                ChartDatas = chartDatas,
                ToDate = ToDate.Value,
                FromDate = FromDate.Value,
                PeriodType = PeriodType,
                XAxisTitle = _localizationService.GetResource("Date"),
                YAxisTitle = _localizationService.GetResource("NumberOfUsers"),
                ControllerName = "Reports",
                ActionName = "UsersChart",
                ReportName = _localizationService.GetResource("Users")
            });
        }

        [UserHasPermission("Reports_DownloadsChart")]
        public virtual async Task<ActionResult> DownloadsChart(DateTime? FromDate, DateTime? ToDate, TimePeriodType PeriodType = TimePeriodType.Day)
        {
            if (FromDate == null || ToDate == null || FromDate >= ToDate)
            {
                FromDate = DateTime.Now.AddDays(-6);
                FromDate = new DateTime(FromDate.Value.Year, FromDate.Value.Month, FromDate.Value.Day, 0, 0, 0);
                ToDate = DateTime.Now;
                ToDate = new DateTime(ToDate.Value.Year, ToDate.Value.Month, ToDate.Value.Day, 23, 59, 59);
            }

            var datetimeToStringFormat = "g";
            switch (PeriodType)
            {
                case TimePeriodType.Hour:
                    datetimeToStringFormat = "yyyy/MM/dd HH:mm";
                    break;
                case TimePeriodType.Day:
                    datetimeToStringFormat = "yyyy/MM/dd";
                    break;
                case TimePeriodType.Month:
                    datetimeToStringFormat = "yyyy/MM";
                    break;
                case TimePeriodType.Year:
                    datetimeToStringFormat = "yyyy";
                    break;
            }

            var allDownloads = await _productDownloadsLogService.DownloadReportAsync(FromDate.Value, ToDate.Value, PeriodType, null);
            var fullDownloads = await _productDownloadsLogService.DownloadReportAsync(FromDate.Value, ToDate.Value, PeriodType, false);
            var demoDownloads = await _productDownloadsLogService.DownloadReportAsync(FromDate.Value, ToDate.Value, PeriodType, true);
            var allDownloadsCount = allDownloads.Sum(p => p.Value);

            var chartDatas = new List<ChartData>
            {
                new ChartData()
                {
                    ChartItems = allDownloads.Select(p => new ChartPoint() {Y = p.Value, X = p.Key.ToString(datetimeToStringFormat)}).ToList(),
                    Name = $"{_localizationService.GetResource("Total")} ({allDownloadsCount})",
                    Color = "#526ef5"
                },
                new ChartData()
                {
                    ChartItems = fullDownloads.Select(p => new ChartPoint() {Y = p.Value, X = p.Key.ToString(datetimeToStringFormat)}).ToList(),
                    Name = $"{_localizationService.GetResource("FullVersionFiles")} ({fullDownloads.Sum(p=> p.Value)} - %{(allDownloadsCount > 0 ? (fullDownloads.Sum(p=> p.Value) * 100) / allDownloadsCount : 0)})",
                    Color = "#28a745"
                },
                new ChartData()
                {
                    ChartItems = demoDownloads.Select(p => new ChartPoint() {Y = p.Value, X = p.Key.ToString(datetimeToStringFormat)}).ToList(),
                    Name = $"{_localizationService.GetResource("DemoVersionFiles")} ({demoDownloads.Sum(p=> p.Value)} - %{(allDownloadsCount > 0 ? (demoDownloads.Sum(p=> p.Value) * 100) / allDownloadsCount : 0)})",
                    Color = "#ff0000"
                }
            };

            return View("ChartReport", new ChartModel()
            {
                ChartDatas = chartDatas,
                ToDate = ToDate.Value,
                FromDate = FromDate.Value,
                PeriodType = PeriodType,
                XAxisTitle = _localizationService.GetResource("Date"),
                YAxisTitle = _localizationService.GetResource("NumberOfDownloads"),
                ControllerName = "Reports",
                ActionName = "DownloadsChart",
                ReportName = _localizationService.GetResource("Downloads")
            });
        }

        [UserHasPermission("Reports_DownloadLogs")]
        public virtual ActionResult DownloadLogs()
        {
            return View();
        }

        [HttpPost]
        [UserHasPermission("Reports_DownloadLogs")]
        public virtual async Task<ActionResult> DeleteDownloadLogs(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _productDownloadsLogService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [UserHasPermission("Reports_DownloadLogs")]
        public virtual ActionResult DownloadLogsGridDataSource(DataManager dm)
        {
            var query = _productDownloadsLogService.GetAsQueryable().Include(p=> p.Product).Include(p=> p.User);
            var postUrl = Url.Action("Index", "Product", new { area = "" });
            var userProfileUrl = Url.Action("Index", "Profile", new { area = "" });
            var dataSource = query.Select(p => new
            {
                p.Id,
                p.User.Email,
                p.UserId,
                p.DownloadDate,
                p.IsDemoVersion,
                p.UserIp,
                p.ProductId,
                ProductTitle = p.Product.Title,
                p.Product.Slug,
                p.DownloadLink,
                ProductUrl = "<a target='_blank' href='" + postUrl + "/" + p.Id + "/" + p.Product.Slug + "'>" + p.Product.Title + "</a>",
                UserProfileUrl = "<a target='_blank' href='" + userProfileUrl + "?userId=" + p.UserId + "'>" + p.User.Email + "</a>",
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        } 

        [UserHasPermission("Reports_DownloadLogs")]
        [HttpPost]
        public virtual async Task<ActionResult> WhoHasDownloadedThisProducts(int[] productIds, DateTime? fromDate, DateTime? toDate)
        {
            if (productIds == null || productIds.Length == 0)
            {
                return Content("");
            }
            var query = _productDownloadsLogService.GetAsQueryable()
                .Where(p => productIds.Contains(p.ProductId) && p.UserId != null);
            if (fromDate != null)
            {
                query = query.Where(p => p.DownloadDate >= fromDate);
            }
            if (toDate != null)
            {
                query = query.Where(p => p.DownloadDate <= toDate);
            }

            var downloadLogs = await query
                .Include(p=> p.User)
                .Select(p=> new {p.UserId, p.User.Email})
                .Distinct()
                .ToListAsync();

            var userProfileUrl = Url.Action("Index", "Profile", new { area = "" });

            var result = "<style>.ThreeCol{-webkit-column-count: 2;-moz-column-count: 2;column-count: 2;}</style><ol class='ThreeCol'>";
            result += string.Join("", downloadLogs.Select(p =>
                "<li><a target='_blank' href='" + userProfileUrl + "?userId=" + p.UserId + "'>" + p.Email + "</a></li>"));
            result += "</ol>";
            
            return Content(result);
        }

        [UserHasPermission("Reports_DownloadLogs")] 
        [HttpPost]
        public virtual async Task<ActionResult> WhoHasDownloadedAllOfThisProducts(int[] productIds, DateTime? fromDate, DateTime? toDate)
        {
            if (productIds == null || productIds.Length == 0)
            {
                return Content("");
            }

            var query = _productDownloadsLogService.GetAsQueryable()
                .Where(p => productIds.Contains(p.ProductId) && p.UserId != null);
            if (fromDate != null)
            {
                query = query.Where(p => p.DownloadDate >= fromDate);
            }
            if (toDate != null)
            {
                query = query.Where(p => p.DownloadDate <= toDate);
            }

            var downloadLogs = await query
                .Include(p => p.User)
                .Select(p => new { p.UserId, p.User.Email, p.ProductId })
                .Distinct()
                .ToListAsync();

            var userList = new List<string>();
            foreach (var userId in downloadLogs.Select(p=> p.UserId).Distinct())
            {
                bool thisUserDownloadedAllProducts = true;
                foreach (var productId in productIds)
                {
                    if (!downloadLogs.Any(p=> p.UserId == userId && p.ProductId == productId))
                    {
                        thisUserDownloadedAllProducts = false;
                        break;
                    }
                }

                if (thisUserDownloadedAllProducts)
                {
                    userList.Add(userId);
                }
            }

            var logs = userList.Select(p => new { UserId = p, Email = downloadLogs.FirstOrDefault(x => x.UserId == p)?.Email });
            var userProfileUrl = Url.Action("Index", "Profile", new { area = "" });
            var result = "<style>.ThreeCol{-webkit-column-count: 2;-moz-column-count: 2;column-count: 2;}</style><ol class='ThreeCol'>";
            result += string.Join("", logs.Select(p =>
                "<li><a target='_blank' href='" + userProfileUrl + "?userId=" + p.UserId + "'>" + p.Email + "</a></li>"));
            result += "</ol>";

            return Content(result);
        }
    }
}