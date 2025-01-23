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
    public class MetaReportController : Controller
    {
        // GET: Meta/MetaReport
        MetaReport oMetaReport;
        public ActionResult Option()
        {
            oMetaReport = new MetaReport();
            DataTable dtcontract = oMetaReport.GetSitewiseContract();
            ViewBag.ddContract = cCommon.ToDropDown(dtcontract, "contract", "programName", "");
            return View(oMetaReport);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oMetaReport = new MetaReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oMetaReport);
        }
        public JsonResult GetList(string contract, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaReport = new MetaReport();
            oMetaReport.GetList(Session["ProgramIdBySiteForMeta"].ToString(), "META");
            var jsonResult = Json(oMetaReport, JsonRequestBehavior.AllowGet);
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