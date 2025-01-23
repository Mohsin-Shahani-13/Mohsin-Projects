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
    public class WOFaultRepairController : Controller
    {
        WOFaultRepair oWOFaultRepair = new WOFaultRepair();
        // GET: SupplyChain/WOFaultRepair
        public ActionResult Option()
        {
            oWOFaultRepair = new WOFaultRepair();
            DataTable dtProgram = oWOFaultRepair.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            //return PartialView("GetProgramBySite");
            return View();
        }
        public ActionResult OptionBose()
        {
            oWOFaultRepair = new WOFaultRepair();
            DataTable dtProgram = oWOFaultRepair.Program();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            //return PartialView("GetProgramBySite");
            return View();
        }

        public ActionResult Index(string RptCode, string menuTitle, bool ischecked)
        {
            oWOFaultRepair = new WOFaultRepair();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ischecked = ischecked;
            ViewBag.programName = Request.QueryString["programName"].ToString();

            return View();
        }
        public JsonResult GetList(string frmDt, string toDate, string programId, string programName, string serialNo, bool ischecked,string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oWOFaultRepair = new WOFaultRepair();
            oWOFaultRepair.GetList(frmDt, toDate,programId, programName, serialNo, ischecked, custRef);
            var jsonResult = Json(oWOFaultRepair, JsonRequestBehavior.AllowGet);
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