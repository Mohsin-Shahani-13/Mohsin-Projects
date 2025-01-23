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
    public class VFNVerifoneModelOutputController : Controller
    {
        VFNVerifoneModelOutput oVFNVerifoneModelOutput;
        // GET: ListingReports/VFNVerifoneModelOutput
        public ActionResult Option()
        {
            oVFNVerifoneModelOutput = new VFNVerifoneModelOutput();
            return View(oVFNVerifoneModelOutput);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oVFNVerifoneModelOutput = new VFNVerifoneModelOutput();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oVFNVerifoneModelOutput);
        }

        public JsonResult GetList(string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oVFNVerifoneModelOutput = new VFNVerifoneModelOutput();
            oVFNVerifoneModelOutput.GetList(frmDt, toDate);
            var jsonResult = Json(oVFNVerifoneModelOutput, JsonRequestBehavior.AllowGet);
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