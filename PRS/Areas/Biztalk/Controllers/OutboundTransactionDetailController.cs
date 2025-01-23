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
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;

namespace IP.Areas.Biztalk.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class OutboundTransactionDetailController : Controller
    {
        OutboundTransactionDetail oOutboundTransactionDetail = new OutboundTransactionDetail();

        public object queryResult { get; private set; }

        // GET: Biztalk/OutboundTransactionDetail
        public ActionResult Option(string Contract, string MessageType)
        {
            oOutboundTransactionDetail = new OutboundTransactionDetail();
            DataTable dtcontract = oOutboundTransactionDetail.Contract();
            ViewBag.ddContract = cCommon.ToDropDown(dtcontract, "Contract", "Contract", "");
            //DataTable dtMessageType = oOutboundTransactionDetail.MessageType(Contract);
            //ViewBag.ddMessageType = cCommon.ToDropDown(dtMessageType, "MessageType", "MessageType", "");

            return View(oOutboundTransactionDetail);
        }

        //Call in option Popup
        public JsonResult GetMessageType(string contract)
        {
            oOutboundTransactionDetail = new OutboundTransactionDetail();
            oOutboundTransactionDetail.MessageType(contract);
            var jsonResult = Json(oOutboundTransactionDetail, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Index(string RptCode, string menuTitle, string contract, string MessageType)
        {
            oOutboundTransactionDetail = new OutboundTransactionDetail();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            ViewBag.contract = contract;

            return View(oOutboundTransactionDetail);
        }

        public JsonResult GetList(string contract, string MsgType, string frmDt, string toDt)
        {
            string menuTitle = string.Empty;
            string RptCode;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }

            oOutboundTransactionDetail = new OutboundTransactionDetail();
            oOutboundTransactionDetail.GetList(contract, MsgType, frmDt, toDt);
            var jsonResult = Json(oOutboundTransactionDetail, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Detail(string contract, string SequenceNo)
        {
            oOutboundTransactionDetail = new OutboundTransactionDetail();


            bool success = oOutboundTransactionDetail.GetDetail(contract, SequenceNo);
            oOutboundTransactionDetail.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            if (success)
                return View(oOutboundTransactionDetail);
            else
                return View();

        }

        public ActionResult GetMessage(string TransactionId)
        {
            oOutboundTransactionDetail = new OutboundTransactionDetail();
            string XMLmsg = oOutboundTransactionDetail.GetMessage(TransactionId);
            //count of child if count>1 append PlusOutbound
            XMLmsg = "<PlusProcessOutbound xmlns='http://PlusProcessOutbound'>" +
               XMLmsg +
              "</PlusProcessOutbound>";

            int HeaderNode = NodeCount(XMLmsg);

            if (HeaderNode == 1)
            {
                XMLmsg = XMLmsg.Replace("<PlusProcessOutbound xmlns='http://PlusProcessOutbound'>", "").Replace("</PlusProcessOutbound>", "").Trim();
            }

            ViewBag.XMLData = XElement.Parse(XMLmsg);

            return View();
        }

        public int NodeCount(string xmlString)
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("p", "http://PlusProcessOutbound");

            XmlNodeList nodeList = xmlDoc.SelectNodes("//p:Header", nsManager);

            int count = nodeList.Count;
            return count;
        }



    }
}