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
    public class ROController : Controller
    {
        RO oROH = new RO();
        // GET: SupplyChain/ROH
        public ActionResult Option()
        {
            oROH = new RO();
            //DataTable dtProgram = oROH.Program();
            //ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "All");
            //bool statusSuccess = oROH.GetStatus();
            //ViewBag.ddlStatus = cCommon.ToDropDown(dtStatus, "StatusID", "StatusID", "All");
            //if (statusSuccess)
            //    return View(oROH);
            //else
            //    return View();
            return View(oROH);
        }
      
        public ActionResult Index(string rptCode, string menuTitle,String Option,string type)
        {
            oROH = new RO();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ReportTitle1 = type;
            ViewBag.Option = Option;
           
            // cLog oLog = new cLog();
            // oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, rptCode);
            //  TempData.Keep();
            return View(oROH);
        }

        public ActionResult GetRODetail(string rptCode, string menuTitle, String Option, string type, string ProgramName)
        {
            oROH = new RO();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ReportTitle1 = type;
            ViewBag.Option = Option;
            ViewBag.programName = ProgramName;

            return View(oROH);
        }

        public ActionResult ROWithNoWO(string rptCode, string menuTitle, String Option, string type)
        {
            oROH = new RO();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ReportTitle1 = type;
            ViewBag.Option = Option;

            return View(oROH);
        }
        public ActionResult PreRegisteredUnit(string rptCode, string menuTitle, String Option, string type)
        {
            oROH = new RO();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ReportTitle1 = type;
            ViewBag.Option = Option;

            return View(oROH);
        }
        public JsonResult GetPreRegisteredUnit(string frmDt, string toDt, bool isAllDate, string serialNo, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oROH = new RO();
            oROH.GetPreRegisteredUnit(frmDt, toDt,isAllDate, serialNo,  programId, ProgramName);
            var jsonResult = Json(oROH, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetROWithNoWO(string ROHeaderId, string custRef, string status, string statusId, string type, string programId, string ProgramName)
        {
            string menuTitle = string.Empty;
            string RptCode;

            oROH = new RO();
            oROH.GetROWithNoWo(ROHeaderId, custRef, status, statusId, type, programId, ProgramName);
            var jsonResult = Json(oROH, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetRODtl(string ROHeaderId, string frmDt, string toDate, string custRef, bool isAllDate, string status, string statusId, string type, string programId, string ProgramName)
        { 
            string menuTitle = string.Empty;
            string RptCode;

            oROH = new RO();
            oROH.GetRODetail(ROHeaderId, frmDt, toDate, isAllDate,custRef, status, statusId, type, programId, ProgramName);
            
            var jsonResult = Json(oROH, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetROH(string Id, string frmDt, string toDate,bool isAllDate, string custRef,  string type)
        {
            string menuTitle = string.Empty;
            string RptCode;
            

            oROH = new RO();
            oROH.GetROH(Id, frmDt, toDate, isAllDate, custRef,type);
            var jsonResult = Json(oROH, JsonRequestBehavior.AllowGet);
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

        public ActionResult Detail(string Id, string custRef)
        {
            if (string.IsNullOrEmpty(custRef))
                return View();
            try
            {
                oROH = new RO();
                bool success = oROH.GetDetail(Id, custRef);
                if (success)
                    return View(oROH);
                else
                    return View(oROH);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }
        public JsonResult GetROH1(string status)
        {
            

            oROH = new RO();
           // oROH.GetROH1(status);
            var jsonResult = Json(oROH, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Detail1(string status)
        {
            oROH = new RO();
         //  var data= oROH.GetROH1(status);
           ViewBag.filter= oROH.filterString;

            return View(oROH);

        }


        public ActionResult ROUnit(string ROLineID, string statusId)
        {
            try { 
            oROH = new RO();
            ViewBag.ReportTitle = "RO Unit" ;
                ViewBag.data = null;
                if (statusId != null)
                {
                    ViewBag.data = " > Received";
                }
                else
                {
                    ViewBag.data = " > Ordered";
                }
                bool success = oROH.GetROUnit(ROLineID, statusId);
            if (success)
                return View(oROH);
            else
                return View();
            }
            catch (Exception e) {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }
    }
}