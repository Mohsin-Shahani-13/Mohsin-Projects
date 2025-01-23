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
    public class RepairConsumptionReportController : Controller
    {
        RepairConsumptionReport oRepairConsumptionReport;
        // GET: Meta/RepairConsumptionReport
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oRepairConsumptionReport = new RepairConsumptionReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRepairConsumptionReport);
        }
        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRepairConsumptionReport = new RepairConsumptionReport();
            oRepairConsumptionReport.GetList();
            var jsonResult = Json(oRepairConsumptionReport, JsonRequestBehavior.AllowGet);
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