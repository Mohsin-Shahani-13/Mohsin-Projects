using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IP.Areas.BRT.Models;
using System.Web.Mvc;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.BRT.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class BRTLookupController : Controller
    {
        // GET: BRT/BRTLookup
        BRTLookup oBRTLookup;
        public ActionResult Option(string programId, string ProgramName)
        {
            oBRTLookup = new BRTLookup();
            DataTable dtbuilding = oBRTLookup.Building(programId, ProgramName);
            ViewBag.ddlBuilding = cCommon.ToDropDown(dtbuilding, "Building", "Building", "");
            return View(oBRTLookup);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oBRTLookup = new BRTLookup();
            ViewBag.ReportTitle = menuTitle;
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oBRTLookup); return View();
        }

        public JsonResult GetList(string building)
        {
            string menuTitle = string.Empty;
            string RptCode;
            TempData["building"] = building;
            oBRTLookup = new BRTLookup();
            oBRTLookup.GetList(building);
            var jsonResult = Json(oBRTLookup, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetData(string bay, string building)
        {
            //string building = TempData["building"].ToString();
            //TempData.Keep();

            oBRTLookup = new BRTLookup();
            ViewBag.bay = bay;
            ViewBag.building = building;
            ViewBag.ReportTitle = "BRT Lookup For Building = '" + building + "' | Bay = '" + bay + "'";
            oBRTLookup.GetRowList(bay, building);
            oBRTLookup.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return View(oBRTLookup);

        }

        public ActionResult Location(string building, string bay, string tier, string row)
        {
            oBRTLookup = new BRTLookup();

            ViewBag.ReportTitle = "Location For Row = '" + row + "' ";
            if (!string.IsNullOrEmpty(tier))
                ViewBag.ReportTitle += "| Tier = '" + tier + "' ";
            oBRTLookup.GetLocaton(building, bay, tier, row);
            oBRTLookup.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return View(oBRTLookup);
        }

        public ActionResult SerialNo(string locationId, string partNo)
        {
            oBRTLookup = new BRTLookup();
            ViewBag.ReportTitle = "Serial No. For Part No. = '" + partNo + "' ";
            bool success = oBRTLookup.GetSerial(locationId, partNo);
            oBRTLookup.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return View(oBRTLookup);
        }

        public ActionResult LocationByPart(string locationId, string partNo)
        {
            oBRTLookup = new BRTLookup();
            ViewBag.ReportTitle = "Location For Part No. = '" + partNo + "' ";
            bool success = oBRTLookup.GetLocationByPart(locationId, partNo);
            oBRTLookup.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return View(oBRTLookup);
        }
    }
}
