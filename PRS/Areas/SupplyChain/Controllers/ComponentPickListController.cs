using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.SupplyChain.Models;
using System.Data;

using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class ComponentPickListController : Controller
    {
        ComponentPickList oComponentPickList = new ComponentPickList();
        // GET: SupplyChain/ComponentPickList

        public ActionResult Option()
        {
            oComponentPickList = new ComponentPickList();
            return View();
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oComponentPickList = new ComponentPickList();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string compPartNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oComponentPickList = new ComponentPickList();
            oComponentPickList.GetList(programId, ProgramName, compPartNo);
            var jsonResult = Json(oComponentPickList, JsonRequestBehavior.AllowGet);
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