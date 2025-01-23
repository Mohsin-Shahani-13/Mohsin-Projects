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
{// Name change to Month to Date Receiving Report
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MetaReceivingReportController : Controller
    {
        MetaReceivingReport oMetaReceivingReport = new MetaReceivingReport();
        // GET: Meta/MetaReceivingReport
        public ActionResult Option()
        {
            oMetaReceivingReport = new MetaReceivingReport();
            return View();
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaReceivingReport = new MetaReceivingReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string RMARef, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaReceivingReport = new MetaReceivingReport();
            oMetaReceivingReport.GetList(programId, ProgramName, RMARef, frmDt, toDate);
            var jsonResult = Json(oMetaReceivingReport, JsonRequestBehavior.AllowGet);
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