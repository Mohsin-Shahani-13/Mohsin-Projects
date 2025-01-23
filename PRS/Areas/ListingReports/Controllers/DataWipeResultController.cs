using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.ActionFilters;
using IP.Externals;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Net;
using System.IO.Compression;
using IP.Classess;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DataWipeResultController : Controller
    {
        DataWipeResult oDataWipeResult = new DataWipeResult();
        // GET: ListingReports/DataWipeResult
        public ActionResult Option()
        {
            oDataWipeResult = new DataWipeResult();
            //DataTable dtProgram = oDataWipeResult.GetSitewiseContract();
            //ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "Contract", "programName", "");
            ViewBag.Site = Session["DefaultSite"].ToString(); 
            DataTable dtProgram = oDataWipeResult.Program();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "Program", "Program", "");
            return View(oDataWipeResult);
        }

        

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDataWipeResult = new DataWipeResult();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.site = @Session["DefaultSite"];
            ViewBag.program = "Meta";
            return View(oDataWipeResult);
        }

        public JsonResult GetList(string Program, string frmDt, bool isAllDate, string toDt, string serialNo, string partNo, string contract)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oDataWipeResult = new DataWipeResult();
            oDataWipeResult.GetList(Program, frmDt, toDt, isAllDate, serialNo, partNo, Session["ProgramIdBySiteForMeta"].ToString());
            var jsonResult = Json(oDataWipeResult, JsonRequestBehavior.AllowGet);
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

        public ActionResult DownloadFilesAsZip(bool countCheck, string frmDt, string toDt, bool isAllDate, string serialNo, string partNo,string Program, string Contract)
        {
            oDataWipeResult = new DataWipeResult();
            var fileUrls = oDataWipeResult.GetXmlFileUrls(frmDt, toDt, isAllDate, serialNo, partNo, Program, Session["ProgramIdBySiteForMeta"].ToString());
            //For Alert Popup: Only count of urls check only when countCheck is true. Else return zip.
            if (countCheck)
            {
                int urlsCount = fileUrls.Count;

                var result = new
                {
                    UrlsCount = urlsCount
                };
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else {
                
                try { 
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                     using (new cImpersonate())
                     {
                          foreach (string fileUrl in fileUrls)
                          {
                                  byte[] fileData = System.IO.File.ReadAllBytes(fileUrl);
                                  string fileName = Path.GetFileName(fileUrl);
                                  var entry = zipArchive.CreateEntry(fileName);
                                  using (var entryStream = entry.Open())
                                    {
                                        entryStream.Write(fileData, 0, fileData.Length);
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
            }
            }
        }

        public ActionResult Detail(string Id)
        {
            if (string.IsNullOrEmpty(Id))
                return View();

            try
            {
                oDataWipeResult = new DataWipeResult();
                bool success = oDataWipeResult.GetDetail(Id);
                if (success)
                    return View(oDataWipeResult);
                else
                    return View(oDataWipeResult);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }

        public ActionResult ViewContent(string Id)
        {
            oDataWipeResult = new DataWipeResult();
            (string fileContents, string extension) result = oDataWipeResult.GetContent(Id);
            if (result.extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Data = result.fileContents;
                ViewBag.ContentType = "text/plain;charset=utf-8";
            }
            else if (result.extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                // Parse the XML content to extract the necessary values for the filename
                XDocument doc = XDocument.Parse(result.fileContents);
                XElement outputElement = doc.Root?.Element("output");

                if (outputElement != null)
                {
                    XCData cdata = new XCData(outputElement.Value);
                    outputElement.ReplaceNodes(cdata);
                }

                // Format the XML string with CDATA
                string xmlStringWithCData = doc.ToString();

                // Extract values for the filename
                string timestamp = doc.Root?.Attribute("timestamp")?.Value;
                string finished = doc.Root?.Attribute("finished")?.Value;
                string serial = doc.Root?.Element("device")?.Element("serial")?.Value;

                // Generate the filename
                string fileName = $"log-{serial}-{timestamp}-{finished}.xml";

                // Set the data and content type for the view
                ViewBag.Data = XElement.Parse(xmlStringWithCData);
                ViewBag.ContentType = "text/xml;charset=utf-8";

                // Set the Content-Disposition header with the custom filename
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            }
            return View();
        }



        //public ActionResult GetXML(string Id)
        //{
        //    oDataWipeResult = new DataWipeResult();
        //    string XMLmsg = oDataWipeResult.GetContent(Id);
        //    //count of child if count>1 append PlusOutbound
        //    ViewBag.XMLData = XElement.Parse(XMLmsg);
        //    //int HeaderNode = NodeCount(XMLmsg);
        //    //else { ViewBag.XMLData = XElement.Parse(XMLmsg); }
        //    return View();
        //}

        //public ActionResult GetTxt(string Id)
        //{
        //    oDataWipeResult = new DataWipeResult();
        //    string Txtmsg = oDataWipeResult.GetContent(Id);
        //    //count of child if count>1 append PlusOutbound
        //    //int HeaderNode = NodeCount(XMLmsg);
        //    ViewBag.TxtData = Txtmsg; 
        //    return View();
        //}

        public int NodeCount(string xmlString)
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("p", "http://PlusProcessOutbound");

            XmlNodeList nodeList = xmlDoc.SelectNodes("//p:Header", nsManager);

            int count = nodeList.Count;
            return count;
        }
    }
}