using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.Meta.Models;
using System.Data;

using IP.ActionFilters;

namespace IP.Areas.Meta.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MetaWipController : Controller
    {
        MetaWip oMetaWip = new MetaWip();
        // GET: Meta/MetaWip
        public ActionResult Option()
        {
            oMetaWip = new MetaWip();
            DataTable dtProgram = oMetaWip.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oMetaWip);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaWip = new MetaWip();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string PartNo, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaWip = new MetaWip();
            oMetaWip.GetList(programId, ProgramName, PartNo, frmDt, toDate);
            var jsonResult = Json(oMetaWip, JsonRequestBehavior.AllowGet);
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