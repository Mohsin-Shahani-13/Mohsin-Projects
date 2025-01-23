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
    public class MetaGrapevineProblemLogController : Controller
    {
        MetaGrapevineProblemLog oMetaGrapevineProblemLog = new MetaGrapevineProblemLog();
        // GET: Meta/MetaGrapevineProblemLog

        public ActionResult Option()
        {
            oMetaGrapevineProblemLog = new MetaGrapevineProblemLog();
            return View();
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaGrapevineProblemLog = new MetaGrapevineProblemLog();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oMetaGrapevineProblemLog);
        }

        public JsonResult GetList(string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }

            oMetaGrapevineProblemLog = new MetaGrapevineProblemLog();
            oMetaGrapevineProblemLog.GetList(programId, ProgramName);
            var jsonResult = Json(oMetaGrapevineProblemLog, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}