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
using System.IO;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class ReceiptAndCreditREFURBController : Controller
    {

        // GET: ListingReports/ReceiptAndCreditREFURB
        ReceiptAndCreditREFURB oReceiptAndCreditREFURB;
        public ActionResult Option()
        {
            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            DataTable dtProgram = oReceiptAndCreditREFURB.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oReceiptAndCreditREFURB);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oReceiptAndCreditREFURB);
        }

        public JsonResult GetRefurbRecvCredit(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            oReceiptAndCreditREFURB.GetRefurbRecvCrediting(programId, ProgramName, rptType, frmDt, toDate);
            var jsonResult = Json(oReceiptAndCreditREFURB, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult RefurbNewRepack(string RptCode, string menuTitle)
        {
            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oReceiptAndCreditREFURB);
        }
        public JsonResult GetRefurbNewRepack(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            oReceiptAndCreditREFURB.RefurbNewRepack(programId, ProgramName, rptType, frmDt, toDate);
            var jsonResult = Json(oReceiptAndCreditREFURB, JsonRequestBehavior.AllowGet);
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

        public ActionResult RefurbProcessFGI(string RptCode, string menuTitle)
        {
            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oReceiptAndCreditREFURB);
        }
        public JsonResult GetRefurbProcessFGI(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            oReceiptAndCreditREFURB.RefurbProcessFGI(programId, ProgramName, rptType, frmDt, toDate);
            var jsonResult = Json(oReceiptAndCreditREFURB, JsonRequestBehavior.AllowGet);
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

        public ActionResult RefurbRecvScrap(string RptCode, string menuTitle)
        {
            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oReceiptAndCreditREFURB);
        }
        public JsonResult GetRefurbRecvScrap(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            oReceiptAndCreditREFURB.RefurbRecvScrap(programId, ProgramName, rptType, frmDt, toDate);
            var jsonResult = Json(oReceiptAndCreditREFURB, JsonRequestBehavior.AllowGet);
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
        public ActionResult RepairRecvCredit(string RptCode, string menuTitle)
        {
            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oReceiptAndCreditREFURB);
        }
        public JsonResult GetRepairRecvCredit(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            oReceiptAndCreditREFURB.RepairRecvCredit(programId, ProgramName, rptType, frmDt, toDate);
            var jsonResult = Json(oReceiptAndCreditREFURB, JsonRequestBehavior.AllowGet);
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
        public ActionResult RepairProcessFGI(string RptCode, string menuTitle)
        {
            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oReceiptAndCreditREFURB);
        }
        public JsonResult GetRepairProcessFGI(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            oReceiptAndCreditREFURB.RepairProcessFGI(programId, ProgramName, rptType, frmDt, toDate);
            var jsonResult = Json(oReceiptAndCreditREFURB, JsonRequestBehavior.AllowGet);
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
        public ActionResult RepairRecvTested(string RptCode, string menuTitle)
        {
            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oReceiptAndCreditREFURB);
        }
        public JsonResult GetRepairRecvTested(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            oReceiptAndCreditREFURB.RepairRecvTested(programId, ProgramName, rptType, frmDt, toDate);
            var jsonResult = Json(oReceiptAndCreditREFURB, JsonRequestBehavior.AllowGet);
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
        public ActionResult RepairRecvPack(string RptCode, string menuTitle)
        {
            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oReceiptAndCreditREFURB);
        }
        public JsonResult GetRepairRecvPack(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();
            oReceiptAndCreditREFURB.RepairRecvPack(programId, ProgramName, rptType, frmDt, toDate);
            var jsonResult = Json(oReceiptAndCreditREFURB, JsonRequestBehavior.AllowGet);
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
        public ActionResult ExportDetailReports(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {
            try
            {
                // Initialize object if null
                if (oReceiptAndCreditREFURB == null)
                    oReceiptAndCreditREFURB = new ReceiptAndCreditREFURB();

                // Check if each list is null or empty and fetch data if needed
                if (oReceiptAndCreditREFURB.lstRefurbRecvCrediting == null || oReceiptAndCreditREFURB.lstRefurbRecvCrediting.Count == 0)
                {
                    // Fetch data for RefurbRecvCrediting if the list is null or empty
                    oReceiptAndCreditREFURB.GetRefurbRecvCrediting(programId, ProgramName, rptType, frmDt, toDate);
                }

                if (oReceiptAndCreditREFURB.lstRefurbNewRepack == null || oReceiptAndCreditREFURB.lstRefurbNewRepack.Count == 0)
                {
                    oReceiptAndCreditREFURB.RefurbNewRepack(programId, ProgramName, rptType, frmDt, toDate);
                }

                if (oReceiptAndCreditREFURB.lstRefurbProcessFGI == null || oReceiptAndCreditREFURB.lstRefurbProcessFGI.Count == 0)
                {
                    oReceiptAndCreditREFURB.RefurbProcessFGI(programId, ProgramName, rptType, frmDt, toDate);
                }

                if (oReceiptAndCreditREFURB.lstRefurbRecvScrap == null || oReceiptAndCreditREFURB.lstRefurbRecvScrap.Count == 0)
                {
                    oReceiptAndCreditREFURB.RefurbRecvScrap(programId, ProgramName, rptType, frmDt, toDate);
                }

                if (oReceiptAndCreditREFURB.lstRepairRecvCredit == null || oReceiptAndCreditREFURB.lstRepairRecvCredit.Count == 0)
                {
                    oReceiptAndCreditREFURB.RepairRecvCredit(programId, ProgramName, rptType, frmDt, toDate);
                }

                if (oReceiptAndCreditREFURB.lstRepairProcessFGI == null || oReceiptAndCreditREFURB.lstRepairProcessFGI.Count == 0)
                {
                    oReceiptAndCreditREFURB.RepairProcessFGI(programId, ProgramName, rptType, frmDt, toDate);
                }

                if (oReceiptAndCreditREFURB.lstRepairRecvTested == null || oReceiptAndCreditREFURB.lstRepairRecvTested.Count == 0)
                {
                    oReceiptAndCreditREFURB.RepairRecvTested(programId, ProgramName, rptType, frmDt, toDate);
                }

                if (oReceiptAndCreditREFURB.lstRepairRecvPack == null || oReceiptAndCreditREFURB.lstRepairRecvPack.Count == 0)
                {
                    oReceiptAndCreditREFURB.RepairRecvPack(programId, ProgramName, rptType, frmDt, toDate);
                }


                string fileName = "Pricing Report.xlsx";
                var excelGenerator = new ExcelReportGenerator();

                var stream = new MemoryStream();  

                excelGenerator.GenerateExcelFromModel(oReceiptAndCreditREFURB, stream); 
                stream.Position = 0;

                byte[] fileBytes = stream.ToArray();

                string base64File = Convert.ToBase64String(fileBytes);

                return Json(new { fileName = fileName, fileContent = base64File }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while generating the excel file." }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}