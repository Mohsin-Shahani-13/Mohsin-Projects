using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class OpenWipOrdersController : Controller
    {
        // GET: SupplyChain/OpenWipOrders
        OpenWipOrders oOpenWipOrders;
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oOpenWipOrders = new OpenWipOrders();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oOpenWipOrders);
        }

        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oOpenWipOrders = new OpenWipOrders();
            oOpenWipOrders.GetList();
            var jsonResult = Json(oOpenWipOrders, JsonRequestBehavior.AllowGet);
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