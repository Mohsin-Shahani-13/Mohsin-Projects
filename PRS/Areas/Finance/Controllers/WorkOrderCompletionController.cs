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
    public class WorkOrderCompletionController : Controller
    {
        // GET: Finance/WorkOrderCompletion
        WorkOrderCompletion oWorkOrderCompletion;
        public ActionResult Option()
        {
            oWorkOrderCompletion = new WorkOrderCompletion();
            DataTable dttransaction = oWorkOrderCompletion.GetTransaction();
            ViewBag.ddtransaction = cCommon.ToDropDown(dttransaction, "Id", "Description", "All");
            return View();
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oWorkOrderCompletion = new WorkOrderCompletion();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string partNo, string frmDt, string toDt, string fromWoId, string toWoId, string Id, string transaction, string InventorySource)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oWorkOrderCompletion = new WorkOrderCompletion();
            oWorkOrderCompletion.GetList(programId, ProgramName, partNo, frmDt, toDt, fromWoId, toWoId, Id, transaction, InventorySource);
            var jsonResult = Json(oWorkOrderCompletion, JsonRequestBehavior.AllowGet);
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