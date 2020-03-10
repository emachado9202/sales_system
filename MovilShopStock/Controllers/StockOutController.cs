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
    [Models.Handlers.Authorize(Roles = RoleManager.Dealer + "," + RoleManager.Editor + "," + RoleManager.Administrator)]
    public class StockOutController : GenericController
    {
        public async Task<ActionResult> Index()
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            List<Product> accesories = await applicationDbContext.Products.Include("Category").Where(x => x.Business_Id == business_working && x.Stock > 0 && x.isAccesory).OrderBy(x => x.Name).ToListAsync();

            StockOutModel model = new StockOutModel()
            {
                Quantity = 0,
                SalePrice = "0.00",
                Accesories = new List<AccesoryModel>()
            };

            foreach (var acc in accesories)
            {
                model.Accesories.Add(new AccesoryModel()
                {
                    Id = acc.Id.ToString(),
                    Name = $"{acc.Category.Name} - {acc.Name}"
                });
            }

            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(StockOutModel model)
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            if (ModelState.IsValid)
            {
                Guid productId = Guid.Parse(model.ProductName);

                Product product = await applicationDbContext.Products.Include("Category").FirstOrDefaultAsync(x => x.Id == productId);

                StockOut stockOut = new StockOut()
                {
                    Id = Guid.NewGuid(),
                    Product_Id = productId,
                    Date = DateTime.Now,
                    Quantity = model.Quantity,
                    SalePrice = decimal.Parse(model.SalePrice.Replace(".", ",")),
                    User_Id = User.Identity.GetUserId(),
                    Gain = decimal.Parse(model.SalePrice.Replace(".", ",")) - product.CurrentPrice,
                    Description = model.Description
                };

                if (RoleManager.IsInRole(RoleManager.Editor) || RoleManager.IsInRole(RoleManager.Administrator))
                {
                    stockOut.Receiver_Id = User.Identity.GetUserId();

                    BusinessUser businessUser = await applicationDbContext.BusinessUsers.FirstOrDefaultAsync(x => x.User_Id == stockOut.Receiver_Id && x.Business_Id == business_working);

                    if (product.Category.ActionOut == ActionConstants.Sum)
                    {
                        businessUser.Cash += stockOut.SalePrice * stockOut.Quantity;
                    }
                    else if (product.Category.ActionOut == ActionConstants.Rest)
                    {
                        businessUser.Cash -= stockOut.SalePrice * stockOut.Quantity;
                    }

                    applicationDbContext.Entry(businessUser).State = System.Data.Entity.EntityState.Modified;
                }

                applicationDbContext.StockOuts.Add(stockOut);
                if (!product.NoCountOut)
                {
                    product.Stock -= stockOut.Quantity;
                    product.LastUpdated = DateTime.Now;

                    applicationDbContext.Entry(product).State = System.Data.Entity.EntityState.Modified;
                }

                if (model.AccesoriesIds != null)
                {
                    foreach (var acc in model.AccesoriesIds)
                    {
                        Guid accesory = Guid.Parse(acc);
                        Product acc_product = await applicationDbContext.Products.Include("Category").FirstOrDefaultAsync(x => x.Id == accesory);

                        StockOut acc_stockOut = new StockOut()
                        {
                            Id = Guid.NewGuid(),
                            Product_Id = accesory,
                            Date = DateTime.Now,
                            Quantity = 1,
                            SalePrice = 0,
                            User_Id = User.Identity.GetUserId(),
                            Gain = 0 - acc_product.CurrentPrice,
                            Description = $"Con producto {product.Name}"
                        };
                        applicationDbContext.StockOuts.Add(acc_stockOut);
                        if (!acc_product.NoCountOut)
                        {
                            acc_product.Stock -= acc_stockOut.Quantity;
                            acc_product.LastUpdated = DateTime.Now;

                            applicationDbContext.Entry(acc_product).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

        [HttpPost]
        [Models.Handlers.Authorize(Roles = RoleManager.Editor + "," + RoleManager.Administrator)]
        public async Task<ActionResult> Receiver(string id)
        {
            Guid out_id = Guid.Parse(id);
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            StockOut stockOut = await applicationDbContext.StockOuts.Include("Product").Include("Product.Category").FirstOrDefaultAsync(x => x.Id == out_id);

            if (stockOut != null)
            {
                stockOut.Receiver_Id = User.Identity.GetUserId();

                applicationDbContext.Entry(stockOut).State = System.Data.Entity.EntityState.Modified;

                BusinessUser businessUser = await applicationDbContext.BusinessUsers.FirstOrDefaultAsync(x => x.User_Id == stockOut.Receiver_Id && x.Business_Id == business_working);

                if (stockOut.Product.Category.ActionOut == ActionConstants.Sum)
                {
                    businessUser.Cash += stockOut.SalePrice * stockOut.Quantity;
                }
                else if (stockOut.Product.Category.ActionOut == ActionConstants.Rest)
                {
                    businessUser.Cash -= stockOut.SalePrice * stockOut.Quantity;
                }

                applicationDbContext.Entry(businessUser).State = System.Data.Entity.EntityState.Modified;

                await applicationDbContext.SaveChangesAsync();
            }

            return Json(true);
        }

        [HttpPost]
        [Models.Handlers.Authorize(Roles = RoleManager.Editor + "," + RoleManager.Administrator)]
        public async Task<ActionResult> AllReceiver()
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
            List<StockOut> stockOuts = await applicationDbContext.StockOuts.Include("Product").Include("Product.Category").Where(x => x.Receiver_Id == null).ToListAsync();

            foreach (var stockOut in stockOuts)
            {
                stockOut.Receiver_Id = User.Identity.GetUserId();

                applicationDbContext.Entry(stockOut).State = System.Data.Entity.EntityState.Modified;

                BusinessUser businessUser = await applicationDbContext.BusinessUsers.FirstOrDefaultAsync(x => x.User_Id == stockOut.Receiver_Id && x.Business_Id == business_working);

                if (stockOut.Product.Category.ActionOut == ActionConstants.Sum)
                {
                    businessUser.Cash += stockOut.SalePrice * stockOut.Quantity;
                }
                else if (stockOut.Product.Category.ActionOut == ActionConstants.Rest)
                {
                    businessUser.Cash -= stockOut.SalePrice * stockOut.Quantity;
                }

                applicationDbContext.Entry(businessUser).State = System.Data.Entity.EntityState.Modified;
            }
            await applicationDbContext.SaveChangesAsync();

            return Json(true);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            try
            {
                Guid stock_id = Guid.Parse(id);

                Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
                ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

                StockOut stockout = await applicationDbContext.StockOuts.Include("Product").FirstOrDefaultAsync(x => x.Id == stock_id);

                if (stockout.Receiver_Id != null)
                {
                    ViewBag.Msg = "La salida ha sido recibida y no puede ser editada";

                    return RedirectToAction("Index");
                }

                StockOutModel model = new StockOutModel()
                {
                    DT_RowId = stockout.Id.ToString(),
                    ProductName = stockout.Product_Id.ToString(),
                    Quantity = stockout.Quantity,
                    SalePrice = stockout.SalePrice.ToString("#,##0.00"),
                    Category = stockout.Product.Category_Id.ToString(),
                    Receivered = stockout.Receiver != null,
                    Description = stockout.Description,
                    Accesories = new List<AccesoryModel>()
                };

                return View(model);
            }
            catch (Exception e)
            {
                ViewBag.Msg = "La salida no existe";

                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string id)
        {
            Guid out_id = Guid.Parse(id);
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            StockOut stockOut = await applicationDbContext.StockOuts.Include("Product").FirstOrDefaultAsync(x => x.Id == out_id);

            if (stockOut != null)
            {
                if (stockOut.Receiver_Id != null)
                {
                    if (!stockOut.Product.NoCountOut)
                    {
                        BusinessUser businessUser = await applicationDbContext.BusinessUsers.FirstOrDefaultAsync(x => x.User_Id == stockOut.Receiver_Id && x.Business_Id == business_working);

                        /*Operacion inversa para revertir la situación*/
                        if (stockOut.Product.Category.ActionOut == ActionConstants.Sum)
                        {
                            businessUser.Cash -= stockOut.SalePrice * stockOut.Quantity;
                        }
                        else if (stockOut.Product.Category.ActionOut == ActionConstants.Rest)
                        {
                            businessUser.Cash += stockOut.SalePrice * stockOut.Quantity;
                        }

                        applicationDbContext.Entry(businessUser).State = System.Data.Entity.EntityState.Modified;
                    }
                }

                stockOut.Product.Stock += stockOut.Quantity;
                stockOut.Product.LastUpdated = DateTime.Now;

                applicationDbContext.Entry(stockOut.Product).State = System.Data.Entity.EntityState.Modified;
                applicationDbContext.Entry(stockOut).State = System.Data.Entity.EntityState.Deleted;

                await applicationDbContext.SaveChangesAsync();
            }

            ViewBag.Msg = "La salida ha sido eliminada correctamente";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Search(TableFilterViewModel filter)
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            List<StockOutModel> result = new List<StockOutModel>();
            long totalRowsFiltered = 0;
            long totalRows = await applicationDbContext.StockOuts.Include("Product").CountAsync(x => x.Product.Business_Id == business_working);
            List<StockOut> model;

            var entity = applicationDbContext.StockOuts.Include("Product").Include("Product.Category").Include("User").Where(x => x.Product.Business_Id == business_working);

            if (filter.type == "1")
            {
                entity = applicationDbContext.StockOuts.Include("Product").Include("Product.Category").Include("User").Where(x => x.Product.Business_Id == business_working && x.Receiver_Id != null);
            }
            else if (filter.type == "2")
            {
                entity = applicationDbContext.StockOuts.Include("Product").Include("Product.Category").Include("User").Where(x => x.Product.Business_Id == business_working && x.Receiver_Id == null);
            }

            IOrderedQueryable<StockOut> sort = null;
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
                    sort = entity.OrderBy(x => x.Date);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Date);
                }
            }
            else if (filter.order[0].column == 4)
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
            else if (filter.order[0].column == 5)
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
            else if (filter.order[0].column == 6)
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
            else if (filter.order[0].column == 7)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Quantity * x.SalePrice);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Quantity * x.SalePrice);
                }
            }
            else if (filter.order[0].column == 8)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Gain);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Gain);
                }
            }
            else if (filter.order[0].column == 9)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Quantity * x.Gain);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Quantity * x.Gain);
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
                   applicationDbContext.StockOuts.Include("Product").Include("User").CountAsync(x => x.Product.Business_Id == business_working && (x.Product.Category.Name.ToString().Contains(filter.search.value) ||
                   x.Product.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.SalePrice.ToString().Contains(filter.search.value) ||
                   x.Quantity.ToString().Contains(filter.search.value)));

                model = await
                    sort.Where(x => x.Product.Category.Name.ToString().Contains(filter.search.value) ||
                   x.Product.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.SalePrice.ToString().Contains(filter.search.value) ||
                   x.Quantity.ToString().Contains(filter.search.value))
                        .Skip(filter.start)
                        .Take(filter.length)
                        .ToListAsync();
            }

            foreach (var stockOut in model)
            {
                result.Add(new StockOutModel()
                {
                    DT_RowId = stockOut.Id.ToString(),
                    ProductName = stockOut.Product.Name,
                    Date = stockOut.Date.ToString("yyyy-MM-dd hh:mm tt"),
                    Quantity = stockOut.Quantity,
                    User = stockOut.User.UserName,
                    SalePrice = stockOut.SalePrice.ToString("#,##0.00"),
                    Gain = stockOut.Gain.ToString("#,##0.00"),
                    Receivered = stockOut.Receiver != null,
                    Receiver = stockOut.Receiver?.UserName,
                    Category = stockOut.Product.Category.Name,
                    Description = stockOut.Description
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