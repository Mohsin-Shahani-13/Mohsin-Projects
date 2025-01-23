using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class SOController : Controller
    {
        SO oSO = new SO();
        // GET: SupplyChain/SO
        public ActionResult Option()
        {
            oSO = new SO();
            //DataTable dtProgram = oSO.Program();
            //ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "All");
            bool statusSuccess = oSO.Status();

            //ViewBag.ddlStatus = cCommon.ToDropDown(dtStatus, "Id", "Description", "All");
            if (statusSuccess)
                return View(oSO);
            else
                return View();
            //return View(oSO);
        }
        public ActionResult OptionBose()
        {
            oSO = new SO();
            DataTable dtProgram = oSO.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            bool statusSuccess = oSO.Status();

            //ViewBag.ddlStatus = cCommon.ToDropDown(dtStatus, "Id", "Description", "All");
            if (statusSuccess)
                return View(oSO);
            else
                return View();
            //return View(oSO);
        }


        public ActionResult Index(string rptCode, string menuTitle, String Option, string type)
        {
            oSO = new SO();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ReportTitle1 = type;
            ViewBag.Option = Option;

            return View(oSO);
        }

        public ActionResult GetSODetail(string rptCode, string menuTitle, String Option, string type)
        {
            oSO = new SO();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ReportTitle1 = type;
            ViewBag.Option = Option;

            return View(oSO);
        }
        public JsonResult GetSODtl(string SOHeaderId, string frmDt, string toDate, string custRef, string status, string statusId, string type, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSO = new SO();
            oSO.GetSODetail(SOHeaderId, frmDt, toDate, custRef, status, statusId, type, programId, ProgramName);
            var jsonResult = Json(oSO, JsonRequestBehavior.AllowGet);
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
        public JsonResult GetSO(string SOHeaderId, string frmDt, string toDate, string custRef, string status, string statusId, string type, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oSO = new SO();
            oSO.GetSO(SOHeaderId, frmDt, toDate, custRef, status, statusId, type, programId, ProgramName);
            var jsonResult = Json(oSO, JsonRequestBehavior.AllowGet);
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
        public ActionResult Detail(string SOHeaderId, string custRef)
        {
            if (string.IsNullOrEmpty(custRef))
                return View();
            try
            {
                oSO = new SO();
                bool success = oSO.GetDetail(SOHeaderId, custRef);
                if (success)
                    return View(oSO);
                else
                    return View(oSO);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }

        public ActionResult SOUnit(string SOLineId, string statusId)
        {
            try
            {
                oSO = new SO();
                ViewBag.ReportTitle = "SO Unit";
                ViewBag.data = null;
                if (statusId != null)
                {
                    ViewBag.data = " > Shipped";
                }
                else
                {
                    ViewBag.data = " > To Ship";
                }
                bool success = oSO.GetSOUnit(SOLineId, statusId);
                if (success)
                    return View(oSO);
                else
                    return View();

            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }
    }
}