using IP.Areas.ListingReports.Models;
using IP.Classess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;
using System.Data;
using IP.Classess;

namespace IP.Areas.ListingReports.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class BoseREMANController : Controller
    {
        BoseREMAN oBoseREMAN;
        // GET: ListingReports/BoseREMAN
        public ActionResult Option()
        {
            oBoseREMAN = new BoseREMAN();
            DataTable dtProgram = oBoseREMAN.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oBoseREMAN);
        }

        public ActionResult Index(string fDate, string tDate, string programId, string programName, string menuTitle, string RptCode)
        {

            try
            {
                BoseREMAN oBoseREMAN = new BoseREMAN();

                bool success = oBoseREMAN.GetList(fDate, tDate, programId, programName);

                ViewBag.ReportTitle = "REMAN > Program = BOSE ";

                ViewBag.ReportTitle += "| From = '" + fDate + "' To = '" + tDate + "' ";
                //LOAD MRU & LOG QUERY
                if (ViewBag.ReportTitle != null && RptCode != null)
                {

                    menuTitle = "Repair";
                    cLog oLog = new cLog();
                    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }




                if (success)
                    return View(oBoseREMAN);
                else
                    return View();
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }

        }
        public ActionResult GetDetail(string programId, string RemanValue, string frmDt, string toDt)
        {
            try
            {
                ViewBag.ReportTitle = "Bose REMAN Detail > " + "REMAN Value = " + RemanValue + "  " + "|" + " From " + frmDt + " To " + toDt + "";
                oBoseREMAN = new BoseREMAN();


                //string programId = oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["DefaultSite"].ToString());
                bool success = oBoseREMAN.GetDetail(programId, RemanValue, frmDt, toDt);
                oBoseREMAN.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

                if (success)
                    return View(oBoseREMAN);
                else
                    return View();
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }
        }

    }
}