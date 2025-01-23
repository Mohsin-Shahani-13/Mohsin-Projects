using IP.Areas.ListingReports.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.ListingReports.Controllers
{
    public class SOReservationDetailsController : Controller
    {
        SOReservationDetails oSOReservationDetails;
        // GET: ListingReports/SOReservationDetails
        public ActionResult Option()
        {
            oSOReservationDetails = new SOReservationDetails();
            //DataTable dtProgram = oSOReservationDetails.GetProgramBySite();
            //ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "ProgramName", "");
            return View(oSOReservationDetails);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oSOReservationDetails = new SOReservationDetails();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oSOReservationDetails);
        }
        public JsonResult GetList(string custRef)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSOReservationDetails = new SOReservationDetails();
            oSOReservationDetails.GetList(Session["ProgramIdBySiteForMeta"].ToString(), "META", custRef);
            var jsonResult = Json(oSOReservationDetails, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog olog = new cLog();
                olog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
    }
}