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
    public class CycleCountAccuracyController : Controller
    {
        CycleCountAccuracy oCycleCountAccuracy;
        // GET: BRT/CycleCountAccuracy
        public ActionResult Option()
        {
            oCycleCountAccuracy = new CycleCountAccuracy();
            DataTable dtProgram = oCycleCountAccuracy.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oCycleCountAccuracy);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oCycleCountAccuracy = new CycleCountAccuracy();
            //Dictionary<string, object> response = new Dictionary<string, object>();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oCycleCountAccuracy);
        }
        public JsonResult GetList(string ProgramId, string ProgramName, string no, string calender)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oCycleCountAccuracy = new CycleCountAccuracy();
            oCycleCountAccuracy.GetList(ProgramId, ProgramName, no, calender);
            var jsonResult = Json(oCycleCountAccuracy, JsonRequestBehavior.AllowGet);
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