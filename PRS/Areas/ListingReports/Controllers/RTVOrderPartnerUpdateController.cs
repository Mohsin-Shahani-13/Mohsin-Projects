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
    public class RTVOrderPartnerUpdateController : Controller
    {
        // GET: ListingReports/RTVOrderPartnerUpdate
        RTVOrderPartnerUpdate oRTVOrderPartnerUpdate;
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oRTVOrderPartnerUpdate = new RTVOrderPartnerUpdate();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRTVOrderPartnerUpdate);
        }
        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRTVOrderPartnerUpdate = new RTVOrderPartnerUpdate();
            oRTVOrderPartnerUpdate.GetList();
            var jsonResult = Json(oRTVOrderPartnerUpdate, JsonRequestBehavior.AllowGet);
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