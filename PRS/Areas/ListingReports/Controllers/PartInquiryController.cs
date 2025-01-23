using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.Classess;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PartInquiryController : Controller
    {
        // GET: ListingReports/PartInquiry
        PartInquiry oPartInquiry = new PartInquiry();
        public ActionResult Option()
        {
            oPartInquiry = new PartInquiry();

            //Loading sites according to Program Name
            //cSite oSite = new cSite();
            //DataTable dtSites = oSite.GetProgramSites();
            //ViewBag.ddlSites = cCommon.ToDropDown(dtSites, "ProgramId", "Site", "All");

            return View(oPartInquiry);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPartInquiry = new PartInquiry();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oPartInquiry);
        }

        public JsonResult GetList(string partNo, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oPartInquiry = new PartInquiry();
            oPartInquiry.GetList(partNo, programId, ProgramName);
            var jsonResult = Json(oPartInquiry, JsonRequestBehavior.AllowGet);
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

        public ActionResult Detail(string partNo, string programId, string locNo)
        {
            //string program = "0";
            if (string.IsNullOrEmpty(partNo))
                return View();
     
            try
            {
                oPartInquiry = new PartInquiry();
                bool success = oPartInquiry.GetDetail(partNo, programId, locNo);
                if (success)
                    return View(oPartInquiry);
                else
                    return View(oPartInquiry);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }
    }
}