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

            categories.Add(new Tuple<string, string>("", "Todas"));

            foreach (var cat in await applicationDbContext.Categories.OrderBy(x => x.Name).ToListAsync())
            {
                categories.Add(new Tuple<string, string>(cat.Id.ToString(), cat.Name));
            }

            ViewBag.Categories = categories;
            ViewBag.Category = id;

            List<Product> products;

            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            if (string.IsNullOrEmpty(id))
            {
                products = await applicationDbContext.Products.Include("Category").Where(x => x.Business_Id == business_working).OrderByDescending(x => x.LastUpdated).ToListAsync();
            }
            else
            {
                Guid categoryId = Guid.Parse(id);

                products = await applicationDbContext.Products.Where(x => x.Category_Id == categoryId && x.Business_Id == business_working).Include("Category").OrderByDescending(x => x.LastUpdated).ToListAsync();
            }

            foreach (var product in products)
            {
                result.Add(new ProductModel()
                {
                    Id = product.Id.ToString(),
                    Category = product.Category.Name,
                    Product = product.Name,
                    CurrentPrice = product.CurrentPrice.ToString("#,##0.00"),
                    LatestUpdated = product.LastUpdated.ToString("yyyy-MM-dd"),
                    In = product.In,
                    Out = product.Out,
                    NoCountOut = product.NoCountOut
                });
            }

            return View(result);
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.Editor + "," + RoleConstants.Administrator)]
        public async Task<ActionResult> Create()
        {
            ProductModel model = new ProductModel();

            ViewBag.Categories = await applicationDbContext.Categories.OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.Editor + "," + RoleConstants.Administrator)]
        public async Task<ActionResult> Create(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

                Product product = new Product()
                {
                    Id = Guid.NewGuid(),
                    Name = model.Product,
                    Category_Id = Guid.Parse(model.Category),
                    Description = "",
                    In = model.In,
                    Out = model.Out,
                    User_Id = User.Identity.GetUserId(),
                    CurrentPrice = decimal.Parse(model.CurrentPrice),
                    LastUpdated = DateTime.Now,
                    NoCountOut = model.NoCountOut,
                    Business_Id = business_working
                };

                applicationDbContext.Products.Add(product);

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("List");
            }
            ViewBag.Categories = await applicationDbContext.Categories.OrderBy(x => x.Name).ToListAsync();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.Editor + "," + RoleConstants.Administrator)]
        public async Task<ActionResult> Edit(string id)
        {
            Guid prod_id = Guid.Parse(id);

            ViewBag.Categories = await applicationDbContext.Categories.OrderBy(x => x.Name).ToListAsync();

            Product product = await applicationDbContext.Products.FirstOrDefaultAsync(x => x.Id == prod_id);

            ProductModel model = new ProductModel()
            {
                Id = product.Id.ToString(),
                Category = product.Category_Id.ToString(),
                Product = product.Name,
                CurrentPrice = product.CurrentPrice.ToString("#,##0.00"),
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
                product.CurrentPrice = decimal.Parse(model.CurrentPrice);
                product.LastUpdated = DateTime.Now;
                product.NoCountOut = model.NoCountOut;

                applicationDbContext.Entry(product).State = System.Data.Entity.EntityState.Modified;

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("List");
            }
            ViewBag.Categories = await applicationDbContext.Categories.OrderBy(x => x.Name).ToListAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> GetProductByCat(string category_id)
        {
            Guid categoryId = Guid.Parse(category_id);
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            List<ProductModel> products = await applicationDbContext.Products.Where(x => x.Category_Id == categoryId && x.Business_Id == business_working).OrderBy(x => x.Name).Select(x => new ProductModel { Id = x.Id.ToString(), Product = x.Name }).ToListAsync();

            return Json(products);
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            DashboardModel model = new DashboardModel();

            DateTime month_init = DateTime.Today.AddDays(-DateTime.Now.Day);

            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

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

            model.StockMoney = new Tuple<decimal, decimal>(money_stock, percent_money_stock);

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
                            model.GainToday += stockOut.SalePrice * stockOut.Quantity;
                        }
                        else if (stockOut.Product.Category.SystemAction == ActionConstants.Rest)
                        {
                            model.GainToday -= stockOut.SalePrice * stockOut.Quantity;
                        }
                    }
                }

                List<string> roles_ids = await applicationDbContext.Roles.Where(x => x.Name.Equals(RoleConstants.Editor) || x.Name.Equals(RoleConstants.Administrator)).Select(x => x.Id).ToListAsync();

                List<User> users = await applicationDbContext.Users.Where(x => x.Roles.FirstOrDefault(r => roles_ids.Contains(r.RoleId)) != null).ToListAsync();
                model.UserMoney = new List<Tuple<string, decimal>>();
                foreach (var u in users)
                {
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
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> SetBusiness(string id, string returnUrl)
        {
            Session["BusinessWorking"] = id;
            return Redirect(returnUrl);
        }
    }
}