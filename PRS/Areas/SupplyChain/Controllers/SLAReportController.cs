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
    public class SLAReportController : Controller
    {
        // GET: SupplyChain/SLAReport
        SLAReport oSLAReport = new SLAReport();
        public ActionResult Option()
        {
            // oSLAReport = new SLAReport();
            DataTable dtProgram = oSLAReport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            //return PartialView("GetProgramBySite");
            

            bool success = oSLAReport.GetStatus();
            if (success)
                return View(oSLAReport);
            else
                return View();
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oSLAReport = new SLAReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oSLAReport);
        }
        public JsonResult GetList(string frmDt, string toDt, string CustomerReference, string PartNo, string programId, string ProgramName, string statusId, string Status)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSLAReport = new SLAReport();
            oSLAReport.GetList(frmDt, toDt, CustomerReference, PartNo, programId, ProgramName, statusId, Status);
            var jsonResult = Json(oSLAReport, JsonRequestBehavior.AllowGet);
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