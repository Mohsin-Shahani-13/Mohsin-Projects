using IP.ActionFilters;
using IP.Areas.ListingReports.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PLUSGenericInformationController : Controller
    {
        PLUSGenericInformation oPLUSGenericInformation;
        // GET: ListingReports/PLUSGenericInformation
        public ActionResult RedirectToNewTab(string menuTitle)
        {
            ViewBag.UrlToOpen = Url.Action("Index", "PLUSGenericInformation", new { menuTitle });
            return View();
        }
        public ActionResult Index(string menuTitle)
        {
            oPLUSGenericInformation = new PLUSGenericInformation();
            ViewBag.ReportTitle = menuTitle;
            return View(oPLUSGenericInformation);
        }


        public JsonResult GetReport()
        {
            oPLUSGenericInformation = new PLUSGenericInformation();
            oPLUSGenericInformation.GetReportsList();
            var jsonResult = Json(oPLUSGenericInformation, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Option(string rptName, int rptId, string RptInputField, string RptLabel, string RptCtrlType, bool isDetail = false, bool RptHasProgramId = false)
        {
            oPLUSGenericInformation = new PLUSGenericInformation();
            //oPLUSGenericInformation._lstProgram = oPLUSGenericInformation.GetContract();
            //oPLUSGenericInformation._lstProgram.Replace("'","0");
            //oPLUSGenericInformation.GetCustomList(tblName);
            DataTable dtProgram = oPLUSGenericInformation.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programID", "programName", "");
            DataTable dtStatus = oPLUSGenericInformation.GetStatus();
            ViewBag.ddStatus = cCommon.ToDropDown(dtStatus, "Id", "Description", "");
            //DataTable dtStatusForCodeGenericView = oPLUSGenericInformation.StatusForCodeGenericView();
            //ViewBag.ddStatusForCodeGenericView = cCommon.ToDropDown(dtStatusForCodeGenericView, "ID", "Description", "All");
            DataTable dtProcess = oPLUSGenericInformation.GetProcessList();
            ViewBag.ddProcess = cCommon.ToDropDown(dtProcess, "Process", "Process", "");

            //// Get the initially selected program ID (e.g., the first one in the program list)
            //var ProgramId = dtProgram.Rows.Count > 0 ? Convert.ToInt32(dtProgram.Rows[0]["programID"]) : 0;
            //DataTable dtWarehouseForPartSerialView = oPLUSGenericInformation.WarehouseForPartSerialView(ProgramId);
            //ViewBag.ddWarehouseForPartSerialView = cCommon.ToDropDown(dtWarehouseForPartSerialView, "Warehouse", "Warehouse", "All");

            //oPLUSGenericInformation.CtrlType += ViewCtrlType;
            oPLUSGenericInformation.CtrlType = string.IsNullOrEmpty(oPLUSGenericInformation.CtrlType)
            ? RptCtrlType
            : $"{oPLUSGenericInformation.CtrlType},{RptCtrlType}";
            oPLUSGenericInformation.Parameter = RptLabel;
            oPLUSGenericInformation.RptId = rptId;
            oPLUSGenericInformation.HaveProgramId = RptHasProgramId;
            oPLUSGenericInformation.isDetail = isDetail;
            ViewBag.ReportTitle = Request.QueryString["rptName"];
            ViewBag.rptId = Request.QueryString["rptId"];
            ViewBag.RptLabel = RptLabel;
            return View(oPLUSGenericInformation);
        }
        public ActionResult Detail(string[] param, string contract, string programName, string frmDt, string toDate, string rptName, string rptId, bool RptHasProgramId, string statusId, string status, string rptType, string CGStatusId, string CGStatus, string ddProcess, string CGTableName, string PSWarehouse, string DDLDataFeed, bool RptHasOptionPage = false, bool isAllDate = false)
        {
            oPLUSGenericInformation = new PLUSGenericInformation();
            bool success = false;
            ViewBag.Param = param;
            oPLUSGenericInformation.HaveProgramId = RptHasProgramId;
            ViewBag.hasoptionPage = RptHasProgramId;
            if (RptHasOptionPage != true)
            {
                ViewBag.ReportTitle = rptName;
                ViewBag.rptId = rptId;
                success = oPLUSGenericInformation.GetDetail(rptName, rptId, RptHasProgramId);
                oPLUSGenericInformation.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                cLog oLog = new cLog();
                oLog.SaveLog(rptName, Request.Url.PathAndQuery, rptId);
            }
            else
            {
                //oPLUSGenericInformation = new PLUSGenericInformation();
                rptName = oPLUSGenericInformation.GetReportName(rptId);
                ViewBag.ReportTitle = rptName;
                ViewBag.rptId = rptId;
                success = oPLUSGenericInformation.GetDetailParam(param, contract, programName, isAllDate, frmDt, toDate, rptName, rptId, RptHasProgramId, statusId, status, rptType, CGStatusId, CGStatus, ddProcess, CGTableName, PSWarehouse, DDLDataFeed);
                oPLUSGenericInformation.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                cLog oLog = new cLog();
                oLog.SaveLog(rptName, Request.Url.PathAndQuery, rptId);

            }
            if (success)
                return View(oPLUSGenericInformation);
            else
                return View(oPLUSGenericInformation);
        }
        public ActionResult DetailReports(string[] param, string contract, string programName, string frmDt, string toDate, string rptName, string rptId, bool RptHasProgramId, string statusId, string status, string rptType, string CGStatusId, string CGStatus, string ddProcess, string CGTableName, string PSWarehouse, string DDLDataFeed, bool RptHasOptionPage = false, bool isAllDate = false)
        {
            //if (string.IsNullOrEmpty(serialNo))
            //    return View();
            try
            {
                oPLUSGenericInformation = new PLUSGenericInformation();
                ViewBag.ReportTitle = rptName;
                ViewBag.rptId = rptId;
                bool success = oPLUSGenericInformation.GetDetailReport(param, contract, programName, isAllDate, frmDt, toDate, rptName, rptId, RptHasProgramId, statusId, status, rptType, CGStatusId, CGStatus, ddProcess, CGTableName, PSWarehouse, DDLDataFeed);
                oPLUSGenericInformation.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                if (success)
                    return View(oPLUSGenericInformation);
                else
                    return View(oPLUSGenericInformation);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }

            return View();
        }
        public ActionResult DownloadExcel(string rptId, string rptName, string[] param, string contract, string programName, string frmDt, string toDate, bool RptHasProgramId, string statusId, string status, string rptType, string CGStatusId, string CGStatus, string ddProcess, string CGTableName, string PSWarehouse, string DDLDataFeed, bool RptHasOptionPage = false, bool isAllDate = false)
        {
            oPLUSGenericInformation = new PLUSGenericInformation();
            bool success = false;

            if (RptHasOptionPage != true)
            {
                ViewBag.ReportTitle = rptName;
                ViewBag.rptId = rptId;
                success = oPLUSGenericInformation.GetDetail(rptName, rptId, RptHasProgramId);
                oPLUSGenericInformation.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                cLog oLog = new cLog();
                oLog.SaveLog(rptName, Request.Url.PathAndQuery, rptId);
            }
            else
            {
                //oPLUSGenericInformation = new PLUSGenericInformation();
                rptName = oPLUSGenericInformation.GetReportName(rptId);
                ViewBag.ReportTitle = rptName;
                ViewBag.rptId = rptId;
                success = oPLUSGenericInformation.GetDetailParam(param, contract, programName, isAllDate, frmDt, toDate, rptName, rptId, RptHasProgramId, statusId, status, rptType, CGStatusId, CGStatus, ddProcess, CGTableName, PSWarehouse, DDLDataFeed);
                oPLUSGenericInformation.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                cLog oLog = new cLog();
                oLog.SaveLog(rptName, Request.Url.PathAndQuery, rptId);

            }
            DataTable dt = oPLUSGenericInformation.dt; // Generate or retrieve your DataTable
            var filterString = rptName + " " + oPLUSGenericInformation.filterString;
            byte[] compressedCsv = WriteCsvWithZipCompression(dt, filterString, rptName);
            return File(compressedCsv, "application/zip", "" + rptName + ".zip");

        }
        static byte[] WriteCsvWithZipCompression(DataTable dataTable, string filterString, string rptName)
        {
            // Exception handling with try-catch
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Create a ZIP archive in memory
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        // Create a new entry for the CSV file in the ZIP archive
                        var zipEntry = zipArchive.CreateEntry("" + rptName + ".csv", System.IO.Compression.CompressionLevel.Optimal);

                        using (var zipStream = zipEntry.Open())
                        {
                            using (var writer = new StreamWriter(zipStream))
                            {
                                // Write filter string as the first row
                                writer.WriteLine($"\"{filterString}\"");

                                // Writing column headers
                                for (int i = 0; i < dataTable.Columns.Count; i++)
                                {
                                    writer.Write($"\"{dataTable.Columns[i].ColumnName}\"");
                                    if (i < dataTable.Columns.Count - 1)
                                    {
                                        writer.Write(",");
                                    }
                                }
                                writer.WriteLine();

                                // Writing data rows
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    for (int i = 0; i < dataTable.Columns.Count; i++)
                                    {
                                        writer.Write($"\"{row[i]}\"");
                                        if (i < dataTable.Columns.Count - 1)
                                        {
                                            writer.Write(",");
                                        }
                                    }
                                    writer.WriteLine();
                                }
                            }
                        }
                    }

                    // Return the compressed ZIP file as a byte array
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while generating ZIP file: {ex.Message}", ex);
            }
        }


    }
}