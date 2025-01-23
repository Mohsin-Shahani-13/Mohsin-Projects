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
    public class FimerTesterResultsController : Controller
    {
        FimerTesterResults oFimerTesterResults = new FimerTesterResults();

        public ActionResult Option()
        {
            oFimerTesterResults = new FimerTesterResults();
            return View(oFimerTesterResults);
        }
        // GET: ListingReports/FimerTesterResults
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oFimerTesterResults = new FimerTesterResults();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oFimerTesterResults);
        }
        public JsonResult GetList(string serialNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oFimerTesterResults = new FimerTesterResults();
            oFimerTesterResults.GetList(serialNo);
            var jsonResult = Json(oFimerTesterResults, JsonRequestBehavior.AllowGet);
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