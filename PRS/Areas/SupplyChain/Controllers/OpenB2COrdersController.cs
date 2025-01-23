using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.SupplyChain.Controllers
{
    public class OpenB2COrdersController : Controller
    {
        // GET: SupplyChain/OpenB2COrders
        OpenB2COrders oOpenB2COrders;
        public ActionResult Option()
        {
            oOpenB2COrders = new OpenB2COrders();
            DataTable dtProgram = oOpenB2COrders.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oOpenB2COrders);
        }
        public ActionResult Index(string menuTitle, string rptCode)
        {
            oOpenB2COrders = new OpenB2COrders();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oOpenB2COrders);
        }
        public JsonResult GetList(string programId, string programName)
        {
            oOpenB2COrders = new OpenB2COrders();
            string menuTitle = string.Empty;
            string RptCode = string.Empty;

            oOpenB2COrders.GetList(programId, programName);
            var jsonResult = Json(oOpenB2COrders, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"].ToString();
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }

    }
}