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
    public class VFNWSHistoryController : Controller
    {
        VFNWSHistory oVFNWSHistory;
        // GET: ListingReports/VFNWSHistory
        public ActionResult Option()
        {
            oVFNWSHistory = new VFNWSHistory();
            return View(oVFNWSHistory);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oVFNWSHistory = new VFNWSHistory();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oVFNWSHistory);
        }

        public JsonResult GetList(string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oVFNWSHistory = new VFNWSHistory();
            oVFNWSHistory.GetList(frmDt, toDate);
            var jsonResult = Json(oVFNWSHistory, JsonRequestBehavior.AllowGet);
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