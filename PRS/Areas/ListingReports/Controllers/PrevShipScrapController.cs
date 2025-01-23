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
    public class PrevShipScrapController : Controller
    {
        // GET: ListingReports/PrevShipScrap
        PrevShipScrap oPrevShipScrap = new PrevShipScrap();
        public ActionResult Option()
        {
            oPrevShipScrap = new PrevShipScrap();
            return View(oPrevShipScrap);
        }

        public ActionResult Index(string menuTitle, string rptCode)
        {
            oPrevShipScrap = new PrevShipScrap();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oPrevShipScrap);
        }

        public JsonResult GetList(string fDate)
        {
            oPrevShipScrap = new PrevShipScrap();
            string menuTitle = string.Empty;
            string RptCode = string.Empty;


            oPrevShipScrap.GetList(fDate);
            var jsonResult = Json(oPrevShipScrap, JsonRequestBehavior.AllowGet);
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