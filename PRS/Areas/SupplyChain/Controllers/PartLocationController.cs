using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PartLocationController : Controller
    {
        PartLocation oPartLocation = new PartLocation();
        // GET: SupplyChain/PartLocation
        public ActionResult Option()
        {
            oPartLocation = new PartLocation();

            return View(oPartLocation);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPartLocation = new PartLocation();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oPartLocation);
        }

        public JsonResult GetList(string programId, String ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oPartLocation = new PartLocation();
            oPartLocation.GetList(programId, ProgramName);
            var jsonResult = Json(oPartLocation, JsonRequestBehavior.AllowGet);
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
    }
}