using IP.ActionFilters;
using IP.Areas.ListingReports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class ExceptionsReportController : Controller
    {
        // GET: ListingReports/ExceptionsReport
        ExceptionsReport oExceptionsReport;


        public ActionResult Option()
        {
            oExceptionsReport = new ExceptionsReport();
            DataTable dtProgram = oExceptionsReport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "ProgramName", "");
            return View(oExceptionsReport);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oExceptionsReport = new ExceptionsReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oExceptionsReport);
        }
        public JsonResult GetList(string frmDt, string toDt, bool isAllDate, string ProgramId, string ProgramName, string serialNo, string status)
        {
            string menuTitle = string.Empty;
            string RptCode = string.Empty;

            oExceptionsReport = new ExceptionsReport();
            oExceptionsReport.GetList(frmDt, toDt, isAllDate, ProgramId, ProgramName, serialNo, status);
            var jsonResult = Json(oExceptionsReport, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"].ToString();
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
    }
}