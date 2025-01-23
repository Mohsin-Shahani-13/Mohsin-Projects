using IP.ActionFilters;
using IP.Areas.ListingReports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DiscrepancyCaseController : Controller
    {
        // GET: ListingReports/DiscrepancyCase
        DiscrepancyCase oDiscrepancyCase;
        public ActionResult Option()
        {
            oDiscrepancyCase = new DiscrepancyCase();
            DataTable dtProgram = oDiscrepancyCase.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oDiscrepancyCase);
        }

        public ActionResult Index(string menuTitle, string rptCode)
        {
            oDiscrepancyCase = new DiscrepancyCase();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDiscrepancyCase);
        }

        public JsonResult GetList(string programID, string programName, string queue)
        {
            oDiscrepancyCase = new DiscrepancyCase();
            string menuTitle = string.Empty;
            string RptCode = string.Empty;


            oDiscrepancyCase.GetList(programID, programName, queue);
            var jsonResult = Json(oDiscrepancyCase, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"].ToString();
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
    }
}