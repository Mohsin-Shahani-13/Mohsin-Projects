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
    public class WIPAgingController : Controller
    {
        WIPAging oWIPAging = new WIPAging();
        // GET: SupplyChain/WIPAging
        public ActionResult Option()
        {
            oWIPAging = new WIPAging();
            return View(oWIPAging);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oWIPAging = new WIPAging();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oWIPAging);
        }
        public JsonResult GetList(string IdleOver, string programId, String ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oWIPAging = new WIPAging();
            oWIPAging.GetList(IdleOver, programId, ProgramName);
            var jsonResult = Json(oWIPAging, JsonRequestBehavior.AllowGet);
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