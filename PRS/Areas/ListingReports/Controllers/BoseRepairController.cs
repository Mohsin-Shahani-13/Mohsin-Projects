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
    public class BoseRepairController : Controller
    {
        // GET: ListingReports/BoseRepair
        BoseRepair oBoseRepair = new BoseRepair();
        public ActionResult Option()
        {
            oBoseRepair = new BoseRepair();
            DataTable dtProgram = oBoseRepair.Program();
            ViewBag.ddlProgram = cCommon.ToDropDown(dtProgram, "ProgramId", "Program", "");
            return View(oBoseRepair);
        }

        public ActionResult GetList(string fDate, string tDate, string programId, string programName, string menuTitle, string RptCode)
        {
            
            try
            {
                BoseRepair oBoseRepair = new BoseRepair();
               
                bool success = oBoseRepair.GetList(fDate, tDate, programId, programName);
               
                    ViewBag.ReportTitle = "Repair > Program = BOSE ";

                ViewBag.ReportTitle += "| From = '" + fDate + "' To = '" + tDate + "' ";
                //LOAD MRU & LOG QUERY
                if (ViewBag.ReportTitle != null && RptCode != null)
                {

                    menuTitle = "Repair";
                    cLog oLog = new cLog();
                    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }




                if (success)
                    return View(oBoseRepair);
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