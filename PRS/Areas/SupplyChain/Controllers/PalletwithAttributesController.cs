using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;
namespace IP.Areas.SupplyChain
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PalletwithAttributesController : Controller
    {
        PalletwithAttributes oPalletwithAttributes;
        // GET: SupplyChain/PalletwithAttributes
        public ActionResult Option()
        {
            oPalletwithAttributes = new PalletwithAttributes();
            DataTable dtProgram = oPalletwithAttributes.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");

            // Get the initially selected program ID (e.g., the first one in the program list)
            var ProgramId = dtProgram.Rows.Count > 0 ? Convert.ToInt32(dtProgram.Rows[0]["programID"]) : 0;
            DataTable ddwarehouse = oPalletwithAttributes.GetWarehouse(ProgramId);
            ViewBag.ddwarehouse = cCommon.ToDropDown(ddwarehouse, "Warehouse", "Warehouse", "All");
            //return PartialView("GetProgramBySite");
            return View(oPalletwithAttributes);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPalletwithAttributes = new PalletwithAttributes();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oPalletwithAttributes);
        }
        public JsonResult GetList(string programId, string programName, string warehouse)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oPalletwithAttributes = new PalletwithAttributes();
            oPalletwithAttributes.GetList(programId, programName, warehouse);
            var jsonResult = Json(oPalletwithAttributes, JsonRequestBehavior.AllowGet);
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
        public JsonResult GetWarehousesByProgram(int ProgramId)
        {
            oPalletwithAttributes = new PalletwithAttributes();
            // Query the warehouses for the given programId
            DataTable dtWarehouseForPartSerialView = new DataTable();
            dtWarehouseForPartSerialView = oPalletwithAttributes.GetWarehouse(ProgramId);
            var warehouseList = cCommon.ToDropDown(dtWarehouseForPartSerialView, "Warehouse", "Warehouse", "All");

            return Json(warehouseList, JsonRequestBehavior.AllowGet);
        }
    }
}