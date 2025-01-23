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
    public class RepeatReturnController : Controller
    {
        // GET: SupplyChain/RepeatReturn
        RepeatReturn oRepeatReturn = new RepeatReturn();

        public ActionResult Option()
        {
            oRepeatReturn = new RepeatReturn();
            DataTable dtProgram = oRepeatReturn.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oRepeatReturn);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oRepeatReturn = new RepeatReturn();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oRepeatReturn);
        }

        public JsonResult GetList(string frmDt, string toDate,bool isAllDate, string programId, string programName, string serialNo, string hasRepaired, string hasRepairedText)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oRepeatReturn = new RepeatReturn();
            oRepeatReturn.GetList(frmDt, toDate, isAllDate, programId, programName, serialNo, hasRepaired, hasRepairedText);
            var jsonResult = Json(oRepeatReturn, JsonRequestBehavior.AllowGet);
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
    }
}