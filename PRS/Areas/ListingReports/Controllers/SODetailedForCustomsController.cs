using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.Areas.ListingReports.Models;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    public class SODetailedForCustomsController : Controller
    {
        SODetailedForCustoms oSODetailedForCustoms;
        // GET: ListingReports/SODetailedForCustoms
        public ActionResult Option()
        {
            oSODetailedForCustoms = new SODetailedForCustoms();
            return View(oSODetailedForCustoms);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oSODetailedForCustoms = new SODetailedForCustoms();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oSODetailedForCustoms);
        }
        public JsonResult GetList(string frmDt, string toDt, string SerialNo, string OrderNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSODetailedForCustoms = new SODetailedForCustoms();
            oSODetailedForCustoms.GetList(frmDt, toDt, SerialNo, OrderNo);
            var jsonResult = Json(oSODetailedForCustoms, JsonRequestBehavior.AllowGet);
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