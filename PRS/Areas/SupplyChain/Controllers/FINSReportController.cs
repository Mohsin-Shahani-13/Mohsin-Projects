using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.Areas.SupplyChain.Models;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    public class FINSReportController : Controller
    {
        FINSReport FINSReport;
        // GET: SupplyChain/FINSReport
        public ActionResult Option()
        {
            FINSReport = new FINSReport();
            return View(FINSReport);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            FINSReport = new FINSReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(FINSReport);
        }
        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName, string SerialNo, string PartNo, string CustRef)
        {
            string menuTitle = string.Empty;
            string RptCode;
            FINSReport = new FINSReport();
            FINSReport.GetList(frmDt, toDate, programId, ProgramName, SerialNo, PartNo, CustRef);
            var jsonResult = Json(FINSReport, JsonRequestBehavior.AllowGet);
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