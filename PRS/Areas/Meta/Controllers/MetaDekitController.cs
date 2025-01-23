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
    public class MetaDekitController : Controller
    {
        MetaDekit oMetaDekit = new MetaDekit();
        // GET: Meta/MetaDekit
        public ActionResult Option()
        {
            oMetaDekit = new MetaDekit();
            return View();
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaDekit = new MetaDekit();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string PartNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaDekit = new MetaDekit();
            oMetaDekit.GetList(programId, ProgramName,PartNo);
            var jsonResult = Json(oMetaDekit, JsonRequestBehavior.AllowGet);
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