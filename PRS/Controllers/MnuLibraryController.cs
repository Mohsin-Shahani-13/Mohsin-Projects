using IP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;
using System.Data;

namespace IP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]

    public class MnuLibraryController : Controller
    {
        MnuLibrary oMnuLibrary = new MnuLibrary();
        // GET: MnuLibrary
        public ActionResult Index()
        {

            ViewBag.ReportTitle = "Menu Library";
            ViewBag.CONN_TYPE = Session["CONN_TYPE"].ToString();
            ViewBag.EmpId = Session["EmpId"].ToString();

            //Save Programe in session
            //ViewBag.ProgramForSite = Session["ProgramForSite"].ToString().Replace("'","");
            string programBySite = oMnuLibrary.GetProgramBysite(Session["DefaultSite"].ToString()); // Get Programs for site selected
            ViewBag.programBySite = programBySite;

            //ViewBag.isFavourite = oMnuLibrary.IsFav();
            MnuLibrary oMnu = new MnuLibrary();
            //oMnu.GetList();
            return View(oMnu);
        }

        public JsonResult GetList()
        {
            oMnuLibrary.GetList();
            var jsonResult = Json(oMnuLibrary, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpPost]
        public JsonResult GetFav(string rptCode, string rptTitle, string MnuId)
        {

            oMnuLibrary.GetFavorite(rptCode, rptTitle, MnuId);
            var jsonResult = Json(oMnuLibrary, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpPost]
        public JsonResult DeleteFav(string MnuId)
        {
            oMnuLibrary.DeleteFav(MnuId);
            var jsonResult = Json(oMnuLibrary, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}