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
    public class GoodInventoryReadytoShipController : Controller
    {
        GoodInventoryReadytoShip oGoodInventoryReadytoShip;
        // GET: ListingReports/GoodInventoryReadytoShip
        public ActionResult Option()
        {
            oGoodInventoryReadytoShip = new GoodInventoryReadytoShip();
            return View(oGoodInventoryReadytoShip);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oGoodInventoryReadytoShip = new GoodInventoryReadytoShip();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oGoodInventoryReadytoShip);
        }
        public JsonResult GetList(string partNo, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oGoodInventoryReadytoShip = new GoodInventoryReadytoShip();
            oGoodInventoryReadytoShip.GetList(partNo, programId, ProgramName);
            var jsonResult = Json(oGoodInventoryReadytoShip, JsonRequestBehavior.AllowGet);
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