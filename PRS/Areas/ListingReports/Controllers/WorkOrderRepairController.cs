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
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class WorkOrderRepairController : Controller
    {
        // GET: ListingReports/WorkOrderRepair

        WorkOrderRepair oWorkOrderRepair = new WorkOrderRepair();
        //public string manuTitle { get; private set; }

        public ActionResult Option()
        {
            oWorkOrderRepair = new WorkOrderRepair();
            DataTable dtProgram = oWorkOrderRepair.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "ProgramName", "");
            return View(oWorkOrderRepair);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oWorkOrderRepair = new WorkOrderRepair();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oWorkOrderRepair);
        }

        public JsonResult GetList(string frmDt, string toDt, string ProgramId, string ProgramName, string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oWorkOrderRepair = new WorkOrderRepair();
            oWorkOrderRepair.GetList(frmDt, toDt, ProgramId, ProgramName, custRef);
            var jsonResult = Json(oWorkOrderRepair, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog olog = new cLog();
                olog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
    }
}