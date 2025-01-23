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
    public class MetaMTDGlobalShippingController : Controller
    {
        MetaMTDGlobalShipping oMetaMTDGlobalShipping = new MetaMTDGlobalShipping();
        
        // GET: Meta/MetaMTDGlobalShipping
        public ActionResult Option()
        {
            oMetaMTDGlobalShipping = new MetaMTDGlobalShipping();
            DataTable dtStatus = oMetaMTDGlobalShipping.Status();
            ViewBag.ddlStatus = cCommon.ToDropDown(dtStatus, "Id", "Description", "All");
            return View(oMetaMTDGlobalShipping);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaMTDGlobalShipping = new MetaMTDGlobalShipping();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string RMARef, string frmDt, string toDate, string status, string statusId)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaMTDGlobalShipping = new MetaMTDGlobalShipping();
            oMetaMTDGlobalShipping.GetList(programId, ProgramName, RMARef, frmDt, toDate, status, statusId);
            var jsonResult = Json(oMetaMTDGlobalShipping, JsonRequestBehavior.AllowGet);
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