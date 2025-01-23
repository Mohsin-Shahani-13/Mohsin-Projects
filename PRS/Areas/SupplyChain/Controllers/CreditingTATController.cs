using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.Areas.SupplyChain.Models;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class CreditingTATController : Controller
    {
        CreditingTAT oCreditingTAT = new CreditingTAT();
        // GET: SupplyChain/CreditingTAT
        public ActionResult Option()
        {
            oCreditingTAT = new CreditingTAT();
            DataTable dtProgram = oCreditingTAT.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oCreditingTAT);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oCreditingTAT = new CreditingTAT();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oCreditingTAT); 
        }

        public JsonResult GetList(string frmDt, string toDate, string programId, string programName)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oCreditingTAT = new CreditingTAT();
            oCreditingTAT.GetList(frmDt, toDate, programId, programName);
            var jsonResult = Json(oCreditingTAT, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetDetail(string programId, string ReturnReason, string frmDt, string toDt)
        {
            try
            {
                ViewBag.ReportTitle = "Crediting TAT Detail > " + "Return Reason = " + ReturnReason + "  " + "|" + " From " + frmDt + " To " + toDt + "";
                CreditingTAT oCreditingTAT = new CreditingTAT();


                //string programId = oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["DefaultSite"].ToString());
                bool success = oCreditingTAT.GetDetail(programId, ReturnReason, frmDt, toDt);
                oCreditingTAT.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

                if (success)
                    return View(oCreditingTAT);
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