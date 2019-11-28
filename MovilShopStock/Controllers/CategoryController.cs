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
    public class CategoryController : GenericController
    {
        private ApplicationDbContext applicationDbContext = new ApplicationDbContext();

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

            return View(await applicationDbContext.Categories.Where(x => x.Business_Id == business_working).OrderByDescending(x => x.LastUpdated).ToListAsync());
        }

        [HttpGet]
        public ActionResult Create()
        {
            List<Tuple<string, string>> tuples = new List<Tuple<string, string>>();

            tuples.Add(new Tuple<string, string>(ActionConstants.Rest + "", "Restar"));
            tuples.Add(new Tuple<string, string>(ActionConstants.Sum + "", "Sumar"));

            ViewBag.ActionList = tuples;

            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Category model)
        {
            if (ModelState.IsValid)
            {
                Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());
                model.Id = Guid.NewGuid();
                model.LastUpdated = DateTime.Now;
                model.Business_Id = business_working;
                applicationDbContext.Categories.Add(model);

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            List<Tuple<string, string>> tuples = new List<Tuple<string, string>>();

            tuples.Add(new Tuple<string, string>(ActionConstants.Rest + "", "Restar"));
            tuples.Add(new Tuple<string, string>(ActionConstants.Sum + "", "Sumar"));

            ViewBag.ActionList = tuples;

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            Guid cat_id = Guid.Parse(id);

            List<Tuple<string, string>> tuples = new List<Tuple<string, string>>();

            tuples.Add(new Tuple<string, string>(ActionConstants.Rest + "", "Restar"));
            tuples.Add(new Tuple<string, string>(ActionConstants.Sum + "", "Sumar"));

            ViewBag.ActionList = tuples;

            return View(await applicationDbContext.Categories.FirstOrDefaultAsync(x => x.Id == cat_id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, Category model)
        {
            if (ModelState.IsValid)
            {
                Guid cat_id = Guid.Parse(id);

                Category category = await applicationDbContext.Categories.FirstOrDefaultAsync(x => x.Id == cat_id);
                category.Name = model.Name;
                category.LastUpdated = DateTime.Now;
                category.ShowDashboard = model.ShowDashboard;
                category.ActionIn = model.ActionIn;
                category.ActionOut = model.ActionOut;
                category.SystemAction = model.SystemAction;

                applicationDbContext.Entry(category).State = System.Data.Entity.EntityState.Modified;

                await applicationDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            List<Tuple<string, string>> tuples = new List<Tuple<string, string>>();

            tuples.Add(new Tuple<string, string>(ActionConstants.Rest + "", "Restar"));
            tuples.Add(new Tuple<string, string>(ActionConstants.Sum + "", "Sumar"));

            ViewBag.ActionList = tuples;

            return View(model);
        }

        public ActionResult View()
        {
            CategoryListModel model = new CategoryListModel();

            model.GridPageSize = 10;

            return View(model);
        }
    }
}