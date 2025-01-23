using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Models
{
    public class LogQueries
    {
        cDAL oDAL;
        #region fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public string RptName { get; set; }
        public List<ArrayList> lstOnHand { get; set; }
        public List<ArrayList> lstLogQueries { get; set; }
        public List<ArrayList> lstUnitList { get; set; }
        public List<ArrayList> lstActivityLog { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }
        #endregion

        #region Methods 

        public bool GetUnitList(string RptName, string RptCode, string fromDt, string toDt)
        {
            string query = string.Empty;
            bool allDate = false;
            query = @" 
SELECT  SigninName,
		RemoteHost,
		RptUrl,InsertOn as LastUpdatedOn
FROM    dbo.zLogQuery AS  l
INNER JOIN ip.Employee AS e 
ON l.SigninId = e.EmpId ";
            if (String.IsNullOrEmpty(fromDt))
            {
                allDate = true;
                query += "WHERE RptCode='<RptCode>' AND  RptName='<RptName>' AND e.IsWinIT=0 AND l.Origin ='IP' AND InsertOn >= (GetDate() - 90) ";

            }
            else
            {
                allDate = false;
                query += "WHERE RptCode='<RptCode>' AND  RptName='<RptName>' AND e.IsWinIT=0 AND l.Origin ='IP' AND CONVERT(Date, InsertOn) >= '<fromDt>' AND CONVERT(Date, InsertOn) <= '<toDt>' ";

            }
            query = query.Replace("<fromDt>", fromDt);
            query = query.Replace("<toDt>", toDt);
            query += @"Order By LastUpdatedOn DESC , SigninName ";


            query = query.Replace("<RptCode>", RptCode);
            query = query.Replace("<RptName>", RptName);

            oDAL = new cDAL("INIT");
            DataTable dt = oDAL.GetData(query);

            //if (allDate)
            //{
            //    filterString += " > 90 Days";
            //}
            //else
            //{
            //    filterString += " > From = '" + fromDt + "' To = '" + toDt + "' ";
            //}

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstUnitList = cCommon.ConvertDtToArrayList(dt);
                }
                return true;
            }
            return false;
        }


        public bool GetLogQueries(string fromDt, string toDt)
        {
            string query = string.Empty;
            bool allDate = false;  
            query = @"
SELECT RptName,
Count(RptName), RptCode
FROM dbo.zLogQuery AS l
INNER JOIN ip.Employee AS e
ON l.SigninId = e.EmpId ";
            if (String.IsNullOrEmpty(fromDt))
            {
                allDate = true;
                query += "WHERE e.IsWinIT=0 AND l.Origin ='IP' AND InsertOn >= (GetDate() - 90) ";

            }
            else
            {
                allDate = false;
                query += "WHERE e.IsWinIT=0 AND l.Origin ='IP' AND CONVERT(Date, InsertOn) >= '<fromDt>' AND CONVERT(Date, InsertOn) <= '<toDt>' ";

            }
            query = query.Replace("<fromDt>", fromDt);
            query = query.Replace("<toDt>", toDt);
            query  += @"GROUP BY RptName , RptCode
                      ORDER BY Count(RptName) DESC,RptName ";

            oDAL = new cDAL("INIT");
            DataTable dt = oDAL.GetData(query);

            TimeSpan ts = new TimeSpan();
            if (!string.IsNullOrEmpty(fromDt))
            {
                DateTime startdate = Convert.ToDateTime(toDt);
                DateTime enddate = Convert.ToDateTime(fromDt);
                ts = startdate - enddate;
            }
            

            if (allDate)
            {
                filterString += " > 90 Days";
            }
            else
            {
                filterString += " > " + ts.Days + " Days";
            }

            //filterString += " > From = '" + fromDt + "' To = '" + toDt + "' ";

            //For SQL Documentation
            // cLog oLog = new cLog();
            //  oLog.AddSqlQuery("0017", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstLogQueries = cCommon.ConvertDtToArrayList(dt);
                return true;

            }
        }
        public bool GetActivityLog(string rptCode, string rptName)
        {
            string query = string.Empty;

            query = @"
SELECT  SigninName,
		RemoteHost,
		RptUrl,InsertOn as LastUpdatedOn , RptName
FROM    dbo.zLogQuery AS  l
INNER JOIN ip.Employee AS e 
ON l.SigninId = e.EmpId 
WHERE e.IsWinIT=0 AND RptCode ='<rpt_Code>' AND l.Origin ='IP' AND InsertOn >= (GetDate() - 90)
Order By LastUpdatedOn DESC , SigninName ";

            //query = query.Replace("<signin_id>", signinId);
            query = query.Replace("<rpt_Code>", rptCode);

            oDAL = new cDAL("INIT");
            DataTable dt = oDAL.GetData(query);
            if (rptName == "undefined")
            {
                string sql = "SELECT NAME AS RptName FROM  [RPT].[IMPORTED_LIST] WHERE RPTCODE = '" + rptCode + "'";
                DataTable dtRptName = oDAL.GetData(sql);
                if (dtRptName.Rows.Count > 0)
                    RptName = dtRptName.Rows[0]["RptName"].ToString();
            }
            cLog oLog = new cLog();
            oLog.AddSqlQuery("", query, string.Empty, false);


            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstActivityLog = cCommon.ConvertDtToArrayList(dt);
                return true;

            }
        }
        #endregion

    }
}