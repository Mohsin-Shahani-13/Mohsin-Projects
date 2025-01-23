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
    public class SODetailedDataController : Controller
    {
        SODetailedData oSODetailedData;
        // GET: ListingReports/SODetailedData
        public ActionResult Option()
        {
            oSODetailedData = new SODetailedData();
            return View(oSODetailedData);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oSODetailedData = new SODetailedData();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oSODetailedData);
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName, string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSODetailedData = new SODetailedData();
            oSODetailedData.GetList(frmDt, toDate, programId, ProgramName, custRef);
            var jsonResult = Json(oSODetailedData, JsonRequestBehavior.AllowGet);
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