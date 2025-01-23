using IP.ActionFilters;
using IP.Areas.ListingReports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web;
using System.IO;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DellOpenOrderRepairController : Controller
    {
        // GET: ListingReports/DellOpenOrderRepair
        DellOpenOrderRepair oDellOpenOrderRepair = new DellOpenOrderRepair();
        public ActionResult Option()
        {
            oDellOpenOrderRepair = new DellOpenOrderRepair();
            return View(oDellOpenOrderRepair);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDellOpenOrderRepair = new DellOpenOrderRepair();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDellOpenOrderRepair);
        }

        public JsonResult GetList()
        {
            oDellOpenOrderRepair = new DellOpenOrderRepair();
            string menuTitle = string.Empty;
            string RptCode = string.Empty;


            oDellOpenOrderRepair.GetList();
            var jsonResult = Json(oDellOpenOrderRepair, JsonRequestBehavior.AllowGet);
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