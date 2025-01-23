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
    public class BoseCreditingTATByWeekController : Controller
    {
        // GET: ListingReports/BoseCreditingTATByWeek
        BoseCreditingTATByWeek oBoseCreditingTATByWeek = new BoseCreditingTATByWeek();
        public ActionResult Option()
        {
            oBoseCreditingTATByWeek = new BoseCreditingTATByWeek();
            DataTable dtProgram = oBoseCreditingTATByWeek.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oBoseCreditingTATByWeek);
        }

        public ActionResult Index(string fDate, string tDate, string programId, string programName, string menuTitle, string RptCode)
        {

            try
            {
                BoseCreditingTATByWeek oBoseCreditingTATByWeek = new BoseCreditingTATByWeek();

                bool success = oBoseCreditingTATByWeek.GetList(fDate, tDate, programId, programName);

                ViewBag.ReportTitle = " Crediting TAT by Weeks > Program = BOSE ";

                ViewBag.ReportTitle += "| From = '" + fDate + "' To = '" + tDate + "' ";
                //LOAD MRU & LOG QUERY
                if (ViewBag.ReportTitle != null && RptCode != null)
                {

                    menuTitle = "Crediting TAT By Weeks";
                    cLog oLog = new cLog();
                    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }




                if (success)
                    return View(oBoseCreditingTATByWeek);
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