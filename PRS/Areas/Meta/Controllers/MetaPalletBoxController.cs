using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.Meta.Models;
using System.Data;

using IP.ActionFilters;
namespace IP.Areas.Meta.Controllers
{
    public class MetaPalletBoxController : Controller
    {
        MetaPalletBox oMetaPalletBox = new MetaPalletBox();
        // GET: Meta/MetaPalletBox
        public ActionResult Option()
        {
            oMetaPalletBox = new MetaPalletBox();
            //DataTable dtProgram = oMetaPalletBox.GetProgramBySite();
            //ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oMetaPalletBox);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaPalletBox = new MetaPalletBox();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string frmDt, string toDate, string programId, string ProgramName, string PalletNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaPalletBox = new MetaPalletBox();
            oMetaPalletBox.GetList(frmDt, toDate, Session["ProgramIdBySiteForMeta"].ToString(), "META", PalletNo);
            var jsonResult = Json(oMetaPalletBox, JsonRequestBehavior.AllowGet);
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