using System.Web;
using System.Web.Optimization;

namespace PrancingTurtle
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui-1.12.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/highcharts").Include(
                "~/Scripts/Highcharts-4.0.1/js/highcharts-all.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/fullcalendar").Include(
                "~/Scripts/gcal.js",
                "~/Scripts/fullcalendar.js"));

            bundles.Add(new ScriptBundle("~/bundles/socialmedia").Include(
                "~/Scripts/fb/fbjs.js",
                "~/Scripts/twitter/twjs.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                //"~/Content/bootstrap.superhero.min.css",
                "~/Content/superhero/bootstrap.css",
                "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/fullcalendarcss").Include(
                "~/Content/fullcalendar.css"));

            bundles.Add(new StyleBundle("~/Content/themebase").Include(
                "~/Content/themes/base/core.css",
                "~/Content/themes/base/theme.css",
                "~/Content/themes/base/accordion.css",
                "~/Content/themes/base/autocomplete.css",
                "~/Content/themes/base/button.css",
                "~/Content/themes/base/datepicker.css",
                "~/Content/themes/base/dialog.css",
                "~/Content/themes/base/draggable.css",
                "~/Content/themes/base/menu.css",
                "~/Content/themes/base/progressbar.css",
                "~/Content/themes/base/resizable.css",
                "~/Content/themes/base/selectable.css",
                "~/Content/themes/base/selectmenu.css",
                "~/Content/themes/base/sortable.css",
                "~/Content/themes/base/slider.css",
                "~/Content/themes/base/spinner.css",
                //"~/Content/themes/base/tabs.css",
                "~/Content/themes/base/tooltip.css"));

            //bundles.Add(new StyleBundle("~/Content/redmondtheme").Include(
            //    "~/Content/themes/redmond/jquery-ui.css"));

            //bundles.Add(new StyleBundle("~/Content/flicktheme").Include(
            //    "~/Content/themes/flick/jquery-ui.css"));

            bundles.Add(new ScriptBundle("~/bundles/dropzonescripts").Include(
                "~/Scripts/dropzone/dropzone.js"));

            //bundles.Add(new StyleBundle("~/Content/dropzonecss").Include(
            //         "~/Scripts/dropzone/css/basic.css",
            //         "~/Scripts/dropzone/css/dropzone.css"));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
        }
    }
}
