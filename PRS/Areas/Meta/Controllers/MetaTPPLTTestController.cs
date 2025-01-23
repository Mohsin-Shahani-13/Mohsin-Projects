using IP.ActionFilters;
using IP.Areas.Meta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.Meta.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MetaTPPLTTestController : Controller
    {
        MetaTPPLTTest oMetaTPPLTTest;
        // GET: Meta/MetaTPPLTTest
        public ActionResult Option()
        {
            oMetaTPPLTTest = new MetaTPPLTTest();
            return View(oMetaTPPLTTest);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oMetaTPPLTTest = new MetaTPPLTTest();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName)
        {
            oMetaTPPLTTest = new MetaTPPLTTest();

            string menuTitle = string.Empty;
            string RptCode;
            oMetaTPPLTTest.GetList(Session["ProgramIdBySiteForMeta"].ToString(), "META");
            var jsonResult = Json(oMetaTPPLTTest, JsonRequestBehavior.AllowGet);
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