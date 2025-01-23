using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using IP.Areas.Biztalk.Models;
using System.Data;
using IP.ActionFilters;
using IP.Externals;

namespace IP.Areas.Biztalk.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class OutboundController : Controller
    {
        Outbound oOutbound = new Outbound();
        // GET: Biztalk/Outbound
        public ActionResult Option()
        {
            oOutbound = new Outbound();
            DataTable dtcontract = oOutbound.Contract();
            ViewBag.ddContract = cCommon.ToDropDown(dtcontract, "Contract", "ProgramName", "");

            bool success = oOutbound.GetMessageType();
            if (success)
                return View(oOutbound);
            else
                return View();
            //return View(oOutbound);
        }
        public ActionResult Index(string RptCode, string menuTitle, string contract)
        {
            oOutbound = new Outbound();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.contract = contract;
            //ViewBag.dockPath = oOutbound.GetDocPath(contract); //comment by huzaifa
            //ViewBag.HasWinIT = Session["LogonUser"].ToString();
            return View(oOutbound);
        }

        public JsonResult GetList(string contract, string frmDt, string toDt, string orderNo, string serialNo, string custordertype, string OutMsgType, string isFail, string isSuccessed, string isUnprocessed)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oOutbound = new Outbound();
            oOutbound.GetList(contract, frmDt, toDt, orderNo, serialNo, custordertype, OutMsgType, isFail, isSuccessed, isUnprocessed);
            var jsonResult = Json(oOutbound, JsonRequestBehavior.AllowGet);
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
            oOutbound = new Outbound();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.ReportTitle1 = type;
            ViewBag.Option = Option;

            return View(oOutbound);
        }
        public JsonResult Summary(string contract, string type, string isFail, string isSuccessed, string isUnprocessed)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oOutbound = new Outbound();
            oOutbound.Summary(contract, type, isFail, isSuccessed, isUnprocessed);
            var jsonResult = Json(oOutbound, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetOutbound(string contract, string msgType, string Today, string WithInAWeek, string WithInAMonth,string Total, string isFail)
        {
            ViewBag.ReportTitle = "Outbound Message > Contract = '" + contract + "' | Msg. Type = '" + msgType + "' ";

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
                oOutbound = new Outbound();
                bool success = oOutbound.GetOutbound(contract, msgType, Today, WithInAWeek, WithInAMonth, Total, isFail);
                oOutbound.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                if (success)
                    return View(oOutbound);
                else
                    return View(oOutbound);
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
                oOutbound = new Outbound();
                bool success = oOutbound.GetDetail(hdrId);
                if (success)
                    return View(oOutbound);
                else
                    return View(oOutbound);
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
                Outbound oOutbound = new Outbound();
                bool success = oOutbound.GetSerial(hdrId, lineNo);
                oOutbound.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                if (success)
                    return View(oOutbound);
                else
                    return View();
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }
        public JsonResult GetAttribute(string hdrId)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oOutbound = new Outbound();
            oOutbound.GetAttribute(hdrId);
            var jsonResult = Json(oOutbound, JsonRequestBehavior.AllowGet);
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
            oOutbound = new Outbound();
            oOutbound.GetLineAttr(hdrId);
            var jsonResult = Json(oOutbound, JsonRequestBehavior.AllowGet);
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
            oOutbound = new Outbound();
            oOutbound.GetSerialAttr(hdrId);
            var jsonResult = Json(oOutbound, JsonRequestBehavior.AllowGet);
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