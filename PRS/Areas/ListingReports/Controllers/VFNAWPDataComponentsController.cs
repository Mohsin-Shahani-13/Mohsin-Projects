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
    public class VFNAWPDataComponentsController : Controller
    {
        // GET: ListingReports/VFNAWPDataComponents
        VFNAWPDataComponents oVFNAWPDataComponents;
        public ActionResult Index(string RptCode, string menuTitle)
        {
            oVFNAWPDataComponents = new VFNAWPDataComponents();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oVFNAWPDataComponents);
        }

        public JsonResult GetList()
        {
            string menuTitle = string.Empty;
            string RptCode;
            oVFNAWPDataComponents = new VFNAWPDataComponents();
            oVFNAWPDataComponents.GetList();
            var jsonResult = Json(oVFNAWPDataComponents, JsonRequestBehavior.AllowGet);
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