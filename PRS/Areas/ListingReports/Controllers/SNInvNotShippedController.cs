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
    public class SNInvNotShippedController : Controller
    {
        SNInvNotShipped oSNInvNotShipped = new SNInvNotShipped();
        // GET: ListingReports/SNInvNotShipped

        public ActionResult Option()
        {
            bool success = oSNInvNotShipped.GetStatus();

            
            if (success)
                return View(oSNInvNotShipped);
            else
                return View();
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oSNInvNotShipped = new SNInvNotShipped();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oSNInvNotShipped);
        }

        public JsonResult GetList(string programId, string ProgramName, string status, string statusId)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSNInvNotShipped = new SNInvNotShipped();
            oSNInvNotShipped.GetList(programId, ProgramName, status, statusId);
            var jsonResult = Json(oSNInvNotShipped, JsonRequestBehavior.AllowGet);
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