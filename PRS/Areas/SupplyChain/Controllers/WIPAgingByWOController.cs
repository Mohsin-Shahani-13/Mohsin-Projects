using IP.Areas.SupplyChain.Models;
using IP.Classess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class WIPAgingByWOController : Controller
    {
        WIPAgingByWO oWIPAgingByWO = new WIPAgingByWO();
        public ActionResult Option()
        {
            oWIPAgingByWO = new WIPAgingByWO();
            return View();
        }
        // GET: SupplyChain/WIPAgingByWO
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oWIPAgingByWO = new WIPAgingByWO();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oWIPAgingByWO);
        }
        public ActionResult GetData(string workstation, string frmDt, string toDt)
        {
            oWIPAgingByWO = new WIPAgingByWO();
            ViewBag.ReportTitle = "WIP By Orders > Workstation  = '" + workstation + "' | Aging = '" + frmDt + " Days" + "'";
            if (frmDt == "1" && toDt != "10000" && workstation != "null")
            {
                ViewBag.ReportTitle = "WIP By Orders > Workstation  = '" + workstation + "' | Aging = '" + frmDt + " Day" + "'";
            }
            else if (frmDt == "0" && toDt == "0" && workstation != "null")
            {
                ViewBag.ReportTitle = "WIP By Orders > Workstation  = '" + workstation + "' | '" + "NO Aging" + "'";
            }
            else if (frmDt == "6" && toDt == "9" && workstation != "null")
            {
                ViewBag.ReportTitle = "WIP By Orders > Workstation  = '" + workstation + "' | Aging = '" + frmDt + "-" + toDt + " Days" + "'";
            }
            else if (frmDt == "10" && toDt == "15" && workstation != "null")
            {
                ViewBag.ReportTitle = "WIP By Orders > Workstation  = '" + workstation + "' | Aging = '" + frmDt + "-" + toDt + " Days" + "'";
            }
            else if (frmDt == "16" && toDt == "30" && workstation != "null")
            {
                ViewBag.ReportTitle = "WIP By Orders > Workstation  = '" + workstation + "' | Aging = '" + frmDt + "-" + toDt + " Days" + "'";
            }
            else if (frmDt == "31" && toDt == "10000" && workstation != "null")
            {
                ViewBag.ReportTitle = "WIP By Orders > Workstation  = '" + workstation + "' | Aging '" + " >30 Days" + "'";
            }
            else if (frmDt == "0" && toDt == "10000" && workstation != "null")
            {
                ViewBag.ReportTitle = "WIP By Orders > Workstation  = '" + workstation + "'";
            }
            else if (workstation == "null" && frmDt == "0" && toDt == "10000")
            {
                ViewBag.ReportTitle = "WIP By Orders";
            }
            else if (workstation == "null" && frmDt == "0" && toDt == "0")
            {
                ViewBag.ReportTitle = "WIP By Orders > '" + "NO Aging" + "'";
            }
            else if (workstation == "null" && frmDt == "1" && toDt == "1")
            {
                ViewBag.ReportTitle = "WIP By Orders > Aging = '" + frmDt + " Day" + "'";
            }
            else if (workstation == "null" && frmDt == "2" && toDt == "2")
            {
                ViewBag.ReportTitle = "WIP By Orders > Aging = '" + frmDt + " Days" + "'";
            }
            else if (workstation == "null" && frmDt == "3" && toDt == "3")
            {
                ViewBag.ReportTitle = "WIP By Orders > Aging = '" + frmDt + " Days" + "'";
            }
            else if (workstation == "null" && frmDt == "4" && toDt == "4")
            {
                ViewBag.ReportTitle = "WIP By Orders > Aging = '" + frmDt + " Days" + "'";
            }
            else if (workstation == "null" && frmDt == "5" && toDt == "5")
            {
                ViewBag.ReportTitle = "WIP By Orders > Aging = '" + frmDt + " Days" + "'";
            }
            else if (workstation == "null" && frmDt == "6" && toDt == "9")
            {
                ViewBag.ReportTitle = "WIP By Orders > Aging = '" + frmDt + "-9 Days" + "'";
            }
            else if (workstation == "null" && frmDt == "10" && toDt == "15")
            {
                ViewBag.ReportTitle = "WIP By Orders > Aging = '" + frmDt + "-15 Days" + "'";
            }
            else if (workstation == "null" && frmDt == "16" && toDt == "30")
            {
                ViewBag.ReportTitle = "WIP By Orders > Aging = '" + frmDt + "-30 Days" + "'";
            }
            else if (workstation == "null" && frmDt == "31" && toDt == "10000")
            {
                ViewBag.ReportTitle = "WIP By Orders > Aging >= '" + frmDt + " Days" + "'";
            }
            if (workstation == "HOLD")
            {
                string status = ViewBag.ReportTitle;
                if (status.Contains("Workstation"))
                {
                    string status2 = status.Replace("Workstation", "Status");
                    ViewBag.ReportTitle = status2;
                }
              
            }
            //cSite oSite = new cSite(); commit: 1
            //string programId = oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["DefaultSite"].ToString()); // it was not used. it just checking the value is not null. commit: 1

            //bool success = oWIPAgingByWO.GetWOUnit(workstation, frmDt, toDt, programId); //commit: 1
            bool success = oWIPAgingByWO.GetWOUnit(workstation, frmDt, toDt, "1");
            oWIPAgingByWO.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            if (success)
                return View(oWIPAgingByWO);
            else
                return View();
        }
        public JsonResult GetList(string programId, String ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oWIPAgingByWO = new WIPAgingByWO();
            oWIPAgingByWO.GetList(programId, ProgramName);
            var jsonResult = Json(oWIPAgingByWO, JsonRequestBehavior.AllowGet);
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