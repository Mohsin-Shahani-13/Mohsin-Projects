using IP.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;

namespace IP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class CodeTableListingController : Controller
    {
        CodeTableListing oCodeTableListing = new CodeTableListing();

        // GET: CodeTableListing
        public ActionResult Index(string rptCode, string menuTitle)
        {
            oCodeTableListing = new CodeTableListing();

            //oCodeTableListing.GetList();
            // cLog oLog = new cLog();
            // oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, rptCode);
            return View(oCodeTableListing);
        }
        [HttpPost]
        public ActionResult GetList(string rptCode, string menuTitle)
        {
            oCodeTableListing = new CodeTableListing();
            oCodeTableListing.GetList();
            //oCodeTableListing.GetCustomList();
            var jsonResult = Json(oCodeTableListing, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Option(string tblName, int ViewId, string ViewInputField, string ViewLabel, string ViewCtrlType, bool ViewHasProgramId = false)
        {
            oCodeTableListing = new CodeTableListing();
            //oCodeTableListing._lstProgram = oCodeTableListing.GetContract();
            //oCodeTableListing._lstProgram.Replace("'","0");
            oCodeTableListing.GetCustomList(tblName);
            DataTable dtProgram = oCodeTableListing.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programID", "programName", "");
            DataTable dtStatus = oCodeTableListing.GetStatus();
            ViewBag.ddStatus = cCommon.ToDropDown(dtStatus, "ID", "Description", "All");
            DataTable dtStatusForCodeGenericView = oCodeTableListing.StatusForCodeGenericView();
            ViewBag.ddStatusForCodeGenericView = cCommon.ToDropDown(dtStatusForCodeGenericView, "ID", "Description", "All");
            DataTable dtCodeGenericTableName = oCodeTableListing.CodeGenericTableName();
            ViewBag.ddCodeGenericTableName = cCommon.ToDropDown(dtCodeGenericTableName,"Name", "Name", "All");

            // Get the initially selected program ID (e.g., the first one in the program list)
            var ProgramId = dtProgram.Rows.Count > 0 ? Convert.ToInt32(dtProgram.Rows[0]["programID"]) : 0;
            DataTable dtWarehouseForPartSerialView = oCodeTableListing.WarehouseForPartSerialView(ProgramId);
            ViewBag.ddWarehouseForPartSerialView = cCommon.ToDropDown(dtWarehouseForPartSerialView, "Warehouse", "Warehouse", "All");

            //oCodeTableListing.CtrlType += ViewCtrlType;
            oCodeTableListing.CtrlType = string.IsNullOrEmpty(oCodeTableListing.CtrlType)
            ? ViewCtrlType
            : $"{oCodeTableListing.CtrlType},{ViewCtrlType}";
            oCodeTableListing.Parameter = ViewLabel;
            oCodeTableListing.viewId = ViewId;
            oCodeTableListing.HaveProgramId = ViewHasProgramId;
            ViewBag.ReportTitle = tblName;
            return View(oCodeTableListing);
        }
        public ActionResult detail(string[] param, string contract,string programName, string frmDt, string toDate, string tblName, string ViewId, bool ViewHasProgramId, string statusId, string status, string dockType, string CGStatusId, string CGStatus, string CGTableId, string CGTableName, string PSWarehouse, bool ViewHasOptionPage = false, bool isAllDate = false)
        {
            bool success = false;
            
            ViewBag.hasoptionPage = ViewHasOptionPage;
            if (ViewHasOptionPage != true)
            {
                ViewBag.ReportTitle = "Data for View = '" + tblName + "'" + "  ";
                oCodeTableListing = new CodeTableListing();
                success = oCodeTableListing.GetDetail(tblName, ViewId, ViewHasProgramId);
                oCodeTableListing.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                cLog oLog = new cLog();
                oLog.SaveLog("Views - "+tblName, Request.Url.PathAndQuery, tblName);
            }
            else
            {
                oCodeTableListing = new CodeTableListing();
                tblName = oCodeTableListing.GetReportName(ViewId);
                ViewBag.ReportTitle = "Data for View = '" + tblName + "'" + "  ";
                success = oCodeTableListing.GetDetailParam(param,contract,programName,isAllDate,frmDt, toDate, tblName, ViewId, ViewHasProgramId, statusId, status, dockType, CGStatusId, CGStatus, CGTableId, CGTableName, PSWarehouse);
                oCodeTableListing.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                cLog oLog = new cLog();
                oLog.SaveLog("Views - " + tblName, Request.Url.PathAndQuery, tblName);

            }
            if (success)
                return View(oCodeTableListing);
            else
                return View(oCodeTableListing);
        }

        public JsonResult GetImageSize(byte[]imgSize)
        {
            var jpegQuality = 50;
            Image image;
            Byte[] outputBytes;
            using (var inputStream = new MemoryStream(imgSize))
            {
                image = Image.FromStream(inputStream);
                var jpegEncoder = ImageCodecInfo.GetImageDecoders()
                  .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, jpegQuality);
               // Byte[] outputBytes;
                using (var outputStream = new MemoryStream())
                {
                    image.Save(outputStream, jpegEncoder, encoderParameters);
                    outputBytes = outputStream.ToArray();
                }
            }

            var jsonResult = Json(outputBytes, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public JsonResult GetImage(string imageDataId)
        {
            oCodeTableListing = new CodeTableListing();
            oCodeTableListing.GetImage(imageDataId);
            var jsonResult = Json(oCodeTableListing, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public JsonResult GetWarehousesByProgram(int ProgramId)
        {
            // Query the warehouses for the given programId
            DataTable dtWarehouseForPartSerialView = oCodeTableListing.WarehouseForPartSerialView(ProgramId);
            var warehouseList = cCommon.ToDropDown(dtWarehouseForPartSerialView, "Warehouse", "Warehouse", "All");

            return Json(warehouseList, JsonRequestBehavior.AllowGet);
        }

    }
}