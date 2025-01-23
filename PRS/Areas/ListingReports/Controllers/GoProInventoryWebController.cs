using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.Areas.ListingReports.Models;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    [SessionTimeout]

    public class GoProInventoryWebController : Controller
    {
        GoProInventoryWeb oGoProInventoryWeb = new GoProInventoryWeb();
        // GET: ListingReports/GoProInventoryWeb
        public ActionResult Option()
        {
            oGoProInventoryWeb = new GoProInventoryWeb();
            DataTable dtProgram = oGoProInventoryWeb.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oGoProInventoryWeb);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oGoProInventoryWeb = new GoProInventoryWeb();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oGoProInventoryWeb);
        }

        public JsonResult GetList(string programId, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oGoProInventoryWeb = new GoProInventoryWeb();
            oGoProInventoryWeb.GetList(programId, programName);
            var jsonResult = Json(oGoProInventoryWeb, JsonRequestBehavior.AllowGet);
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