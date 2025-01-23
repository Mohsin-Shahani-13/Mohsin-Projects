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
    public class RFCExportController : Controller
    {
        RFCExport oRFCExport = new RFCExport();
        public string manuTitle { get; private set; }

        // GET: SupplyChain/RFCExport
        public ActionResult Option()
        {
            oRFCExport = new RFCExport();
            DataTable dtProgram = oRFCExport.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "ProgramName", "");
            DataTable dtId = oRFCExport.GetIDBySite();
            ViewBag.ddId = cCommon.ToDropDown(dtId, "ID", "ID", "");
            return View(oRFCExport);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oRFCExport = new RFCExport();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRFCExport);
        }
        public JsonResult GetList(string frmDt, string toDt, bool isAllDate, string ProgramId, string ProgramName, string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRFCExport = new RFCExport();
            oRFCExport.GetList(frmDt, toDt, isAllDate, ProgramId, ProgramName, custRef);
            var jsonResult = Json(oRFCExport, JsonRequestBehavior.AllowGet);
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