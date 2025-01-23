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
    public class MetaRMAController : Controller
    {
        MetaRMA oMetaRMA = new MetaRMA();

        // GET: Meta/MetaRMA
        public ActionResult Option()
        {
            oMetaRMA = new MetaRMA();
            return View(oMetaRMA);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oMetaRMA = new MetaRMA();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }

        public JsonResult GetList(string programId, string ProgramName, string RMARef, string frmDt, string toDate)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaRMA = new MetaRMA();
            oMetaRMA.GetList(programId, ProgramName, RMARef, frmDt, toDate);
            var jsonResult = Json(oMetaRMA, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
    }
}