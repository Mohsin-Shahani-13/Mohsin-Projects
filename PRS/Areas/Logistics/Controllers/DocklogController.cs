using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.Logistics.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.Logistics.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DocklogController : Controller
    {
        // GET: Logistics/Docklog
        Docklog oDocklog = new Docklog();
        public ActionResult Option()
        {
            oDocklog = new Docklog();
            return View(oDocklog);
        }
        public ActionResult OptionDell()
        {
            oDocklog = new Docklog();
            return View(oDocklog);
        }
        public ActionResult OptionBose()
        {
            oDocklog = new Docklog();
            return View(oDocklog);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDocklog = new Docklog();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            string program = Request.QueryString["ProgramName"];
            ViewBag.ProgramName = program;
            return View(oDocklog);
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oDocklog = new Docklog();
            oDocklog.GetList(frmDt, toDate, programId, ProgramName);
            var jsonResult = Json(oDocklog, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
    }
}