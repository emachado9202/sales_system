using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MovilShopStock.Models;
using MovilShopStock.Models.Catalog;
using MovilShopStock.Models.Handlers;
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

        [HttpGet]
        public async Task<ActionResult> List(string id)
        {
            List<ProductModel> result = new List<ProductModel>();

            List<Tuple<string, string>> categories = new List<Tuple<string, string>>();

            categories.Add(new Tuple<string, string>("", "Categoría Todas"));

            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            foreach (var cat in await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync())
            {
                categories.Add(new Tuple<string, string>(cat.Id.ToString(), cat.Name));
            }

            ViewBag.Categories = categories;
            ViewBag.Category = id;

            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.Editor + "," + RoleConstants.Administrator)]
        public async Task<ActionResult> Create()
        {
            ProductModel model = new ProductModel();

            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.Editor + "," + RoleConstants.Administrator)]
        public async Task<ActionResult> Create(ProductModel model)
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
            if (ModelState.IsValid)
            {
                Product product = new Product()
                {
                    Id = Guid.NewGuid(),
                    Name = model.Product,
                    Category_Id = Guid.Parse(model.Category),
                    Description = "",
                    In = model.In,
                    Out = model.Out,
                    User_Id = User.Identity.GetUserId(),
                    CurrentPrice = decimal.Parse(model.CurrentPrice.Replace(".", ",")),
                    LastUpdated = DateTime.Now,
                    NoCountOut = model.NoCountOut,
                    Business_Id = business_working,
                    SalePrice = decimal.Parse(model.SalePrice.Replace(".", ","))
                };

                applicationDbContext.Products.Add(product);

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("List");
            }

            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.Editor + "," + RoleConstants.Administrator)]
        public async Task<ActionResult> Edit(string id)
        {
            Guid prod_id = Guid.Parse(id);

            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            Product product = await applicationDbContext.Products.FirstOrDefaultAsync(x => x.Id == prod_id);

            ProductModel model = new ProductModel()
            {
                DT_RowId = product.Id.ToString(),
                Category = product.Category_Id.ToString(),
                Product = product.Name,
                CurrentPrice = product.CurrentPrice.ToString("#,##0.00"),
                SalePrice = product.SalePrice.ToString("#,##0.00"),
                In = product.In
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.Editor + "," + RoleConstants.Administrator)]
        public async Task<ActionResult> Edit(string id, ProductModel model)
        {
            if (ModelState.IsValid)
            {
                Guid prod_id = Guid.Parse(id);

                Product product = await applicationDbContext.Products.FirstOrDefaultAsync(x => x.Id == prod_id);

                product.Name = model.Product;
                product.Category_Id = Guid.Parse(model.Category);
                product.In = model.In;
                product.CurrentPrice = decimal.Parse(model.CurrentPrice.Replace(".", ","));
                product.SalePrice = decimal.Parse(model.SalePrice.Replace(".", ","));
                product.LastUpdated = DateTime.Now;
                product.NoCountOut = model.NoCountOut;

                applicationDbContext.Entry(product).State = System.Data.Entity.EntityState.Modified;

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("List");
            }
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

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
        public async Task<ActionResult> Index()
        {
            DashboardModel model = new DashboardModel();

            DateTime month_init = DateTime.Today.AddDays(-DateTime.Now.Day);

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

        [HttpPost]
        public async Task<ActionResult> Search(TableFilterViewModel filter)
        {
            List<ProductModel> result = new List<ProductModel>();
            long totalRowsFiltered = 0;
            long totalRows = await applicationDbContext.Products.CountAsync();
            List<Product> model;

            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            var entity = applicationDbContext.Products.Include("Category").Where(x => x.Business_Id == business_working);

            if (!string.IsNullOrEmpty(filter.type))
            {
                Guid categoryId = Guid.Parse(filter.type);

                entity = applicationDbContext.Products.Include("Category").Where(x => x.Business_Id == business_working && x.Category_Id == categoryId);
            }

            IOrderedQueryable<Product> sort = null;
            if (filter.order[0].column == 0)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Category.Name);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Category.Name);
                }
            }
            else if (filter.order[0].column == 1)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Name);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Name);
                }
            }
            else if (filter.order[0].column == 2)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.CurrentPrice);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.CurrentPrice);
                }
            }
            else if (filter.order[0].column == 3)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.SalePrice);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.SalePrice);
                }
            }
            else if (filter.order[0].column == 4)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.In);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.In);
                }
            }
            else if (filter.order[0].column == 5)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Out);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Out);
                }
            }
            else if (filter.order[0].column == 6)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.In - x.Out);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.In * x.Out);
                }
            }
            else if (filter.order[0].column == 7)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.LastUpdated);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.LastUpdated);
                }
            }
            else if (filter.order[0].column == 8)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.NoCountOut);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.NoCountOut);
                }
            }

            if (string.IsNullOrEmpty(filter.search.value))
            {
                totalRowsFiltered = totalRows;
                model = await sort.Skip(filter.start)
                    .Take(filter.length)
                    .ToListAsync();
            }
            else
            {
                totalRowsFiltered = await
                   applicationDbContext.Products.CountAsync(x => x.Category.Name.ToString().Contains(filter.search.value) ||
                   x.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.CurrentPrice.ToString().Contains(filter.search.value) ||
                   x.SalePrice.ToString().Contains(filter.search.value) ||
                   x.In.ToString().Contains(filter.search.value) ||
                   x.Out.ToString().Contains(filter.search.value));

                model = await
                    sort.Where(x => x.Category.Name.ToString().Contains(filter.search.value) ||
                   x.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.CurrentPrice.ToString().Contains(filter.search.value) ||
                   x.SalePrice.ToString().Contains(filter.search.value) ||
                   x.In.ToString().Contains(filter.search.value) ||
                   x.Out.ToString().Contains(filter.search.value))
                        .Skip(filter.start)
                        .Take(filter.length)
                        .ToListAsync();
            }

            foreach (var product in model)
            {
                result.Add(new ProductModel()
                {
                    DT_RowId = product.Id.ToString(),
                    Product = product.Name,
                    LatestUpdated = product.LastUpdated.ToString("yyyy-MM-dd hh:mm"),
                    CurrentPrice = product.CurrentPrice.ToString("#,##0.00"),
                    SalePrice = product.SalePrice.ToString("#,##0.00"),
                    NoCountOut = product.NoCountOut,
                    In = product.In,
                    Out = product.Out,
                    Category = product.Category.Name
                });
            }

            return Json(new
            {
                draw = filter.draw,
                recordsTotal = totalRows,
                recordsFiltered = totalRowsFiltered,
                data = result
            });
        }
    }
}