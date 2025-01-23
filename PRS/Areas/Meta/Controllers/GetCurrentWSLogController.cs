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
    public class GetCurrentWSLogController : Controller
    {
        // GET: Meta/GetCurrentWSLog
        GetCurrentWSLog oGetCurrentWSLog;
        public ActionResult Option()
        {
            oGetCurrentWSLog = new GetCurrentWSLog();
            return View(oGetCurrentWSLog);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oGetCurrentWSLog = new GetCurrentWSLog();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string frmDt, string toDate, string SerialNo, string testerName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oGetCurrentWSLog = new GetCurrentWSLog();
            oGetCurrentWSLog.GetList(frmDt, toDate, SerialNo, testerName);
            var jsonResult = Json(oGetCurrentWSLog, JsonRequestBehavior.AllowGet);
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