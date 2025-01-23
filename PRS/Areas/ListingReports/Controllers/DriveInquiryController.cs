using IP.ActionFilters;
using IP.Areas.ListingReports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DriveInquiryController : Controller
    {
        DriveInquiry odriveInquiry;
        // GET: ListingReports/DriveInquiry
        public ActionResult Option()
        {
            odriveInquiry = new DriveInquiry();
           // DataTable dtProgram = odriveInquiry.GetProgramBySite();
           // ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "ProgramName", "");
            return View(odriveInquiry);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            odriveInquiry = new DriveInquiry();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(odriveInquiry);
        }
      
        public JsonResult GetList(string pcbaValues, string serialValues)
        {
            string menuTitle = string.Empty;
            string RptCode = string.Empty;

            odriveInquiry = new DriveInquiry();
            odriveInquiry.GetList(pcbaValues,serialValues);
            var jsonResult = Json(odriveInquiry, JsonRequestBehavior.AllowGet);
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