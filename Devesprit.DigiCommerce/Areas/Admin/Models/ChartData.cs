using System.Collections.Generic;

namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class ChartData
    {
        public List<ChartPoint> ChartItems { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }
}