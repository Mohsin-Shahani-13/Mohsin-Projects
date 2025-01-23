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
    public class WorkOrderDetailController : Controller
    {
        WorkOrderDetail oWorkOrderDetail = new WorkOrderDetail();
        // GET: SupplyChain/WorkOrderDetail
        public ActionResult Option()
        {
            oWorkOrderDetail = new WorkOrderDetail();
            //DataTable dtProgram = oWorkOrderDetail.GetProgramBySite();
            //ViewBag.ddprogram = cCommon.ToDropDown(dtProgram, "programID", "programName", "");
            return View(oWorkOrderDetail);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oWorkOrderDetail = new WorkOrderDetail();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oWorkOrderDetail);
        }
        public JsonResult GetList(string ProgramId, string ProgramName, string frmDt, string toDate, bool isAllDate, string serialNo, string partNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oWorkOrderDetail = new WorkOrderDetail();
            oWorkOrderDetail.GetList(Session["ProgramIdBySiteForMeta"].ToString(), "META", frmDt, toDate, isAllDate, serialNo, partNo);
            var jsonResult = Json(oWorkOrderDetail, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            // LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog olog = new cLog();
                olog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
    }
}