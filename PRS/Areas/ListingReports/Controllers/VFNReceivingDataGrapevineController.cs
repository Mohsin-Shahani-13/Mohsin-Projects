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
    public class VFNReceivingDataGrapevineController : Controller
    {
        VFNReceivingDataGrapevine oVFNReceivingDataGrapevine;
        // GET: ListingReports/VFNReceivingDataGrapevine
        public ActionResult Option()
        {
            oVFNReceivingDataGrapevine = new VFNReceivingDataGrapevine();
            return View(oVFNReceivingDataGrapevine);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oVFNReceivingDataGrapevine = new VFNReceivingDataGrapevine();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oVFNReceivingDataGrapevine);
        }

        public JsonResult GetList(string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oVFNReceivingDataGrapevine = new VFNReceivingDataGrapevine();
            oVFNReceivingDataGrapevine.GetList(frmDt, toDate);
            var jsonResult = Json(oVFNReceivingDataGrapevine, JsonRequestBehavior.AllowGet);
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