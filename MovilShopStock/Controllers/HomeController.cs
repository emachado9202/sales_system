using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MovilShopStock.Models;
using MovilShopStock.Models.Catalog;
using MovilShopStock.Models.Handlers;
using MovilShopStock.Models.View;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MovilShopStock.Controllers
{
    [Authorize(Roles = RoleConstants.Dealer + "," + RoleConstants.Editor + "," + RoleConstants.Administrator)]
    public class HomeController : GenericController
    {
        private ApplicationDbContext applicationDbContext = new ApplicationDbContext();

        [HttpPost]
        public async Task<ActionResult> GetProductByCat(string category_id)
        {
            Guid categoryId = Guid.Parse(category_id);
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            List<ProductModel> products = await applicationDbContext.Products.Where(x => x.Category_Id == categoryId && x.Business_Id == business_working).OrderBy(x => x.Name).Select(x => new ProductModel { DT_RowId = x.Id.ToString(), Product = x.Name }).ToListAsync();

            return Json(products);
        }

        [HttpPost]
        public async Task<ActionResult> GetSalePriceProduct(string product_id)
        {
            Guid productId = Guid.Parse(product_id);

            Product product = await applicationDbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);

            return Json(product?.SalePrice);
        }

        [HttpPost]
        public async Task<ActionResult> GetExistProduct(string product_id)
        {
            Guid productId = Guid.Parse(product_id);

            Product product = await applicationDbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);

            return Json(product?.In - product?.Out);
        }

        [HttpGet]
        public async Task<ActionResult> Dashboard()
        {
            DashboardModel model = new DashboardModel();

            DateTime month_init = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);

            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            decimal netprofit = 0;

            long quantity_in = (await applicationDbContext.StockIns.Include("Product").Where(x => x.Product.Business_Id == business_working).SumAsync(x => (int?)x.Quantity)) ?? 0;
            long quantity_in_month = (await applicationDbContext.StockIns.Include("Product").Where(x => x.Date > month_init && x.Product.Business_Id == business_working).SumAsync(x => (int?)x.Quantity)) ?? 0;
            decimal percent_quantity_in = quantity_in == 0 ? 0 : (quantity_in_month * 100) / quantity_in;

            model.InQuantity = new Tuple<long, decimal>(quantity_in, percent_quantity_in);

            List<StockIn> stockIns = await applicationDbContext.StockIns.Include("Product").Include("Product.Category").Where(x => x.Product.Business_Id == business_working).ToListAsync();
            decimal money_in = 0, money_in_month = 0;

            foreach (var stockIn in stockIns)
            {
                if (stockIn.Product.Category.SystemAction == ActionConstants.Sum)
                {
                    money_in += stockIn.ShopPrice * stockIn.Quantity;
                }
                else if (stockIn.Product.Category.SystemAction == ActionConstants.Rest)
                {
                    money_in -= stockIn.ShopPrice * stockIn.Quantity;
                }

                if (stockIn.Date > month_init)
                {
                    if (stockIn.Product.Category.SystemAction == ActionConstants.Sum)
                    {
                        money_in_month += stockIn.ShopPrice * stockIn.Quantity;
                    }
                    else if (stockIn.Product.Category.SystemAction == ActionConstants.Rest)
                    {
                        money_in_month -= stockIn.ShopPrice * stockIn.Quantity;
                    }
                }
            }

            decimal percent_money_in = money_in == 0 ? 0 : (money_in_month * 100) / money_in;

            model.InMoney = new Tuple<decimal, decimal>(money_in, percent_money_in);

            long quantity_out = (await applicationDbContext.StockOuts.Include("Product").Where(x => x.Product.Business_Id == business_working).SumAsync(x => (int?)x.Quantity)) ?? 0;
            long quantity_out_month = (await applicationDbContext.StockOuts.Include("Product").Where(x => x.Date > month_init && x.Product.Business_Id == business_working).SumAsync(x => (int?)x.Quantity)) ?? 0;
            decimal percent_quantity_out = quantity_out == 0 ? 0 : (quantity_out_month * 100) / quantity_out;

            model.OutQuantity = new Tuple<long, decimal>(quantity_out, percent_quantity_out);

            List<StockOut> stockOuts = await applicationDbContext.StockOuts.Include("Product").Include("Product.Category").Where(x => x.Product.Business_Id == business_working).ToListAsync();
            decimal money_out = 0, money_out_month = 0;

            foreach (var stockOut in stockOuts)
            {
                if (stockOut.Product.Category.SystemAction == ActionConstants.Sum)
                {
                    money_out += stockOut.SalePrice * stockOut.Quantity;
                }
                else if (stockOut.Product.Category.SystemAction == ActionConstants.Rest)
                {
                    money_out -= stockOut.SalePrice * stockOut.Quantity;
                }

                if (stockOut.Date > month_init)
                {
                    if (stockOut.Product.Category.SystemAction == ActionConstants.Sum)
                    {
                        money_out_month += stockOut.SalePrice * stockOut.Quantity;
                    }
                    else if (stockOut.Product.Category.SystemAction == ActionConstants.Rest)
                    {
                        money_out_month -= stockOut.SalePrice * stockOut.Quantity;
                    }
                }
            }

            decimal percent_money_out = money_out == 0 ? 0 : (money_out_month * 100) / money_out;

            model.OutMoney = new Tuple<decimal, decimal>(money_out, percent_money_out);

            long quantity_stock = (await applicationDbContext.Products.Where(x => x.Business_Id == business_working).SumAsync(x => (int?)x.In - x.Out)) ?? 0;
            long quantity_stock_month = (await applicationDbContext.Products.Where(x => x.LastUpdated > month_init && x.Business_Id == business_working).SumAsync(x => (int?)x.In - x.Out)) ?? 0;
            decimal percent_quantity_stock = quantity_stock == 0 ? 0 : (quantity_stock_month * 100) / quantity_stock;

            model.StockQuantity = new Tuple<long, decimal>(quantity_stock, percent_quantity_stock);

            decimal money_stock = (await applicationDbContext.Products.Where(x => x.Business_Id == business_working).SumAsync(x => (decimal?)x.CurrentPrice * (x.In - x.Out))) ?? 0;
            decimal money_stock_month = (await applicationDbContext.Products.Where(x => x.LastUpdated > month_init && x.Business_Id == business_working).SumAsync(x => (decimal?)x.CurrentPrice * (x.In - x.Out))) ?? 0;
            decimal percent_money_stock = money_stock == 0 ? 0 : (money_stock_month * 100) / money_stock;

            netprofit += money_stock;

            model.StockMoney = new Tuple<decimal, decimal>(money_stock, percent_money_stock);

            model.PendentMoney = new List<Tuple<string, decimal>>();
            List<string> dealer_ids = await applicationDbContext.Roles.Where(x => x.Name.Equals(RoleConstants.Dealer)).Select(x => x.Id).ToListAsync();
            List<User> dealers = await applicationDbContext.Users.Where(x => x.Roles.FirstOrDefault(r => dealer_ids.Contains(r.RoleId)) != null && x.BusinessUsers.FirstOrDefault(y => y.Business_Id == business_working) != null).ToListAsync();
            model.UserMoney = new List<Tuple<string, decimal>>();
            foreach (var u in dealers)
            {
                decimal money = 0;

                foreach (var stockOut in stockOuts.Where(x => x.User_Id == u.Id && x.Receiver_Id == null))
                {
                    money += stockOut.Quantity * stockOut.SalePrice;
                }
                netprofit += money;
                model.PendentMoney.Add(new Tuple<string, decimal>(u.UserName, money));
            }

            if (User.IsInRole(RoleConstants.Administrator) || User.IsInRole(RoleConstants.Editor))
            {
                DateTime today = DateTime.Today;

                foreach (var stockOut in stockOuts)
                {
                    if (stockOut.Product.Category.SystemAction == ActionConstants.Sum)
                    {
                        model.TotalGain += stockOut.Gain * stockOut.Quantity;
                    }
                    else if (stockOut.Product.Category.SystemAction == ActionConstants.Rest)
                    {
                        model.TotalGain -= stockOut.Gain * stockOut.Quantity;
                    }

                    if (stockOut.Date > today)
                    {
                        if (stockOut.Product.Category.SystemAction == ActionConstants.Sum)
                        {
                            model.GainToday += stockOut.Gain * stockOut.Quantity;
                        }
                        else if (stockOut.Product.Category.SystemAction == ActionConstants.Rest)
                        {
                            model.GainToday -= stockOut.Gain * stockOut.Quantity;
                        }
                    }
                }

                List<string> roles_ids = await applicationDbContext.Roles.Where(x => x.Name.Equals(RoleConstants.Editor) || x.Name.Equals(RoleConstants.Administrator)).Select(x => x.Id).ToListAsync();

                List<User> users = await applicationDbContext.Users.Where(x => x.Roles.FirstOrDefault(r => roles_ids.Contains(r.RoleId)) != null && x.BusinessUsers.FirstOrDefault(y => y.Business_Id == business_working) != null).ToListAsync();
                model.UserMoney = new List<Tuple<string, decimal>>();
                foreach (var u in users)
                {
                    netprofit += u.Cash;
                    model.UserMoney.Add(new Tuple<string, decimal>(u.UserName, u.Cash));
                }

                model.Categories = new List<Tuple<string, List<Tuple<string, decimal>>>>();
                var outs_categories = await applicationDbContext.StockOuts.Include("Product").Include("Product.Category").Where(x => x.Product.Category.ShowDashboard && x.Date > month_init && x.Product.Business_Id == business_working).GroupBy(x => x.Product.Category).ToListAsync();

                foreach (var ocat in outs_categories)
                {
                    List<Tuple<string, decimal>> temp = new List<Tuple<string, decimal>>();

                    foreach (var oup in ocat.GroupBy(x => x.Product))
                    {
                        decimal money_out_sale = 0;

                        foreach (var ou in oup)
                        {
                            money_out_sale += Math.Abs(ou.SalePrice) * ou.Quantity;
                        }

                        temp.Add(new Tuple<string, decimal>(oup.Key.Name, money_out_sale));
                    }

                    model.Categories.Add(new Tuple<string, List<Tuple<string, decimal>>>($"{ocat.Key.Name} (Mes)", temp));
                }

                model.NetProfit = netprofit;
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> SetBusiness(string id, string returnUrl)
        {
            string userId = User.Identity.GetUserId();
            User user = await applicationDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);

            user.CurrentBusiness_Id = Guid.Parse(id);

            applicationDbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
            await applicationDbContext.SaveChangesAsync();

            Session["BusinessWorking"] = id;

            return Redirect(returnUrl);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DashboardChart()
        {
            DateTime init_month = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);
            DateTime init_year = new DateTime(DateTime.Now.Year, 01, 01);
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            #region Grahp Daily

            List<IGrouping<int, StockIn>> stockIns = await applicationDbContext.StockIns.Include("Product.Category").Where(x => x.Date > init_month && x.Product.Category.Business_Id == business_working).GroupBy(x => x.Date.Day).ToListAsync();

            List<DashboardChartModel> inout_daily = new List<DashboardChartModel>();

            DashboardChartModel model_daily_in = new DashboardChartModel()
            {
                label = "Entradas",
                backgroundColor = "rgba(52, 143, 226, 0.2)",
                borderColor = "rgba(52, 143, 226, 0.8)",
                pointBackgroundColor = "rgba(52, 143, 226, 0.8)",
                data = new List<decimal>()
            };

            decimal[] data_in = new decimal[31];
            for (int i = 0; i < data_in.Length; i++)
            {
                var element = stockIns.FirstOrDefault(x => x.Key - 1 == i);

                decimal total = 0;
                if (element != null)
                {
                    foreach (var stock in element)
                    {
                        if (stock.Product.Category.SystemAction == ActionConstants.Sum)
                        {
                            total += stock.ShopPrice * stock.Quantity;
                        }
                        else if (stock.Product.Category.SystemAction == ActionConstants.Rest)
                        {
                            total -= stock.ShopPrice * stock.Quantity;
                        }
                    }
                }

                data_in[i] = total;
            }
            model_daily_in.data.AddRange(data_in);
            inout_daily.Add(model_daily_in);

            List<IGrouping<int, StockOut>> stockOuts = await applicationDbContext.StockOuts.Include("Product.Category").Where(x => x.Date > init_month && x.Product.Category.Business_Id == business_working).GroupBy(x => x.Date.Day).ToListAsync();

            DashboardChartModel model_daily_out = new DashboardChartModel()
            {
                label = "Salidas",
                backgroundColor = "rgba(45, 53, 60, 0.2)",
                borderColor = "rgba(45, 53, 60, 0.8)",
                pointBackgroundColor = "rgba(45, 53, 60, 0.8)",
                data = new List<decimal>()
            };

            DashboardChartModel model_daily_gain = new DashboardChartModel()
            {
                label = "Ganancia",
                backgroundColor = "rgba(52, 143, 226, 0.2)",
                borderColor = "rgba(52, 143, 226, 0.8)",
                pointBackgroundColor = "rgba(52, 143, 226, 0.8)",
                data = new List<decimal>()
            };

            decimal[] data_out = new decimal[31];
            decimal[] data_gain = new decimal[31];
            for (int i = 0; i < data_out.Length; i++)
            {
                var element = stockOuts.FirstOrDefault(x => x.Key - 1 == i);

                decimal total = 0, total_gain = 0;
                if (element != null)
                {
                    foreach (var stock in element)
                    {
                        if (stock.Product.Category.SystemAction == ActionConstants.Sum)
                        {
                            total += stock.SalePrice * stock.Quantity;
                            total_gain += stock.Gain * stock.Quantity;
                        }
                        else if (stock.Product.Category.SystemAction == ActionConstants.Rest)
                        {
                            total -= stock.SalePrice * stock.Quantity;
                            total_gain -= stock.Gain * stock.Quantity;
                        }
                    }
                }

                data_out[i] = total;
                data_gain[i] = total_gain;
            }
            model_daily_gain.data.AddRange(data_gain);
            model_daily_out.data.AddRange(data_out);
            inout_daily.Add(model_daily_out);

            #endregion Grahp Daily

            #region Grahp Monthly

            List<IGrouping<int, StockIn>> stockIns_monthly = await applicationDbContext.StockIns.Include("Product.Category").Where(x => x.Date > init_year && x.Product.Category.Business_Id == business_working).GroupBy(x => x.Date.Month).ToListAsync();

            List<DashboardChartModel> inout_monthly = new List<DashboardChartModel>();

            DashboardChartModel model_monthly_in = new DashboardChartModel()
            {
                label = "Entradas",
                backgroundColor = "rgba(52, 143, 226, 0.2)",
                borderColor = "rgba(52, 143, 226, 0.8)",
                pointBackgroundColor = "rgba(52, 143, 226, 0.8)",
                data = new List<decimal>()
            };

            decimal[] data_monthly_in = new decimal[12];
            for (int i = 0; i < data_monthly_in.Length; i++)
            {
                var element = stockIns_monthly.FirstOrDefault(x => x.Key - 1 == i);

                decimal total = 0;
                if (element != null)
                {
                    foreach (var stock in element)
                    {
                        if (stock.Product.Category.SystemAction == ActionConstants.Sum)
                        {
                            total += stock.ShopPrice * stock.Quantity;
                        }
                        else if (stock.Product.Category.SystemAction == ActionConstants.Rest)
                        {
                            total -= stock.ShopPrice * stock.Quantity;
                        }
                    }
                }

                data_monthly_in[i] = total;
            }
            model_monthly_in.data.AddRange(data_monthly_in);
            inout_monthly.Add(model_monthly_in);

            List<IGrouping<int, StockOut>> stockOuts_monthly = await applicationDbContext.StockOuts.Include("Product.Category").Where(x => x.Date > init_year && x.Product.Category.Business_Id == business_working).GroupBy(x => x.Date.Month).ToListAsync();

            DashboardChartModel model_monthly_out = new DashboardChartModel()
            {
                label = "Salidas",
                backgroundColor = "rgba(45, 53, 60, 0.2)",
                borderColor = "rgba(45, 53, 60, 0.8)",
                pointBackgroundColor = "rgba(45, 53, 60, 0.8)",
                data = new List<decimal>()
            };

            DashboardChartModel model_monthly_gain = new DashboardChartModel()
            {
                label = "Entradas",
                backgroundColor = "rgba(52, 143, 226, 0.2)",
                borderColor = "rgba(52, 143, 226, 0.8)",
                pointBackgroundColor = "rgba(52, 143, 226, 0.8)",
                data = new List<decimal>()
            };

            decimal[] data_monthly_out = new decimal[12];
            decimal[] data_monthly_gain = new decimal[12];
            for (int i = 0; i < data_monthly_out.Length; i++)
            {
                var element = stockOuts_monthly.FirstOrDefault(x => x.Key - 1 == i);

                decimal total = 0, total_gain = 0;
                if (element != null)
                {
                    foreach (var stock in element)
                    {
                        if (stock.Product.Category.SystemAction == ActionConstants.Sum)
                        {
                            total += stock.SalePrice * stock.Quantity;
                            total_gain += stock.Gain * stock.Quantity;
                        }
                        else if (stock.Product.Category.SystemAction == ActionConstants.Rest)
                        {
                            total -= stock.SalePrice * stock.Quantity;
                            total_gain -= stock.Gain * stock.Quantity;
                        }
                    }
                }

                data_monthly_out[i] = total;
                data_monthly_gain[i] = total_gain;
            }
            model_monthly_gain.data.AddRange(data_monthly_gain);
            model_monthly_out.data.AddRange(data_monthly_out);
            inout_monthly.Add(model_monthly_out);

            #endregion Grahp Monthly

            #region Radar Month

            DashboardRadarModel radar_month = new DashboardRadarModel();
            radar_month.labels = new List<string>();
            radar_month.datasets = new List<DashboardChartModel>();

            List<Category> categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).ToListAsync();

            DashboardChartModel radar_in = new DashboardChartModel()
            {
                label = "Entrada",
                borderWidth = 2,
                borderColor = "rgba(114, 124, 182, 0.8)",
                pointBackgroundColor = "rgba(114, 124, 182, 0.8)",
                backgroundColor = "rgba(114, 124, 182, 0.2)",
                data = new List<decimal>()
            };

            DashboardChartModel radar_out = new DashboardChartModel()
            {
                label = "Salida",
                borderWidth = 2,
                borderColor = "rgba(45, 53, 60, 0.8)",
                pointBackgroundColor = "rgba(45, 53, 60, 0.8)",
                backgroundColor = "rgba(45, 53, 60, 0.2)",
                data = new List<decimal>()
            };
            foreach (var cat in categories)
            {
                radar_month.labels.Add(cat.Name);

                List<StockIn> stockIns1 = await applicationDbContext.StockIns.Include("Product").Where(x => x.Product.Category_Id == cat.Id).ToListAsync();

                decimal number = 0;

                foreach (var stockIn in stockIns1)
                {
                    number += stockIn.ShopPrice * stockIn.Quantity;
                }

                radar_in.data.Add(number);

                List<StockOut> stockOut1 = await applicationDbContext.StockOuts.Include("Product").Where(x => x.Product.Category_Id == cat.Id).ToListAsync();

                number = 0;

                foreach (var stockOut in stockOut1)
                {
                    number += stockOut.SalePrice * stockOut.Quantity;
                }

                radar_out.data.Add(number);
            }

            radar_month.datasets.Add(radar_in);
            radar_month.datasets.Add(radar_out);

            #endregion Radar Month

            return Json(new { inout_daily = inout_daily, inout_monthly = inout_monthly, radar_month = radar_month, model_daily_gain = model_daily_gain, model_monthly_gain = model_monthly_gain });
        }
    }
}