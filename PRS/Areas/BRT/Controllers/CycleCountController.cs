using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.BRT.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.BRT.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class CycleCountController : Controller
    {
        // GET: BRT/CycleCount
        CycleCount oCycleCount;
        public ActionResult Option()
        {
            oCycleCount = new CycleCount();
            return View(oCycleCount);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oCycleCount = new CycleCount();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oCycleCount);
        }
        public JsonResult GetList(string partNo, string programId, string ProgramName, string Warehouse, string iteration, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oCycleCount = new CycleCount();
            oCycleCount.GetList(partNo, programId, ProgramName, Warehouse, iteration, frmDt, toDate);
            var jsonResult = Json(oCycleCount, JsonRequestBehavior.AllowGet);
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

        public ActionResult Detail(string programId, string program, string frmDt, string toDate, string no, string calender)
        {
            ViewBag.ReportTitle = "Cycle Count > Program = '" + program + "' " ;
            if (calender == "Days")
            {
                ViewBag.ReportTitle += " | For Date = '" + frmDt + "' ";
            }
            if (calender == "Weeks")
            {
                ViewBag.ReportTitle += " | Week Start Date = '" + frmDt + "' ";
            }
            if (calender == "Months")
            {
                ViewBag.ReportTitle += " | For Month = '" + frmDt + "' ";
            }
            if (string.IsNullOrEmpty(program))
                return View();
            try
            {
                oCycleCount = new CycleCount();
                bool success = oCycleCount.GetDetail(programId, program, frmDt, toDate, no, calender);
                oCycleCount.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                if (success)
                    return View(oCycleCount);
                else
                    return View(oCycleCount);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }

            return View();
        }
    }
}
