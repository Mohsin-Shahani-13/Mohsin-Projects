using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.SupplyChain.Models;
using System.Data;

namespace IP.Areas.SupplyChain.Controllers
{
    public class VALVEComponentUsageController : Controller
    {
        // GET: SupplyChain/VALVEComponentUsage
        VALVEComponentUsage oVALVEComponentUsage = new VALVEComponentUsage();

        public ActionResult Option()
        {
            oVALVEComponentUsage = new VALVEComponentUsage();
            //DataTable dtStatus = oVALVEComponentUsage.Status();
            //ViewBag.ddlStatus = cCommon.ToDropDown(dtStatus, "StatusID", "Status", "");
            return View(oVALVEComponentUsage);

        }
        public ActionResult OptionBose()
        {
            oVALVEComponentUsage = new VALVEComponentUsage();
            DataTable dtProgram = oVALVEComponentUsage.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oVALVEComponentUsage);

        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oVALVEComponentUsage = new VALVEComponentUsage();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oVALVEComponentUsage);
        }
        public JsonResult GetList(string programId, string ProgramName, string fromDt, string toDt, string statusId, string status)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oVALVEComponentUsage = new VALVEComponentUsage();
            oVALVEComponentUsage.GetList(programId, ProgramName, fromDt, toDt, statusId, status);
            var jsonResult = Json(oVALVEComponentUsage, JsonRequestBehavior.AllowGet);
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