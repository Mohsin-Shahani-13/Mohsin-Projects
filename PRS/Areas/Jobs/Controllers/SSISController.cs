using IP.Areas.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.Jobs.Controllers
{
    public class SSISController : Controller
    {
        // GET: Jobs/SSIS
        SSIS oSSIS = new SSIS();
        public ActionResult Option()
        {
            DataTable dtStatus = oSSIS.Status();
            ViewBag.ddlStatus = cCommon.ToDropDown(dtStatus, "Status", "Status", "All");
            return View(oSSIS);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oSSIS = new SSIS();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oSSIS);
        }
        public JsonResult GetList(string jobName, string status, string ProgramId, string ProgramName, string jobStatus)
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
            oSSIS = new SSIS();
            oSSIS.GetList(jobName, status, ProgramId, ProgramName, jobStatus);
            var jsonResult = Json(oSSIS, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        //public JsonResult RunJob(string jobName)
        //{
        //    oSSIS = new SSIS();
        //    bool jobStatus = oSSIS.RunJob(jobName);
        //    //return Json(new { Status = jobStatus, Message = oSSIS.ErrorMessage});
        //    return Json(new { Status = jobStatus, Message = oSSIS.ErrorMessage }, JsonRequestBehavior.AllowGet);
        //    //return jsonResult;
        //}
    }
}