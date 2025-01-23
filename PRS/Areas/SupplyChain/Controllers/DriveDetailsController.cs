using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using IP.Areas.SupplyChain.Models;
using System.Web.Mvc;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DriveDetailsController : Controller
    {
        // GET: SupplyChain/DriveDetails 
        DriveDetails oDriveDetails;
        public ActionResult Option()
        {
            oDriveDetails = new DriveDetails();
            DataTable dtProgram = oDriveDetails.GetProgramBySite();
            ViewBag.ddprogram = cCommon.ToDropDown(dtProgram, "programID", "programName", "");
            return View(oDriveDetails);
        }
        public ActionResult Index(string menuTitle, string rptCode)
        {
            oDriveDetails = new DriveDetails();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDriveDetails);
        }

        public JsonResult GetList(string frmDt, string toDt, bool isAllDate, string SerialNo, string custReference, string programId, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oDriveDetails = new DriveDetails();
            oDriveDetails.GetList(frmDt, toDt, isAllDate, programId, programName, SerialNo, custReference);
            var jsonResult = Json(oDriveDetails, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
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