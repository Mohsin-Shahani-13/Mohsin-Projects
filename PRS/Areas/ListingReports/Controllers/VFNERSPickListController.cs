using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.ListingReports.Models;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class VFNERSPickListController : Controller
    {
        // GET: ListingReports/VFNERSPickList
        VFNERSPickList oVFNERSPickList = new VFNERSPickList();
        public ActionResult Option()
        {
            oVFNERSPickList = new VFNERSPickList();
            DataTable dtProgram = oVFNERSPickList.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oVFNERSPickList);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {

            oVFNERSPickList = new VFNERSPickList();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.programName = Request.QueryString["ProgramName"];
            return View(oVFNERSPickList);
        }
        public JsonResult GetList(string Type, string TypeText, string programId, String ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            ViewBag.ProgramName = ProgramName;
            oVFNERSPickList = new VFNERSPickList();
            oVFNERSPickList.GetList(Type, TypeText, programId, ProgramName);
            var jsonResult = Json(oVFNERSPickList, JsonRequestBehavior.AllowGet);
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