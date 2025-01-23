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
  
    public class TATController : Controller
    {
        TAT oTAT = new TAT();
        // GET: SupplyChain/TAT
        public ActionResult Option()
        {
            oTAT = new TAT();
            DataTable dtProgram = oTAT.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "ProgramName", "");
            return View(oTAT);
        }
        public ActionResult OptionBose()
        {
            oTAT = new TAT();
            DataTable dtProgram = oTAT.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oTAT);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oTAT = new TAT();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oTAT);
        }
        public JsonResult GetList(string frmDt, string toDt, string ProgramId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oTAT = new TAT();
            oTAT.GetList(frmDt, toDt, ProgramId, ProgramName);
            var jsonResult = Json(oTAT, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            // LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog olog = new cLog();
                olog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
    }
}