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
    public class InvoiceRepairlevelController : Controller
    {
        // GET: SupplyChain/InvoiceRepairlevel

        InvoiceRepairlevel oInvoiceRepairlevel = new InvoiceRepairlevel();
        public ActionResult Option()
        {
            oInvoiceRepairlevel = new InvoiceRepairlevel();
            DataTable dtProgram = oInvoiceRepairlevel.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
           
            return View(oInvoiceRepairlevel);
        }
        public ActionResult Index(string RptCode, string menuTitle, string frmDt, string toDate, string ProgramID, string ProgramName, string reportName)
        {
            oInvoiceRepairlevel = new InvoiceRepairlevel();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ProgramId = ProgramID;
            ViewBag.ProgramName = ProgramName;
            ViewBag.FrmDt = frmDt;
            ViewBag.toDate = toDate;
            ViewBag.ReportTitle = " Invoice Repair Level > Program = '" + ProgramName + "' | From = '" + frmDt + "' | To = '" + toDate + "'";
            //ViewBag.ischecked = ischecked;
            try
            {
                oInvoiceRepairlevel = new InvoiceRepairlevel();
                bool success = oInvoiceRepairlevel.GetList(frmDt, toDate, ProgramID, ProgramName);

                //LOAD MRU & LOG QUERY
                if (ViewBag.ReportTitle != null && RptCode != null)
                {

                    menuTitle = TempData["ReportTitle"] as string;
                    cLog oLog = new cLog();
                    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }

                if (success)
                    return View(oInvoiceRepairlevel);
                else
                    return View(oInvoiceRepairlevel);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
                
            }
        }
        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            

            oInvoiceRepairlevel = new InvoiceRepairlevel();
            oInvoiceRepairlevel.GetList(frmDt, toDate, programId, ProgramName);
            var jsonResult = Json(oInvoiceRepairlevel, JsonRequestBehavior.AllowGet);
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


        public ActionResult GetDetail(string programId, string Repairlevel, string frmDt, string toDate, string Invoicefamily)
         {

            oInvoiceRepairlevel = new InvoiceRepairlevel();
            ViewBag.ReportTitle = "SNo. > Family = '" + Invoicefamily + "' | R.Lvl= '" + Repairlevel + "'";
            //ViewBag.ReportTitle = "Serial No. > Invoice family = '" + Invoicefamily + "' | Repair level= '" + Repairlevel + "'";

            bool success = oInvoiceRepairlevel.GetDetail(programId, Repairlevel, frmDt,  toDate, Invoicefamily);
            oInvoiceRepairlevel.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            if (success)
                return View(oInvoiceRepairlevel);
            else
                return View();
        }
    }
}