using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.ListingReports.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class PartsSerialController : Controller
    {
        // GET: ListingReports/PartsSerial
        PartsSerial oPartsSerial = new PartsSerial();

        public ActionResult Option()
        {
            oPartsSerial = new PartsSerial();
            return View(oPartsSerial);

        }
        public ActionResult OptionBose()
        {
            oPartsSerial = new PartsSerial();
            DataTable dtProgram = oPartsSerial.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oPartsSerial);

        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oPartsSerial = new PartsSerial();
            //TempData["ReportTitle"] = menuTitle;
            //TempData["RptCode"] = RptCode;
            //ViewBag.ReportTitle = menuTitle;
            if (menuTitle != null)
            {
                ViewBag.ReportTitle = menuTitle;
                TempData["ReportTitle"] = menuTitle;
                TempData["RptCode"] = RptCode;
                ViewBag.RptCode = RptCode;
            }
            else
            {
                ViewBag.ReportTitle = "Serial Number History";
                TempData["ReportTitle"] = "Serial Number History ";
                TempData.Keep("ReportTitle");
                TempData["RptCode"] = RptCode;
                ViewBag.RptCode = RptCode;
            }

            ViewBag.list = menuTitle;
            return View(oPartsSerial);
        }
        public JsonResult GetList(string frmDt, string toDate, bool isAllDate, string serialNo, string programId, string ProgramName, string partNo, string locNo, string palletBoxNo, string config = "", string originFrom = "")
        {
            string menuTitle = string.Empty;
            string RptCode;
            //LOAD MRU & LOG QUERY
            //if (palletNo == "")
            //{
            //    palletNo = "0";
            //}
            

            oPartsSerial = new PartsSerial();
            oPartsSerial.GetList(frmDt, toDate, isAllDate, serialNo, programId, ProgramName, partNo, locNo, palletBoxNo, config, originFrom);
            var jsonResult = Json(oPartsSerial, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
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
        public ActionResult Detail(string serialNo, string programId, string PartNo)
        {
            if (string.IsNullOrEmpty(serialNo))
                return View();
            try
            {
                oPartsSerial = new PartsSerial();
                bool success = oPartsSerial.GetDetail(serialNo, programId, PartNo);
                if (success)
                    return View(oPartsSerial);
                else
                    return View(oPartsSerial);
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }

            return View();
        }
    }
}