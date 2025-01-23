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
    public class MinMaxLevelController : Controller
    {
        MinMaxLevel oMinMaxLevel;
        // GET: SupplyChain/MinMaxLevel
        public ActionResult Option()
        {
            oMinMaxLevel = new MinMaxLevel();
            return View(oMinMaxLevel);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMinMaxLevel = new MinMaxLevel();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oMinMaxLevel);
        }
        public JsonResult GetList(string partNo, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMinMaxLevel = new MinMaxLevel();
            oMinMaxLevel.GetList(partNo, programId, ProgramName);
            var jsonResult = Json(oMinMaxLevel, JsonRequestBehavior.AllowGet);
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