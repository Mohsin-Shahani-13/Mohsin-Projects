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
    public class SOPickListController : Controller
    {
        // GET: SupplyChain/SOPickList
        SOPickList oSOPickList = new SOPickList();
        public ActionResult option()
        {
            SOPickList oSOPickList = new SOPickList();
            return View();
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            SOPickList oSOPickList = new SOPickList();
            TempData["ReportTitle"] = menuTitle + " Not Shipped"; 
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle + " Not Shipped"; 
            return View(oSOPickList);
        }

        public JsonResult GetList(string programId, string ProgramName, string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;
            SOPickList oSOPickList = new SOPickList();
            oSOPickList.GetList(programId, ProgramName, custRef);
            var jsonResult = Json(oSOPickList, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetERSPickList(string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;
            menuTitle = "ERS Pick List";
            RptCode = "127";

            SOPickList oSOPickList = new SOPickList();
            oSOPickList.GetERSPickList(custRef);
            var jsonResult = Json(oSOPickList, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            cLog oLog = new cLog();
            oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            return jsonResult;
        }

        public ActionResult GetNotReserved(string programId, string ProgramName, string custRef, string rptCode)
        {
            //SOPickList oSOPickList = new SOPickList();
            TempData["ReportTitle"] =  "SO Pick List Not Reserved";
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = "SO Pick List Not Reserved";
            return View();
        }
        public JsonResult GetNot_Reserved(string programId, string ProgramName, string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;
            SOPickList oSOPickList = new SOPickList();
            oSOPickList.GetNotReserved(programId, ProgramName, custRef);
            var jsonResult = Json(oSOPickList, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetClearToBuild(string programId, string ProgramName, string custRef, string rptCode)
        {
            //SOPickList oSOPickList = new SOPickList();
            TempData["ReportTitle"] = "SO Pick List Clear to Build";
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = "SO Pick List Clear to Build";
            return View();
        }
        public JsonResult GetClear_ToBuild(string programId, string ProgramName, string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;
            SOPickList oSOPickList = new SOPickList();
            oSOPickList.GetClearToBuild(programId, ProgramName, custRef);
            var jsonResult = Json(oSOPickList, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetSOPickList()
        {
            SOPickList oSOPickList = new SOPickList();
            return View();
        }

    }
}