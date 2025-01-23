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
    public class DtaUploadController : Controller
    {
        //GET: ListingReports/DtaUpload
       DtaUpload oDtaUpload;
    public ActionResult Option()
        {
            oDtaUpload = new DtaUpload();
            DataTable dtProgram = oDtaUpload.Program();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramID", "ProgramName", "");
            return View(oDtaUpload);
        }

        public ActionResult Index(string RptCode, string menuTitle, string rptName)
        {
            oDtaUpload = new DtaUpload();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.rptName = rptName.Trim();

            return View(oDtaUpload);
        }

        public ActionResult DataUpload(string RptCode, string menuTitle, string rptName)
        {
            oDtaUpload = new DtaUpload();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.rptName = rptName;
            return View(oDtaUpload);
        }

        public ActionResult DataUploadSchedule(string RptCode, string menuTitle, string rptName)
        {
            oDtaUpload = new DtaUpload();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.rptName = rptName;
            return View(oDtaUpload);
        }

        public ActionResult DataUploadArchive(string RptCode, string menuTitle, string rptName)
        {
            oDtaUpload = new DtaUpload();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.rptName = rptName;
            return View(oDtaUpload);
        }

        public ActionResult DataUploadScheduleArchive(string RptCode, string menuTitle, string rptName)
        {
            oDtaUpload = new DtaUpload();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.rptName = rptName;
            return View(oDtaUpload);
        }

        public JsonResult GetList(string ProgramID, string ProgramName, string rptName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            

            oDtaUpload = new DtaUpload();
            oDtaUpload.GetList(ProgramID, ProgramName, rptName);
            var jsonResult = Json(oDtaUpload, JsonRequestBehavior.AllowGet);
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