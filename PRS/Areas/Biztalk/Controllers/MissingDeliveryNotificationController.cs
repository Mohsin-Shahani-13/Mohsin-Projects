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


namespace IP.Areas.Biztalk.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MissingDeliveryNotificationController : Controller
    {
        MissingDeliveryNotification oMissingDeliveryNotification = new MissingDeliveryNotification();
        // GET: Biztalk/MissingDeliveryNotification
        public ActionResult Option()
        {
            oMissingDeliveryNotification = new MissingDeliveryNotification();
       
            return View(oMissingDeliveryNotification);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMissingDeliveryNotification = new MissingDeliveryNotification();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oMissingDeliveryNotification);
        }

        public JsonResult GetList( string frmDt)
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

            oMissingDeliveryNotification = new MissingDeliveryNotification();
            oMissingDeliveryNotification.GetList( frmDt);
            var jsonResult = Json(oMissingDeliveryNotification, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}