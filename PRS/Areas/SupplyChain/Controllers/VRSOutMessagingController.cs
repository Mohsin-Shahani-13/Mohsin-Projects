using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;
using IP.Externals;
using System.Collections;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class VRSOutMessagingController : Controller
    {
        // GET: SupplyChain/VRSOutMessaging
        VRSOutMessaging oVRSOutMessaging;

        public ActionResult Option()
        {
            oVRSOutMessaging = new VRSOutMessaging();
            DataTable dtcontract = oVRSOutMessaging.Contract();
            ViewBag.ddContract = cCommon.ToDropDown(dtcontract, "Contract", "ProgramName", "");
            var contract = dtcontract.Rows.Count > 0 ? Convert.ToInt32(dtcontract.Rows[0]["Contract"]) : 0;
            List<ArrayList> outMsgTypeList = oVRSOutMessaging.GetMessageType(contract);

            return View(oVRSOutMessaging);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oVRSOutMessaging = new VRSOutMessaging();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oVRSOutMessaging);
        }

        public JsonResult GetList(string contract, string fDate, string tDate, string orderNo, string serialNo, string OutMsgType, string isFail, string isSuccessed, string isUnprocessed)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oVRSOutMessaging = new VRSOutMessaging();
            oVRSOutMessaging.GetList(contract, fDate, tDate, orderNo, serialNo, OutMsgType, isFail, isSuccessed, isUnprocessed);
            var jsonResult = Json(oVRSOutMessaging, JsonRequestBehavior.AllowGet);
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
        public JsonResult MsgTypesByProgram(int contract)
        {
            oVRSOutMessaging = new VRSOutMessaging();

            List<ArrayList> outMsgTypeList = oVRSOutMessaging.GetMessageType(contract);
            var jsonCompatibleList = outMsgTypeList.Select(item => new
            {
                Value = item[0].ToString(), 
                Text = item[0].ToString()    

            }).ToList();

            return Json(jsonCompatibleList, JsonRequestBehavior.AllowGet);
        }

    }
}