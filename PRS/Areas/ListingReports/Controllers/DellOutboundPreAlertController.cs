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
    public class DellOutboundPreAlertController : Controller
    {
        // GET: ListingReports/DellOutboundPreAlert
        DellOutboundPreAlert oDellOutboundPreAlert;
        public ActionResult Option()
        {
            oDellOutboundPreAlert = new DellOutboundPreAlert();
            return View(oDellOutboundPreAlert);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDellOutboundPreAlert = new DellOutboundPreAlert();
            ViewBag.ReportTitle = menuTitle;
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.RptCode = RptCode;
            return View(oDellOutboundPreAlert);
        }

        public JsonResult GetList(string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oDellOutboundPreAlert = new DellOutboundPreAlert();
            oDellOutboundPreAlert.GetList(custRef);
            var jsonResult = Json(oDellOutboundPreAlert, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
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