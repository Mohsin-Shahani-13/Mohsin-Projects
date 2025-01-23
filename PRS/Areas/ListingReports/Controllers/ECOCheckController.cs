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
    public class ECOCheckController : Controller
    {
        // GET: ListingReports/ECOCheck
        ECOCheck oECOCheck = new ECOCheck();
        public ActionResult Option()
        {
            oECOCheck = new ECOCheck();
            return View();
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oECOCheck = new ECOCheck();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oECOCheck);
        }
        public JsonResult GetList(string partNo, string frmDt, bool isAllDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oECOCheck = new ECOCheck();
            oECOCheck.GetList(partNo, frmDt, isAllDate);
            var jsonResult = Json(oECOCheck, JsonRequestBehavior.AllowGet);
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