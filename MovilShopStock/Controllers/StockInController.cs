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
    public class StockInController : Controller
    {
        private ApplicationDbContext applicationDbContext = new ApplicationDbContext();

        public async Task<ActionResult> Index()
        {
            List<StockInModel> result = new List<StockInModel>();

            List<StockIn> stockIns = await applicationDbContext.StockIns.Include("Product").Include("Product.Category").Include("User").OrderByDescending(x => x.Date).ToListAsync();

            foreach (var stockIn in stockIns)
            {
                result.Add(new StockInModel()
                {
                    Id = stockIn.Id.ToString(),
                    ProductName = stockIn.Product.Name,
                    ShopPrice = stockIn.ShopPrice,
                    Date = stockIn.Date.ToString("yyyy-MM-dd"),
                    Quantity = stockIn.Quantity,
                    User = stockIn.User.UserName,
                    Category = stockIn.Product.Category.Name
                });
            }

            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            StockInModel model = new StockInModel();

            ViewBag.Categories = await applicationDbContext.Categories.OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(StockInModel model)
        {
            if (ModelState.IsValid)
            {
                Guid productId = Guid.Parse(model.ProductName);
                StockIn stockIn = new StockIn()
                {
                    Id = Guid.NewGuid(),
                    Product_Id = productId,
                    Date = DateTime.Now,
                    Quantity = model.Quantity,
                    ShopPrice = model.ShopPrice,
                    User_Id = User.Identity.GetUserId()
                };

                applicationDbContext.StockIns.Add(stockIn);

                Product product = await applicationDbContext.Products.Include("Category").FirstOrDefaultAsync(x => x.Id == productId);

                product.In += stockIn.Quantity;
                product.CurrentPrice = (product.CurrentPrice + stockIn.ShopPrice) / 2;
                product.LastUpdated = DateTime.Now;

                if (User.IsInRole(RoleConstants.Editor) || User.IsInRole(RoleConstants.Administrator))
                {
                    if (!product.NoCountOut)
                    {
                        User user = await applicationDbContext.Users.FirstOrDefaultAsync(x => x.Id == stockIn.User_Id);

                        if (product.Category.ActionIn == ActionConstants.Sum)
                        {
                            user.Cash += stockIn.ShopPrice * stockIn.Quantity;
                        }
                        else if (product.Category.ActionIn == ActionConstants.Rest)
                        {
                            user.Cash -= stockIn.ShopPrice * stockIn.Quantity;
                        }

                        applicationDbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    }
                }

                applicationDbContext.Entry(product).State = System.Data.Entity.EntityState.Modified;

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            ViewBag.Categories = await applicationDbContext.Categories.OrderBy(x => x.Name).ToListAsync();
            return View(model);
        }
    }
}