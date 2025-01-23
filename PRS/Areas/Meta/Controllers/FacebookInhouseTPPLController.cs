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
using IP.Classess;

namespace IP.Areas.Meta.Controllers
{
    [OutputCache(Duration = 0)]

    public class FacebookInhouseTPPLController : Controller
    {
        // GET: Meta/FacebookInhouseTPPL
        FacebookInhouseTPPL oFacebookInhouseTPPL = new FacebookInhouseTPPL();

        public ActionResult Option()
        {

            oFacebookInhouseTPPL = new FacebookInhouseTPPL();
            //oFacebookInhouseTPPL.GetWareHouse();
            //ViewBag.Sites = Session["DefaultSite"].ToString();
            return View(oFacebookInhouseTPPL);
        }
        [HttpPost]
        public ActionResult GetWarehouse(string ProgramId)
        {
            oFacebookInhouseTPPL = new FacebookInhouseTPPL();
            oFacebookInhouseTPPL.GetWareHouse(ProgramId);
            var jsonResult = Json(oFacebookInhouseTPPL, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oFacebookInhouseTPPL = new FacebookInhouseTPPL();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oFacebookInhouseTPPL);
        }

        public JsonResult GetList(string programId, string ProgramName, string partNo, string warehouse)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oFacebookInhouseTPPL = new FacebookInhouseTPPL();
            oFacebookInhouseTPPL.GetList(programId, ProgramName, partNo, warehouse);
            var jsonResult = Json(oFacebookInhouseTPPL, JsonRequestBehavior.AllowGet);
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
        public ActionResult Download(string programId, string ProgramName, string partNo, string warehouse, string RptCode, string menuTitle)
        {
            string sites = Session["DefaultSite"].ToString();
            byte[] fileBytes;
            oFacebookInhouseTPPL = new FacebookInhouseTPPL();
            bool flag = oFacebookInhouseTPPL.Download(programId, ProgramName, partNo, warehouse);
            //LOAD MRU & LOG QUERY
            if (!string.IsNullOrEmpty(menuTitle) && !string.IsNullOrEmpty(RptCode))
            {
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
            }

            var contentType = "application/octet-stream";

            var zipName = $"MetaInhouseTPPL";
            if (sites == "GRAPEVINE") zipName += "(SFW)";
            else if (sites == "PRAGUE") zipName += "(TEU)";
            else if (sites == "TOKYO") zipName += "(TJP)";
            else if (sites == "SYDNEY") zipName += "(TAU)";
            else if (sites == "HAVANT") zipName += "(SGB)";
            zipName += $"{DateTime.Now.ToString("_yyyyMMddHHmmss")}.csv.gz";

            //var fileName = "MetaInhouseTPPL.csv.gz";

            if (!flag)
            {
                // No Data Found or File Writing Error.
                return Content(oFacebookInhouseTPPL.ErrorMessage, "text/plain");
            }
            else
            {
                // File reading Exception: Try and Catch
                try
                {
                        fileBytes = System.IO.File.ReadAllBytes(oFacebookInhouseTPPL.outputCsvPath);
                    return File(fileBytes, contentType, zipName);
                }

                catch (Exception e)
                {
                    ViewBag.ErrMessage = e.Message;
                    return Content(e.Message, "text/plain");
                }
            }
        }
    }

}