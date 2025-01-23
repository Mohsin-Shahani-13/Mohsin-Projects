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
    public class MetaTestRunFaultCodesController : Controller
    {
        MetaTestRunFaultCodes oMetaTestRunFaultCodes = new MetaTestRunFaultCodes();
        // GET: Meta/MetaTestRunFaultCodes

        public ActionResult Option()
        {
            oMetaTestRunFaultCodes = new MetaTestRunFaultCodes();
            return View(oMetaTestRunFaultCodes);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaTestRunFaultCodes = new MetaTestRunFaultCodes();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oMetaTestRunFaultCodes);
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaTestRunFaultCodes = new MetaTestRunFaultCodes();
            oMetaTestRunFaultCodes.GetList(frmDt, toDate, programId, ProgramName);
            var jsonResult = Json(oMetaTestRunFaultCodes, JsonRequestBehavior.AllowGet);
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