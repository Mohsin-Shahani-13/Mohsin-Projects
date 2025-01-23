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
    public class WorkStationHistoryController : Controller
    {
        WorkStationHistory oWorkStationHistory;
        // GET: ListingReports/WorkStationHistory

        public ActionResult Option()
        {
            oWorkStationHistory = new WorkStationHistory();
            return View(oWorkStationHistory);
        }
        public ActionResult Index(string RptCode, string menuTitle, bool ischecked)
        {
            oWorkStationHistory = new WorkStationHistory();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ischecked = ischecked;

            return View(oWorkStationHistory);
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName, bool ischecked, bool isAllDate, string serialNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            

            oWorkStationHistory = new WorkStationHistory();
            oWorkStationHistory.GetList(frmDt, toDate, programId, ProgramName, ischecked, isAllDate, serialNo);
            var jsonResult = Json(oWorkStationHistory, JsonRequestBehavior.AllowGet);
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