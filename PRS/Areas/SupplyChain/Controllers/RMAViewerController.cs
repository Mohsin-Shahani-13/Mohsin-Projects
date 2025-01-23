using IP.Areas.SupplyChain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.SupplyChain.Controllers
{
    public class RMAViewerController : Controller
    {
        RMAViewer oRMAViewer = new RMAViewer();
        // GET: SupplyChain/RMAViewer
        public ActionResult Option()
        {
            oRMAViewer = new RMAViewer();
            //DataTable dtProgram = oRMAViewer.GetProgramBySite();
            //ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programID", "ProgramName", "");
            return View(oRMAViewer);
        }
        public ActionResult Summary(string menuTitle, string rptCode)
        {
            oRMAViewer = new RMAViewer();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRMAViewer);
        }
        public ActionResult Detail(string menuTitle, string rptCode)
        {
            oRMAViewer = new RMAViewer();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;

            return View(oRMAViewer);
        }
        public JsonResult GetSummary(string frmDt, string toDt, string rptType, string RMANo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRMAViewer = new RMAViewer();
            oRMAViewer.GetSummary(frmDt, toDt, rptType, RMANo);
            var jsonResult = Json(oRMAViewer, JsonRequestBehavior.AllowGet);
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
        public JsonResult GetDetail(string frmDt, string toDt, string rptType, string RMANo, string waybill , string serialNo, string partNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRMAViewer = new RMAViewer();
            oRMAViewer.GetDetail(frmDt, toDt, rptType, RMANo, waybill, serialNo, partNo);
            var jsonResult = Json(oRMAViewer, JsonRequestBehavior.AllowGet);
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