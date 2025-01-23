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
    public class WidgetController : Controller
    {
        // GET: Widget
        public ActionResult Index(string rptCode, string menuTitle)
        {
           WidgetModel  oModel = new WidgetModel();
            oModel.ReportTitle = menuTitle;
            oModel.ReportCode = rptCode;
            TempData["ReportTitle"] = menuTitle;
            TempData["RptCode"] = rptCode;
            if (TempData["ReportTitle"] != null && TempData["RptCode"] != null)
            {
                TempData.Keep();
                cLog oLog = new cLog();
                oLog.SaveLog(menuTitle, Request.Url.PathAndQuery, rptCode);
            }

            return View(oModel);
        }

        [HttpGet]
        public JsonResult GetWidgetList(string report_code)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            WidgetModel oModel = new WidgetModel();
            oModel.ReportCode = report_code;
            oModel.Get_Widget_List();
            response["lst_widgets"] = oModel.lst_widgets;
            response["IsValid"] = true;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetWidget(string widget_id)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            WidgetModel oModel = new WidgetModel();
            oModel.widget_id = widget_id;
            oModel.Get_Widget();
            response["title"] = oModel.widget_title;
            response["title_short"] = oModel.widget_title_short;
            response["desc"] = oModel.widget_desc;
            response["type"] = oModel.widget_type;
            response["min_size"] = oModel.widget_min_size;
            response["max_size"] = oModel.widget_max_size;
            response["query"] = oModel.widget_query;
            response["URL"] = oModel.widget_URL;
            response["bg_color"] = oModel.widget_background_color;
            response["column_format"] = oModel.widget_column_format;
            response["group_id"] = oModel.widget_group_id;
            response["count_type"] = oModel.widget_count_type;
            response["type"] = oModel.widget_type;

            response["IsValid"] = true;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddWidget(WidgetModel oModel)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            oModel.Save();
            response["IsValid"] = true;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateWidget(WidgetModel oModel)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            oModel.Update();
            response["IsValid"] = true;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}