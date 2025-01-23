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
    public class RICInventoryControlController : Controller
    {
        // GET: SupplyChain/RICInventoryControl
        RICInventoryControl oRICInventoryControl;
        public ActionResult Option()
        {
            oRICInventoryControl = new RICInventoryControl();
            DataTable dtProgram = oRICInventoryControl.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oRICInventoryControl);
        }
        public ActionResult Index(string menuTitle, string rptCode)
        {
            oRICInventoryControl = new RICInventoryControl();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRICInventoryControl);
        }
        public JsonResult GetList(string programId, string programName, string location, string customerPO, string RMA, string standardName)
        {
            oRICInventoryControl = new RICInventoryControl();
            string menuTitle = string.Empty;
            string RptCode = string.Empty;


            oRICInventoryControl.GetList(programId, programName, location, customerPO, RMA, standardName);
            var jsonResult = Json(oRICInventoryControl, JsonRequestBehavior.AllowGet);
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
    }
}