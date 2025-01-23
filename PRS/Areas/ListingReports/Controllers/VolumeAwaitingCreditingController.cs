using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;
using IP.Areas.ListingReports.Models;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class VolumeAwaitingCreditingController : Controller
    {
        VolumeAwaitingCrediting oVolumeAwaitingCrediting;
        // GET: ListingReports/VolumeAwaitingCrediting
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oVolumeAwaitingCrediting = new VolumeAwaitingCrediting();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oVolumeAwaitingCrediting);
        }

        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oVolumeAwaitingCrediting = new VolumeAwaitingCrediting();
            oVolumeAwaitingCrediting.GetList();
            var jsonResult = Json(oVolumeAwaitingCrediting, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetUnit(string programId, string Area, string Range)
        {
            try
            {
                ViewBag.ReportTitle = Area + "  " + "[" + Range + "]";
                VolumeAwaitingCrediting oVolumeAwaitingCrediting = new VolumeAwaitingCrediting();


                //string programId = oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["DefaultSite"].ToString());
                bool success = oVolumeAwaitingCrediting.GetUnits(programId, Area, Range);
                oVolumeAwaitingCrediting.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

                if (success)
                    return View(oVolumeAwaitingCrediting);
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