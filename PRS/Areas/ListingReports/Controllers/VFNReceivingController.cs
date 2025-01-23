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
    public class VFNReceivingController : Controller
    {
        VFNReceiving oVFNReceiving;
        // GET: ListingReports/VFNReceiving
        public ActionResult Option()
        {
            oVFNReceiving = new VFNReceiving();
            return View(oVFNReceiving);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oVFNReceiving = new VFNReceiving();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oVFNReceiving);
        }

        public JsonResult GetList(string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oVFNReceiving = new VFNReceiving();
            oVFNReceiving.GetList(frmDt, toDate);
            var jsonResult = Json(oVFNReceiving, JsonRequestBehavior.AllowGet);
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