using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.Classess;
using IP.ActionFilters;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MaterialInventoryController : Controller
    {
        // GET: ListingReports/MaterialInventory
        MaterialInventory oMaterialInventory;
        public ActionResult Option()
        {
            oMaterialInventory = new MaterialInventory();
            return View(oMaterialInventory);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMaterialInventory = new MaterialInventory();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oMaterialInventory);
        }
        public JsonResult GetList(string partNo, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMaterialInventory = new MaterialInventory();
            oMaterialInventory.GetList(partNo, programId, ProgramName);
            var jsonResult = Json(oMaterialInventory, JsonRequestBehavior.AllowGet);
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