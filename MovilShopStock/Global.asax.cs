using Microsoft.AspNet.Identity;
using MovilShopStock.Models;
using MovilShopStock.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Telerik.Web.Mvc;

namespace MovilShopStock
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private ApplicationDbContext applicationDbContext = new ApplicationDbContext();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            WebAssetDefaultSettings.ScriptFilesPath = "~/Content/telerik/js";
            WebAssetDefaultSettings.StyleSheetFilesPath = "~/Content/telerik/css";
        }

        protected void Session_Start()
        {
            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.GetUserId();
                Guid business_working = Guid.Empty;

                User user = applicationDbContext.Users.FirstOrDefault(x => x.Id == userId);
                if (user.CurrentBusiness_Id == null)
                {
                    try
                    {
                        BusinessUser business_user = applicationDbContext.BusinessUsers.Include("Business").FirstOrDefault(x => x.User_Id == userId && x.Business.IsPrimary);

                        business_working = business_user.Business_Id;
                    }
                    catch (Exception e)
                    {
                    }
                }
                else
                {
                    business_working = user.CurrentBusiness_Id.Value;
                }

                Session["BusinessWorking"] = business_working;
            }
        }
    }
}