using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PartsUsageController : Controller
    {
        // GET: SupplyChain/PartsUsage
        PartsUsage oPartsUsage;
        public ActionResult Option()
        {
            oPartsUsage = new PartsUsage();
            DataTable dtProgram = oPartsUsage.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oPartsUsage);
        }
        public ActionResult Index(string RptCode, string menuTitle, string type)
        {
            oPartsUsage = new PartsUsage();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.type = type;
            return View(oPartsUsage);
        }
        public JsonResult GetList(string partNo, string fromDt, string toDt, string programId, string ProgramName, string type)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oPartsUsage = new PartsUsage();
            oPartsUsage.GetList(partNo, fromDt, toDt, programId, ProgramName, type);
            var jsonResult = Json(oPartsUsage, JsonRequestBehavior.AllowGet);
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