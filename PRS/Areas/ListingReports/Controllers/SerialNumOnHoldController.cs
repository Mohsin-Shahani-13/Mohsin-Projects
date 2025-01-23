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
    public class SerialNumOnHoldController : Controller
    {
        SerialNumOnHold oserialNumOnHold = new SerialNumOnHold();
        // GET: ListingReports/SerialNumOnHold
        public ActionResult Option()
        {
            SerialNumOnHold oserialNumOnHold = new SerialNumOnHold();
            return View(oserialNumOnHold);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            SerialNumOnHold oserialNumOnHold = new SerialNumOnHold();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oserialNumOnHold);
        }

        public JsonResult GetList(string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;

            SerialNumOnHold oserialNumOnHold = new SerialNumOnHold();
            oserialNumOnHold.GetList(programId, ProgramName);
            var jsonResult = Json(oserialNumOnHold, JsonRequestBehavior.AllowGet);
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