using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class RepairInventory
    {
        cDAL oDAL = new cDAL("INIT");
        #region Fields
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }

        public string filterString { get; set; }
        public string refreshDate { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstRepairInventory { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList(string serialNo)
        {
            refreshDate = string.Empty;
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT  Location, 
        [Location Desc], 
        [Item Number], 
        Description, 
        [Serial Number], 
        [Good Quantity], 
        [Bad Quantity], 
        [Good Reserve Quantity], 
        [Bad Reserve Quantity], 
        [Total Reserve Quantity], 
        [Total Quantity], 
        [Last Transaction Date],
        InsertDate
FROM rpt.RepairInventory
       ";
            if (!string.IsNullOrEmpty(serialNo))
                query += "WHERE [Serial Number] LIKE '%" + serialNo + "%' ";

            DataTable dt = oDAL.GetData(query);


            if (!string.IsNullOrEmpty(serialNo))
                filterString += " > Serial No. Like '" + serialNo + "' ";

            cLog oLog = new cLog();
            oLog.AddSqlQuery("168", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0) {
                    refreshDate = dt.Rows[0]["InsertDate"].ToString();
                    if (refreshDate.Length == 10 || refreshDate.Contains("12:00:00 AM"))
                        refreshDate = Convert.ToDateTime(refreshDate).ToString("yyyy.MM.dd");
                    else
                        refreshDate = Convert.ToDateTime(refreshDate).ToString("yyyy.MM.dd HH:mm:ss");
                    refreshDate = " | Last Data Refresh Time: " + refreshDate;
                    lstRepairInventory = cCommon.ConvertDtToHashTable(dt);
                }
                return true;

            }
        }

        public bool RunJob()
        {
            oDAL = new cDAL("JOB3");
            List<DbParameter> parameters = new List<DbParameter>();
            parameters.Add(new SqlParameter("@job_name", SqlDbType.VarChar) { Value = "WinIT - VF_Orders" });

            oDAL.ExecuteProcedure("msdb.dbo.sp_start_job", parameters);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}