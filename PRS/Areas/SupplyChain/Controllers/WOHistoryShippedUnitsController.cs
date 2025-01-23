using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;
namespace IP.Areas.SupplyChain.Controllers
{
   
    public class WOHistoryShippedUnitsController : Controller
    {
        WOHistoryShippedUnits oWOHistoryShippedUnits;
        // GET: SupplyChain/WOHistoryShippedUnits
        public ActionResult Option()
        {
            oWOHistoryShippedUnits = new WOHistoryShippedUnits();
            DataTable dtProgram = oWOHistoryShippedUnits.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oWOHistoryShippedUnits);
        }

        public ActionResult Index(string programId, string ProgramName, string frmDt, string toDate, string menuTitle, string RptCode)
        {
            
            try
            {
                WOHistoryShippedUnits oWOHistoryShippedUnits = new WOHistoryShippedUnits();
                ViewBag.ProgramID = programId;
                ViewBag.ProgramName = ProgramName;

                bool success = oWOHistoryShippedUnits.GetList(programId, ProgramName, frmDt, toDate);
                if (!string.IsNullOrEmpty(ProgramName))
                    ViewBag.ReportTitle = "Work Order History For Shipped Units > Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDate + "' ";
                else
                    ViewBag.ReportTitle = "Work Order History For Shipped Units";

                //LOAD MRU & LOG QUERY
                if (ViewBag.ReportTitle != null && RptCode != null)
                {

                    menuTitle = "Work Order History For Shipped Units";
                    cLog oLog = new cLog();
                    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }

                if (success)
                    return View(oWOHistoryShippedUnits);
                else
                    return View();
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }

        }
        //public JsonResult GetList(string programId, string ProgramName, string frmDt, string toDate)
        //{
        //    string menuTitle = string.Empty;
        //    string RptCode;
        //    oWOHistoryShippedUnits = new WOHistoryShippedUnits();
        //    oWOHistoryShippedUnits.GetList(programId, ProgramName, frmDt, toDate);
        //    var jsonResult = Json(oWOHistoryShippedUnits, JsonRequestBehavior.AllowGet);
        //    jsonResult.MaxJsonLength = int.MaxValue;
        //    //LOAD MRU & LOG QUERY
        //    if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
        //    {
        //        menuTitle = TempData["ReportTitle"] as string;
        //        RptCode = TempData["RptCode"].ToString();
        //        TempData.Keep();
        //        cLog oLog = new cLog();
        //        oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
        //    }
        //    return jsonResult;
        //}
    }
}