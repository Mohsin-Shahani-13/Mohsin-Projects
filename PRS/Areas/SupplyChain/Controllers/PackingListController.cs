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
    public class PackingListController : Controller
    {
        PackingList OpackingList;
        // GET: SupplyChain/PackingList
        public ActionResult Option()
        {
            OpackingList = new PackingList();
            return View();
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            OpackingList = new PackingList();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(OpackingList);
        }

        public JsonResult GetList(string PalletBoxNo)
        {
            string menuTitle = string.Empty;
            string RptCode;
            OpackingList = new PackingList();
            OpackingList.GetList(PalletBoxNo);
            var jsonResult = Json(OpackingList, JsonRequestBehavior.AllowGet);
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