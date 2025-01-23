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
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class InvoiceValveController : Controller
    {
        // GET: SupplyChain/InvoiceValve
        InvoiceValve oInvoiceVal = new InvoiceValve();
        public ActionResult Option()
        {
            oInvoiceVal = new InvoiceValve();
            DataTable dtProgram = oInvoiceVal.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oInvoiceVal);
        }


        public ActionResult Index(string RptCode, string menuTitle)
        {
            oInvoiceVal = new InvoiceValve();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
          
            return View(oInvoiceVal);

        }
        public JsonResult GetList(string fromDt, string toDt, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
          
            oInvoiceVal = new InvoiceValve();
            oInvoiceVal.GetList(fromDt, toDt, programId, ProgramName);
            var jsonResult = Json(oInvoiceVal, JsonRequestBehavior.AllowGet);
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
    
        public ActionResult GetDetail(string rptCode, string menuTitle)
        {
            oInvoiceVal = new InvoiceValve();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
       

            return View(oInvoiceVal);
        }
        public JsonResult GetDtl(string fromDt, string toDt, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            

            oInvoiceVal = new InvoiceValve();
            oInvoiceVal.GetDetail(fromDt, toDt, programId, ProgramName);
            var jsonResult = Json(oInvoiceVal, JsonRequestBehavior.AllowGet);
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

        //public ActionResult GetDetail(string rptCode, string menuTitle, String Option, string type)
        //{

        //    InvoiceVal = new InvoiceValve();
        //    TempData["ReportTitle"] = menuTitle;
        //    TempData["RptCode"] = rptCode;
        //    ViewBag.ReportTitle = menuTitle;
        //    ViewBag.ReportTitle1 = type;
        //    ViewBag.Option = Option;

        //    return View(InvoiceVal);

        //}






    }
}