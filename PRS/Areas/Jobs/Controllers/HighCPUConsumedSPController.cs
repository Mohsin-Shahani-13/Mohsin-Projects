using IP.Areas.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc; 
namespace IP.Areas.Jobs.Controllers
{
    public class HighCPUConsumedSPController : Controller
    {
        HighCPUConsumedSP oHighCPUConsumedSP = new HighCPUConsumedSP();
        // GET: Jobs/HighCPUConsumedSP
        public ActionResult Option(string serverName)
        {
            oHighCPUConsumedSP = new HighCPUConsumedSP();
            DataTable dtServers = oHighCPUConsumedSP.GetServerNames(serverName);
            ViewBag.ddlServers = cCommon.ToDropDown(dtServers, "serverName", "serverName", "All");
            return View(oHighCPUConsumedSP);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oHighCPUConsumedSP = new HighCPUConsumedSP();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oHighCPUConsumedSP);
        }
        public JsonResult GetList(string server, string ProcName)
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
            oHighCPUConsumedSP = new HighCPUConsumedSP();
            oHighCPUConsumedSP.GetList(server, ProcName);
            var jsonResult = Json(oHighCPUConsumedSP, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}