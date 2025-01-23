using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using IP.Areas.SupplyChain.Models;
using System.Data;
using IP.ActionFilters;

namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class BonepileController : Controller
    {
        // GET: SupplyChain/Bonepile
        Bonepile OBonepile;
        public ActionResult Option() 
        {
            OBonepile = new Bonepile();
            DataTable dtProgram = OBonepile.GetProgramBySite();
            ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "ProgramName", "");
            //DataTable dtId = OBonepile.GetIDBySite();
            //ViewBag.ddId = cCommon.ToDropDown(dtId, "ID", "ID", "");
            return View(OBonepile);
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            OBonepile = new Bonepile();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(OBonepile);
        }
        public JsonResult GetList( string ProgramId, string ProgramName, string partNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            OBonepile = new Bonepile();
            OBonepile.GetList(ProgramId, ProgramName, partNo);
            var jsonResult = Json(OBonepile, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //LOAD MRU & LOG QUERY
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                menuTitle = TempData["ReportTitle"] as string;
                RptCode = TempData["RptCode"].ToString();
                TempData.Keep();
                cLog olog = new cLog();
                olog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }
            return jsonResult;
        }
    }
}