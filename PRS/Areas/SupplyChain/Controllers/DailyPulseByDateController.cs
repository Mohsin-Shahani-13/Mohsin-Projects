using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;

namespace IP.Areas.SupplyChain.Controllers
{
    public class DailyPulseByDateController : Controller
    {
        DailyPulseByDate oDailyPulseByDate;
        // GET: SupplyChain/DailyPulseByDate
        public ActionResult Option()
        {
            oDailyPulseByDate = new DailyPulseByDate();
            return View(oDailyPulseByDate);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDailyPulseByDate = new DailyPulseByDate();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDailyPulseByDate);
        }

        public JsonResult GetList(string frmDt, string toDt)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oDailyPulseByDate = new DailyPulseByDate();
            oDailyPulseByDate.GetList(frmDt, toDt);
            var jsonResult = Json(oDailyPulseByDate, JsonRequestBehavior.AllowGet);
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