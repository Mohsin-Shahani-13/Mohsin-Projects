using IP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;

namespace IP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class LogQueriesController : Controller
    {
        // GET: LogQueries
        LogQueries oLogQueries;
        public ActionResult Option()
        {
            oLogQueries = new LogQueries();
            return View(oLogQueries);
        }
        public ActionResult Index(string rptCode, string menuTitle, string fromDt, string toDt)
        {
            try
            {
                ViewBag.ReportTitle = menuTitle;
                //cLog oLog = new cLog();
                //oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, rptCode);

                oLogQueries = new LogQueries();
                bool success = oLogQueries.GetLogQueries(fromDt, toDt);
                if (success)
                    return View(oLogQueries);
                else
                    return View();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

       

        public ActionResult ActivityLog(string rptCode, string rptTitle)
        {
            oLogQueries = new LogQueries();
            bool success = oLogQueries.GetActivityLog(rptCode, rptTitle);
            oLogQueries.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            if(rptTitle == "undefined")
            ViewBag.ReportTitle = "Log Details for '" + oLogQueries.RptName + "' Last 90 Days ";
            else
                ViewBag.ReportTitle = "Log Details for '" + rptTitle + "' Last 90 Days ";
            

            if (success)
                return View(oLogQueries);
            else
                return View();

        }

        public JsonResult GetList(string fromDt, string toDt)
        {
            string menuTitle = string.Empty;
            string RptCode;
            
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }

            oLogQueries = new LogQueries();
            oLogQueries.GetLogQueries(fromDt, toDt);
            oLogQueries.fromDt = fromDt;
            oLogQueries.toDt = toDt;
            var jsonResult = Json(oLogQueries, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult UnitList(string RptName, string qty, string RptCode, string fromDt, string toDt)
        {
        
            oLogQueries = new LogQueries();
            ViewBag.ReportTitle = "Log Details for '" + RptName + "'";
            bool success = oLogQueries.GetUnitList(RptName, RptCode, fromDt, toDt);
            if (success)
            {
                oLogQueries.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                return View(oLogQueries);
            }
            else
                return View();

        }
    }
}