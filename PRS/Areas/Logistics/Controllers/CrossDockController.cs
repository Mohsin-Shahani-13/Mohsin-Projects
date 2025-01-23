using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.Logistics.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.Logistics.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class CrossDockController : Controller
    {
        // GET: Logistics/CrossDock
        CrossDock oCrossDock = new CrossDock();
        public ActionResult Option()
        {
            oCrossDock = new CrossDock();
            return View(oCrossDock);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oCrossDock = new CrossDock();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oCrossDock);
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oCrossDock = new CrossDock();
            oCrossDock.GetList(frmDt, toDate, programId, ProgramName);
            var jsonResult = Json(oCrossDock, JsonRequestBehavior.AllowGet);
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