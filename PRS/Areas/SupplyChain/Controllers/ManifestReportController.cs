using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class ManifestReportController : Controller
    {
        // GET: SupplyChain/ManifestReport
        ManifestReport oManifestReport = new ManifestReport();
        public ActionResult Option()
        {
            oManifestReport = new ManifestReport();
            return View(oManifestReport);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oManifestReport = new ManifestReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oManifestReport);
        }

        public JsonResult GetList(string programId, string ProgramName, string custRef, string manifest)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oManifestReport = new ManifestReport();
            oManifestReport.GetList(programId, ProgramName, custRef, manifest);
            var jsonResult = Json(oManifestReport, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetDetail(string programId, string manifest)
        {

            oManifestReport = new ManifestReport();
            ViewBag.ReportTitle = "Details ";

            if (!string.IsNullOrEmpty(manifest))
                ViewBag.ReportTitle += "> Menifest = '" + manifest + "' ";



            bool success = oManifestReport.GetDetail(programId, manifest);
            oManifestReport.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            if (success)
                return View(oManifestReport);
            else
                return View();
        }
    }
}