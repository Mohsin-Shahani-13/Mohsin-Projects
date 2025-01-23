using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using IP.Areas.Meta.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.Meta.Controllers
{
    public class DataWipeResultLogController : Controller
    {
        DataWipeResultLog oDataWipeResultLogTest;
        // GET: Meta/DataWipeResultLog
        public ActionResult Option()
        {
            oDataWipeResultLogTest = new DataWipeResultLog();
            DataTable dtProgram = oDataWipeResultLogTest.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "ProgramName", "");
            return View(oDataWipeResultLogTest);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDataWipeResultLogTest = new DataWipeResultLog();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string programName,string frmDt, string toDate, string SerialNo, string testerName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oDataWipeResultLogTest = new DataWipeResultLog();
            oDataWipeResultLogTest.GetList(programId, programName, frmDt, toDate, SerialNo, testerName);
            var jsonResult = Json(oDataWipeResultLogTest, JsonRequestBehavior.AllowGet);
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