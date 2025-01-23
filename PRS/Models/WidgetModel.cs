using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace IP.Models
{
    public class WidgetModel
    {
        #region Data Fields
        public string ReportTitle { get; set; }
        public string ReportCode { get; set; }
        public List<ArrayList> lst_widgets { get; set; }
        public string widget_id { get; set; }
        public List<string> error_messages { get; set; }
        public string widget_title { get; set; }
        public string widget_title_short { get; set; }
        public string widget_desc { get; set; }
        public string widget_type { get; set; }
        public string widget_min_size { get; set; }
        public string widget_max_size { get; set; }
        public string widget_query { get; set; }
        public string widget_URL { get; set; }
        public string widget_column_format { get; set; }
        public string widget_background_color { get; set; }
        public string widget_count_type { get; set; }
        public string widget_group_id { get; set; }
        public string widget_icon { get; set; }

        #endregion
        public void Get_Widget_List()
        {
            cDAL oDAL = new cDAL("INIT");
            string query = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT WidgetId, WidgetTitle, WidgetType, CASE WHEN WidgetStatus = 1 THEN 'Y' ELSE 'N' END FROM IP.Widgets ");
            sb.AppendLine("WHERE Company = '" + HttpContext.Current.Session["CompanyCode"].ToString() + "' ");
            sb.AppendLine("ORDER BY WidgetType, WidgetTitle");
            sb.AppendLine();
            DataTable dt = oDAL.GetData(sb.ToString());
            if (!ReportCode.Equals(string.Empty))
            {
                //For SQL Documentation
                cLog oLog = new cLog();
                oLog.AddSqlQuery(ReportCode, sb.ToString(), string.Empty, false);
            }

            lst_widgets = cCommon.ConvertDtToArrayList(dt);
        }

        public void Get_Widget()
        {
            cDAL oDAL = new cDAL("INIT");
            string query = string.Empty;

            query += "SELECT * FROM IP.Widgets ";
            query += "WHERE Company = '" + HttpContext.Current.Session["CompanyCode"].ToString() + "' ";
            query += "AND WIdgetId = " + widget_id + " ";
            DataTable dt = oDAL.GetData(query);

            widget_title = dt.Rows[0]["WidgetTitle"].ToString();
            widget_desc = dt.Rows[0]["WidgetDesc"].ToString();
            widget_URL = dt.Rows[0]["URL"].ToString();
            widget_title_short = dt.Rows[0]["WidgetTitleShort"].ToString();
            widget_min_size = dt.Rows[0]["WidgetMinSize"].ToString();
            widget_max_size = dt.Rows[0]["WidgetMaxSize"].ToString();
            widget_query = dt.Rows[0]["WidgetQuery"].ToString();
            widget_column_format = dt.Rows[0]["ColumnFormat"].ToString();
            widget_group_id = dt.Rows[0]["GroupId"].ToString();
            widget_background_color = dt.Rows[0]["BackgroundColor"].ToString();
            widget_icon = dt.Rows[0]["WidgetIcon"].ToString();
            widget_count_type = dt.Rows[0]["CountType"].ToString();
            widget_type = dt.Rows[0]["WidgetType"].ToString();


            lst_widgets = cCommon.ConvertDtToArrayList(dt);
        }

        public void Save()
        {
            cDAL oDAL = new cDAL("INIT");
            string company = HttpContext.Current.Session["CompanyCode"].ToString();

            if (widget_group_id == null || widget_group_id.Trim().Length==0)
                widget_group_id = "NULL";

            if (widget_query!=null)
                widget_query = widget_query.Replace("'", "''");
            string query = "INSERT  INTO IP.WIDGETS (Company, WidgetTitle, WidgetTitleShort, ";
            query += "WidgetDesc, WidgetType, WidgetMinSize, WidgetMaxSize,WidgetQuery, URL, ColumnFormat, ";
            query += "BackgroundColor, CountType, WidgetIcon, GroupId, WidgetStatus) VALUES ";
            query += "('" + company + "', '" + widget_title + "','" + widget_title_short + "', ";
            query += "'" + widget_desc + "', '" + widget_type + "','" + widget_min_size + "','" + widget_max_size + "', ";
            query += "'" + widget_query + "','" + widget_URL + "','" + widget_column_format + "', ";
            query += "'" + widget_background_color + "','" + widget_count_type + "', ";
            query += "'" + widget_icon + "', " + widget_group_id + ", 1)";

            oDAL.Execute(query);
        }
        public void Update()
        {
            cDAL oDAL = new cDAL("INIT");
            string company = HttpContext.Current.Session["CompanyCode"].ToString();

            if (widget_group_id == null || widget_group_id.Trim().Length == 0)
                widget_group_id = "NULL";


            if (widget_query!= null)
                widget_query = widget_query.Replace("'", "''");

            string query = "UPDATE IP.WIDGETS SET ";
            query += "WidgetTitle = '" + widget_title + "', ";
            query += "WidgetTitleShort  = '" + widget_title_short + "', ";
            query += " WidgetDesc = '" + widget_desc + "', ";
            query += "WidgetType = '" + widget_type + "', ";
            query += " WidgetMinSize = '" + widget_min_size + "', ";
            query += "WidgetMaxSize = '" + widget_max_size + "', ";
            query += "WidgetQuery = '" + widget_query + "', ";
            query += "URL = '" + widget_URL + "', ";
            query += "ColumnFormat = '" + widget_column_format + "', ";
            query += "BackgroundColor = '" + widget_background_color + "', ";
            query += " CountType = '" + widget_count_type + "', ";
            query += " WidgetIcon = '" + widget_icon + "', ";
            query += " GroupId = " + widget_group_id + "  ";
            query += "WHERE WidgetId = " + widget_id + " ";

            oDAL.Execute(query);
        }

    }
}