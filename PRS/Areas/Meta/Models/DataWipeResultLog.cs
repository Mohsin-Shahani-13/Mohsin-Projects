using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.Meta.Models
{
    public class DataWipeResultLog
    {
        #region Fields
        [Display(Name = "Contract:")]
        public string program { get; set; }
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
        public List<Hashtable> lstDataWipeResultLog { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        cDAL oDAL = new cDAL("INIT");
        //new cDAL("ACTIVE");


        #endregion
        private string GetInValue(string Value)
        {
            string[] arr = Value.Split(',');
            string _arr = null;
            foreach (var item in arr)
            {
                if (_arr == null)
                {
                    _arr = "\'" + item.Trim() + "\'";
                }
                else
                {
                    _arr += "," + "\'" + item.Trim() + "\'";
                }

            }
            return _arr;
        }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("Active");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,CONCAT(ID,' - ', Site) AS ProgramName
                             FROM pls.PROGRAM  
                      WHERE NAME = 'Meta'
                      ORDER BY ID ";
            DataTable dt = oDAL.GetData(query);


            return dt;
        }

        public bool GetList(string programId, string programName, string frmDt, string toDate, string SerialNo, string testerName)
        {
            string query = string.Empty;
            string _SerialNo = string.Empty;
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            query = @" 
  SELECT 
       [ID]
      ,[SerialNumber]
      ,[WoHeaderId]
      ,[Contract]
      ,[WorkStation]
      ,[TesterName]
      ,[AsOf]
      ,[Source]
      ,[Request]
      ,[Response]
      ,[Result]
      ,[Message]
      ,[RemoteHost]
  ";
            if (conType == "PROD")
            {
                query += @"FROM [PlusRS].[tia].[TiaServiceLog]";
            }
            else
            {
                query += @"FROM [PlusRS].[tia].[TiaServiceLog_Test]";
            }
            query += "WHERE CONVERT(Date, AsOf) >= '<frmDt>' AND CONVERT(Date, AsOf) <= '<toDt>'";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);
            filterString += " > From = '" + frmDt + "' To = '" + toDate + "' ";

            if (!string.IsNullOrEmpty(programId))
            {
                query += "\nAND Contract =  '" + programId + "' ";
             
                //_SerialNo = GetInValue(SerialNo);
                //filterString += " > Serial No. = " + _SerialNo;
                //query += "\nWHERE SerialNumber IN (" + _SerialNo + ") ";
            }

            if (!string.IsNullOrEmpty(SerialNo))
            {
                query += "\nAND SerialNumber LIKE '%" + SerialNo + "%' ";
                filterString += " | Serial No. Like '" + SerialNo + "'";
                //_SerialNo = GetInValue(SerialNo);
                //filterString += " > Serial No. = " + _SerialNo;
                //query += "\nWHERE SerialNumber IN (" + _SerialNo + ") ";
            }

            if (!string.IsNullOrEmpty(testerName))
            {
                query += "\nAND TesterName LIKE '%" + testerName + "%' ";
                filterString += " | Tester Name Like '" + testerName + "'";
            }
            if (sites == "GRAPEVINE")
            {
                query += "AND Contract = '10009' ";
            }
            else if (sites == "PRAGUE")
            {
                query += "AND Contract = '10034' ";
            }
            else if (sites == "SYDNEY")
            {
                query += "AND Contract = '10041' ";
            }
            else if (sites == "TOKYO")
            {
                query += "AND Contract = '10042' ";
            }
            query += "\nORDER BY AsOf DESC";

            DataTable dt = oDAL.GetData(query);


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("204", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDataWipeResultLog = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}