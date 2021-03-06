﻿using Microsoft.AspNet.Identity;
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
    [Models.Handlers.Authorize(Roles = RoleManager.Dealer + "," + RoleManager.Editor + "," + RoleManager.Administrator + "," + RoleManager.Reading)]
    public class TransferController : GenericController
    {
        public ActionResult Index(string selectedTab)
        {
            ViewBag.selectedTab = selectedTab;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> BusinessSearch(TableFilterViewModel filter)
        {
            List<TransferBusinessModel> result = new List<TransferBusinessModel>();
            long totalRowsFiltered = 0;
            long totalRows = await applicationDbContext.TransferBusinessProducts.CountAsync();
            List<TransferBusinessProduct> model;

            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            var entity = applicationDbContext.TransferBusinessProducts.Include("User").Include("ProductTo").Include("ProductFrom").Include("ProductFrom.Category").Include("ProductFrom.Business").Include("ProductTo.Business")
                    .Where(x => x.ProductFrom.Business_Id == business_working || x.ProductTo.Business_Id == business_working);

            IOrderedQueryable<TransferBusinessProduct> sort = null;
            if (filter.order[0].column == 1)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.ProductFrom.Name).ThenBy(x => x.ProductTo.Name);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.ProductFrom.Name).ThenByDescending(x => x.ProductTo.Name);
                }
            }
            else if (filter.order[0].column == 2)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.ProductFrom.Category.Name);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.ProductFrom.Category.Name);
                }
            }
            else if (filter.order[0].column == 3)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.ProductFrom.Name);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.ProductFrom.Name);
                }
            }
            else if (filter.order[0].column == 4)
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
                    sort = entity.OrderBy(x => x.Date);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Date);
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
                   applicationDbContext.TransferBusinessProducts.CountAsync(x => x.ProductFrom.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.ProductFrom.Name.ToString().Contains(filter.search.value) ||
                   x.Quantity.ToString().Contains(filter.search.value));

                model = await
                    sort.Where(x => x.ProductFrom.Name.ToString().Contains(filter.search.value) ||
                   x.User.UserName.ToString().Contains(filter.search.value) ||
                   x.ProductFrom.Name.ToString().Contains(filter.search.value) ||
                   x.Quantity.ToString().Contains(filter.search.value))
                        .Skip(filter.start)
                        .Take(filter.length)
                        .ToListAsync();
            }

            foreach (var tbp in model)
            {
                string fromto = "";
                bool sent = false;
                if (tbp.ProductFrom.Business_Id == business_working)
                {
                    fromto = $"Hacia {tbp.ProductTo.Business.Name}";
                    sent = true;
                }
                else
                {
                    fromto = $"Desde {tbp.ProductFrom.Business.Name}";
                }

                result.Add(new TransferBusinessModel
                {
                    DT_RowId = tbp.Id.ToString(),
                    Category = tbp.ProductFrom.Category.Name,
                    ProductName = tbp.ProductFrom.Name,
                    Quantity = tbp.Quantity,
                    Date = tbp.Date.ToString("yyyy-MM-dd hh:mm tt"),
                    User = tbp.User.UserName,
                    FromTo = fromto,
                    Sent = sent
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

        public async Task<ActionResult> CreateBusiness()
        {
            TransferBusinessModel model = new TransferBusinessModel();

            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            string userId = User.Identity.GetUserId();

            ViewBag.To = await applicationDbContext.Businesses.Include("BusinessUsers").Where(x => x.Id != business_working && x.BusinessUsers.FirstOrDefault(y => y.User_Id == userId) != null).OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateBusiness(TransferBusinessModel model)
        {
            string userId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                Guid productId = Guid.Parse(model.ProductName);

                Product product = await applicationDbContext.Products.Include("Category").FirstOrDefaultAsync(x => x.Id == productId);

                Guid business_to = Guid.Parse(model.FromTo);
                Category category = await applicationDbContext.Categories.FirstOrDefaultAsync(x => x.Business_Id == business_to && x.Name.Equals(product.Category.Name));
                if (category == null)
                {
                    category = new Category()
                    {
                        Id = Guid.NewGuid(),
                        Business_Id = business_to,
                        Name = product.Category.Name,
                        ActionIn = product.Category.ActionIn,
                        ActionOut = product.Category.ActionOut,
                        LastUpdated = DateTime.Now,
                        ShowDashboard = product.Category.ShowDashboard,
                        SystemAction = product.Category.SystemAction
                    };
                    applicationDbContext.Categories.Add(category);
                }

                Product product_to = await applicationDbContext.Products.FirstOrDefaultAsync(x => x.Name.Equals(product.Name) && x.Category_Id == category.Id);

                if (product_to == null)
                {
                    product_to = new Product()
                    {
                        Id = Guid.NewGuid(),
                        Category_Id = category.Id,
                        Business_Id = business_to,
                        Name = product.Name,
                        CurrentPrice = product.CurrentPrice,
                        Description = "",
                        Stock = model.Quantity,
                        SalePrice = product.SalePrice,
                        User_Id = userId,
                        LastUpdated = DateTime.Now,
                        isAccesory = product.isAccesory
                    };
                    applicationDbContext.Products.Add(product_to);
                }
                else
                {
                    product_to.Stock += model.Quantity;
                    product_to.CurrentPrice = ((product_to.CurrentPrice * product_to.Stock) + (product.CurrentPrice * model.Quantity)) / product_to.Stock + model.Quantity;
                    product_to.LastUpdated = DateTime.Now;

                    product.Stock -= model.Quantity;
                    product.LastUpdated = DateTime.Now;

                    applicationDbContext.Entry(product).State = System.Data.Entity.EntityState.Modified;
                    applicationDbContext.Entry(product_to).State = System.Data.Entity.EntityState.Modified;
                }

                TransferBusinessProduct transfer = new TransferBusinessProduct()
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.Now,
                    ProductFrom_Id = product.Id,
                    ProductTo_Id = product_to.Id,
                    Quantity = model.Quantity,
                    User_Id = userId
                };
                applicationDbContext.TransferBusinessProducts.Add(transfer);

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index", new { selectedTab = "nav-business" });
            }
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
            ViewBag.Categories = await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderBy(x => x.Name).ToListAsync();

            ViewBag.To = await applicationDbContext.Businesses.Include("BusinessUsers").Where(x => x.Id != business_working && x.BusinessUsers.FirstOrDefault(y => y.User_Id == userId) != null).OrderBy(x => x.Name).ToListAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> PrivateSearch(TableFilterViewModel filter)
        {
            List<TransferPrivateModel> result = new List<TransferPrivateModel>();
            long totalRowsFiltered = 0;
            long totalRows = await applicationDbContext.TransferMoneyUsers.CountAsync();
            List<TransferMoneyUser> model;

            string userId = User.Identity.GetUserId();

            var entity = applicationDbContext.TransferMoneyUsers.Include("UserFrom").Include("UserTo")
                    .Where(x => x.UserFrom_Id == userId || x.UserTo_Id == userId);

            IOrderedQueryable<TransferMoneyUser> sort = null;
            if (filter.order[0].column == 1)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.UserFrom.UserName).ThenBy(x => x.UserTo.UserName);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.UserFrom.UserName).ThenByDescending(x => x.UserTo.UserName);
                }
            }
            else if (filter.order[0].column == 2)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Amount);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Amount);
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
                   applicationDbContext.TransferMoneyUsers.CountAsync(x => x.UserFrom.UserName.ToString().Contains(filter.search.value) ||
                   x.UserTo.UserName.ToString().Contains(filter.search.value) ||
                   x.Amount.ToString().Contains(filter.search.value));

                model = await
                    sort.Where(x => x.UserFrom.UserName.ToString().Contains(filter.search.value) ||
                   x.UserTo.UserName.ToString().Contains(filter.search.value) ||
                   x.Amount.ToString().Contains(filter.search.value))
                        .Skip(filter.start)
                        .Take(filter.length)
                        .ToListAsync();
            }
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            foreach (var tbp in model)
            {
                string fromto = "";
                bool sent = false;
                if (tbp.UserFrom_Id == userId && tbp.BusinessFrom_Id == business_working)
                {
                    fromto = tbp.BusinessTo == null ? $"Hacia {tbp.UserTo.UserName}" : $"Hacia {tbp.BusinessTo.Name} -> {tbp.UserTo.UserName}";
                    sent = true;
                }
                else
                {
                    fromto = tbp.BusinessFrom == null ? $"Desde {tbp.UserFrom.UserName}" : $"Desde {tbp.BusinessFrom.Name} -> {tbp.UserFrom.UserName}";
                }

                result.Add(new TransferPrivateModel
                {
                    DT_RowId = tbp.Id.ToString(),
                    Date = tbp.Date.ToString("yyyy-MM-dd hh:mm tt"),
                    Amount = tbp.Amount.ToString("#,##0.00"),
                    FromTo = fromto,
                    Sent = sent
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

        public async Task<ActionResult> CreatePrivate()
        {
            TransferPrivateModel model = new TransferPrivateModel();

            ViewBag.BusinessUsers = await GetSelect();

            return View(model);
        }

        private async Task<List<TransferSelectModel>> GetSelect()
        {
            string userId = User.Identity.GetUserId();
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            List<BusinessUser> businessUsers = await applicationDbContext.BusinessUsers.Where(x => x.User_Id == userId).ToListAsync();

            List<TransferSelectModel> selects = new List<TransferSelectModel>();

            foreach (var businessUser in businessUsers)
            {
                List<BusinessUser> anotherUsers = await applicationDbContext.BusinessUsers.Include("Business").Include("User").Where(x => (x.Business_Id == businessUser.Business_Id && x.User_Id != userId) || x.Business_Id != business_working).ToListAsync();

                foreach (var anotherUser in anotherUsers)
                {
                    if (selects.FirstOrDefault(x => x.Id == $"{anotherUser.User_Id}_{anotherUser.Business_Id}") == null)
                    {
                        selects.Add(new TransferSelectModel()
                        {
                            Id = $"{anotherUser.User_Id}_{anotherUser.Business_Id}",
                            Name = $"{anotherUser.Business.Name} -> {anotherUser.User.UserName}"
                        });
                    }
                }
            }

            return selects.OrderBy(x => x.Name).ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePrivate(TransferPrivateModel model)
        {
            string userId = User.Identity.GetUserId();
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            if (ModelState.IsValid)
            {
                BusinessUser user_from = await applicationDbContext.BusinessUsers.FirstOrDefaultAsync(x => x.User_Id == userId && x.Business_Id == business_working);

                var ids = model.FromTo.Split('_');
                string toUserId = ids[0];
                Guid toBusinessId = Guid.Parse(ids[1]);

                BusinessUser user_to = await applicationDbContext.BusinessUsers.FirstOrDefaultAsync(x => x.User_Id == toUserId && x.Business_Id == toBusinessId);

                user_from.Cash -= decimal.Parse(model.Amount);
                user_to.Cash += decimal.Parse(model.Amount);

                applicationDbContext.Entry(user_from).State = System.Data.Entity.EntityState.Modified;
                applicationDbContext.Entry(user_to).State = System.Data.Entity.EntityState.Modified;

                TransferMoneyUser transfer = new TransferMoneyUser()
                {
                    Id = Guid.NewGuid(),
                    Amount = decimal.Parse(model.Amount.Replace(".", ",")),
                    Date = DateTime.Now,
                    UserFrom_Id = userId,
                    UserTo_Id = toUserId,
                    BusinessTo_Id = toBusinessId,
                    BusinessFrom_Id = business_working
                };
                applicationDbContext.TransferMoneyUsers.Add(transfer);

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index", new { selectedTab = "nav-private-tab" });
            }

            ViewBag.BusinessUsers = await GetSelect();

            return View(model);
        }
    }
}