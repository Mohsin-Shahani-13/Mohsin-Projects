using IP.ActionFilters;
using IP.Areas.SupplyChain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MaterialFulfillmentController : Controller
    {
        // GET: SupplyChain/MaterialFulfillment
        MaterialFulfillment oMaterialFulfillment;
        public ActionResult Option()
        {
            oMaterialFulfillment = new MaterialFulfillment();
            DataTable dtProgram = oMaterialFulfillment.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oMaterialFulfillment);
        }
        public ActionResult Index(string menuTitle, string rptCode)
        {
            oMaterialFulfillment = new MaterialFulfillment();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oMaterialFulfillment);
        }
        public JsonResult GetList(string programID, string programName, string orderType, string reference)
        {
            oMaterialFulfillment = new MaterialFulfillment();
            string menuTitle = string.Empty;
            string RptCode = string.Empty;


            oMaterialFulfillment.GetList(programID, programName, orderType, reference);
            var jsonResult = Json(oMaterialFulfillment, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"].ToString();
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }

        public ActionResult Detail(string reference, string programID)
        {
            ViewBag.ReportTitle = "Reference = " + reference + " ";
            oMaterialFulfillment = new MaterialFulfillment();

            bool success = oMaterialFulfillment.Detail(reference, programID);
            oMaterialFulfillment.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return View(oMaterialFulfillment);
        }
    }
}