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
    public class SummaryRR60Controller : Controller
    {
        // GET: SupplyChain/SummaryRR60
        SummaryRR60 oSummaryRR60 = new SummaryRR60();
        public ActionResult Option()
        {
            DataTable dtProgram = oSummaryRR60.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            //DataTable dtyear = oSummaryRR60.GetYear();
            //ViewBag.ddyear = cCommon.ToDropDown(dtyear, "RRYear", "RRYear", "");
            oSummaryRR60 = new SummaryRR60();
            return View();
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oSummaryRR60 = new SummaryRR60();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }

        public JsonResult GetList(string year, string programId, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oSummaryRR60 = new SummaryRR60();
            oSummaryRR60.GetList(year, programId, programName);
            var jsonResult = Json(oSummaryRR60, JsonRequestBehavior.AllowGet);
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