using IP.Areas.ListingReports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.ListingReports.Controllers
{
    public class BlacklistController : Controller
    {
        Blacklist oBlacklist;
        // GET: ListingReports/Blacklist
        public ActionResult Option()
        {
            oBlacklist = new Blacklist();
            DataTable dtProgram = oBlacklist.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oBlacklist);
        }
        public ActionResult Index(string menuTitle, string rptCode)
        {
            oBlacklist = new Blacklist();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oBlacklist);
        }
        public JsonResult GetList(string programId, string programName, string fDate, string tDate, string user, string reason, string attribute, string attributeValue)
        {
            oBlacklist = new Blacklist();
            string menuTitle = string.Empty;
            string RptCode = string.Empty;


            oBlacklist.GetList(programId, programName, fDate, tDate, user, reason, attribute, attributeValue);
            var jsonResult = Json(oBlacklist, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"].ToString();
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
    }
}