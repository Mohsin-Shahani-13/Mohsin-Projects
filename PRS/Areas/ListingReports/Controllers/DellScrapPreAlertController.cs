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
    public class DellScrapPreAlertController : Controller
    {
        // GET: ListingReports/DellScrapPreAlert
        DellScrapPreAlert oDellScrapPreAlert;
        public ActionResult Option()
        {
            oDellScrapPreAlert = new DellScrapPreAlert();
            return View(oDellScrapPreAlert);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDellScrapPreAlert = new DellScrapPreAlert();
            ViewBag.ReportTitle = menuTitle;
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.RptCode = RptCode;
            return View(oDellScrapPreAlert);
        }

        public JsonResult GetList(string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oDellScrapPreAlert = new DellScrapPreAlert();
            oDellScrapPreAlert.GetList(custRef);
            var jsonResult = Json(oDellScrapPreAlert, JsonRequestBehavior.AllowGet);
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