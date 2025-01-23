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
    public class AlcatelShippingReportController : Controller
    {
        AlcatelShippingReport oAlcatelShippingReport = new AlcatelShippingReport();
        // GET: SupplyChain/AlcatelShippingReport
        public ActionResult Option()
        {
            oAlcatelShippingReport = new AlcatelShippingReport();
            DataTable dtProgram = oAlcatelShippingReport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "ProgramName", "");
            return View(oAlcatelShippingReport);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oAlcatelShippingReport = new AlcatelShippingReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oAlcatelShippingReport);
        }

        public JsonResult GetList(string frmDt, string toDt, string ProgramId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oAlcatelShippingReport = new AlcatelShippingReport();
            oAlcatelShippingReport.GetList(frmDt, toDt, ProgramId, ProgramName);
            var jsonResult = Json(oAlcatelShippingReport, JsonRequestBehavior.AllowGet);
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