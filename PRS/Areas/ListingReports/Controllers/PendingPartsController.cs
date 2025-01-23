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
    public class PendingPartsController : Controller
    {

        
        PendingParts oPendingParts = new PendingParts();
        // GET: ListingReports/PendingParts
        public ActionResult Option()
        {
            oPendingParts = new PendingParts();
            return View();
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPendingParts = new PendingParts();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string partNo)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oPendingParts = new PendingParts();
            oPendingParts.GetList(programId, ProgramName, partNo);
            var jsonResult = Json(oPendingParts, JsonRequestBehavior.AllowGet);
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