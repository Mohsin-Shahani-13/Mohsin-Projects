using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    public class DailyProductionCopyController : Controller
    {

        // GET: ListingReports/DailyProductionCopy
        DailyProductionCopy oDailyProductionCopy;
        public ActionResult Option()
        {
            oDailyProductionCopy = new DailyProductionCopy();
            DataTable dtProgram = oDailyProductionCopy.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programID", "programName", "");
            //DataTable dtId = oDailyProductionCopy.GetIdBySite();
            //ViewBag.ddId = cCommon.ToDropDown(dtId, "ID", "ID", "");
            return View(oDailyProductionCopy); // why did we return odailyproduction
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oDailyProductionCopy = new DailyProductionCopy();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDailyProductionCopy);
        }

        public JsonResult GetList(string frmDt, string toDate, string Workstation, string serialNo, string WoHeaderId, string programID, string ProgramName, bool IsChecked, bool NotChecked)
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
            oDailyProductionCopy = new DailyProductionCopy();
            oDailyProductionCopy.GetList(frmDt, toDate, Workstation, serialNo, programID, ProgramName, WoHeaderId, IsChecked, NotChecked);
            var jsonResult = Json(oDailyProductionCopy, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult Details(string serialNo)
        {
 
             
            try
            {
                oDailyProductionCopy = new DailyProductionCopy();
                bool success = oDailyProductionCopy.GetDetail(serialNo);
                if (success)
                    return View(oDailyProductionCopy);
                else
                    return View(oDailyProductionCopy);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }
    }
}