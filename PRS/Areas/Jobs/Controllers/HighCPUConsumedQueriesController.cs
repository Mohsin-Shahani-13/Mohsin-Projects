using IP.Areas.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace IP.Areas.Jobs.Controllers
{
    public class HighCPUConsumedQueriesController : Controller
    {
        HighCPUConsumedQueries oHighCPUConsumedQueries = new HighCPUConsumedQueries();
        // GET: Jobs/HighCPUconsumedQueries
        public ActionResult Option(string serverName)
        {
            oHighCPUConsumedQueries = new HighCPUConsumedQueries();
            DataTable dtServers = oHighCPUConsumedQueries.GetServerNames(serverName);
            ViewBag.ddlServers = cCommon.ToDropDown(dtServers, "serverName", "serverName", "All");
            return View(oHighCPUConsumedQueries);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oHighCPUConsumedQueries = new HighCPUConsumedQueries();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oHighCPUConsumedQueries);
        }
        public JsonResult GetList(string server, string count)
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
            oHighCPUConsumedQueries = new HighCPUConsumedQueries();
            oHighCPUConsumedQueries.GetList(server, count);
            var jsonResult = Json(oHighCPUConsumedQueries, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}