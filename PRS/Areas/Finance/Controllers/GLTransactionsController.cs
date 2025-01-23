using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.Finance.Models;
using System.Data;
using IP.ActionFilters;
    
    namespace IP.Areas.Finance.Controllers
{
        [OutputCache(Duration = 0)]
        [SessionTimeout]
        public class GLTransactionsController : Controller
    {
        // GET: ListingReports/GLTransactions
        GLTransactions oGLTransactions;
        public ActionResult Option()
        {
            oGLTransactions = new GLTransactions();
            return View(oGLTransactions);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oGLTransactions = new GLTransactions();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oGLTransactions);
        }
        public JsonResult GetList(string FiscalYear, string FiscalPeriod, string FiscalPeriodText)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oGLTransactions = new GLTransactions();
            oGLTransactions.GetList(FiscalYear, FiscalPeriod, FiscalPeriodText);
            var jsonResult = Json(oGLTransactions, JsonRequestBehavior.AllowGet);
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