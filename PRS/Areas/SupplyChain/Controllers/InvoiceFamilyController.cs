using IP.Areas.SupplyChain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.ActionFilters;
namespace IP.Areas.SupplyChain.Controllers
{
   
    public class InvoiceFamilyController : Controller
    {
        InvoiceFamily oInvoiceFamily = new InvoiceFamily();
   

        public ActionResult Option() {
            oInvoiceFamily = new InvoiceFamily();
            DataTable dtProgram = oInvoiceFamily.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");

            return View(oInvoiceFamily);
     

        }
        public ActionResult Index(string RptCode, string menuTitle, string frmDt, string toDate, string programId, string ProgramName)
        {

            oInvoiceFamily = new InvoiceFamily();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ProgramId = programId;
            ViewBag.ProgramName = ProgramName;
            ViewBag.FrmDt = frmDt;
            ViewBag.toDate = toDate;
            ViewBag.ReportTitle = " Invoice Family > Program = '" + ProgramName + "' | From = '" + frmDt + "' | To = '" + toDate + "'";
     
            try
            {
                oInvoiceFamily = new InvoiceFamily();
                bool success = oInvoiceFamily.GetList(frmDt, toDate, programId, ProgramName);

           
                if (ViewBag.ReportTitle != null && RptCode != null)
                {

                    menuTitle = TempData["ReportTitle"] as string;
                    cLog oLog = new cLog();
                    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }

                if (success)
                    return View(oInvoiceFamily);
                else
                    return View(oInvoiceFamily);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();

            }

        }

        public JsonResult GetList(string programId, String ProgramName, string SerialNo, string frmDt, string toDate) {

            string menuTitle = string.Empty;
            string RptCode;

            oInvoiceFamily = new InvoiceFamily();
            oInvoiceFamily.GetList(frmDt, toDate, programId, ProgramName);
            var jsonResult = Json(oInvoiceFamily, JsonRequestBehavior.AllowGet);
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
