using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    public class GoProTestsAppController : Controller
    {
        // GET: ListingReports/GoProTestsApp
        GoProTestsApp oGoProTestsApp;
        public ActionResult Option()
        {
            oGoProTestsApp = new GoProTestsApp();
            DataTable dtCustomer = oGoProTestsApp.GetCustomer();
            ViewBag.ddCustomer = cCommon.ToDropDown(dtCustomer, "ID", "Description", "");
            DataTable dtApp = oGoProTestsApp.GetApp();
            ViewBag.ddApp = cCommon.ToDropDown(dtApp, "ID", "Description", "");
            return View(oGoProTestsApp);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oGoProTestsApp = new GoProTestsApp();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string frmDt, string toDate, string customer, string app, string result, string active)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oGoProTestsApp = new GoProTestsApp();
            oGoProTestsApp.GetList(frmDt, toDate, customer, app, result, active);
            var jsonResult = Json(oGoProTestsApp, JsonRequestBehavior.AllowGet);
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