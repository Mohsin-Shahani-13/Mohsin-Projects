using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PendingPartController : Controller
    {
        // GET: SupplyChain/PendingPart
        PendingPart oPendingPart;
        public ActionResult Option()
        {
            oPendingPart = new PendingPart();
            DataTable dtProgram = oPendingPart.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oPendingPart);
        }

        public ActionResult Index(string menuTitle, string RptCode)
        {
            oPendingPart = new PendingPart();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oPendingPart);
        }
        public JsonResult GetList(string programID, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode = string.Empty;

            oPendingPart = new PendingPart();
            oPendingPart.GetList(programID, programName);
            var jsonResult = Json(oPendingPart, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU &LOG QUERY
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
    }
}