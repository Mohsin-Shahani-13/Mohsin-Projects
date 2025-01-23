using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.SupplyChain.Models;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class WOController : Controller
    {
        WO oWO = new WO();
        // GET: SupplyChain/WO
        public ActionResult Option()
        {
            WO oWO = new WO();


            return View(oWO);
        }
        public ActionResult OptionBose()
        {
            DataTable dtProgram = oWO.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            //DataTable dtStatus = oWO.Status();
            //ViewBag.ddlStatus = cCommon.ToDropDown(dtStatus, "Id", "Description", "All");
            //ViewBag.Option = "op";
            //return View(oWO);

            //DataTable dtProgram = oWO.GetProgramBySite();
            //ViewBag.ddprogram = cCommon.ToDropDown(dtProgram, "programID", "programName", "");


            bool success = oWO.GetStatus();

            bool success1 = oWO.Repairtype();
            if (success && success1)
                return View(oWO);
            else
                return View();
        }

        public ActionResult Index(string rptCode, string menuTitle, string Option, string type)
        {
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ReportTitle1 = type;
            ViewBag.Option = Option;
            //cLog oLog = new cLog();
            //oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, rptCode);
            TempData.Keep();
            return View();
        }

        public JsonResult GetWO(string Id, string frmDt, string toDate, bool isAllDate, bool ischecked3, string custRef, bool ischecked)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oWO = new WO();
            oWO.GetWO(Id, frmDt, toDate, isAllDate, ischecked3, custRef, ischecked);
            var jsonResult = Json(oWO, JsonRequestBehavior.AllowGet);
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



        public ActionResult Detail(string Id, string custRef)
        {
            if (string.IsNullOrEmpty(custRef))
                return View();
            try
            {
                oWO = new WO();
                bool success = oWO.GetDetail(Id, custRef);
                if (success)
                    return View(oWO);
                else
                    return View(oWO);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }

        public ActionResult WOUnit(string Id , string StatusId)
        {
            //try
            //{
            //    oWO = new WO();
            //    ViewBag.ReportTitle = "WO Unit";
            //    ViewBag.data = null;
            //    if (StatusId != null)
            //    {
            //        ViewBag.data = " > Consumed ";
            //    }
            //    else
            //    {
            //        ViewBag.data = " > Requested ";
            //    }
            //    bool success = oWO.GetWOUnit(Id, StatusId);
            //    if (success)
            //        return View(oWO);
            //    else
            //        return View();
            //}
            //catch (Exception e)
            //{
            //    ViewBag.ErrMessage = e.Message;
            //    return View();
            //}
            return View();
        }


        public ActionResult repairedUnits(string rptCode, string menuTitle)
        {
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            //cLog oLog = new cLog();
            //oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, rptCode);
            TempData.Keep();
            return View();

        }
        public JsonResult GetRepairedUnits(string frmDt, string toDate, string custRef, string status, string statusid, string Repair, string RepairTypeID, string programId, string ProgramName, bool ischecked2)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oWO = new WO();
            oWO.GetRepairedUnit(frmDt, toDate, custRef, status, statusid, Repair, RepairTypeID, programId, ProgramName, ischecked2);
            var jsonResult = Json(oWO, JsonRequestBehavior.AllowGet);
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