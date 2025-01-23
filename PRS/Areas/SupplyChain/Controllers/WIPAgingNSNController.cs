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
    public class WIPAgingNSNController : Controller
    {
        // GET: SupplyChain/WIPAgingNSN
        WIPAgingNSN oWIPAgingNSN;
        public ActionResult Option()
        {
            oWIPAgingNSN = new WIPAgingNSN();
            DataTable dtProgram = oWIPAgingNSN.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oWIPAgingNSN);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {

            oWIPAgingNSN = new WIPAgingNSN();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.programName = Request.QueryString["ProgramName"];
            return View(oWIPAgingNSN);
        }
        public JsonResult GetList(string frmDt, string toDate, string programId, String ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            ViewBag.ProgramName = ProgramName;
            oWIPAgingNSN = new WIPAgingNSN();
            oWIPAgingNSN.GetList(frmDt, toDate, programId, ProgramName);
            var jsonResult = Json(oWIPAgingNSN, JsonRequestBehavior.AllowGet);
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