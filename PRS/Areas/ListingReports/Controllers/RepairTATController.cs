using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.Areas.ListingReports.Models;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    public class RepairTATController : Controller
    {
        // GET: ListingReports/RepairTAT
        RepairTAT oRepairTAT;
        public ActionResult Option()
        {
            oRepairTAT = new RepairTAT();
            DataTable dtProgram = oRepairTAT.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oRepairTAT);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oRepairTAT = new RepairTAT();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRepairTAT);
        }
        public JsonResult GetList(string programId, string ProgramName, string frmDt, string toDt)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRepairTAT = new RepairTAT();
            oRepairTAT.GetList(programId, ProgramName, frmDt, toDt);
            var jsonResult = Json(oRepairTAT, JsonRequestBehavior.AllowGet);
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