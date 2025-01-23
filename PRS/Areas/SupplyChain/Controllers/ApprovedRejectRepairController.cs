using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    public class ApprovedRejectRepairController : Controller
    {
        // GET: SupplyChain/ApprovedRejectRepair
        ApprovedRejectRepair oApprovedRejectRepair;
        public ActionResult Option()
        {
            oApprovedRejectRepair = new ApprovedRejectRepair();
            return View(oApprovedRejectRepair);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oApprovedRejectRepair = new ApprovedRejectRepair();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oApprovedRejectRepair);
        }
        public JsonResult GetList(string partNo, string serialNo, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oApprovedRejectRepair = new ApprovedRejectRepair();
            oApprovedRejectRepair.GetList(partNo, serialNo, programId, ProgramName);
            var jsonResult = Json(oApprovedRejectRepair, JsonRequestBehavior.AllowGet);
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