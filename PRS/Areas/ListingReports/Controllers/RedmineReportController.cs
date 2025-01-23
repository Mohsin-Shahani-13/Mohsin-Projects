using IP.Areas.ListingReports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.ListingReports.Controllers
{
    public class RedmineReportController : Controller
    {
        RedmineReport oRedmineReport;
        // GET: ListingReports/RedmineReport
        public ActionResult Option()
        {
            oRedmineReport = new RedmineReport();
            DataTable dtProgram = oRedmineReport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "program", "program", "All");
            DataTable dtStatus = oRedmineReport.GetStatus();
            ViewBag.ddStatus = cCommon.ToDropDown(dtStatus, "status", "status", "All");
            return View(oRedmineReport);
        }
        public ActionResult Index(string menuTitle, string rptCode)
        {
            oRedmineReport = new RedmineReport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRedmineReport);
        }
        public JsonResult GetList(string program, string status)
        {
            oRedmineReport = new RedmineReport();
            string menuTitle = string.Empty;
            string RptCode = string.Empty;


            oRedmineReport.GetList(program, status);
            var jsonResult = Json(oRedmineReport, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"].ToString();
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }

        [HttpPost]
        public ActionResult SaveData(List<RedmineReport> dataList)
        {
            if (dataList != null || dataList.Count != 0)
            {
                try
                {
                    oRedmineReport = new RedmineReport();
                    oRedmineReport.SaveData(dataList);

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    // Handle exception and return error message
                    return Json(new { success = false, message = ex.Message });
                }
            }
            else
            {
                return Json(new { success = false, message = "No data received" });
            }
        }

        [HttpPost]
        public ActionResult SaveRemarks(string id, string remarks)
        {
            try
            {
                oRedmineReport = new RedmineReport();
                oRedmineReport.SaveRemarks(id, remarks);
                var jsonResult = Json(oRedmineReport, JsonRequestBehavior.AllowGet);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateMultipleStatuses(List<RedmineReport> issueUpdates)
        {
            oRedmineReport = new RedmineReport();

            var updates = issueUpdates.Select(u => Tuple.Create(u.IssueId, u.StatusId)).ToList();
            var response = await oRedmineReport.UpdateMultipleStatuses(issueUpdates);

            //if (response)
            //{
               return Json(new { success = true });
            //}
            //else
            //{
            //    return Json(new { success = false, error = response });
            //}
            
        }
    }
}