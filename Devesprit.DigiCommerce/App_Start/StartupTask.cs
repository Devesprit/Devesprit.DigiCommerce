using System;
using System.Linq;
using System.Net;
using Devesprit.Core;
using Devesprit.Core.Settings;
using Devesprit.Services;
using Devesprit.Services.Invoice;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.Users;
using Hangfire;
using Hangfire.Storage;

namespace Devesprit.DigiCommerce
{
    public partial class StartupTask : IStartupTask
    {
        private readonly ISettingService _settingService;

        public StartupTask(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public virtual void Execute()
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var jobs = connection.GetRecurringJobs().ToList();
                if (jobs.Count(p => p.Id == "Search Engine Indexes Generator") == 0)
                {
                    RecurringJob.AddOrUpdate<ISearchEngine>("Search Engine Indexes Generator", searchEngine => searchEngine.CreateIndex(), () => Cron.HourInterval(6),
                        TimeZoneInfo.Local);
                }                
                

                if (jobs.Count(p => p.Id == "Product & User Plan Expiration Notifications Sender") == 0)
                {
                    //Remainder Notifications Task
                    RecurringJob.AddOrUpdate<IUsersService>("Product & User Plan Expiration Notifications Sender",
                        usersService => usersService.SendExpirationNotificationsAsync(), () => Cron.Daily(23, 59),
                        TimeZoneInfo.Local);
                }

                if (jobs.Count(p => p.Id == "Delete Empty & Pending Invoices") == 0)
                {
                    //Remainder Notifications Task
                    RecurringJob.AddOrUpdate<IInvoiceService>("Delete Empty & Pending Invoices",
                        invoiceService => invoiceService.DeletePendingInvoices(), () => Cron.Daily(23, 59),
                        TimeZoneInfo.Local);
                }

                //KeepAlive Task
                var url = _settingService.LoadSetting<SiteSettings>().SiteUrl.Trim().TrimEnd('/') + "/KeepAlive/Index";
                RecurringJob.AddOrUpdate("Keep Website Alive", () => FireKeepAliveController(url), () => Cron.MinuteInterval(5));
            }
        }

        public virtual void FireKeepAliveController(string url)
        {
            using (var wc = new WebClient())
            {
                wc.DownloadString(url);
            }
        }

        public int Order => 0;
    }
}