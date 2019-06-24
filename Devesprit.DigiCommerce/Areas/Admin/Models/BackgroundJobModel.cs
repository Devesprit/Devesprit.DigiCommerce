using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Devesprit.WebFramework.Attributes;

namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class BackgroundJobModel
    {
        public string Id { get; set; }

        public string Job { get; set; }
        public string LastJobId { get; set; }

        public string LastExecution { get; set; }

        public string NextExecution { get; set; }

        public string LastExecutionState { get; set; }

        public string CronDesc { get; set; }
        public bool Paused { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("Cron")]
        public string Cron { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("TimeZone")]
        public string TimeZoneId { get; set; }
        public List<SelectListItem> TimeZoneList { get
        {
            var list = TimeZoneInfo.GetSystemTimeZones().Select(p=> new SelectListItem()
            {
                Value = p.Id,
                Text = p.DisplayName
            }).ToList();
            return list;
        } }
    }
}