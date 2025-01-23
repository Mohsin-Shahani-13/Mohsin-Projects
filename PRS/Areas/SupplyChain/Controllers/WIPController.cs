using IP.Areas.SupplyChain.Models;
using IP.Classess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;
using IP.Classess;


namespace IP.Areas.SupplyChain.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class WIPController : Controller
    {
        // GET: ListingReports/WIP
        WIP oWIP = new WIP();

        public ActionResult Option()
        {
            oWIP = new WIP();
            return View(oWIP);
        }

        public ActionResult GetWIP(string programId, string programName, String Option, string menuTitle, string RptCode)
        {
            ViewBag.Option = Option;
            try
            {
                WIP oWIP = new WIP();
                ViewBag.ProgramID = programId;
                ViewBag.ProgramName = programName;

                bool success = oWIP.GetList(programId, programName);
                if (!string.IsNullOrEmpty(programName))
                    ViewBag.ReportTitle = "Work In Progress > Program = '" + programName + "'";
                else
                    ViewBag.ReportTitle = "Work In Progress";

                //LOAD MRU & LOG QUERY
                if (ViewBag.ReportTitle != null && RptCode != null)
                {

                    menuTitle = "Work In Progress";
                    cLog oLog = new cLog();
                    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }
                



                if (success)
                    return View(oWIP);
                else
                    return View();
            }
            catch (Exception e)
            {
                ViewBag.ErrMessage = e.Message;
                return View();
            }

        }
       
        public ActionResult GetUnits(string partNo, string workStation, string programId, string statusId, string attribute)
        {
            try
            {
                ViewBag.ReportTitle = "SN List @ "+ workStation +"";
                WIP oWIP = new WIP();
                cSite oSite = new cSite();
               
                //string programId = oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["DefaultSite"].ToString());
                bool success = oWIP.GetWOUnit(partNo, workStation, programId, statusId, attribute);
                oWIP.serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };

                if (success)
                    return View(oWIP);
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