using Microsoft.AspNet.Identity;
using MovilShopStock.Models;
using MovilShopStock.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovilShopStock.Controllers
{
    public class GenericController : Controller
    {
        private ApplicationDbContext applicationDbContext = new ApplicationDbContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.GetUserId();
                List<BusinessUser> business_user = applicationDbContext.BusinessUsers.Include("Business").Where(x => x.User_Id == userId).ToList();
                List<Business> businesses = business_user != null ? business_user.Select(x => x.Business).ToList() : new List<Business>();
                Guid business_working = Guid.Parse(Session["BusinessWorking"].ToString());

                ViewBag.Business = businesses;
                ViewBag.BusinessWorking = businesses.FirstOrDefault(x => x.Id == business_working);
            }
        }
    }
}