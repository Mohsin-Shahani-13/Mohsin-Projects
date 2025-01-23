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
    public class MetaClaimManagementController : Controller
    {
        // GET: Meta/MetaClaimManagement
        MetaClaimManagement oMetaClaimManagement;
        public ActionResult Option()
        {
            oMetaClaimManagement = new MetaClaimManagement();
            return View(oMetaClaimManagement);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oMetaClaimManagement = new MetaClaimManagement();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string frmDt, string toDate, string claimclaimSerialNo)
        {
            oMetaClaimManagement = new MetaClaimManagement();

            string menuTitle = string.Empty;
            string RptCode;
            oMetaClaimManagement.GetList(Session["ProgramIdBySiteForMeta"].ToString(), "META", frmDt, toDate, claimclaimSerialNo);
            var jsonResult = Json(oMetaClaimManagement, JsonRequestBehavior.AllowGet);
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