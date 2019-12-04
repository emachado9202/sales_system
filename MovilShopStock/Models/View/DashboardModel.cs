using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models
{
    public class DashboardModel
    {
        public Tuple<long, decimal> InQuantity { get; set; }
        public Tuple<decimal, decimal> InMoney { get; set; }
        public Tuple<long, decimal> OutQuantity { get; set; }
        public Tuple<decimal, decimal> OutMoney { get; set; }

        public Tuple<long, decimal> StockQuantity { get; set; }
        public Tuple<decimal, decimal> StockMoney { get; set; }

        public decimal GainToday { get; set; }
        public decimal TotalGain { get; set; }

        public List<Tuple<string, decimal>> UserMoney { get; set; }
        public List<Tuple<string, decimal>> PendentMoney { get; set; }
        public List<Tuple<string, List<Tuple<string, decimal>>>> Categories { get; set; }
    }
}