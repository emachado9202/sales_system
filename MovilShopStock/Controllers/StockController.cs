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
    [Authorize(Roles = RoleConstants.Dealer + "," + RoleConstants.Editor + "," + RoleConstants.Administrator)]
    public class StockController : GenericController
    {
        private ApplicationDbContext applicationDbContext = new ApplicationDbContext();

        [HttpGet]
        public async Task<ActionResult> Index(string id)
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
            ProductModel model = new ProductModel()
            {
                SalePrice = "0.00",
                CurrentPrice = "0.00",
                Stock = 0
            };

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
                    Stock = model.Stock,
                    User_Id = User.Identity.GetUserId(),
                    CurrentPrice = decimal.Parse(model.CurrentPrice.Replace(".", ",")),
                    LastUpdated = DateTime.Now,
                    NoCountOut = model.NoCountOut,
                    Business_Id = business_working,
                    SalePrice = decimal.Parse(model.SalePrice.Replace(".", ",")),
                    isAccesory = model.isAccesory
                };

                applicationDbContext.Products.Add(product);

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
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
                Stock = product.Stock,
                NoCountOut = product.NoCountOut,
                isAccesory = product.isAccesory
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
                product.Stock = model.Stock;
                product.CurrentPrice = decimal.Parse(model.CurrentPrice.Replace(".", ","));
                product.SalePrice = decimal.Parse(model.SalePrice.Replace(".", ","));
                product.LastUpdated = DateTime.Now;
                product.NoCountOut = model.NoCountOut;
                product.isAccesory = model.isAccesory;

                applicationDbContext.Entry(product).State = System.Data.Entity.EntityState.Modified;

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Search(StockFilterViewModel filter)
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            List<ProductModel> result = new List<ProductModel>();
            long totalRowsFiltered = 0;
            long totalRows = await applicationDbContext.Products.CountAsync(x => x.Business_Id == business_working);
            List<Product> model;

            var entity = applicationDbContext.Products.Include("Category").Where(x => x.Business_Id == business_working);

            if (!string.IsNullOrEmpty(filter.type))
            {
                Guid categoryId = Guid.Parse(filter.type);

                totalRows = await applicationDbContext.Products.CountAsync(x => x.Business_Id == business_working && x.Category_Id == categoryId);
                entity = entity.Where(x => x.Business_Id == business_working && x.Category_Id == categoryId);
            }

            if (filter.exist == "1")
            {
                totalRows = await applicationDbContext.Products.CountAsync(x => x.Stock > 0 && x.Business_Id == business_working);
                entity = entity.Where(x => x.Stock > 0);
            }
            else if (filter.exist == "2")
            {
                totalRows = await applicationDbContext.Products.CountAsync(x => x.Stock <= 0 && x.Business_Id == business_working);
                entity = entity.Where(x => x.Stock <= 0);
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
                    sort = entity.OrderBy(x => x.Stock);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Stock);
                }
            }
            else if (filter.order[0].column == 5)
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
            else if (filter.order[0].column == 6)
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
            else if (filter.order[0].column == 7)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.isAccesory);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.isAccesory);
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
                if (filter.exist == "1")
                {
                    totalRowsFiltered = await
                   applicationDbContext.Products.CountAsync(x => x.Stock > 0 && x.Business_Id == business_working && (x.Category.Name.ToString().Contains(filter.search.value) ||
                   x.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.CurrentPrice.ToString().Contains(filter.search.value) ||
                   x.SalePrice.ToString().Contains(filter.search.value) ||
                   x.Stock.ToString().Contains(filter.search.value)));
                }
                else if (filter.exist == "2")
                {
                    totalRowsFiltered = await
                   applicationDbContext.Products.CountAsync(x => x.Stock <= 0 && x.Business_Id == business_working && (x.Category.Name.ToString().Contains(filter.search.value) ||
                   x.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.CurrentPrice.ToString().Contains(filter.search.value) ||
                   x.SalePrice.ToString().Contains(filter.search.value) ||
                   x.Stock.ToString().Contains(filter.search.value)));
                }
                else
                {
                    totalRowsFiltered = await
                   applicationDbContext.Products.CountAsync(x => x.Business_Id == business_working && (x.Category.Name.ToString().Contains(filter.search.value) ||
                   x.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.CurrentPrice.ToString().Contains(filter.search.value) ||
                   x.SalePrice.ToString().Contains(filter.search.value) ||
                   x.Stock.ToString().Contains(filter.search.value)));
                }

                model = await
                    sort.Where(x => x.Category.Name.ToString().Contains(filter.search.value) ||
                   x.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.CurrentPrice.ToString().Contains(filter.search.value) ||
                   x.SalePrice.ToString().Contains(filter.search.value) ||
                   x.Stock.ToString().Contains(filter.search.value))
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
                    LatestUpdated = product.LastUpdated.ToString("yyyy-MM-dd hh:mm tt"),
                    CurrentPrice = product.CurrentPrice.ToString("#,##0.00"),
                    SalePrice = product.SalePrice.ToString("#,##0.00"),
                    NoCountOut = product.NoCountOut,
                    Stock = product.Stock,
                    isAccesory = product.isAccesory,
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