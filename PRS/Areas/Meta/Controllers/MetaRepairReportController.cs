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
    public class MetaRepairReportController : Controller
    {
        MetaRepairReport oMetaRepairReport;
        // GET: Meta/MetaRepairReport
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oMetaRepairReport = new MetaRepairReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oMetaRepairReport);
        }
        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaRepairReport = new MetaRepairReport();
            oMetaRepairReport.GetList();
            var jsonResult = Json(oMetaRepairReport, JsonRequestBehavior.AllowGet);
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