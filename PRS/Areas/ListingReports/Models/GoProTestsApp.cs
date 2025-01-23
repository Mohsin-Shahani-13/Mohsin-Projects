using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.ListingReports.Models
{
    public class GoProTestsApp
    {
        #region Fields
        [Display(Name = "Customer:")]
        public string customer { get; set; }
        [Display(Name = "App:")]
        public string app { get; set; }

        [Display(Name = "Result:")]
        public string result { get; set; }
        [Display(Name = "Active:")]
        public string active { get; set; }

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);

        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Serial No.:")]
        public string SerialNo { get; set; }
        [Display(Name = "Tester Name:")]
        public string testerName { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstGoProTestsApp { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        cDAL oDAL;


        #endregion

        public DataTable GetCustomer()
        {
            oDAL = new cDAL("GOPRO");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select upper(Customer) as ID,upper(Customer) as Description
from dbo.UnitResult
where Customer in ('ARLO','BBACK','DELL','FIRSTGROUP','GOPRO','NETGEAR','POYNT','TTR','VERIZONCALEXICO')
and App != ''
group by Customer order by Customer ";
            DataTable dt = oDAL.GetData(query);


            return dt;
        }
        public DataTable GetApp()
        {
            oDAL = new cDAL("GOPRO");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select upper(App) as ID,upper(App) as Description
from dbo.UnitResult
where Customer in ('ARLO','BBACK','DELL','FIRSTGROUP','GOPRO','NETGEAR','POYNT','TTR','VERIZONCALEXICO')
and App != ''
group by App order by App ";
            DataTable dt = oDAL.GetData(query);


            return dt;
        }
        public bool GetList(string frmDt, string toDate, string customer, string app, string result, string active)
        {
            oDAL = new cDAL("GOPRO");
            string query = string.Empty;
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            query = @" 
 
select 
	ur.UnitResultId
	,ur.SerialNumber
	,ur.Result
	,ur.Customer
	,ur.Area
	,CASE WHEN ur.Active = 1 THEN 'Yes' ELSE 'No' END AS Active
	,ur.App
	,ur.InsertedBy
	,ur.InsertedOn	
	,ur.XML
from 
	dbo.UnitResult ur	


  ";
            query += "WHERE CONVERT(Date, ur.InsertedOn) >= '<frmDt>' AND CONVERT(Date, ur.InsertedOn) <= '<toDt>'";

            if (!string.IsNullOrEmpty(customer))
            {
                query += "AND ur.Customer = '" + customer + "' ";
            }

            if (!string.IsNullOrEmpty(app))
            {
                query += "AND ur.App = '" + app + "' ";
            }

            if (!string.IsNullOrEmpty(result))
            {
                query += "AND ur.Result = '" + result + "' ";
            }

            if (active != "ALL")
            {
                query += "AND ur.Active = '" + active + "' ";
            }

            query += "order by ur.UnitResultId ";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            filterString += " > From = '" + frmDt + "' To = '" + toDate + "' ";

         
                

            DataTable dt = oDAL.GetData(query);


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("260", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstGoProTestsApp = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}