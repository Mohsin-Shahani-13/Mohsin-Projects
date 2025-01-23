using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PartsTransactionController : Controller
    {
        // GET: ListingReports/PartsTransaction
        PartsTransaction oPartsTransaction = new PartsTransaction();

        public ActionResult Option()
        {
            oPartsTransaction = new PartsTransaction();
            DataTable dtProgram = oPartsTransaction.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            //return View(oPartsTransaction);

            bool success = oPartsTransaction.GetTransType();
            if (success)
                return View(oPartsTransaction);
            else
                return View();
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPartsTransaction = new PartsTransaction();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.Program = Request.QueryString["ProgramName"];
            return View(oPartsTransaction);
        }
        public JsonResult GetList(string frmDt, string toDate, bool isAllDate, string serialNo, string partNo, string Location, string ToLocation, string programId,string ProgramName, string transTypeID, string transType)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oPartsTransaction = new PartsTransaction();
            oPartsTransaction.GetList(frmDt, toDate, isAllDate, serialNo, partNo, Location, ToLocation, programId, ProgramName, transTypeID, transType);
            var jsonResult = Json(oPartsTransaction, JsonRequestBehavior.AllowGet);
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