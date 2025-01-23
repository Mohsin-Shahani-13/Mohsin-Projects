using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.ActionFilters;
using IP.Areas.Meta.Models;

namespace IP.Areas.Meta.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DriveDetailController : Controller
    {
        DriveDetail oDriveDetail;
        // GET: Meta/DriveDetail
        public ActionResult Option()
        {
            oDriveDetail = new DriveDetail();
            DataTable dtProgram = oDriveDetail.GetProgramBySite();
            ViewBag.ddprogram = cCommon.ToDropDown(dtProgram, "programID", "programName", "");
            return View(oDriveDetail);
        }

        public ActionResult Index(string menuTitle, string rptCode)
        {
            oDriveDetail = new DriveDetail();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDriveDetail); 
        }

        public JsonResult GetList(string recFDate, string recTDate, string dockLogFrmDt, string dockLogToDt,bool recAllDate,bool dLogAllDate, string serialNo, string ProgramID, string ProgramName, string referenceNo, string RfcNo, string boxID, string currentInventoryLocation)
        {
            string menuTitle = string.Empty;
            string RptCode;
            
            oDriveDetail = new DriveDetail();
            oDriveDetail.GetList(recFDate, recTDate, dockLogFrmDt, dockLogToDt, recAllDate, dLogAllDate, serialNo, ProgramID, ProgramName, referenceNo, RfcNo, boxID, currentInventoryLocation);
            var jsonResult = Json(oDriveDetail, JsonRequestBehavior.AllowGet);
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