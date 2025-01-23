using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.Models;
using IP.ActionFilters;


namespace IP.Controllers

{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DBShiftsController : Controller
    {
        // GET: DBShifts
        DBShifts odbShift;
        public ActionResult IngenicoShift1()
        {
            ViewBag.ReportTitle = "Ingenico";
            odbShift = new DBShifts();
            return View(odbShift);
        }

        public JsonResult GetList()
        {
            odbShift = new DBShifts();
            odbShift.GetShift1();
            var jsonResult = Json(odbShift, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            odbShift.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return jsonResult;
        }
    }
}