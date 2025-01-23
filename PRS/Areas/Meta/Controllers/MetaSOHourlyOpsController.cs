using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.Meta.Models;
using System.Data;

using IP.ActionFilters;


namespace IP.Areas.Meta.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MetaSOHourlyOpsController : Controller
    {
        
        MetaSOHourlyOps oMetaSOHourlyOps = new MetaSOHourlyOps();
        // GET: Meta/MetaSOHourlyOps
        public ActionResult Option()
        {
            oMetaSOHourlyOps = new MetaSOHourlyOps();
            return View();
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaSOHourlyOps = new MetaSOHourlyOps();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string serialNo, string frmDt, string toDate, bool ischecked)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaSOHourlyOps = new MetaSOHourlyOps();
            oMetaSOHourlyOps.GetList(programId, ProgramName, serialNo, frmDt, toDate , ischecked);
            var jsonResult = Json(oMetaSOHourlyOps, JsonRequestBehavior.AllowGet);
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