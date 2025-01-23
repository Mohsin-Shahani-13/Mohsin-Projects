using IP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Controllers
{
    [OutputCache(Duration = 0)]
    public class DBIngenicoThirdController : Controller
    {
        // GET: DBIngenicoThird
        DBIngenicoThird odbIngenico;

        public ActionResult Index()
        {
            ViewBag.ReportTitle = "Ingenico-Third";
            odbIngenico = new DBIngenicoThird();
            // oIngenico.GetData(shift);

            return View(odbIngenico);
        }

        public JsonResult GetList()
        {
            odbIngenico = new DBIngenicoThird();
            odbIngenico.GetData();
            var jsonResult = Json(odbIngenico, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            odbIngenico.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return jsonResult;
        }
    }
}