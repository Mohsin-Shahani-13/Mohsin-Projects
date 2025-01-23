using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using System.Text;
using System.Data;
using IP.ActionFilters;

namespace IP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class LinkPowerBIController : Controller
    {        
        // GET: LinkPowerBI 

        LinkPowerBI oLinkPowerBI = new LinkPowerBI();
            public ActionResult Option()
            {

            oLinkPowerBI = new LinkPowerBI();
            DataTable dtcategory = oLinkPowerBI.Category();
            ViewBag.ddCategory = cCommon.ToDropDown(dtcategory, "Category", "Category", "All");
            return View(oLinkPowerBI);
            }
            public ActionResult Index(string ddCategory)
            {
            ViewBag.ReportTitle = "Power BI Reports";
            ViewBag.Category = ddCategory;
            ViewBag.CONN_TYPE = Session["CONN_TYPE"].ToString();
            LinkPowerBI oLinkPowerBI = new LinkPowerBI();
            oLinkPowerBI.GetList(ddCategory);
            return View(oLinkPowerBI);
            }
        public JsonResult GetLog(string rptTitle, string rptUrl, string rptCode)
        {
            //LOAD MRU & LOG QUERY
            cLog oLog = new cLog();

            oLog.SaveLog("PBI-" + rptTitle, rptUrl, rptCode);

            var jsonResult = Json(rptUrl, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}