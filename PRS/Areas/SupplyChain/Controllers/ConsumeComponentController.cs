using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.SupplyChain.Models;
using System.Data;

namespace IP.Areas.SupplyChain.Controllers
{
    public class ConsumeComponentController : Controller
    {
        ConsumeComponent oConsumeComponent = new ConsumeComponent();
        // GET: SupplyChain/ConsumeComponent
        public ActionResult Option()
        {
            oConsumeComponent = new ConsumeComponent();
            DataTable dtProgram = oConsumeComponent.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");

            return View(oConsumeComponent);
            
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oConsumeComponent = new ConsumeComponent();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oConsumeComponent);
        }
        public JsonResult GetList(string programId, string ProgramName, string fromDt, string toDt)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oConsumeComponent = new ConsumeComponent();
            oConsumeComponent.GetList(programId, ProgramName, fromDt, toDt);
            var jsonResult = Json(oConsumeComponent, JsonRequestBehavior.AllowGet);
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