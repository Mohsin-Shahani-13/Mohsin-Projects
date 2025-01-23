using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class ImageController : Controller
    {
        Image oImage;
        // GET: SupplyChain/Image
        public ActionResult Option()
        {
            oImage = new Image();
            DataTable dtProgram = oImage.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "ProgramName", "");
            return View(oImage);
        }
        public ActionResult Index(bool checkAllDate)
        {
            string serialNo = Request.QueryString["serialNo"];
            string custRef = Request.QueryString["custRef"];
            string programId = Request.QueryString["programId"];
            string programName = Request.QueryString["programName"];
            string fromDt = Request.QueryString["fromDt"];
            string toDt = Request.QueryString["toDt"];
            string menuTitle = Request.QueryString["menuTitle"];
            string rptCode = Request.QueryString["rptCode"];
            oImage = new Image();
            bool success = false;
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            success = oImage.GetList(programId, programName, fromDt, toDt, serialNo, custRef, checkAllDate);
            oImage.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                rptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog olog = new cLog();
                olog.SaveLog(menuTitle, Request.Url.PathAndQuery, rptCode);
            }
            if (success)
                return View(oImage);
            else
                return View();
        }
        public JsonResult GetImage(string imageDataId)
        {
            oImage = new Image();
            oImage.GetImage(imageDataId);
            var jsonResult = Json(oImage, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}