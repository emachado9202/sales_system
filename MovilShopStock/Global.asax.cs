using Microsoft.AspNet.Identity;
using MovilShopStock.Models;
using MovilShopStock.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
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
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            WebAssetDefaultSettings.ScriptFilesPath = "~/Content/telerik/js";
            WebAssetDefaultSettings.StyleSheetFilesPath = "~/Content/telerik/css";
        }
    }
}