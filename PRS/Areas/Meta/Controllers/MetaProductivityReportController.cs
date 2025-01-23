using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.Meta.Models;
using System.Data;

using IP.ActionFilters;


namespace IP.Areas.Meta.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MetaProductivityReportController : Controller
    {
        MetaProductivityReport oMetaProductivityReport = new MetaProductivityReport();
        // GET: Meta/MetaProductivityReport
        public ActionResult Option()
        {
            oMetaProductivityReport = new MetaProductivityReport();
            DataTable dtProgram = oMetaProductivityReport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oMetaProductivityReport);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaProductivityReport = new MetaProductivityReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName,string PartNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaProductivityReport = new MetaProductivityReport();
            oMetaProductivityReport.GetList(frmDt, toDate, programId, ProgramName,PartNo);
            var jsonResult = Json(oMetaProductivityReport, JsonRequestBehavior.AllowGet);
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