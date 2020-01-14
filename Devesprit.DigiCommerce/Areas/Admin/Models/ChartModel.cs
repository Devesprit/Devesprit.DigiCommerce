using System;
using System.Collections.Generic;
using Devesprit.Data.Enums;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class ChartModel
    {
        public List<ChartData> ChartDatas { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("FromDate")]
        public DateTime FromDate { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("ToDate")]
        public DateTime ToDate { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("GroupBy")]
        public TimePeriodType PeriodType { get; set; }

        public string ReportName { get; set; }

        public string XAxisTitle { get; set; }
        public string YAxisTitle { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
    }
}