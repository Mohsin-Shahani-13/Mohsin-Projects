using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace IP.Areas.SupplyChain.Controllers
{
    public class VENUnitsPendingReportController : Controller
    {
        // GET: SupplyChain/VENUnitsPendingReport
        VENUnitsPendingReport oVENUnitsPendingReport;
        public ActionResult Option()
        {
            oVENUnitsPendingReport = new VENUnitsPendingReport();
            DataTable dtProgram = oVENUnitsPendingReport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oVENUnitsPendingReport);
        }
        public ActionResult Index(string menuTitle, string rptCode)
        {
            oVENUnitsPendingReport = new VENUnitsPendingReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oVENUnitsPendingReport);
        }
        public JsonResult GetList(string programId, string programName, string fDate, string tDate, string venType, string venNo, string pendingVen)
        {
            oVENUnitsPendingReport = new VENUnitsPendingReport();
            string menuTitle = string.Empty;
            string RptCode = string.Empty;


            oVENUnitsPendingReport.GetList(programId, programName, fDate, tDate, venType, venNo, pendingVen);
            var jsonResult = Json(oVENUnitsPendingReport, JsonRequestBehavior.AllowGet);
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