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
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class FaultCodeTest2StationController : Controller
    {
        // GET: SupplyChain/FaultCodeTest2Station
        FaultCodeTest2Station oFaultCodeTest2Station;
        public ActionResult Option()
        {
            oFaultCodeTest2Station = new FaultCodeTest2Station();
            return View(oFaultCodeTest2Station);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oFaultCodeTest2Station = new FaultCodeTest2Station();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oFaultCodeTest2Station);
        }
        public JsonResult GetList(string frmDt, string toDate, string SerialNo, string programId, String ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oFaultCodeTest2Station = new FaultCodeTest2Station();
            oFaultCodeTest2Station.GetList(frmDt, toDate, SerialNo, programId, ProgramName);
            var jsonResult = Json(oFaultCodeTest2Station, JsonRequestBehavior.AllowGet);
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