using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class RMADiscrepancyController : Controller
    {
        // GET: SupplyChain/RMADiscrepancy
        RMADiscrepancy oRMADiscrepancy;
        public ActionResult Option()
        {
            oRMADiscrepancy = new RMADiscrepancy();
            DataTable dtProgram = oRMADiscrepancy.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programID", "ProgramName", "");
            return View(oRMADiscrepancy);
        }
        public ActionResult Summary(string menuTitle, string rptCode)
        {
            oRMADiscrepancy = new RMADiscrepancy();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
          
            return View(oRMADiscrepancy);
        }
        public ActionResult Detail(string menuTitle, string rptCode)
        {
            oRMADiscrepancy = new RMADiscrepancy();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;

            return View(oRMADiscrepancy);
        }
        public JsonResult GetList(string CreateFDate, string CreateTDate, bool isAllDate, string serialNo, string ProgramID, string ProgramName, string custRefer, string boxNo, string rptName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRMADiscrepancy = new RMADiscrepancy();
            oRMADiscrepancy.GetList(CreateFDate, CreateTDate, isAllDate, serialNo, ProgramID, ProgramName, custRefer, boxNo, rptName);
            var jsonResult = Json(oRMADiscrepancy, JsonRequestBehavior.AllowGet);
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
