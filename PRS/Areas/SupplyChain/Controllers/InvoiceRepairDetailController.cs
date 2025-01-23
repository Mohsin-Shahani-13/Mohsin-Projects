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
    public class InvoiceRepairDetailController : Controller
    {
        // GET: SupplyChain/InvoiceRepairDetail
        InvoiceRepairDetail oInvoiceRepairDetail;
        public ActionResult Option()
        {
            oInvoiceRepairDetail = new InvoiceRepairDetail();
            DataTable dtProgram = oInvoiceRepairDetail.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "ProgramName", "");
            return View(oInvoiceRepairDetail);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oInvoiceRepairDetail = new InvoiceRepairDetail();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oInvoiceRepairDetail);
        }
        public JsonResult GetList(string frmDt, string toDate, string SerialNo, string programId, string ProgramName, string rpt_name)
        {
            string menuTitle = string.Empty;
            string RptCode;
            
            oInvoiceRepairDetail = new InvoiceRepairDetail();
            oInvoiceRepairDetail.GetList(frmDt, toDate, SerialNo, programId, ProgramName, rpt_name);
            var jsonResult = Json(oInvoiceRepairDetail, JsonRequestBehavior.AllowGet);
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