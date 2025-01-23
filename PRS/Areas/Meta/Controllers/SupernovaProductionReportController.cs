using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.Meta.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.Meta.Controllers
{
    public class SupernovaProductionReportController : Controller
    {
        SupernovaProductionReport oSupernovaProductionReport;
        // GET: Meta/SupernovaProductionReport
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oSupernovaProductionReport = new SupernovaProductionReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oSupernovaProductionReport);
        }
        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSupernovaProductionReport = new SupernovaProductionReport();
            oSupernovaProductionReport.GetList();
            var jsonResult = Json(oSupernovaProductionReport, JsonRequestBehavior.AllowGet);
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