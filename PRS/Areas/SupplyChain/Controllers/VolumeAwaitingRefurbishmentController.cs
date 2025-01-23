using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.Areas.SupplyChain.Models;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class VolumeAwaitingRefurbishmentController : Controller
    {
        VolumeAwaitingRefurbishment oVolumeAwaitingRefurbishment = new VolumeAwaitingRefurbishment();
        // GET: SupplyChain/VolumeAwaitingRefurbishment
        public ActionResult Option()
        {
            oVolumeAwaitingRefurbishment = new VolumeAwaitingRefurbishment();
            //DataTable dtProgram = oVolumeAwaitingRefurbishment.GetProgramBySite();
            //ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oVolumeAwaitingRefurbishment);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oVolumeAwaitingRefurbishment = new VolumeAwaitingRefurbishment();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }

        public JsonResult GetList(string programId, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oVolumeAwaitingRefurbishment = new VolumeAwaitingRefurbishment();
            oVolumeAwaitingRefurbishment.GetList(programId, programName);
            var jsonResult = Json(oVolumeAwaitingRefurbishment, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetUnits(string programId, string AreaCell, string Range)
        {
            try
            {
                ViewBag.ReportTitle = AreaCell + "  "+ "[" + Range + "]";
                VolumeAwaitingRefurbishment oVolumeAwaitingRefurbishment = new VolumeAwaitingRefurbishment();
         

                //string programId = oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["DefaultSite"].ToString());
                bool success = oVolumeAwaitingRefurbishment.GetUnits(programId, AreaCell, Range);
                oVolumeAwaitingRefurbishment.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

                if (success)
                    return View(oVolumeAwaitingRefurbishment);
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