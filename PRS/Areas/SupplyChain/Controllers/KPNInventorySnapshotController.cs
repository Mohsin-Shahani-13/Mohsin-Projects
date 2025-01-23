using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.Areas.SupplyChain.Models;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    public class KPNInventorySnapshotController : Controller
    {
        // GET: SupplyChain/KPNInventorySnapshot
        KPNInventorySnapshot oKPNInventorySnapshot = new KPNInventorySnapshot();
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oKPNInventorySnapshot = new KPNInventorySnapshot();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oKPNInventorySnapshot);
        }

        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oKPNInventorySnapshot = new KPNInventorySnapshot();
            oKPNInventorySnapshot.GetList();
            var jsonResult = Json(oKPNInventorySnapshot, JsonRequestBehavior.AllowGet);
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