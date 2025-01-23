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
    public class PartsBackLogController : Controller
    {
        // GET: SupplyChain/PartsBackLog
        PartsBackLog oPartsBackLog = new PartsBackLog();
        public ActionResult Option()
        {
            PartsBackLog oPartsBackLog = new PartsBackLog();
            DataTable dtProgram = oPartsBackLog.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programID", "ProgramName", "");
            return View(oPartsBackLog);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPartsBackLog = new PartsBackLog();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;

            return View(oPartsBackLog);

        }
        public JsonResult GetList(string PalletBoxNo, string LocationNo, string PartNo, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oPartsBackLog = new PartsBackLog();
            oPartsBackLog.GetList( PalletBoxNo, LocationNo, PartNo, programId, ProgramName);
            var jsonResult = Json(oPartsBackLog, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetDetail(string rptCode, string menuTitle)
        {
            oPartsBackLog = new PartsBackLog();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;


            return View(oPartsBackLog);
        }
        public JsonResult GetDtl(string PalletBoxNo, string LocationNo, string PartNo, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            

            oPartsBackLog = new PartsBackLog();
            oPartsBackLog.GetDetail(PalletBoxNo, LocationNo, PartNo, programId, ProgramName);
            var jsonResult = Json(oPartsBackLog, JsonRequestBehavior.AllowGet);
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