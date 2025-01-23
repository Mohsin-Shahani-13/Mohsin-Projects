using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.Logistics.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.Logistics.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DockReceiptsController : Controller
    {
        // GET: Logistics/DockReceipts
        DockReceipts oDockReceipts = new DockReceipts();
        public ActionResult Option()
        {
            DockReceipts oDockReceipts = new DockReceipts();
            return View(oDockReceipts);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            DockReceipts oDockReceipts = new DockReceipts();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oDockReceipts);
        }

        public JsonResult GetList(string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            DockReceipts oDockReceipts = new DockReceipts();
            oDockReceipts.GetList(programId, ProgramName);
            var jsonResult = Json(oDockReceipts, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetDetail(string ROHeaderId)
        {

            oDockReceipts = new DockReceipts();
            ViewBag.ReportTitle = " RO Header Id  = '" + ROHeaderId + "' ";

            //if (!string.IsNullOrEmpty(partNo))
            //    ViewBag.ReportTitle += "| Part No. = '" + partNo + "' ";

            bool success = oDockReceipts.GetDetail(ROHeaderId);
            oDockReceipts.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            if (success)
                return View(oDockReceipts);
            else
                return View();
        }
    }
}