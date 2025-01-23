using IP.Areas.Meta.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace IP.Areas.Meta.Controllers
{
    public class MetaRepairPartConsumptionController : Controller
    {

        // GET: Meta/MetaRepairPartConsumption
        MetaRepairPartConsumption oMetaRepairPartConsumption = new MetaRepairPartConsumption();
        public ActionResult Option()
        {
            oMetaRepairPartConsumption = new MetaRepairPartConsumption();
            //DataTable dtProgram = oMetaRepairPartConsumption.GetProgramBySite();
            //ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "programId", "programName", "");
            return View(oMetaRepairPartConsumption);
        }
        public ActionResult Index(string RptCode, string menuTitle)
        { 
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = RptCode;
            ViewBag.ReportTitle = menuTitle;
            return View(oMetaRepairPartConsumption);
        }
        
        [HttpPost]
        public JsonResult GetList(string programId, string programName, HttpPostedFileBase importFile)
        {
            string menuTitle = string.Empty;
            string RptCode = string.Empty;
            oMetaRepairPartConsumption = new MetaRepairPartConsumption();
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("SerialNo", typeof(string));
            dataTable.Columns.Add("UploadedBy", typeof(string));
            dataTable.Columns.Add("UploadFrom", typeof(string));
            string empName = Session["EmpName"].ToString();

            try
            {
                string serialNos = string.Empty;

                using (var reader = new StreamReader(importFile.InputStream))
                {
                    int serialCount = 1;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            if (serialCount > 5000) return Json(new { Status = 2, Message = "There is a limit of 5000 Serial No.s only." });
                            //if (!Regex.IsMatch(line, "^[a-zA-Z0-9]*$")) ^[a-zA-Z0-9 ]*$
                            if (!Regex.IsMatch(line, "^[A-Za-z0-9]{5,50}$|^[A-Za-z0-9\\|]{5,50}$"))
                            {
                                return Json(new { Status = 3, Message = "The input file contains characters that are not valid or acceptable." });
                            }
                            //serialNos += "'" + line.Trim() + "'" + ",";
                            //lines.Add(line.Trim().TrimEnd(','));
                            dataTable.Rows.Add(line, empName, "PRS");
                        }
                        serialCount++;
                    }
                    //serialNos = serialNos.TrimEnd(',');
                }
                oMetaRepairPartConsumption.GetList(Session["ProgramIdBySiteForMeta"].ToString(), "META", dataTable);
                var jsonResult = Json(oMetaRepairPartConsumption, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
                {
                    menuTitle = TempData["ReportTitle"] as string;
                    RptCode = TempData["RptCode"].ToString();
                    TempData.Keep();
                    cLog oLog = new cLog();
                    oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, RptCode);
                }
                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = "An error occurred. Contact Support Team" });
                //Message = ex.Message
            }

        }

    }
}