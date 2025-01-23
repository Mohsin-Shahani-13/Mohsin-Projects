using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.SupplyChain.Models;
using System.Data;
using System.Web.Script.Serialization;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PartTransactionSummaryController : Controller
    {
        // GET: SupplyChain/PartTransactionSummary
        PartTransactionSummary oPartTransaction = new PartTransactionSummary();
        public ActionResult Option()
        {
            oPartTransaction = new PartTransactionSummary();
            DataTable dtProgram = oPartTransaction.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oPartTransaction);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPartTransaction = new PartTransactionSummary();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oPartTransaction);

        }
        public JsonResult GetList(string programId, string partNo, string frmDate, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oPartTransaction = new PartTransactionSummary();
            oPartTransaction.GetList(programId, partNo, frmDate, toDate);
            var jsonResult = Json(oPartTransaction, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = Int32.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return (jsonResult);
        }
        public ActionResult Detail(string programId, string partNo, string frmDate, string toDate)
        {
            oPartTransaction = new PartTransactionSummary();
            string desc = oPartTransaction.GetPartDesc(programId, partNo);
            ViewBag.ReportTitle += " Program = '" + programId + "' | From = '" + frmDate + "' To = '" + toDate + "' | Part No. = '" + partNo + "' ";
            if (!string.IsNullOrEmpty(desc))
            {
                ViewBag.ReportTitle += "| Part Desc. = '" + desc + "' ";
            }

            if (string.IsNullOrEmpty(partNo))
                return View();
            try
            {
                oPartTransaction = new PartTransactionSummary();
                bool success = oPartTransaction.GetDetail(programId, partNo, frmDate, toDate);
                oPartTransaction.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                if (success)
                    return View(oPartTransaction);
                else
                    return View();
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }

        }
    }
}