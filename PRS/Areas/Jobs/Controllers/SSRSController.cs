using IP.Areas.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.Jobs.Controllers
{
    public class SSRSController : Controller
    {
        // GET: Jobs/SSRS
        SSRS oSSRS = new SSRS();
        public ActionResult Option()
        {
            DataTable dtProgram = oSSRS.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "program", "program", "All");

            DataTable dtReportStatus = oSSRS.Status();
            ViewBag.ddlReportStatus = cCommon.ToDropDown(dtReportStatus, "Status", "Status", "All");
            return View(oSSRS);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oSSRS = new SSRS();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            
            return View(oSSRS);
        }
        public JsonResult GetList(string ReportName, string status, string ProgramId, string ProgramName, string frequency)
        {
            string menuTitle = string.Empty;
            string RptCode;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            oSSRS = new SSRS();
            oSSRS.GetList(ReportName, status, ProgramId, ProgramName, frequency);
            var jsonResult = Json(oSSRS, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public JsonResult RunSSRS(string subscriptionId)
        {
            //LOAD MRU & LOG QUERY
            cLog oLog = new cLog();
            oSSRS = new SSRS();
            oSSRS.RunSSRS(subscriptionId);


            var jsonResult = Json(oSSRS, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}