using IP.Areas.BRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.BRT.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class CycleCountSummaryController : Controller
    {
        // GET: BRT/CycleCountSummary
        CycleCountSummary oCycleCountSummary;
        public ActionResult Option()
        {
            oCycleCountSummary = new CycleCountSummary();
            DataTable dtProgram = oCycleCountSummary.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oCycleCountSummary);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oCycleCountSummary = new CycleCountSummary();
            //Dictionary<string, object> response = new Dictionary<string, object>();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oCycleCountSummary);
        }
        public JsonResult GetList(string ProgramId, string ProgramName, string no, string calender)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oCycleCountSummary = new CycleCountSummary();
            oCycleCountSummary.GetList(ProgramId, ProgramName, no, calender);
            var jsonResult = Json(oCycleCountSummary, JsonRequestBehavior.AllowGet);
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