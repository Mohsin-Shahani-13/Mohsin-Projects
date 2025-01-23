using IP.ActionFilters;
using IP.Areas.ListingReports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PCBABoxEnquiryController : Controller
    {
        // GET: ListingReports/PCBABoxEnquiry
        PCBABoxEnquiry oPCBABoxEnquiry;
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oPCBABoxEnquiry = new PCBABoxEnquiry();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oPCBABoxEnquiry);
        }
        public JsonResult GetList(string pcbaBoxId)
        {
            string menuTitle = string.Empty;
            string RptCode = string.Empty;

            oPCBABoxEnquiry = new PCBABoxEnquiry();
            oPCBABoxEnquiry.GetList(pcbaBoxId);
            var jsonResult = Json(oPCBABoxEnquiry, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU &LOG QUERY
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