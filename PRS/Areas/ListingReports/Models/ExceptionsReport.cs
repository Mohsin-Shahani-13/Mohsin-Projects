using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class ExceptionsReport
    {
        [Display(Name = "Program:")]
        public string ProgramName { get; set; }
        [Display(Name = "Program:")]
        public string ProgramID { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Status Description:")]
        public string statusDescription { get; set; }
        //[Display(Name = "Search From Archive:")]
        public string status { get; set; }

        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstExceptionReport { get; set; }
        cDAL oDAL = new cDAL("ACTIVE");

        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName 
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }

        public bool GetList(string frmDt, string toDt, bool isAllDate, string ProgramId, string ProgramName, string serialNo, string status)
        {
            string _SerialNo = string.Empty;
            string[] arr = serialNo.Split(',');
            string query = string.Empty;

            query = @"
        select * INTO #rptException 
from (
  SELECT 
    DE.id,
    DE.programid,
    DE.dataentryscriptid,
    DS.Name AS DataEntryScriptName,
    CS.Description AS StatusDescription,
    DE.message,
    U.Username AS username,
    DE.batchid,
    DE.createdate,
    DE.lastactivitydate,
   REPLACE(REPLACE(REPLACE(InputFields, '^^', ' | '), '^_',' = '), '_', ' ') AS FieldNames,
    DE.c01,
    DE.c02,
    DE.c03,
    DE.c04,
    DE.c05,
    DE.c06,
    DE.c07,
    DE.c08,
    DE.c09,
    DE.c10,
    DE.c11,
    DE.c12,
    DE.c13,
    DE.c14,
    DE.c15,
    DE.c16,
    DE.c17,
    DE.c18,
    DE.c19,
    DE.c20,
    DE.c21,
    DE.c22,
    DE.c23,
    DE.c24,
    DE.c25,
    DE.c26,
    DE.c27,
    DE.c28,
    DE.c29,
    DE.c30,
    DE.attributes
FROM   
    pls.dataentry DE
INNER JOIN 
    pls.dataentryscript DS ON DE.dataentryscriptid = DS.id 
LEFT JOIN 
    pls.CodeStatus CS ON DE.StatusID = CS.ID
INNER JOIN 
    pls.[User] U ON DE.UserID = U.ID
WHERE 
    DE.ProgramID = '<programId>'
 
UNION ALL
 
-- Query for plusext.pls.dataentryarchive table
SELECT 
    DEA.id,
    DEA.programid,
    DEA.dataentryscriptid,
    DS.Name AS DataEntryScriptName,
    CS.Description AS StatusDescription,
    DEA.message,
    U.Username AS username,
    DEA.batchid,
    DEA.createdate,
    DEA.lastactivitydate,
    REPLACE(REPLACE(REPLACE(InputFields, '^^', ' | '), '^_',' = '), '_', ' ') AS FieldNames,
    DEA.c01,
    DEA.c02,
    DEA.c03,
    DEA.c04,
    DEA.c05,
    DEA.c06,
    DEA.c07,
    DEA.c08,
    DEA.c09,
    DEA.c10,
    DEA.c11,
    DEA.c12,
    DEA.c13,
    DEA.c14,
    DEA.c15,
    DEA.c16,
    DEA.c17,
    DEA.c18,
    DEA.c19,
    DEA.c20,
    DEA.c21,
    DEA.c22,
    DEA.c23,
    DEA.c24,
    DEA.c25,
    DEA.c26,
    DEA.c27,
    DEA.c28,
    DEA.c29,
    DEA.c30,
    DEA.attributes
FROM   
    plusext.pls.dataentryarchive DEA
INNER JOIN 
    pls.dataentryscript DS ON DEA.dataentryscriptid = DS.id
LEFT JOIN 
    pls.CodeStatus CS ON DEA.StatusID = CS.ID
INNER JOIN 
    pls.[User] U ON DEA.UserID = U.ID
WHERE 
    DEA.programid = '<programId>'
    
  ) tmp

  SELECT * FROM #rptException       
   WHERE 1 = 1 
  
        ";

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (isAllDate != true)
            {
                query += " AND CONVERT(Date, createdate) >= '<frmDt>' AND CONVERT(Date, createdate) <= '<toDt>' ";
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            }

            if (!string.IsNullOrEmpty(status) && status != "ALL")
            {
                query += " AND StatusDescription = '" + status + "' ";
                filterString += " | Status Description = '" + status + "' ";
            }

            //if (!string.IsNullOrEmpty(statusDescription))
            //{
            //    query += " AND StatusDescription LIKE '%" + statusDescription + "%' ";
            //    filterString += " | Status Description Like '" + statusDescription + "' ";
            //}



            if (!string.IsNullOrEmpty(serialNo))
            {
                if (arr.Length == 1)
                {
                    //for comlete serialNo or some part Filter
                    _SerialNo = "\'%" + arr[0].Trim() + "%\'";
                    query += " AND (c01 LIKE (" + _SerialNo + ") OR c02 LIKE (" + _SerialNo + ") OR c03 LIKE (" + _SerialNo + ") )";
                }
                else if (arr.Length > 1)
                {
                    //for comlete multiple serialNo Filter
                    _SerialNo = GetInValue(serialNo);
                    query += " AND (c01 IN (" + _SerialNo + ") OR c02 IN (" + _SerialNo + ") OR c03 IN (" + _SerialNo + ") )";
                }
                filterString += " | Serial No. Like '" + serialNo + "' ";
            }



            query = query.Replace("<programId>", ProgramId);
            //query = query.Replace("<StatusDescription>", status);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            query += "ORDER BY createdate DESC";

            query += " \n\n DROP TABLE #rptException";



            DataTable dt = oDAL.GetData(query);


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("175", query, string.Empty, false);


            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstExceptionReport = cCommon.ConvertDtToHashTable(dt);
                return true;
            }

        }

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
    }
}