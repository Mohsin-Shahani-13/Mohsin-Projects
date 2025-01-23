using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PartStructureBOMController : Controller
    {
        PartStructureBOM oPartStructureBOM = new PartStructureBOM();
        // GET: ListingReports/PartStructureBOM
        public ActionResult Option()
        {
            oPartStructureBOM = new PartStructureBOM();
            return View(oPartStructureBOM);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPartStructureBOM = new PartStructureBOM();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oPartStructureBOM);
        }
        public JsonResult GetList(string programId, string ProgramName, string partNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            

            oPartStructureBOM = new PartStructureBOM();
            oPartStructureBOM.GetList(programId, ProgramName, partNo);
            var jsonResult = Json(oPartStructureBOM, JsonRequestBehavior.AllowGet);
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