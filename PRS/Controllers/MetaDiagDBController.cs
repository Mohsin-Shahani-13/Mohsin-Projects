using IP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IP.Controllers
{
    public class MetaDiagDBController : Controller
    {
        MetaDiagDB oMetaDiagDB;
        // GET: MetaDiagDB
        public ActionResult Index(string selected, string selectedText, string programId, string site, string programName)
        {
            oMetaDiagDB = new MetaDiagDB();
            ViewBag.ReportTitle = "Meta" + site;
            return View(oMetaDiagDB);
        }
        public JsonResult GetList(string selectedVal, string selectedText, string programId, string site, string programName)
        {
            oMetaDiagDB = new MetaDiagDB();
            oMetaDiagDB.GetData(selectedVal, selectedText,  programId, site, programName);
            var jsonResult = Json(oMetaDiagDB, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            oMetaDiagDB.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            return jsonResult;
        }
        public ActionResult METACounterDB(string selected, string selectedText, string programId, string site, string programName)
        {
            oMetaDiagDB = new MetaDiagDB();
            ViewBag.ReportTitle = programName;
            return View(oMetaDiagDB);
        }
        public JsonResult GetCounterDB(string selectedVal, string selectedText, string programId, string site, string programName)
        {
            oMetaDiagDB = new MetaDiagDB();

            if (oMetaDiagDB.GetCounter(out var resultData , selectedVal, selectedText, programId, site, programName))
            {
                return Json(new { DBTitle = oMetaDiagDB.DBTitle, Data = resultData }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = oMetaDiagDB.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}