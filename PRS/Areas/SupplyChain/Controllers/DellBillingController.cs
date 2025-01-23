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
using System.Globalization;

namespace IP.Areas.SupplyChain.Controllers
{
    public class DellBillingController : Controller
    {
        // GET: SupplyChain/DellBilling
        DellBilling oDellBilling = new DellBilling();
        public ActionResult Option()
        {
            DataTable dtProgram = oDellBilling.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oDellBilling);
        }
        public ActionResult Index(string programId, string trackingNo, string menuTitle, string rptCode)
        {
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.programId = programId;
            ViewBag.trackingNo = trackingNo;
            return View(oDellBilling);
        }
        public JsonResult GetList(string programId, string programName, string trackingNo, string rptType, string InvNumber)
        {
            string menuTitle = string.Empty;
            string RptCode = string.Empty;

            oDellBilling.GetList(programId, programName, InvNumber);
            var jsonResult = Json(oDellBilling, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"].ToString();
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
        public ActionResult GetDetail(string programId, string trackingNo, string menuTitle, string rptCode)
        {
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.programId = programId;
            ViewBag.trackingNo = trackingNo;
            return View(oDellBilling);
        }
        public JsonResult GetDetailList(string programId, string programName, string trackingNo, string rptType)
        {
            string menuTitle = string.Empty;
            string RptCode = string.Empty;
            ViewBag.programId = programId;
            ViewBag.trackingNo = trackingNo;

            oDellBilling.GetDetail(programId, programName, trackingNo);
            var jsonResult = Json(oDellBilling, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"].ToString();
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
        public JsonResult GetInvoice(string programId, string trackingNo)
        {
            ViewBag.programId = programId;
            ViewBag.trackingNo = trackingNo;
            DataTable invDt = new DataTable();
            invDt = oDellBilling.Invoice(programId, trackingNo);
            List<DellBilling> items = ConvertDataTableToInvoiceItemList(invDt);

            // Allow GET requests for this JsonResult
            return Json(new { item = items, model = oDellBilling }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveExtraLine(string trackingNo, string programId, DellBilling model)
        {
            ViewBag.InvNo = model.InvNo;
            ViewBag.InvDate = model.InvDate;
            Session["InvNo"] = model.InvNo;
            Session["InvDate"] = model.InvDate;
            try
            {
                DellBilling oDellBilling = new DellBilling();
                oDellBilling.SaveFirstGridData(trackingNo, programId, model.InvNo, model.InvDate);

                if (model == null || model.ExtraLinesData == null || !model.ExtraLinesData.Any())
                {
                    return Json(new { success = false, message = "No data received." });
                }
                else
                {
                    oDellBilling.SaveExtraLineData(model.InvNo, model.InvDate, model.ExtraLinesData);
                }
                return Json(new { success = true, data = oDellBilling });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult ExportToExcel(string programId, string trackingNo)
        {

            try
            {
                // Initialize DellBilling instance and fetch invoice data

                DataTable dt = oDellBilling.Invoice(programId, trackingNo);
                DataTable InvoiceDt = oDellBilling.InvoiceDt(programId, trackingNo);
                DataTable GetExtraLineDt = oDellBilling.GetExtraLineDt();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Create a new Excel package
                using (var package = new ExcelPackage())
                {
                    // Define the starting row and column for the first DataTable
                    int startRow1 = 20;
                    int startColumn1 = 2;
                    var InvWorksheet = package.Workbook.Worksheets.Add("Inv\t" + oDellBilling.InvNo);


                    if (InvoiceDt.Rows.Count > 0)
                    {
                        // Add the first DataTable to the worksheet
                        AddDataTableToWorksheet(InvWorksheet, InvoiceDt, startRow1, startColumn1, false);
                        // Calculate the sum of the Amount column for the first grid
                        double totalAmount1 = InvoiceDt.AsEnumerable().Sum(row => Convert.ToDouble(row["Amount"]));
                        string formattedAmount1 = totalAmount1.ToString("#,##0.00");

                        // Determine the row to place the total for the first grid
                        int totalRow1 = startRow1 + InvoiceDt.Rows.Count + 1; // +1 to leave one row of space
                        #region total formatting1
                        // Write the total for the first grid
                        //style total label
                        InvWorksheet.Cells[totalRow1, startColumn1 + 4].Value = "Total:";
                        InvWorksheet.Cells[totalRow1, startColumn1 + 4].Style.Font.Bold = true;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 4].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // Center alignment

                        //style total value
                        InvWorksheet.Cells[totalRow1, startColumn1 + 5].Value = "$" + formattedAmount1;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 5].Style.Font.Bold = true;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 5].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow1, startColumn1 + 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right; // Center alignment
                        #endregion
                    }


                    // Calculate the starting row for the ExtraLine table
                    int numberOfRowsInFirstTable = InvoiceDt.Rows.Count;
                    int startRow2 = startRow1 + numberOfRowsInFirstTable + 3; // Add 2 to leave an empty row between tables
                    int startColumn2 = startColumn1;
                    if (GetExtraLineDt.Rows.Count > 0)
                    {
                        AddDataTableToWorksheet(InvWorksheet, GetExtraLineDt, startRow2, startColumn2, true);
                        #region total formatting2
                        double totalAmount2 = GetExtraLineDt.AsEnumerable().Sum(row => Convert.ToDouble(row["Amount"]));
                        string formattedAmount2 = totalAmount2.ToString("#,##0.00");

                        int totalRow2 = startRow2 + GetExtraLineDt.Rows.Count + 1;

                        // Write the total for the second grid
                        InvWorksheet.Cells[totalRow2, startColumn2 + 4].Value = "Total:";
                        InvWorksheet.Cells[totalRow2, startColumn2 + 4].Style.Font.Bold = true;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 4].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // Center alignment

                        InvWorksheet.Cells[totalRow2, startColumn2 + 5].Value = "$" + formattedAmount2;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 5].Style.Font.Bold = true;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 5].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        InvWorksheet.Cells[totalRow2, startColumn2 + 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right; // Center alignment
                        #endregion
                    }
                    if (trackingNo != "")
                    {
                        // Add a worksheet for the InvDet-report
                        var InvDetWorksheet = package.Workbook.Worksheets.Add("Det \t" + oDellBilling.InvNo);
                        AddDataTableToWorksheet(InvDetWorksheet, oDellBilling.InvoiceDetail(programId, trackingNo), 1, 1, false); //ConvertToDataTable(oDellBilling.lstInvoiceDetail)
                    }

                    #region Excel_Formatting 

                    InvWorksheet.Cells["A1:I1"].Style.Font.Bold = true;
                    InvWorksheet.Cells["A1:I1"].Style.Border.Top.Style = ExcelBorderStyle.Thick;
                    InvWorksheet.Cells["A1:I1"].Style.Border.Left.Style = ExcelBorderStyle.Thick;
                    InvWorksheet.Cells["A1:I1"].Style.Border.Right.Style = ExcelBorderStyle.Thick;
                    InvWorksheet.Cells["A1:I1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    InvWorksheet.Cells["A1:I1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["A1:I1"].Merge = true;
                    InvWorksheet.Cells["A1"].Value = "INVOICE";


                    // Apply border styling to the merged range
                    InvWorksheet.Cells["B3:C6"].Merge = true;
                    InvWorksheet.Cells["B3:C6"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B3:C6"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B3:C6"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B3:C6"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    // Apply text wrapping
                    InvWorksheet.Cells["B3:C6"].Style.WrapText = true;

                    // Align text to the left
                    InvWorksheet.Cells["B3:C6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    // Set the value of the merged range
                    InvWorksheet.Cells["B3"].Value = "VALUTECH OUTSOURCING, LLC 122 W. MADISON ST / 2ND FLOOR OTTAWA, IL 61350 630-492-0342 ext. 1111";


                    // Apply styling for the merged range B9:C9 (Header)
                    InvWorksheet.Cells["B9:C9"].Merge = true;
                    InvWorksheet.Cells["B9:C9"].Style.Font.Bold = true; // Make text bold
                    InvWorksheet.Cells["B9:C9"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B9:C9"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B9:C9"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B9:C9"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B9:C9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // Align text to the left
                    InvWorksheet.Cells["B9:C9"].Style.VerticalAlignment = ExcelVerticalAlignment.Center; // Center alignment vertically
                    InvWorksheet.Cells["B9"].Value = "BILL TO:";

                    // Apply styling for the merged range B10:C13 (Address)
                    InvWorksheet.Cells["B10:C13"].Merge = true;
                    InvWorksheet.Cells["B10:C13"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B10:C13"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B10:C13"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B10:C13"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    // Apply text wrapping
                    InvWorksheet.Cells["B10:C13"].Style.WrapText = true;

                    // Align text to the left
                    InvWorksheet.Cells["B10:C13"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    InvWorksheet.Cells["B10:C13"].Style.VerticalAlignment = ExcelVerticalAlignment.Top; // Align text to the top

                    // Set the value of the merged range
                    InvWorksheet.Cells["B10"].Value = "Dell Computer\nP. O. Box 149257\nAustin, Texas 78714-9257";





                    // Apply styling for the merged range F9:H9 (Header)
                    InvWorksheet.Cells["F9:H9"].Merge = true;
                    InvWorksheet.Cells["F9:H9"].Style.Font.Bold = true; // Make text bold
                    InvWorksheet.Cells["F9:H9"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F9:H9"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F9:H9"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F9:H9"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F9:H9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // Align text to the left
                    InvWorksheet.Cells["F9:H9"].Style.VerticalAlignment = ExcelVerticalAlignment.Center; // Center alignment vertically
                    InvWorksheet.Cells["F9"].Value = "SHIP TO:";

                    // Apply styling for the merged range F10:H13 (Address)
                    InvWorksheet.Cells["F10:H13"].Merge = true;
                    InvWorksheet.Cells["F10:H13"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F10:H13"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F10:H13"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F10:H13"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    // Apply text wrapping
                    InvWorksheet.Cells["F10:H13"].Style.WrapText = true;

                    // Align text to the left
                    InvWorksheet.Cells["F10:H13"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    InvWorksheet.Cells["F10:H13"].Style.VerticalAlignment = ExcelVerticalAlignment.Top; // Align text to the top

                    // Set the value of the merged range
                    InvWorksheet.Cells["F10"].Value = "Dell Computer\nBraker 3\n2214 W. Braker Lane, Suite D\nAustin, TX  78714-9257";





                    // Apply styling for cell range B17:C17
                    InvWorksheet.Cells["B17:C17"].Style.Font.Bold = true;
                    InvWorksheet.Cells["B17:C17"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B17:C17"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B17:C17"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B17:C17"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["B17:C17"].Merge = true;
                    InvWorksheet.Cells["B17"].Value = "PO NUMBER";

                    // Apply styling for cell D17
                    InvWorksheet.Cells["D17"].Style.Font.Bold = true;
                    InvWorksheet.Cells["D17"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["D17"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["D17"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["D17"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["D17"].Value = "TERMS";

                    // Apply styling for cell E17
                    InvWorksheet.Cells["E17"].Style.Font.Bold = true;
                    InvWorksheet.Cells["E17"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["E17"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["E17"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["E17"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["E17"].Value = "SHIP";

                    // Apply styling for the merged range F17:G17
                    InvWorksheet.Cells["F17:H17"].Style.Font.Bold = true;
                    InvWorksheet.Cells["F17:H17"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F17:H17"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F17:H17"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F17:H17"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["F17:H17"].Merge = true;
                    InvWorksheet.Cells["F17"].Value = "TRACKING";

                    // Apply styling for cell range B18:C18
                    InvWorksheet.Cells["B18:C18"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B18:C18"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B18:C18"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["B18:C18"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["B18:C18"].Merge = true;
                    InvWorksheet.Cells["B18"].Value = oDellBilling.PO_NUMBER;

                    // Apply styling for cell D18
                    InvWorksheet.Cells["D18"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["D18"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["D18"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["D18"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["D18"].Value = oDellBilling.TERMS;

                    // Apply styling for cell E18
                    InvWorksheet.Cells["E18"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["E18"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["E18"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["E18"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["E18"].Value = oDellBilling.SHIP;

                    // Apply styling for the merged range F18:G18
                    InvWorksheet.Cells["F18:H18"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F18:H18"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F18:H18"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["F18:H18"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["F18:H18"].Merge = true;
                    InvWorksheet.Cells["F18"].Value = oDellBilling.TRACKING;
                    InvWorksheet.Cells["F18"].Style.WrapText = true;


                    InvWorksheet.Cells["G3"].Style.Font.Bold = true;
                    InvWorksheet.Cells["G3"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["G3"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["G3"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["G3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["G3"].Value = "Date";

                    InvWorksheet.Cells["H3"].Style.Font.Bold = true;
                    InvWorksheet.Cells["H3"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["H3"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["H3"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["H3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["H3"].Value = "Invoice #";



                    InvWorksheet.Cells["G4"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["G4"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["G4"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["G4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    // Center alignment
                    //string[] formats = { "dd/MM/yyyy", "MM-dd-yyyy", "yyyy-MM-dd" }; // Add formats as needed
                    //DateTime parsedDate;
                    InvWorksheet.Cells["G4"].Value = oDellBilling.InvDate;
                    //if (DateTime.TryParseExact(oDellBilling.InvDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    //{
                    //    InvWorksheet.Cells["G4"].Value = parsedDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    //}
                    //else
                    //{
                    //    // Handle invalid date format
                    //    InvWorksheet.Cells["G4"].Value = "Invalid Date";
                    //}
                    //InvWorksheet.Cells["G4"].Value = oDellBilling.InvDate;


                    InvWorksheet.Cells["H4"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["H4"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["H4"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    InvWorksheet.Cells["H4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center alignment
                    InvWorksheet.Cells["H4"].Value = oDellBilling.InvNo;

                    InvWorksheet.Cells[InvWorksheet.Dimension.Address].AutoFitColumns();

                    #endregion

                    // Convert the package to a byte array
                    var fileContents = package.GetAsByteArray();
                    // Return the file to the user
                    return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Inv\t" + oDellBilling.InvNo + ".xlsx");

                    //string filePath = Server.MapPath("~/Exports/") + "Inv_" + oDellBilling.InvNo + ".xlsx";

                    //// Save the Excel package to the specified file path
                    //package.SaveAs(new FileInfo(filePath));

                    //// Return a link to the file for the client to download
                    //return Json(new { fileUrl = Url.Content("~/Exports/Inv_" + oDellBilling.InvNo + ".xlsx") });

                }

            }
            catch (Exception ex)
            {
                // Handle the exception
                return View("Error", new HandleErrorInfo(ex, "DellBilling", "ExportToExcel"));
            }
        }

        public JsonResult GetBillingInfo()
        {
            oDellBilling = new DellBilling();
            oDellBilling.BillingInfo();
            var jsonResult = Json(oDellBilling, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }
        private void AddDataTableToWorksheet(ExcelWorksheet worksheet, DataTable dataTable, int startRow, int startColumn, bool isExtraLinesGrid = false)
        {
            int columnCount = isExtraLinesGrid ? dataTable.Columns.Count : dataTable.Columns.Count -1;

            // Add headers
            using (var headerRange = worksheet.Cells[startRow, startColumn, startRow, startColumn + columnCount])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // Find the index of the "Amount" and "Description" columns dynamically
            int amountColumnIndex = -1;
            int descColumnIndex = -1;
            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                if (dataTable.Columns[j].ColumnName.Equals("Amount", StringComparison.OrdinalIgnoreCase))
                {
                    amountColumnIndex = j;
                }
                else if (dataTable.Columns[j].ColumnName.Equals("Description", StringComparison.OrdinalIgnoreCase))
                {
                    descColumnIndex = j;
                }
            }

            // Set header values and merge for Description column
            int columnOffset = 0; // Offset to handle merging
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (i == descColumnIndex)
                {
                    worksheet.Cells[startRow, startColumn + i + columnOffset].Value = dataTable.Columns[i].ColumnName;
                    worksheet.Cells[startRow, startColumn + i + columnOffset, startRow, startColumn + i + columnOffset + 1].Merge = true;
                    worksheet.Cells[startRow, startColumn + i + columnOffset].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    columnOffset += 1;
                }
                else
                {
                    worksheet.Cells[startRow, startColumn + i + columnOffset].Value = dataTable.Columns[i].ColumnName;
                    worksheet.Cells[startRow, startColumn + i + columnOffset].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }

            // Add rows and handle merging for Description column
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                columnOffset = 0; // Reset column offset for each row

                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    int columnIndex = startColumn + j + columnOffset;
                    var cell = worksheet.Cells[startRow + i + 1, columnIndex];

                    if (j == descColumnIndex)
                    {
                        // Merge cells for the Description column
                        worksheet.Cells[startRow + i + 1, columnIndex, startRow + i + 1, columnIndex + 1].Merge = true;
                        cell.Value = dataTable.Rows[i][j];

                        // Apply border to the merged cells
                        worksheet.Cells[startRow + i + 1, columnIndex, startRow + i + 1, columnIndex + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[startRow + i + 1, columnIndex, startRow + i + 1, columnIndex + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[startRow + i + 1, columnIndex, startRow + i + 1, columnIndex + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[startRow + i + 1, columnIndex, startRow + i + 1, columnIndex + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        columnOffset += 1; // Account for merged cells
                    }
                    else if (j == amountColumnIndex)
                    {
                        double amountValue = Convert.ToDouble(dataTable.Rows[i][j]);
                        cell.Value = amountValue.ToString("#,##0.00");
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }
                    else
                    {
                        cell.Value = dataTable.Rows[i][j];
                    }

                    // Apply border to the current cell
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }
            }

            // Apply auto-sizing to columns for better readability
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        public List<DellBilling> ConvertDataTableToInvoiceItemList(DataTable dt)
        {
            List<DellBilling> list = new List<DellBilling>();

            foreach (DataRow row in dt.Rows)
            {
                DellBilling item = new DellBilling
                {
                    Amount = Convert.ToDecimal(row["Amount"]),
                    Qty = Convert.ToInt32(row["Qty"]),
                    Commodity = row["Commodity"].ToString(),
                    WarrantyCode = row["WARRANTY_CODE"].ToString(),
                    ShipType = row["SHIP_TYPE"].ToString(),
                    Cost = Convert.ToDecimal(row["Cost"])
                };

                list.Add(item);
            }

            return list;
        }
        public JsonResult ValidateInvNo(string invNo)
        {
            bool result = oDellBilling.ValidateInvNo(invNo);

            return Json(new { response = result }, JsonRequestBehavior.AllowGet);
        }
    }
}