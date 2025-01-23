using IP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using IP.ActionFilters;

namespace IP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class DocumentsController : Controller
    {
        // GET: Documents
        public ActionResult Index()
        {
            ViewBag.ReportTitle = "Documents";
            //ViewBag.CONN_TYPE = Session["CONN_TYPE"].ToString();
            Documents oDoc = new Documents();
            oDoc.GetList();
            return View(oDoc);
        }

        public JsonResult GetPoliciesList()
        {
            cLog oLog = new cLog();

            Documents oDoc = new Documents();
            oDoc.GetList();
            var jsonResult = Json(oDoc, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public FileResult Download(string LnkUrl, string LnkName)
        {
            Documents oDoc = new Documents();
            byte[] FileBytes = oDoc.GetFile(LnkUrl, LnkName);

            FileContentResult file = null;

            if (LnkUrl.Contains("pdf"))
                file = File(FileBytes, "application/pdf", Path.GetFileName(LnkUrl));
            else if (LnkUrl.Contains("xlsx"))
            {
                return File(FileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(LnkUrl)) ;
            }
            else if (LnkUrl.Contains("docx"))
            {
                return File(FileBytes, "application/vnd.ms-word", Path.GetFileName(LnkUrl));
            }
            else if (LnkUrl.Contains("POWERPOINT"))
            {
                return File(FileBytes, "application/vnd.ms-powerpoint", Path.GetFileName(LnkUrl));
            }
            else if (LnkUrl.Contains("TEXT"))
            {
                return File(FileBytes, "text/plain", Path.GetFileName(LnkUrl));
            }
            return file;
        }
    }
}