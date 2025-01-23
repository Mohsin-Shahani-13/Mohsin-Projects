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
    public class EMCOnSiteDetailEnquiryController : Controller
    {
        EMCOnSiteDetailEnquiry oEMCOnSiteDetailEnquiry;
        // GET: ListingReports/EMCOnSiteDetailEnquiry
        public ActionResult Option()
        {
            oEMCOnSiteDetailEnquiry = new EMCOnSiteDetailEnquiry();
            oEMCOnSiteDetailEnquiry.fromDt = null;
            return View(oEMCOnSiteDetailEnquiry);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oEMCOnSiteDetailEnquiry = new EMCOnSiteDetailEnquiry();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oEMCOnSiteDetailEnquiry);
        }
        public JsonResult GetList(string programId, string site, string frmDt, string toDate, string serialNo, string BoxNo, string palletNo, string PCBASerial, string PCBABox, string PCBAPallet, string documentNo, string searchByDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oEMCOnSiteDetailEnquiry = new EMCOnSiteDetailEnquiry();
            oEMCOnSiteDetailEnquiry.GetList(programId, site, frmDt, toDate, serialNo, BoxNo, palletNo, PCBASerial, PCBABox, PCBAPallet, documentNo, searchByDate);
            var jsonResult = Json(oEMCOnSiteDetailEnquiry, JsonRequestBehavior.AllowGet);
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