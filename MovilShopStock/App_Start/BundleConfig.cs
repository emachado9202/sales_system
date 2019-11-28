using System.Web;
using System.Web.Optimization;

namespace MovilShopStock
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/utils/stock.jquery.utils.js",
                        "~/Scripts/jquery-shims.js",
                        "~/Scripts/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/telerik").Include(
                      "~/Scripts/utils/stock.hacks.js",
                      "~/Scripts/utils/stock.common.js",
                      "~/Content/telerik/js/2012.2.607/telerik.common.min.js",
                      "~/Content/telerik/js/2012.2.607/telerik.textbox.min.js",
                      "~/Content/telerik/js/2012.2.607/telerik.grid.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/css/telerik").Include(
                      "~/Content/telerik/css/2012.2.607/telerik.rtl.css",
                      "~/Content/telerik/css/2012.2.607/telerik.common.css",
                      "~/Content/telerik.css"));
        }
    }
}