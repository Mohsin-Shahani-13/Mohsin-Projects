using IP.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.Meta.Models;
using System.Data;
namespace IP.Areas.Meta.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MetaYieldController : Controller
    {
        // GET: Meta/MetaYield

        MetaYield oMetaYield = new MetaYield();
        // GET: Meta/MetaSOHourlyOps
        public ActionResult Option()
        {
            oMetaYield = new MetaYield();
            DataTable dtStation = oMetaYield.TestArea();
            ViewBag.ddStation = cCommon.ToDropDown(dtStation, "TestArea", "TestArea", "All");

            return View();
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaYield = new MetaYield();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string TestArea, string frmDt, string toDate, string iteration)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaYield = new MetaYield();
            oMetaYield.GetList( programId, TestArea,  frmDt, toDate,  iteration);
            var jsonResult = Json(oMetaYield, JsonRequestBehavior.AllowGet);
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