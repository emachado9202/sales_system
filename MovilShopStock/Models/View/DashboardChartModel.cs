using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models.View
{
    public class DashboardChartModel
    {
        public string label { get; set; }
        public int borderWidth { get; set; }
        public string borderColor { get; set; }
        public string pointBackgroundColor { get; set; }
        public string backgroundColor { get; set; }
        public List<decimal> data { get; set; }
    }

    public class DashboardRadarModel
    {
        public List<string> labels { get; set; }
        public List<DashboardChartModel> datasets { get; set; }
    }
}