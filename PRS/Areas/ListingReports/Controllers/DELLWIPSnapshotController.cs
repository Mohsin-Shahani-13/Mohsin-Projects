using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;
using IP.Areas.ListingReports.Models;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DELLWIPSnapshotController : Controller
    {
        DELLWIPSnapshot oDELLWIPSnapshot;
        // GET: ListingReports/DELLWIPSnapshot
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDELLWIPSnapshot = new DELLWIPSnapshot();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDELLWIPSnapshot);
        }
        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oDELLWIPSnapshot = new DELLWIPSnapshot();
            oDELLWIPSnapshot.GetList();
            var jsonResult = Json(oDELLWIPSnapshot, JsonRequestBehavior.AllowGet);
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