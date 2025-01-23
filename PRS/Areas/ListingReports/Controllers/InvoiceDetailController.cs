using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using IP.Areas.ListingReports.Models;
using System.Web.Mvc;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class InvoiceDetailController : Controller
    {
        // GET: ListingReports/InvoiceDetail
        InvoiceDetail oInvoiceDetail;
        public ActionResult Option()
        {
            oInvoiceDetail = new InvoiceDetail();
            return View(oInvoiceDetail);
        }

        public ActionResult Index(string menuTitle, string rptCode)
        {
            oInvoiceDetail = new InvoiceDetail();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oInvoiceDetail);
        }

        public JsonResult GetList(string frmDt, string toDt, bool isAllDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oInvoiceDetail = new InvoiceDetail();
            oInvoiceDetail.GetList(frmDt, toDt, isAllDate);
            var jsonResult = Json(oInvoiceDetail, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
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