using IP.ActionFilters;
using IP.Areas.Meta.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.Meta.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MetaTrackDataWipeComplianceController : Controller
    {
        // GET: Meta/MetaTrackDataWipeCompliance
        MetaTrackDataWipeCompliance oMetaTrackDataWipeCompliance;
        public ActionResult Option()
        {
            oMetaTrackDataWipeCompliance = new MetaTrackDataWipeCompliance();
            DataTable dtModel = oMetaTrackDataWipeCompliance.Model();
            ViewBag.ddModel = cCommon.ToDropDown(dtModel, "Model", "Model", "All");
            return View(oMetaTrackDataWipeCompliance);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaTrackDataWipeCompliance = new MetaTrackDataWipeCompliance();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string frmDt, string toDt, bool isAllDate, string faFilter, string bounceFilter, string TestFilter, string LogFilter, string ListFilter, string DataWipeTestPerformed24HrsFilter, string IdleFilter, string DataWipeResult, string Model)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaTrackDataWipeCompliance = new MetaTrackDataWipeCompliance();
            oMetaTrackDataWipeCompliance.GetList(frmDt, toDt, isAllDate, faFilter, bounceFilter, TestFilter,LogFilter, ListFilter, DataWipeTestPerformed24HrsFilter, IdleFilter, DataWipeResult, Model);
            var jsonResult = Json(oMetaTrackDataWipeCompliance, JsonRequestBehavior.AllowGet);
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