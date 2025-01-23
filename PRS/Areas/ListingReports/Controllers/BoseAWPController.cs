using IP.Areas.ListingReports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
namespace IP.Areas.ListingReports.Controllers
{
    public class BoseAWPController : Controller
    {
        // GET: ListingReports/BoseAWP
        BoseAWP oBoseAWP = new BoseAWP();
       

        public ActionResult Index(string RptCode, string menuTitle)
        {
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oBoseAWP);
        }

        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oBoseAWP = new BoseAWP();
            oBoseAWP.GetList();
            var jsonResult = Json(oBoseAWP, JsonRequestBehavior.AllowGet);
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