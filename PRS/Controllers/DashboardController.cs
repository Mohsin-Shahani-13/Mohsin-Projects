using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Drawing.Charts;
using IP.ActionFilters;
using IP.Models;
namespace IP.Controllers
{

    [OutputCache(Duration = 0)]
    [SessionTimeout]
    [UpdateLastActivity]
    [CheckSessionTimeoutAtrribute]
    public class DashboardController : Controller
    {
        DashboardModel oDashBoardModel;
        System.Data.DataTable dtProgram;
        System.Data.DataTable dtDashboards;
        System.Data.DataTable dtSites;


        public ActionResult Index()
        {
            if (cCommon.IsSessionExpired())
            {
                return RedirectToAction("logout", "Home");
            }
            return View();
        }

        [HttpGet]
        public JsonResult GetWidgetList()
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            DashboardModel oModel = new DashboardModel();
            oModel.Get_Widget_List();
            response["lst_widgets"] = oModel.lst_widgets;
            response["IsValid"] = true;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult LoadWidget(string widget_id, string widget_type)
        {
            DashboardModel oModel = new DashboardModel();
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (widget_type.ToLower() == "count")
            {
                oModel.Get_Count_Widget(widget_id);
                response["widget_desc"] = oModel.widget_desc;
                response["widget_count_count"] = oModel.widget_count_count;
                response["widget_count_bg"] = oModel.widget_count_bg;
                response["widget_count_fraction"] = oModel.widget_count_fraction;
                response["widget_count_report_URL"] = oModel.widget_count_report_URL;
            }
            else if (widget_type.ToLower() == "table")
            {
                oModel.Get_Table_Widget(widget_id);
                response["lst_widget_table"] = oModel.lst_widget_table;
                response["widget_table_column_format"] = oModel.widget_table_column_format;
                response["widget_table_cols_count"] = oModel.widget_table_cols_count;

            }
            else if (widget_type.ToLower() == "list")
            {
                oModel.Get_List_Widget(widget_id);
                response["lst_widget_list"] = oModel.lst_widget_list;
            }
            else if (widget_type.ToLower() == "hyperlist")
            {
                oModel.Get_HyperList_Widget(widget_id);
                response["lst_widget_hyperlist"] = oModel.lst_widget_hyperlist;
            }
            else if (widget_type.ToLower() == "chart")
            {
                oModel.Get_Chart_Widget(widget_id);
                response["chart_data"] = oModel.chart_data;
                response["chart_title"] = oModel.chart_title;
                response["widget_count_count"] = oModel.widget_count_count;
                response["widget_count_report_URL"] = oModel.widget_count_report_URL;
            }
            else if (widget_type.ToLower() == "stackbarchart")
            {
                oModel.Get_Stack_Chart_Widget(widget_id);
                response["chart_data"] = oModel.chart_data;
                response["chart_title"] = oModel.chart_title;
                response["widget_count_count"] = oModel.widget_count_count;
                response["widget_count_report_URL"] = oModel.widget_count_report_URL;
            }
            else if (widget_type.ToLower() == "doughnut")
            {
                oModel.Get_Doughnut_Chart_Widget(widget_id);
                response["chart_data"] = oModel.chart_data;
                response["chart_title"] = oModel.chart_title;
                response["widget_count_count"] = oModel.widget_count_count;

                // Include percentage values if needed
                if (oModel.chart_data.ContainsKey("percentage_values"))
                {
                    response["percentage_values"] = oModel.chart_data["percentage_values"];
                }
            }
            else if (widget_type.ToLower() == "bar")
            {
                oModel.Get_Bar_Chart_Widget(widget_id);
                response["chart_data"] = oModel.chart_data;
                response["chart_title"] = oModel.chart_title;
                response["widget_count_count"] = oModel.widget_count_count;

                // Include percentage values if needed
                if (oModel.chart_data.ContainsKey("percentage_values"))
                {
                    response["percentage_values"] = oModel.chart_data["percentage_values"];
                }
            }

            else if (widget_type.ToLower() == "line")
            {
                oModel.Get_Line_Chart_Widget(widget_id);
                response["chart_data"] = oModel.chart_data;
                response["chart_title"] = oModel.chart_title;
                response["widget_count_count"] = oModel.widget_count_count;

                // Include percentage values if needed
                if (oModel.chart_data.ContainsKey("percentage_values"))
                {
                    response["percentage_values"] = oModel.chart_data["percentage_values"];
                }
            }
            else if (widget_type.ToLower() == "count_group")
            {
                oModel.Get_Group_Counts_Widget(widget_id);
                response["lst_widget_group_counts"] = oModel.lst_widget_group_counts;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveEmployeeWidgets(List<DashboardModel.Employee_Widgets> lst)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            DashboardModel oModel = new DashboardModel();
            oModel.Save_Employee_Widgets(lst);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmployeeWidgets()
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            DashboardModel oModel = new DashboardModel();

            oModel.Get_Employee_Widgets();

            response["lstWidgetsEmployee"] = oModel.lst_employee_widgets;
            response["IsValid"] = true;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetURLInfo(string link)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            if (cCommon.IsSessionExpired())
            {
                response["Session"] = null;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else
            {
                DashboardModel oModel = new DashboardModel();

                oModel.Get_Report_Info(link);
                response["Session"] = "NotNull";
                response["URL"] = oModel.report_URL;
                response["is_internal"] = oModel.report_is_internal;
                response["code"] = oModel.report_code;
                response["title_short"] = oModel.report_title_short;
                response["designed_by"] = oModel.report_designed_by;
                response["target"] = oModel.report_target;

                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        [Route("Bigtv/Dashboard")]
        public ActionResult BigTv(string program)
        {
            if (string.IsNullOrEmpty(program))
            {
                oDashBoardModel = new DashboardModel();
                dtProgram = new System.Data.DataTable();
                dtProgram = oDashBoardModel.GetProgramsForBigTv();
                ViewBag.DDLProgram = cCommon.ToDropDown(dtProgram, "Name", "Name", "Select Program");
                return View(); 
            }
            else
            {
                oDashBoardModel = new DashboardModel();
                dtSites = new System.Data.DataTable();
                dtSites = oDashBoardModel.GetProgramIdAndSites(program);
                Session["DBProgram"] = program;
                ViewBag.DDSites = cCommon.ToDropDown(dtSites, "Id", "site","");
                dtDashboards = new System.Data.DataTable();
                dtDashboards = oDashBoardModel.GetDashboardDD();
                ViewBag.DDLDashboard = cCommon.ToDropDown(dtDashboards, "LkupType", "LkupDesc", "Select dashboard");
                ViewBag.Program = program;
                return View("GetDashboards");
            }
        }

        public ActionResult GetDashboards(string program)
        {
            oDashBoardModel = new DashboardModel();
            //dtDashboards = new System.Data.DataTable();
            //dtDashboards = oDashBoardModel.GetDashboardDD(program);
            //ViewBag.DDLDashboard = cCommon.ToDropDown(dtDashboards, "LkupId", "LkupDesc", "Select from list");
            return View();
        }

        [Route("Bigtv/PackingCounter/{id}")]
        public ActionResult PackingCounter(string id)
        {
            // Check if the DashboardAccess cookie exists and is valid
            HttpCookie dashboardCookie = HttpContext.Request.Cookies["DashboardAccess"];
            if (dashboardCookie != null && dashboardCookie.Value == id && DateTime.UtcNow.Date == DateTime.Parse(dashboardCookie["Date"]).Date)
            {
                // Cookie is valid; allow access to the dashboard
                Session["ProgramIdForDB"] = id;
                return View();
            }

            // If the cookie is missing or invalid, require login
            if (cCommon.IsSessionExpired())
            {
                return RedirectToAction("Login", "Home");
            }

            // If login is successful, set the cookie
            HttpCookie newDashboardCookie = new HttpCookie("DashboardAccess")
            {
                Value = id,
                Expires = DateTime.UtcNow.Date.AddDays(1), // Expire at midnight
                HttpOnly = true, // Secure against client-side scripts
                Secure = Request.IsSecureConnection // Only send over HTTPS
                //Secure = false
            };
            newDashboardCookie["Date"] = DateTime.UtcNow.ToString("yyyy-MM-dd");
            HttpContext.Response.Cookies.Add(newDashboardCookie);

            // Allow access to the dashboard
            Session["ProgramIdForDB"] = id;
            return View();
        }

        [HttpGet]
        public JsonResult GetPackingCounter(string programId)
        {
            oDashBoardModel = new DashboardModel();
            var counter = oDashBoardModel.GetPackingCounter(programId);
            var dbTitle = oDashBoardModel.DBTitle;
            //ViewBag.DBTitle = oDashBoardModel.DBTitle;
            return Json(new { DBTitle = dbTitle, Counter = counter }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DBIndex()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Samsung_Horizon()
        {

            return View();
        }

        [HttpGet]
        public ActionResult SamsungRefresh()
        {
            ViewBag.ReportTitle = "Samsung Refresh";
            DashboardModel oSamSung = new DashboardModel();
            oSamSung.getStations();
            oSamSung.getTotalCrnt();
            return View(oSamSung);
        }

        [HttpGet]
        public ActionResult SamsungHorizonChart()
        {
            DashboardModel oDashboard = new DashboardModel();
            bool success = oDashboard.getChart();
            if (success)
                return View(oDashboard);
            else
                return View();
        }

    }
}