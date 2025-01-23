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
    public class AuditDefectsReportController : Controller
    {
        AuditDefectsReport oAuditDefectsReport;
        // GET: ListingReports/AuditDefectsReport
        public ActionResult Option()
        {
            oAuditDefectsReport = new AuditDefectsReport();
            DataTable dtProgram = oAuditDefectsReport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oAuditDefectsReport);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oAuditDefectsReport = new AuditDefectsReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oAuditDefectsReport);
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oAuditDefectsReport = new AuditDefectsReport();
            oAuditDefectsReport.GetList(frmDt, toDate, programId, programName);
            var jsonResult = Json(oAuditDefectsReport, JsonRequestBehavior.AllowGet);
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