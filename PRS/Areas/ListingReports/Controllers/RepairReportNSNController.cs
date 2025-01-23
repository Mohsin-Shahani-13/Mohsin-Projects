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
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class RepairReportNSNController : Controller
    {
        RepairReportNSN oRepairReportNSN;
        // GET: ListingReports/RepairReportNSN
        public ActionResult Option()
        {
            oRepairReportNSN = new RepairReportNSN();
            return View(oRepairReportNSN);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oRepairReportNSN = new RepairReportNSN();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRepairReportNSN);
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRepairReportNSN = new RepairReportNSN();
            oRepairReportNSN.GetList(frmDt, toDate, programId, ProgramName);
            var jsonResult = Json(oRepairReportNSN, JsonRequestBehavior.AllowGet);
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