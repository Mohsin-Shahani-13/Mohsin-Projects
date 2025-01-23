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
    public class WOComponentController : Controller
    {
        WOComponent oWOComponent;
        // GET: Finance/WOComponent

        public ActionResult Option()
        {
            oWOComponent = new WOComponent();
            DataTable dtProgram = oWOComponent.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oWOComponent);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oWOComponent = new WOComponent();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string partNo, string frmDt, string toDt, string fromWoId, string toWoId, string InventorySource, string rptType)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oWOComponent = new WOComponent();
            oWOComponent.GetList(programId, ProgramName, partNo, frmDt, toDt, fromWoId, toWoId, InventorySource, rptType);
            var jsonResult = Json(oWOComponent, JsonRequestBehavior.AllowGet);
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