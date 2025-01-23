using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DailyProductionReviewController : Controller
    {
        // GET: ListingReports/DailyProductionReview
        DailyProductionReview oDailyProductionReview;
        public ActionResult Option()
        {
            oDailyProductionReview = new DailyProductionReview();
            DataTable dtProgram = oDailyProductionReview.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oDailyProductionReview);
        }
        public ActionResult OptionBose()
        {
            oDailyProductionReview = new DailyProductionReview();
            DataTable dtProgram = oDailyProductionReview.Program();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oDailyProductionReview);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {

            oDailyProductionReview = new DailyProductionReview();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.programName = Request.QueryString["ProgramName"];
            return View(oDailyProductionReview);
        }
        public JsonResult GetList(string frmDt, string toDate, string Workstation, string programId, String ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            ViewBag.ProgramName = ProgramName;
            oDailyProductionReview = new DailyProductionReview();
            oDailyProductionReview.GetList(frmDt, toDate, Workstation, programId, ProgramName);
            var jsonResult = Json(oDailyProductionReview, JsonRequestBehavior.AllowGet);
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