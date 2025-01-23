using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;
using IP.Areas.ListingReports.Models;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class BoseWIPRemanController : Controller
    {
        // GET: ListingReports/BoseWIPReman
        BoseWIPReman oBoseWIPReman;
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oBoseWIPReman = new BoseWIPReman();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oBoseWIPReman);
        }
        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oBoseWIPReman = new BoseWIPReman();
            oBoseWIPReman.GetList();
            var jsonResult = Json(oBoseWIPReman, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetUnits(string programId, string Line, string CodeName)
        {
            try
            {
                ViewBag.ReportTitle = Line;
                BoseWIPReman oBoseWIPReman = new BoseWIPReman();


                //string programId = oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["DefaultSite"].ToString());
                bool success = oBoseWIPReman.GetUnits(programId, Line, CodeName);
                oBoseWIPReman.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

                if (success)
                    return View(oBoseWIPReman);
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