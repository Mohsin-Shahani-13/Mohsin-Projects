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
    public class JobController : Controller
    {
        // GET: Jobs/Job 
        Job oJob;
        public ActionResult Option(string serverName)
        {
            oJob = new Job();
            DataTable dtServers = oJob.GetServerNames(serverName);
            ViewBag.ddlServers = cCommon.ToDropDown(dtServers, "serverName", "serverName", "All");
            return View(oJob);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oJob = new Job();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oJob);
        }
        public JsonResult GetList(string server)
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
            oJob = new Job();
            oJob.GetList(server);
            var jsonResult = Json(oJob, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult GetDetail(string jobName) 
        {
            oJob = new Job();
            ViewBag.ReportTitle = " Detail for  = '" + jobName + "' ";

            bool success = oJob.GetDetail(jobName);
            oJob.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            if (success)
                return View(oJob);
            else
                return View();
        }
    }
}