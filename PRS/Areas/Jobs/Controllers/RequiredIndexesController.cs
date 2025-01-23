using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.Jobs.Models;
using System.Web.Mvc;
using IP.Models;
using System.Data;


namespace IP.Areas.Jobs.Controllers
{
    public class RequiredIndexesController : Controller
    {
        // GET: Jobs/RequiredIndexes
        RequiredIndexes oRequiredIndexes = new RequiredIndexes();
        public ActionResult Option(string serverName, string Database)
        {
            oRequiredIndexes = new RequiredIndexes();
            DataTable dtServers = oRequiredIndexes.ServerNames(serverName);
            ViewBag.ddlServers = cCommon.ToDropDown(dtServers, "serverName", "serverName", "All");
            return View(oRequiredIndexes);
        }
        public JsonResult GetDatabase(string serverName)
        {
            oRequiredIndexes = new RequiredIndexes();
            oRequiredIndexes.Database(serverName);
            var jsonResult = Json(oRequiredIndexes, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Index(string menuTitle, string RptCode, string serverName, string Database)
        {
            oRequiredIndexes = new RequiredIndexes();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRequiredIndexes);
        }
        public JsonResult GetList(string serverName, string database)
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
            oRequiredIndexes = new RequiredIndexes();
            oRequiredIndexes.GetList(serverName, database);
            var jsonResult = Json(oRequiredIndexes, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}