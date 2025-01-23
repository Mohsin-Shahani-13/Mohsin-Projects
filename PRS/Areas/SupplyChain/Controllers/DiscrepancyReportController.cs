using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DiscrepancyReportController : Controller
    {
        DiscrepancyReport oDiscrepancyReport = new DiscrepancyReport();
        // GET: SupplyChain/DiscrepancyReport
        public ActionResult Option(string programId, string ProgramName)
        {


            DataTable dtassignedTo = oDiscrepancyReport.GetAssignedTo(programId, ProgramName);
            ViewBag.ddlassignedTo = cCommon.ToDropDown(dtassignedTo, "AssignedTo", "AssignedTo", "All");

            bool success = oDiscrepancyReport.Status(programId, ProgramName);

            bool success1 = oDiscrepancyReport.Type(programId, ProgramName);
            if (success && success1)
                return View(oDiscrepancyReport);
            else
                return View();
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDiscrepancyReport = new DiscrepancyReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDiscrepancyReport);
        }

        public JsonResult GetList(string programId, string ProgramName, string frmDt, string toDate, string custRef, string status, string type, string assignedTo, bool OpenWO)
        {
            string menuTitle = string.Empty;
            string RptCode;


            oDiscrepancyReport = new DiscrepancyReport();
            oDiscrepancyReport.GetList(programId, ProgramName, frmDt, toDate, custRef, status, type, assignedTo, OpenWO);
            var jsonResult = Json(oDiscrepancyReport, JsonRequestBehavior.AllowGet);
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