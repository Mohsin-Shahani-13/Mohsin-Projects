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

namespace IP.Areas.SupplyChain.Controllers
{
    public class ShipmentDocumentController : Controller
    {
        ShipmentDocument oShipmentDocument;
        // GET: SupplyChain/ShipmentDocument
        public ActionResult Option()
        {
            oShipmentDocument = new ShipmentDocument();
            return View(oShipmentDocument);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oShipmentDocument = new ShipmentDocument();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            //ViewBag.ShipmentDate = "S";
            return View(oShipmentDocument);
        }
        public JsonResult GetList(string CustRef)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oShipmentDocument = new ShipmentDocument();
            oShipmentDocument.GetList(CustRef);
            ViewBag.CustomerReference = oShipmentDocument.CustomerReference;
            ViewBag.Name = oShipmentDocument.Name;
            ViewBag.Address = oShipmentDocument.Address;
            ViewBag.City = oShipmentDocument.City;
            ViewBag.State = oShipmentDocument.State;
            ViewBag.Zip = oShipmentDocument.Zip;
            ViewBag.Phone = oShipmentDocument.Phone;
            ViewBag.CONSIGNEEName = oShipmentDocument.CONSIGNEEName;
            ViewBag.CONSIGNEEAddress = oShipmentDocument.CONSIGNEEAddress;
            ViewBag.CONSIGNEECity = oShipmentDocument.CONSIGNEECity;
            ViewBag.ShipmentDate = oShipmentDocument.ShipmentDate;
            var jsonResult = Json(oShipmentDocument, JsonRequestBehavior.AllowGet);
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