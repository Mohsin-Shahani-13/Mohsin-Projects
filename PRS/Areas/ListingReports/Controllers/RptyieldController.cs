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
    public class RptyieldController : Controller
    {
        // GET: ListingReports/Rptyield
        Rptyield oRptyield = new Rptyield();
        public ActionResult Option()
        {
            oRptyield = new Rptyield();
            return View(oRptyield);
        }


        public ActionResult Index(string RptCode, string menuTitle, bool ischecked)
        {
            oRptyield = new Rptyield();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ischecked = ischecked;
            return View(oRptyield);
        }

       
        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName, string partNo, string iteration, bool ischecked)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRptyield = new Rptyield();
            oRptyield.GetList(frmDt, toDate, programId, ProgramName, partNo, iteration, ischecked);
            var jsonResult = Json(oRptyield, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetDetail(string programId, string frmDate, string toDate, string workStationId, string workStation, string woNo, string partNo, string type, string iteration)
        {
            
            oRptyield = new Rptyield();
            ViewBag.ReportTitle = "Yield Detail > From = '" + frmDate + "' To = '" + toDate + "' ";

            if (!string.IsNullOrEmpty(partNo))
                ViewBag.ReportTitle += "| Part No. = '" + partNo + "' ";

            if (!string.IsNullOrEmpty(woNo))
                ViewBag.ReportTitle += "| WO No. = '" + woNo + "' ";

            if (!string.IsNullOrEmpty(workStation))
                ViewBag.ReportTitle += "| Work Station = '" + workStation + "' ";


            if (!string.IsNullOrEmpty(type))
                ViewBag.ReportTitle += "| Type = '" + type + "' ";

            if (!string.IsNullOrEmpty(type))
                ViewBag.ReportTitle += "| Iteration = '" + iteration + "' ";

            bool success = oRptyield.GetDetail(programId, frmDate, toDate, workStationId, workStation, woNo, partNo, type, iteration);
            oRptyield.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            if (success)
                return View(oRptyield);
            else
                return View();
        }
    }
}