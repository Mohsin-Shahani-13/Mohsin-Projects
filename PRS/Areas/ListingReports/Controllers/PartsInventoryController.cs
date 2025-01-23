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
    public class PartsInventoryController : Controller
    {
        // GET: ListingReports/PartsInventory
        PartsInventory oPartsInventory = new PartsInventory();

        public ActionResult Option (string programId, string ProgramName)
        {
            oPartsInventory = new PartsInventory();
            DataTable dtwarehouse = oPartsInventory.Getwarehouse(programId, ProgramName);
            ViewBag.ddlWarehouse = cCommon.ToDropDown(dtwarehouse, "Warehouse", "Warehouse", "All");

            return View(oPartsInventory);
        }


        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPartsInventory = new PartsInventory();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.programName = Request.QueryString["ProgramName"];
            return View(oPartsInventory);
        }

        public JsonResult GetList(string partNo, string programId, string ProgramName, string warehouse)
        {
            string menuTitle = string.Empty;
            string RptCode;
            

            oPartsInventory = new PartsInventory();
            oPartsInventory.GetList(partNo, programId, ProgramName, warehouse);
            var jsonResult = Json(oPartsInventory, JsonRequestBehavior.AllowGet);
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

        //public ActionResult GetSerials(string partNo, string locNo)
        //{
        //    oPartsInventory = new PartsInventory();
        //    try
        //    {
        //        ViewBag.ReportTitle = "Serial No. List";
        //        bool success = oPartsInventory.GetSerials(partNo, locNo);

        //        if (success)
        //            return View(oPartsInventory);
        //        else
        //            return View();
        //    }
        //    catch (Exception e)
        //    {
        //        ViewBag.ErrMessage = e.Message;
        //        return View();
        //    }
        //}
    }
}