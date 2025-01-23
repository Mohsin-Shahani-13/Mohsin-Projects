using IP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IP.ActionFilters;

namespace IP.Controllers
{
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public class SqlDocsController : Controller
    {
            // GET: SqlDocs
            SqlDocs oSqlDocs = new SqlDocs();
            string rptCode = string.Empty;

            public ActionResult Index()
            {
                TempData["rptTitle"] = Request.QueryString["rptTitle"];
                TempData.Keep();
                return View();
            }


            public JsonResult GetQuery(string rptCode, string rptTitle, string isDetail)
            {
                // string sessionName = "crntQuery" + rptCode;
                oSqlDocs.GetQuery(rptCode, rptTitle, isDetail);
                return Json(oSqlDocs.lstQuery, JsonRequestBehavior.AllowGet);
            }

            //Get SQL BLogs
            public JsonResult GetBlog(string rptCode)
            {
                oSqlDocs.GetBlog(rptCode);
                return Json(oSqlDocs.lstSqlBlog, JsonRequestBehavior.AllowGet);
            }

            //Add SQL Blogs 
            public void AddBlog(string rptCode, string cmnt)
            {
                oSqlDocs.AddBlog(cmnt, rptCode);
            }


        }
    }