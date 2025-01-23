using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;
using IP.Areas.ListingReports.Models;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class AgedRepairsController : Controller
    {
        AgedRepairs oAgedRepairs;
        // GET: ListingReports/AgedRepairs
        public ActionResult Option()
        {
            oAgedRepairs = new AgedRepairs();
            DataTable dtProgram = oAgedRepairs.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oAgedRepairs);
        }

        public ActionResult Index(string rptCode, string menuTitle)
        {
            oAgedRepairs = new AgedRepairs();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oAgedRepairs);
        }

        public JsonResult GetList(string aged, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oAgedRepairs = new AgedRepairs();
            oAgedRepairs.GetList(aged, programId, ProgramName);
            var jsonResult = Json(oAgedRepairs, JsonRequestBehavior.AllowGet);
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