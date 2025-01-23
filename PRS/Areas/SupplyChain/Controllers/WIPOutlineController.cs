using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    
    public class WIPOutlineController : Controller
    {
        WIPOutline oWIPOutline = new WIPOutline();
        // GET: SupplyChain/WIPOutline
        public ActionResult Option()
        {
            oWIPOutline = new WIPOutline();
            DataTable dtProgram = oWIPOutline.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "ProgramName", "");

            return View(oWIPOutline);
        }

        public ActionResult Index(string rptCode, string menuTitle, String Option, string type, string ProgramId)
        {
            oWIPOutline = new WIPOutline();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ReportTitle1 = type;
            ViewBag.Option = Option;
            ViewBag.site = @Session["DefaultSite"];
            ViewBag.program = ProgramId;
            //cLog oLog = new cLog();
            //oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, rptCode);
            TempData.Keep();
            return View(oWIPOutline);
        }
        public JsonResult GetList(/*string frmDt, string toDt,*/ string custRef, string ProgramId, string ProgramName, string partNo)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oWIPOutline = new WIPOutline();
            oWIPOutline.GetList(/*frmDt, toDt,*/ custRef, ProgramId, ProgramName, partNo);
            var jsonResult = Json(oWIPOutline, JsonRequestBehavior.AllowGet);
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