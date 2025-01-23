using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class SNHistoryWebReportController : Controller
    {
        SNHistoryWebReport oSNHistoryWebReport;
        // GET: ListingReports/SNHistoryWebReport
        public ActionResult Option()
        {
            oSNHistoryWebReport = new SNHistoryWebReport();
            return View(oSNHistoryWebReport);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oSNHistoryWebReport = new SNHistoryWebReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oSNHistoryWebReport);
        }

        public JsonResult GetList(string serialNo, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSNHistoryWebReport = new SNHistoryWebReport();
            oSNHistoryWebReport.GetList(serialNo, programId, ProgramName);
            var jsonResult = Json(oSNHistoryWebReport, JsonRequestBehavior.AllowGet);
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