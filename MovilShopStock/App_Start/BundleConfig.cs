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
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/layout").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/messages_es.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/table").Include(
                "~/Content/DataTables/media/js/jquery.dataTables.js",
                "~/Content/DataTables/media/js/dataTables.bootstrap.min.js",
                "~/Content/DataTables/extensions/Buttons/js/dataTables.buttons.min.js",
                "~/Content/DataTables/extensions/Buttons/js/buttons.bootstrap.min.js",
                "~/Content/DataTables/extensions/Buttons/js/buttons.flash.min.js",
                "~/Content/DataTables/extensions/Buttons/js/jszip.min.js",
                "~/Content/DataTables/extensions/Buttons/js/pdfmake.min.js",
                "~/Content/DataTables/extensions/Buttons/js/vfs_fonts.min.js",
                "~/Content/DataTables/extensions/Buttons/js/buttons.html5.min.js",
                "~/Content/DataTables/extensions/Buttons/js/buttons.print.min.js",
                "~/Content/DataTables/extensions/Responsive/js/dataTables.responsive.min.js",
                "~/Content/DataTables/extensions/Select/js/dataTables.select.min.js"
            ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/table").Include(
               "~/Content//DataTables/media/css/dataTables.bootstrap.min.css",
               "~/Content/DataTables/extensions/buttons/css/buttons.bootstrap.min.css",
               "~/Content/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css",
               "~/Content/DataTables/extensions/Select/css/select.bootstrap.min.css"
           ));
        }
    }
}