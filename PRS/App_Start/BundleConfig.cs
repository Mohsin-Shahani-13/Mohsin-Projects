using System.Web;
using System.Web.Optimization;


public class BundleConfig
{
    public static void RegisterBundles(BundleCollection bundles)
    {
        bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
              "~/Scripts/jquery.min.js",
              "~/Scripts/jquery-3.3.1.js",
              "~/Scripts/jquery-3.3.1.slim.js",
              "~/Scripts/bootstrap.min.js",
              "~/Scripts/bootstrap-multiselect.js",
              "~/Scripts/datepicker.min.js",
              "~/Scripts/datatables.min.js",
              "~/Scripts/datatables.bs.min.js",
              "~/Scripts/dataTables.buttons.min.js",
              "~/Scripts/dataTables.scroller.min.js",
              "~/Scripts/dataTables.select.min.js",
              "~/Scripts/jquery.bootstrap.newsbox.min.js",
              "~/Scripts/jquery.responsive-tables.min.js",
              "~/Scripts/buttons.html5.min.js",
              "~/Scripts/buttons.print.min.js",
              "~/Scripts/buttons.colVis.min.js",
              "~/Scripts/buttons.bootstrap.min.js",
              "~/Scripts/jszip.min.js",
              "~/Scripts/pdfmake.min.js",
              "~/Scripts/vfs_fonts.js",
              "~/Scripts/dataTables.editor.min.js",
              "~/Scripts/Chart.js",
              "~/Scripts/ChartBundle.js",
              "~/Scripts/ChartJsLabeling.js",
              "~/Scripts/ColReorderWithResize.js",
              "~/Scripts/Dragging.js",
              "~/Scripts/Utility.js",
              "~/Scripts/adminlte.js",
              "~/Scripts/common-v1.0.js",
              "~/Scripts/chartCommon.js",
              "~/Scripts/datatable_sum.js",
              "~/Scripts/CommonGrids.js",
              "~/Scripts/percentBar.js",              
              "~/Scripts/zingchart.min.js",
              "~/Scripts/datetime.js",
                "~/Scripts/moment.min.js"
              ));

        bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(//Added by huzaifa
            "~/Scripts/modernizr-*"));

        // Add bootstrap bundle
        bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include( //Added by huzaifa
            "~/Scripts/bootstrap.js",
            "~/Scripts/respond.js"));

        bundles.Add(new StyleBundle("~/Content/css").Include(
             "~/Content/css/Site.css",
                "~/Content/css/bootstrap.min.css",
                //"~/Content/css/font-awesome.min.css",
                "~/Content/css/all.css",
                "~/Content/css/ionicons.min.css",
                "~/Content/css/_all-skins.min.css",
                "~/Content/css/AdminLTE.min.css",
                "~/Content/css/datatable.min.css",
                "~/Content/css/buttons.dataTables.min.css",
                "~/Content/css/bootstrap-datepicker.css",
                "~/Content/css/bootstrap-multiselect.css",
                "~/Content/css/colReorder.dataTables.min.css",
                "~/Content/css/scroller.dataTables.css",
                "~/Content/css/datatable.select.min.css",
                "~/Content/css/responsive-tables.min.css",
                "~/Content/css/Standards.css"
              ));

        BundleTable.EnableOptimizations = false;
    }
}

