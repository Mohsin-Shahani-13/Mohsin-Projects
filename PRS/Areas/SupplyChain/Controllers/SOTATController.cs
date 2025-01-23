using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.Areas.SupplyChain.Models;
using IP.ActionFilters;
namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class SOTATController : Controller
    {
        SOTAT oSOTAT = new SOTAT();
        // GET: SupplyChain/SOTAT
        public ActionResult Option()
        {
            oSOTAT = new SOTAT();
            DataTable dtProgram = oSOTAT.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oSOTAT);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oSOTAT = new SOTAT();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oSOTAT);
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSOTAT = new SOTAT();
            oSOTAT.GetList(frmDt, toDate, programId, programName);
            var jsonResult = Json(oSOTAT, JsonRequestBehavior.AllowGet);
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