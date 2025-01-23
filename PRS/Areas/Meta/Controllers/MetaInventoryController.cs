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
{ //  Name change to Month to Date Inventory
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class MetaInventoryController : Controller
    {
        MetaInventory oMetaInventory = new MetaInventory();

        public string manuTitle { get; private set; }

        // GET: Meta/MetaInventory

        public ActionResult Option()
        {
            oMetaInventory = new MetaInventory();
            return View();
        }

        public ActionResult Index(string RptCode, string menuTitle)
        {
            oMetaInventory = new MetaInventory();
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View();
        }
        public JsonResult GetList(string programId, string ProgramName, string partno, string UnitOfMeasure, string WareHouse, string location, string Total, string source, string timestamp)
        {
            string menuTitle = string.Empty;
            string RptCode;
            oMetaInventory = new MetaInventory();
            oMetaInventory.GetList(programId, ProgramName, partno, UnitOfMeasure, WareHouse, location, Total, source, timestamp);
            var jsonResult = Json(oMetaInventory, JsonRequestBehavior.AllowGet);
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
