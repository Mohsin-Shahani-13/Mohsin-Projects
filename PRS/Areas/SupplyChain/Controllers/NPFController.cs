using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class NPFController : Controller
    {
        // GET: SupplyChain/NPF
        NPF oNPF;
        public ActionResult Option()
        {
            oNPF = new NPF();
            DataTable dtProgram = oNPF.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oNPF);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oNPF = new NPF();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oNPF);
        }
        public JsonResult GetList(string frmDt, string toDt, string partNo, string serialNo, string programId, string ProgramName, string orderno)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oNPF = new NPF();
            oNPF.GetList(frmDt, toDt, partNo, serialNo, programId, ProgramName, orderno);
            var jsonResult = Json(oNPF, JsonRequestBehavior.AllowGet);
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