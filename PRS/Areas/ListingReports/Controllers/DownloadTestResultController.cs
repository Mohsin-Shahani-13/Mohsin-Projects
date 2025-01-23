using IP.Areas.ListingReports.Models;
using IP.Classess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace IP.Areas.ListingReports.Controllers
{
    public class DownloadTestResultController : Controller
    {
        DownloadTestResult oDownloadTestResult = new DownloadTestResult();
        // GET: ListingReports/DownloadTestResult
        public ActionResult Option()
        {
            oDownloadTestResult = new DownloadTestResult();
            //DataTable dtProgram = oDownloadTestResult.GetSitewiseContract();
            //DataTable dtProgram = oDownloadTestResult.Program();
            //ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "Program", "Program", "");
            oDownloadTestResult.GetTestAreas();
            return View(oDownloadTestResult);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            //oDownloadTestResult = new DownloadTestResult();
            //oDownloadTestResult.GetTestAreas();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDownloadTestResult);
        }

        [HttpPost]
        public JsonResult GetList(string frmDt, string toDt, bool isAllDate, HttpPostedFileBase importFile, string testArea, string contract, string program)
        {
            string menuTitle = string.Empty;
            string RptCode = string.Empty;
            oDownloadTestResult = new DownloadTestResult();
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("SerialNo", typeof(string));
            dataTable.Columns.Add("UploadedBy", typeof(string));
            dataTable.Columns.Add("UploadFrom", typeof(string));
            string empName = Session["EmpName"].ToString();

            try
            {
                //List<string> lines = new List<string>();
                string serialNos = string.Empty;

                using (var reader = new StreamReader(importFile.InputStream))
                {
                    int serialCount = 1;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            if (serialCount > 5000) return Json(new { Status = 2, Message = "There is a limit of 5000 Serial No.s only." });
                            //if (!Regex.IsMatch(line, "^[a-zA-Z0-9]*$")) ^[a-zA-Z0-9 ]*$
                            if (!Regex.IsMatch(line, "^[A-Za-z0-9]{5,50}$|^[A-Za-z0-9\\|]{5,50}$"))
                            {
                                return Json(new { Status = 3, Message = "Values in Input file contains invalid characters." });
                            }
                            //serialNos += "'" + line.Trim() + "'" + ",";
                            dataTable.Rows.Add(line, empName, "PRS");
                            //lines.Add(line.Trim().TrimEnd(','));
                        }
                        serialCount++;
                    }
                    //serialNos = serialNos.TrimEnd(',');

                }
                oDownloadTestResult.GetList(frmDt, toDt, isAllDate, testArea, Session["ProgramIdBySiteForMeta"].ToString(), program, dataTable);
                var jsonResult = Json(oDownloadTestResult, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
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
            catch (Exception ex)
            {
                //return Json(new { Status = 0, Message = "An error occurred: " + ex.Message });"Read File Error."
                return Json(new { Status = 0, Message = ex.Message });
                //return Json(new { Status = 0, Message = "An error occurred. Contact Support Team" });
            }
            //oDownloadTestResult.GetList(frmDt, toDt, isAllDate, serialNos, testArea);

        }
        //[HttpPost]
        public ActionResult DownloadFilesAsZip(string frmDt, string toDt, bool isAllDate, string testArea, string program, string contract)
        {
            oDownloadTestResult = new DownloadTestResult();
            List<string> fileUrls = oDownloadTestResult.GetXmlFileUrls(program, frmDt, toDt, isAllDate, testArea, Session["ProgramIdBySiteForMeta"].ToString());
            
            try
            {
                Dictionary<string, int> fileNamesDictionary = new Dictionary<string, int>();
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ZipArchive zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        using (new cImpersonate())
                        {
                            
                            foreach (string fileUrl in fileUrls)
                            {
                                string xmlStringWithCData = string.Empty;
                                string decodedContent = string.Empty;
                                byte[] decodedBytes;

                                string fileContent = System.IO.File.ReadAllText(fileUrl);

                                XDocument doc = XDocument.Parse(fileContent);

                                XElement outputElement = doc.Root?.Element("output");

                                if (outputElement != null)
                                {
                                    XCData cdata = new XCData(outputElement.Value);
                                    outputElement.ReplaceNodes(cdata);
                                }

                                //for xml formating string
                                xmlStringWithCData = doc.ToString();

                                // Extract values
                                string timestamp = doc.Root?.Attribute("timestamp")?.Value;
                                string finished = doc.Root?.Attribute("finished")?.Value;
                                string serial = doc.Root?.Element("device")?.Element("serial")?.Value;

                                // Generate filename
                                string fileName = $"log-{serial}-{timestamp}-{finished}.xml";

                                // Check for duplicates and modify the filename if necessary
                                if (fileNamesDictionary.ContainsKey(fileName))
                                {
                                    fileNamesDictionary[fileName]++;
                                    string[] fileNameParts = fileName.Split('.');
                                    string fileNameWithoutExtension = fileNameParts[0];
                                    string fileExtension = fileNameParts.Length > 1 ? "." + fileNameParts[1] : "";
                                    fileName = $"{fileNameWithoutExtension}_{fileNamesDictionary[fileName]}{fileExtension}";
                                }
                                else
                                {
                                    fileNamesDictionary[fileName] = 0;
                                }
                                var entry = zipArchive.CreateEntry(fileName);
                                using (var entryStream = entry.Open())
                                {
                                    // decoded content string to byte
                                    decodedBytes = System.Text.Encoding.UTF8.GetBytes(xmlStringWithCData);
                                    entryStream.Write(decodedBytes, 0, decodedBytes.Length);
                                }
                            }
                        }
                    }
                    var zipName = $"TestRecords-{DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss")}.zip";
                    return File(ms.ToArray(), "application/zip", zipName);
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return Content(e.Message, "text/plain");
                //return Content("An error occurred. Contact Support Team", "text/plain");

            }

        }

    }
}
