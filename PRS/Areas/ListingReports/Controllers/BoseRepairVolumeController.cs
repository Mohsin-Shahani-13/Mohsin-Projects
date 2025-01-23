using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using IP.ActionFilters;
using IP.Areas.ListingReports.Models;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class BoseRepairVolumeController : Controller
    {
        BoseRepairVolume oBoseRepairVolume;
        // GET: ListingReports/BoseRepairVolume

        public ActionResult Option()
        {
            oBoseRepairVolume = new BoseRepairVolume();
            DataTable dtProgram = oBoseRepairVolume.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oBoseRepairVolume);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oBoseRepairVolume = new BoseRepairVolume();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oBoseRepairVolume);
        }

        public JsonResult GetList(string programId, string programName, string fDate, string tDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oBoseRepairVolume = new BoseRepairVolume();
            oBoseRepairVolume.GetList(programId, programName, fDate, tDate);
            var jsonResult = Json(oBoseRepairVolume, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetUnits(string programId, string AreaCell, string frmDt, string toDt, string Range)
        {
            try
            {
                ViewBag.ReportTitle = AreaCell + "  " + "[" + Range + "]";
                oBoseRepairVolume = new BoseRepairVolume();


                //string programId = oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["DefaultSite"].ToString());
                bool success = oBoseRepairVolume.GetUnits(programId, AreaCell, frmDt, toDt, Range);
                oBoseRepairVolume.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

                if (success)
                    return View(oBoseRepairVolume);
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