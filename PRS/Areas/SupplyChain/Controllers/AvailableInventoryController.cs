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
    public class AvailableInventoryController : Controller
    {
        // GET: SupplyChain/AvailableInventory
        AvailableInventory oAvailableInventory;
        public ActionResult Option()
        {
            oAvailableInventory = new AvailableInventory();
            DataTable dtProgram = oAvailableInventory.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oAvailableInventory);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oAvailableInventory = new AvailableInventory();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oAvailableInventory);
        }
        public JsonResult GetList(string programID, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode = string.Empty;

            oAvailableInventory = new AvailableInventory();
            oAvailableInventory.GetList(programID, programName);
            var jsonResult = Json(oAvailableInventory, JsonRequestBehavior.AllowGet);
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