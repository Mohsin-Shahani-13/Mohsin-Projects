using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class LinkedTrackersRMAController : Controller
    {
        // GET: SupplyChain/LinkedTrackersRMA
        LinkedTrackersRMA oLinkedTrackersRMA;
        public ActionResult Option()
        {
            oLinkedTrackersRMA = new LinkedTrackersRMA();
            DataTable dtProgram = oLinkedTrackersRMA.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oLinkedTrackersRMA);
        }
        public ActionResult Index(string menuTitle, string rptCode)
        {
            oLinkedTrackersRMA = new LinkedTrackersRMA();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oLinkedTrackersRMA);
        }
        public JsonResult GetList(string programID, string programName, string DISPOSITION, string partNo)
        {
            oLinkedTrackersRMA = new LinkedTrackersRMA();
            string menuTitle = string.Empty;
            string RptCode = string.Empty;


            oLinkedTrackersRMA.GetList(programID, programName, DISPOSITION, partNo);
            var jsonResult = Json(oLinkedTrackersRMA, JsonRequestBehavior.AllowGet);
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