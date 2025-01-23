using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.ActionFilters;
using IP.Areas.ListingReports.Models;

namespace IP.Areas.ListingReports.Controllers
{
    public class ReverseController : Controller
    {
        Reverse oReverse;
        // GET: ListingReports/Reverse
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oReverse = new Reverse();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }

        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oReverse = new Reverse();
            oReverse.GetList();
            var jsonResult = Json(oReverse, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }

    }
}