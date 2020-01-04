using Microsoft.AspNet.Identity;
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
    [Authorize(Roles = RoleConstants.Editor + "," + RoleConstants.Administrator)]
    public class StockInController : GenericController
    {
        private ApplicationDbContext applicationDbContext = new ApplicationDbContext();

        public async Task<ActionResult> Index(string id)
        {
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
        public async Task<ActionResult> Create()
        {
            StockInModel model = new StockInModel()
            {
                ShopPrice = "0.00",
                Quantity = 0
            };

            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            List<Provider> providers = new List<Provider>();

            providers.Add(new Provider()
            {
                Id = Guid.Empty,
                Name = "Ninguno"
            });
            providers.AddRange(await applicationDbContext.Providers.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync());

            ViewBag.Providers = providers;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(StockInModel model)
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            if (ModelState.IsValid)
            {
                Guid productId = Guid.Parse(model.ProductName);

                StockIn stockIn = new StockIn()
                {
                    Id = Guid.NewGuid(),
                    Product_Id = productId,
                    Date = DateTime.Now,
                    Quantity = model.Quantity,
                    ShopPrice = decimal.Parse(model.ShopPrice.Replace(".", ",")),
                    User_Id = User.Identity.GetUserId(),
                    Description = model.Description
                };

                if (Guid.Parse(model.Provider) != Guid.Empty)
                {
                    stockIn.Provider_Id = Guid.Parse(model.Provider);
                }

                applicationDbContext.StockIns.Add(stockIn);

                Product product = await applicationDbContext.Products.Include("Category").FirstOrDefaultAsync(x => x.Id == productId);
                
                product.CurrentPrice = ((product.CurrentPrice * (product.In - product.Out)) + (stockIn.ShopPrice * stockIn.Quantity)) / (product.In - product.Out + stockIn.Quantity);
                product.In += stockIn.Quantity;
                product.LastUpdated = DateTime.Now;

                if (User.IsInRole(RoleConstants.Editor) || User.IsInRole(RoleConstants.Administrator))
                {
                    BusinessUser businessUser = await applicationDbContext.BusinessUsers.FirstOrDefaultAsync(x => x.User_Id == stockIn.User_Id && x.Business_Id == business_working);

                    if (product.Category.ActionIn == ActionConstants.Sum)
                    {
                        businessUser.Cash += stockIn.ShopPrice * stockIn.Quantity;
                    }
                    else if (product.Category.ActionIn == ActionConstants.Rest)
                    {
                        businessUser.Cash -= stockIn.ShopPrice * stockIn.Quantity;
                    }

                    applicationDbContext.Entry(businessUser).State = System.Data.Entity.EntityState.Modified;
                }

                applicationDbContext.Entry(product).State = System.Data.Entity.EntityState.Modified;

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();
            ViewBag.Providers = await applicationDbContext.Providers.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Search(TableFilterViewModel filter)
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            List<StockInModel> result = new List<StockInModel>();
            long totalRowsFiltered = 0;
            long totalRows = await applicationDbContext.StockIns.Include("Product").CountAsync(x => x.Product.Business_Id == business_working);
            List<StockIn> model;

            var entity = applicationDbContext.StockIns.Include("Product").Include("Product.Category").Include("User").Include("Provider").Where(x => x.Product.Business_Id == business_working);

            if (!string.IsNullOrEmpty(filter.type))
            {
                Guid categoryId = Guid.Parse(filter.type);

                entity = applicationDbContext.StockIns.Include("Product").Include("Product.Category").Include("User").Include("Provider").Where(x => x.Product.Business_Id == business_working && x.Product.Category_Id == categoryId);
            }

            IOrderedQueryable<StockIn> sort = null;
            if (filter.order[0].column == 0)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Product.Category.Name);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Product.Category.Name);
                }
            }
            else if (filter.order[0].column == 1)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Product.Name);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Product.Name);
                }
            }
            else if (filter.order[0].column == 2)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Description);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Description);
                }
            }
            else if (filter.order[0].column == 3)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Provider.Name);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Provider.Name);
                }
            }
            else if (filter.order[0].column == 4)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Date);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Date);
                }
            }
            else if (filter.order[0].column == 5)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.User.UserName);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.User.UserName);
                }
            }
            else if (filter.order[0].column == 6)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.ShopPrice);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.ShopPrice);
                }
            }
            else if (filter.order[0].column == 7)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Quantity);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Quantity);
                }
            }
            else if (filter.order[0].column == 8)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Quantity * x.ShopPrice);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Quantity * x.ShopPrice);
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
                   applicationDbContext.StockIns.Include("Product").Include("User").Include("Provider").CountAsync(x => x.Product.Business_Id == business_working &&
                   (x.Product.Category.Name.ToString().Contains(filter.search.value) ||
                   x.Product.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.ShopPrice.ToString().Contains(filter.search.value) ||
                   x.Provider.Name.ToString().Contains(filter.search.value) ||
                   x.Quantity.ToString().Contains(filter.search.value)));

                model = await
                    sort.Where(x => x.Product.Category.Name.ToString().Contains(filter.search.value) ||
                   x.Product.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.ShopPrice.ToString().Contains(filter.search.value) ||
                   x.Provider.Name.ToString().Contains(filter.search.value) ||
                   x.Quantity.ToString().Contains(filter.search.value))
                        .Skip(filter.start)
                        .Take(filter.length)
                        .ToListAsync();
            }

            foreach (var stockIn in model)
            {
                result.Add(new StockInModel()
                {
                    DT_RowId = stockIn.Id.ToString(),
                    ProductName = stockIn.Product.Name,
                    Date = stockIn.Date.ToString("yyyy-MM-dd hh:mm tt"),
                    Quantity = stockIn.Quantity,
                    User = stockIn.User.UserName,
                    ShopPrice = stockIn.ShopPrice.ToString("#,##0.00"),
                    Category = stockIn.Product.Category.Name,
                    Description = stockIn.Description,
                    Provider = stockIn.Provider?.Name
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