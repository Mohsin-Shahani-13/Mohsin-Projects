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
    public class DailyProductionsCopyController : Controller
    {
        // GET: ListingReports/DailyProductionReview
        DailyProductionsCopy oDailyProductionsCopy;

        public PartInquiry DailyProductionsCopy { get; private set; }

        public ActionResult Option()
        {
            oDailyProductionsCopy = new DailyProductionsCopy();
            DataTable dtProgram = oDailyProductionsCopy.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            DataTable dtId = oDailyProductionsCopy.GetIDBySite();
            ViewBag.ddId = cCommon.ToDropDown(dtId, "ID", "ID", "");
            return View(oDailyProductionsCopy);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDailyProductionsCopy = new DailyProductionsCopy();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDailyProductionsCopy);
        }
        public JsonResult GetList(string frmDt, string toDate, string Workstation, string serialno, string WOHeaderId , string programId, string ProgramName, bool isPass, bool isFail)
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

            oDailyProductionsCopy = new DailyProductionsCopy();
            oDailyProductionsCopy.GetList(frmDt, toDate, Workstation, serialno, WOHeaderId, programId, ProgramName, isPass, isFail);
            var jsonResult = Json(oDailyProductionsCopy, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
       
        }

        public ActionResult Detail(string serialNo)
        {
            if (string.IsNullOrEmpty(serialNo))
                return View();
            try
            {
                oDailyProductionsCopy = new DailyProductionsCopy();
                bool success = oDailyProductionsCopy.GetDetail(serialNo);
                if (success)
                    return View(oDailyProductionsCopy);
                else
                    return View(oDailyProductionsCopy);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }

            return View();
        }
    }
}