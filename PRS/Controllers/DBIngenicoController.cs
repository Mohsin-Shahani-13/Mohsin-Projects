using IP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Controllers
{
    [OutputCache(Duration = 0)]
    public class DBIngenicoController : Controller
    {
        // GET: DBIngenico
        DBIngenico odbIngenico;
        public ActionResult Index()
        {
            ViewBag.ReportTitle = "Ingenico";
            odbIngenico = new DBIngenico();
            // oIngenico.GetData(shift);

            return View(odbIngenico);
        }

        public JsonResult GetList()
        {
            odbIngenico = new DBIngenico();
            odbIngenico.GetData();
            var jsonResult = Json(odbIngenico, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            odbIngenico.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return jsonResult;
        }
    }
}