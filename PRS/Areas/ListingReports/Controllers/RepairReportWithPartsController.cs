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
    public class RepairReportWithPartsController : Controller
    {
        RepairReportWithParts oRepairReportWithParts;
        // GET: ListingReports/RepairReportWithParts
        public ActionResult Option()
        {
            oRepairReportWithParts = new RepairReportWithParts();
            return View(oRepairReportWithParts);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oRepairReportWithParts = new RepairReportWithParts();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRepairReportWithParts);
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRepairReportWithParts = new RepairReportWithParts();
            oRepairReportWithParts.GetList(frmDt, toDate, programId, ProgramName);
            var jsonResult = Json(oRepairReportWithParts, JsonRequestBehavior.AllowGet);
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