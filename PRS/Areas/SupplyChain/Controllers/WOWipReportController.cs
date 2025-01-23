using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.SupplyChain.Models;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class WOWipReportController : Controller
    {
        // GET: SupplyChain/WOWipReport
        WOWipReport oWOWipReport = new WOWipReport();

        public ActionResult Option()
        {
            oWOWipReport = new WOWipReport();
            DataTable dtProgram = oWOWipReport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oWOWipReport);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oWOWipReport = new WOWipReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oWOWipReport);
        }
        public JsonResult GetList(string frmDt, string toDate, bool isAllDate, string WorkOrderType, string WorkOrderTypeText, string programId, String ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oWOWipReport = new WOWipReport();
            oWOWipReport.GetList(frmDt, toDate, isAllDate, WorkOrderType, WorkOrderTypeText, programId, ProgramName);
            var jsonResult = Json(oWOWipReport, JsonRequestBehavior.AllowGet);
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