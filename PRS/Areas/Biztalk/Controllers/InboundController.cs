using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using IP.Areas.Biztalk.Models;
using System.Data;
using IP.ActionFilters;
using System.Web.Services;
using IP.Externals;

namespace IP.Areas.Biztalk.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class InboundController : Controller
    {
        Inbound oInbound = new Inbound();
        // GET: Biztalk/Inbound
        public ActionResult Option()
        {
            oInbound = new Inbound();
            DataTable dtcontract = oInbound.Contract();
            ViewBag.ddContract = cCommon.ToDropDown(dtcontract, "Contract", "ProgramName", "");
            bool success = oInbound.GetMessageType();
            if (success)
                return View(oInbound);
            else
                return View();
            //return View(oInbound);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oInbound = new Inbound();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oInbound);
        }

        public JsonResult GetList(string contract, string frmDt, string toDt, string orderNo, string serialNo, string inMsgType, string isFail, string isSuccessed, string isUnprocessed)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oInbound = new Inbound();
            oInbound.GetList(contract, frmDt, toDt, orderNo, serialNo, inMsgType, isFail, isSuccessed, isUnprocessed);
            var jsonResult = Json(oInbound, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetSummary(string rptCode, string menuTitle, String Option, string type)
        {
            oInbound = new Inbound();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ReportTitle1 = type;
            ViewBag.Option = Option;

            return View(oInbound);
        }
        public JsonResult Summary(string contract, string type, string isFail, string isSuccessed, string isUnprocessed)
        {
            string menuTitle = string.Empty;
            string RptCode;
           

            oInbound = new Inbound();
            oInbound.Summary(contract, type, isFail, isSuccessed, isUnprocessed);
            var jsonResult = Json(oInbound, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetInbound(string contract, string msgType, string Today, string WithInAWeek, string WithInAMonth, string Total, string isFail)
        {
            ViewBag.ReportTitle = "Inbound Message > Contract = '" + contract + "' | Msg. Type = '" + msgType + "' ";

            if (!string.IsNullOrEmpty(Today))
                ViewBag.ReportTitle += "| For Today ";

            if (!string.IsNullOrEmpty(WithInAWeek))
                ViewBag.ReportTitle += "| For Week ";

            if (!string.IsNullOrEmpty(WithInAMonth))
                ViewBag.ReportTitle += "| For Month ";

            if (!string.IsNullOrEmpty(Total))
                ViewBag.ReportTitle += "| Total ";

            if (!string.IsNullOrEmpty(isFail))
            {
                ViewBag.ReportTitle += " | Failures ";
            }

            if (string.IsNullOrEmpty(msgType))
                return View();

            try
            {
                oInbound = new Inbound();
                bool success = oInbound.GetInbound(contract, msgType, Today, WithInAWeek, WithInAMonth, Total, isFail);
                oInbound.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                if (success)
                    return View(oInbound);
                else
                    return View(oInbound);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }
           
        

        public ActionResult Detail(string hdrId)
        {
            if (string.IsNullOrEmpty(hdrId))
                return View();

            try
            {
                oInbound = new Inbound();
                bool success = oInbound.GetDetail(hdrId);
                if (success)
                    return View(oInbound);
                else
                    return View(oInbound);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }
        public ActionResult GetSerial(string hdrId, string lineNo)
        {
            try
            {
                ViewBag.ReportTitle = "Serial No.";
                ViewBag.LineNo = lineNo;
                ViewBag.CONN_TYPE = Session["CONN_TYPE"].ToString();
                Inbound oInbound = new Inbound();
                bool success = oInbound.GetSerial(hdrId,lineNo);
                oInbound.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength =  int.MaxValue };
                if (success)
                    return View(oInbound);
                else
                    return View();
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }
        public JsonResult GetAttribute (string hdrId)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oInbound = new Inbound();
            oInbound.GetAttribute(hdrId);
            var jsonResult = Json(oInbound, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                //ViewBag.ReportTitle = menuTitle;
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }

        public JsonResult GetLineAttr(string hdrId)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oInbound = new Inbound();
            oInbound.GetLineAttr(hdrId);
            var jsonResult = Json(oInbound, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                //ViewBag.ReportTitle = menuTitle;
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }

        public JsonResult GetSerialAttr(string hdrId)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oInbound = new Inbound();
            oInbound.GetSerialAttr(hdrId);
            var jsonResult = Json(oInbound, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                //ViewBag.ReportTitle = menuTitle;
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }

       
        public JsonResult B2BSendMessage(string hdrId, string rptName)
        {
            B2BMessage oB2B = new B2BMessage();
            string msg = oB2B.GetProcessResult(hdrId, rptName);
            if (msg.Contains("Ok"))
            {
                msg = "Reprocessed Successfully";
            }
            ViewBag.message = msg;
            var jsonResult = Json(msg, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}