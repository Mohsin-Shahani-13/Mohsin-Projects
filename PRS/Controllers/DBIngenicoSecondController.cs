using IP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Controllers
{
    [OutputCache(Duration = 0)]
    public class DBIngenicoSecondController : Controller
    {
        // GET: DBIngenicoSecond
        DBIngenicoSecond odbIngenicoSec;

        public ActionResult Index()
        {
            ViewBag.ReportTitle = "Ingenico Second";
            odbIngenicoSec = new DBIngenicoSecond();
            // oIngenico.GetData(shift);

            return View(odbIngenicoSec);
        }

        public JsonResult GetList()
        {
            odbIngenicoSec = new DBIngenicoSecond();
            odbIngenicoSec.GetDashboard();
            var jsonResult = Json(odbIngenicoSec, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            odbIngenicoSec.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return jsonResult;
        }
    }
}