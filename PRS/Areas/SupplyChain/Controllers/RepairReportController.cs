using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.Areas.SupplyChain.Models;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class RepairReportController : Controller
    {
        RepairReport oRepairReport = new RepairReport();
        // GET: SupplyChain/RepairReport
        public ActionResult Option()
        {
            oRepairReport = new RepairReport();
            DataTable dtProgram = oRepairReport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oRepairReport);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oRepairReport = new RepairReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }

        public JsonResult GetList(string programId, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRepairReport = new RepairReport();
            oRepairReport.GetList(programId, programName);
            var jsonResult = Json(oRepairReport, JsonRequestBehavior.AllowGet);
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