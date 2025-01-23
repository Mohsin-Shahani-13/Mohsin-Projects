using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    public class CopyPartInquiryController : Controller
    {
        CopyPartInquiry oCopyPartInquiry = new CopyPartInquiry();
        // GET: ListingReports/CopyPartInquiry
        public ActionResult Option()
        {
            oCopyPartInquiry = new CopyPartInquiry();
            return View(oCopyPartInquiry);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oCopyPartInquiry = new CopyPartInquiry();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oCopyPartInquiry);
        }
        public JsonResult GetList(string partNo)
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

            oCopyPartInquiry = new CopyPartInquiry();
            oCopyPartInquiry.GetList(partNo);
            var jsonResult = Json(oCopyPartInquiry, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}