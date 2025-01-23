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
    public class MetaRMAOrdersWithErrorsController : Controller
    {
        // GET: Meta/MetaRMAOrdersWithErrors
        MetaRMAOrdersWithErrors oMetaRMAOrdersWithErrors = new MetaRMAOrdersWithErrors();
        public ActionResult Option()
        {
            DataTable dtcontract = oMetaRMAOrdersWithErrors.GetSitewiseContract();
            ViewBag.ddContract = cCommon.ToDropDown(dtcontract, "contract", "programName", "");
            return View(oMetaRMAOrdersWithErrors);
        }
        public ActionResult Index(string menuTitle, string RptCode)
        {
            oMetaRMAOrdersWithErrors = new MetaRMAOrdersWithErrors();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oMetaRMAOrdersWithErrors);
        }
        public JsonResult GetList(string contract, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaRMAOrdersWithErrors = new MetaRMAOrdersWithErrors();
            oMetaRMAOrdersWithErrors.GetList(contract, programName);
            var jsonResult = Json(oMetaRMAOrdersWithErrors, JsonRequestBehavior.AllowGet);
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