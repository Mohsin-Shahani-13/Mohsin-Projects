using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web;
using System.IO;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class Data2LogisticsReportController : Controller
    {
        Data2LogisticsReport oData2LogisticsReport = new Data2LogisticsReport();
        // GET: SupplyChain/Data2LogisticsReport
        public ActionResult Option()
        {
            oData2LogisticsReport = new Data2LogisticsReport();
            DataTable dtProgram = oData2LogisticsReport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View();
        }

        public ActionResult GetExcel(string programId, string programName, string invDate, string invNo, string custRef)
        {
            try
            {
                string menuTitle = string.Empty;
                string RptCode;

                oData2LogisticsReport = new Data2LogisticsReport();
                DataTable getInvData = oData2LogisticsReport.GetList(programId, programName, invDate, invNo, custRef);

                //LOAD MRU & LOG QUERY
                if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
                {
                    menuTitle = TempData["ReportTitle"] as string;
                    RptCode = TempData["RptCode"].ToString();
                    TempData.Keep();
                    cLog oLog = new cLog();
                    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var workesheet = package.Workbook.Worksheets.Add("Data2LogisticsReport");
                    AddDataTableToWorksheet(workesheet, getInvData, 1, 1);
                    var fileContent = package.GetAsByteArray();
                    return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Data2LogisticsReport.xlsx");
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                return View("Error", new HandleErrorInfo(ex, "Data2LogisticsReport", "Option"));
            }
        }
        private void AddDataTableToWorksheet(ExcelWorksheet worksheet, DataTable dataTable, int startRow, int startColumn)
        {
            //// Starting row and column
            //int startRow = 20;
            //int startColumn = 2; // Column B is the second column

            // Add headers
            using (var headerRange = worksheet.Cells[startRow, startColumn, startRow, startColumn + dataTable.Columns.Count - 1])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // Set header values
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                worksheet.Cells[startRow, startColumn + i].Value = dataTable.Columns[i].ColumnName;
            }

            // Add rows
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    var cell = worksheet.Cells[startRow + i + 1, startColumn + j];
                    cell.Value = dataTable.Rows[i][j];

                    // Apply border to each cell
                    //cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    //cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    //cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    //cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }
            }

            // Apply auto-sizing to columns for better readability
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
    }
}