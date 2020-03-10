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
    [Models.Handlers.Authorize(Roles = RoleManager.Editor + "," + RoleManager.Administrator)]
    public class ProviderController : GenericController
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Search(TableFilterViewModel filter)
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            List<ProviderModel> result = new List<ProviderModel>();
            long totalRowsFiltered = 0;
            long totalRows = await applicationDbContext.Providers.CountAsync(x => x.Business_Id == business_working);
            List<Provider> model;

            var entity = applicationDbContext.Providers.Where(x => x.Business_Id == business_working);

            IOrderedQueryable<Provider> sort = null;
            if (filter.order[0].column == 0)
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
            else if (filter.order[0].column == 1)
            {
                if (filter.order[0].dir.Equals("asc"))
                {
                    sort = entity.OrderBy(x => x.Contact);
                }
                else
                {
                    sort = entity.OrderByDescending(x => x.Contact);
                }
            }
            else if (filter.order[0].column == 2)
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
                   applicationDbContext.Providers.CountAsync(x => x.Business_Id == business_working && (x.Name.ToString().Contains(filter.search.value) ||
                   x.Contact.ToString().Contains(filter.search.value)));

                model = await
                    sort.Where(x => x.Name.ToString().Contains(filter.search.value) ||
                   x.Contact.ToString().Contains(filter.search.value))
                        .Skip(filter.start)
                        .Take(filter.length)
                        .ToListAsync();
            }

            foreach (var provider in model)
            {
                result.Add(new ProviderModel()
                {
                    DT_RowId = provider.Id.ToString(),
                    Name = provider.Name,
                    Contact = provider.Contact,
                    LastUpdated = provider.LastUpdated.ToString("yyyy-MM-dd hh:mm tt")
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

        [HttpGet]
        public ActionResult Create()
        {
            return View(new Provider());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Provider model)
        {
            if (ModelState.IsValid)
            {
                Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
                model.Id = Guid.NewGuid();
                model.LastUpdated = DateTime.Now;
                model.Business_Id = business_working;
                applicationDbContext.Providers.Add(model);

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            Guid prov_id = Guid.Parse(id);

            return View(await applicationDbContext.Providers.FirstOrDefaultAsync(x => x.Id == prov_id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Provider model)
        {
            if (ModelState.IsValid)
            {
                Guid prov_id = Guid.Parse(id);

                Provider provider = await applicationDbContext.Providers.FirstOrDefaultAsync(x => x.Id == prov_id);
                provider.Name = model.Name;
                provider.LastUpdated = DateTime.Now;
                provider.Contact = model.Contact;

                applicationDbContext.Entry(provider).State = System.Data.Entity.EntityState.Modified;

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}