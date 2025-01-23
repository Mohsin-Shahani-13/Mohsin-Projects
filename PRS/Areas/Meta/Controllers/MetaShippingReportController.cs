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
    public class MetaShippingReportController : Controller
    {
        MetaShippingReport oMetaShippingReport = new MetaShippingReport();
        // GET: Meta/MetaShippingReport
        public ActionResult Option()
        {
            oMetaShippingReport = new MetaShippingReport();
            return View();
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaShippingReport = new MetaShippingReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string PartNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaShippingReport = new MetaShippingReport();
            oMetaShippingReport.GetList(programId, ProgramName, PartNo);
            var jsonResult = Json(oMetaShippingReport, JsonRequestBehavior.AllowGet);
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