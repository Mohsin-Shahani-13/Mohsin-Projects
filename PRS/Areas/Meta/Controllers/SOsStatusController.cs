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
    public class SOsStatusController : Controller
    {

        SOsStatus oSOsStatus = new SOsStatus();
        // GET: Meta/SOsStatus
        public ActionResult Option()
        {
            oSOsStatus = new SOsStatus();
            return View(oSOsStatus);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oSOsStatus = new SOsStatus();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }

        public JsonResult GetList(string programId, string ProgramName, string frmDt, string toDate, bool ischecked, string custRef, string EvDateFrom, string EvDateTo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSOsStatus = new SOsStatus();
            oSOsStatus.GetList(Session["ProgramIdBySiteForMeta"].ToString(), "META", frmDt, toDate, ischecked, custRef, EvDateFrom, EvDateTo);
            var jsonResult = Json(oSOsStatus, JsonRequestBehavior.AllowGet);
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