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
    public class KPNInvoiceController : Controller
    {
        // GET: Finance/KPNInvoice
        KPNInvoice oKPNInvoice = new KPNInvoice();
        public ActionResult Option()
        {
            oKPNInvoice = new KPNInvoice();
            return View(oKPNInvoice);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oKPNInvoice = new KPNInvoice();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oKPNInvoice);
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oKPNInvoice = new KPNInvoice();
            oKPNInvoice.GetList(frmDt, toDate, "10045", "KPN");
            var jsonResult = Json(oKPNInvoice, JsonRequestBehavior.AllowGet);
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