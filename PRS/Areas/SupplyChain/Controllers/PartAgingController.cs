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
    public class PartAgingController : Controller
    {
        PartAging oPartAging = new PartAging();
        // GET: SupplyChain/PartAging
        public ActionResult Option()
        {
            oPartAging = new PartAging();
            return View(oPartAging);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPartAging = new PartAging();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oPartAging);
        }
        public JsonResult GetList(string IdleOver, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oPartAging = new PartAging();
            oPartAging.GetList(IdleOver, programId, ProgramName);
            var jsonResult = Json(oPartAging, JsonRequestBehavior.AllowGet);
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