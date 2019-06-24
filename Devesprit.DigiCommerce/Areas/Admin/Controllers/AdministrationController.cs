using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Invoice;
using Devesprit.Services.Localization;
using Devesprit.Services.Users;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdministrationController : BaseController
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IUsersService _usersService;
        private readonly IUserGroupsService _userGroupsService;
        private readonly ILocalizationService _localizationService;

        public AdministrationController(IInvoiceService invoiceService, 
            IUsersService usersService,
            IUserGroupsService userGroupsService,
            ILocalizationService localizationService)
        {
            _invoiceService = invoiceService;
            _usersService = usersService;
            _userGroupsService = userGroupsService;
            _localizationService = localizationService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 60)]
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

            return View("Partials/_Chart", new ChartModel()
            {
                ChartDatas = chartDatas,
                ToDate = ToDate.Value,
                FromDate = FromDate.Value,
                PeriodType = PeriodType,
                XAxisTitle = _localizationService.GetResource("Date"),
                YAxisTitle = _localizationService.GetResource("NumberOfInvoices"),
                ChartName = "NumberInvoicesChart",
                ControllerName = "InvoicesChart",
                UpdateTargetId = "invoiceChartHolder"
            });
        }
        
        [OutputCache(Duration = 60)]
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

            var sells = await _invoiceService.SellsReportAsync(FromDate.Value, ToDate.Value, PeriodType);
            var defaultCurrency = CurrencyService.GetDefaultCurrency();
            var chartDatas = new List<ChartData>
            {
                new ChartData()
                {
                    ChartItems = sells.Select(p => new ChartPoint() {Y = p.Value, X = p.Key.ToString(datetimeToStringFormat)}).ToList(),
                    Name = $"{_localizationService.GetResource("Total")} ({string.Format(defaultCurrency.DisplayFormat, sells.Sum(p=> p.Value))})",
                    Color = "#bd30ab"
                }
            };

            return View("Partials/_Chart", new ChartModel()
            {
                ChartDatas = chartDatas,
                ToDate = ToDate.Value,
                FromDate = FromDate.Value,
                PeriodType = PeriodType,
                XAxisTitle = _localizationService.GetResource("Date"),
                YAxisTitle = _localizationService.GetResource("Amount"),
                ChartName = "SellsChart",
                ControllerName = "SellsChart",
                UpdateTargetId = "sellsChartHolder"
            });
        }

        [OutputCache(Duration = 60)]
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

            return View("Partials/_Chart", new ChartModel()
            {
                ChartDatas = chartDatas,
                ToDate = ToDate.Value,
                FromDate = FromDate.Value,
                PeriodType = PeriodType,
                XAxisTitle = _localizationService.GetResource("Date"),
                YAxisTitle = _localizationService.GetResource("NumberOfUsers"),
                ChartName = "NumberUsersChart",
                ControllerName = "UsersChart",
                UpdateTargetId = "usersChartHolder"
            });
        }
    }
}