using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Reflection;
namespace IP.Areas.BRT.Models
{
    public class CycleCountAccuracy
    {
        cDAL oDAL = new cDAL("INIT");
        #region Fields 
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Site")]
        public string Site { get; set; }
        [Display(Name = "Qty.To Count")]
        public int QtyToCount { get; set; }
        [Display(Name = "Qty.To Counted")]
        public int QtyToCounted { get; set; }
        [Display(Name = "Iteration")]
        public int Iteration { get; set; }
        [Display(Name = "Create On")]
        public string _createdate = DateTime.Now.ToString(Format.DateOnly);
        public string Createdate { get { return _createdate; } set { _createdate = value; } }
        [Display(Name = "For Date")]
        public string _fordate = DateTime.Now.ToString(Format.DateOnly);
        public string Fordate { get { return _fordate; } set { _fordate = value; } }
        [Display(Name = "For Weekly")]
        public string _forweekly = DateTime.Now.ToString(Format.DateOnly);
        public string Forweekly { get { return _forweekly; } set { _forweekly = value; } }
        [Display(Name = "For Month")]
        public string _formonth = DateTime.Now.ToString(Format.DateOnly);
        public string Formonth { get { return _formonth; } set { _formonth = value; } }
        [Display(Name = "For Quarter")]
        public string _forquarter = DateTime.Now.ToString(Format.DateOnly);
        public string Forquarter { get { return _forquarter; } set { _forquarter = value; } }
        [Display(Name = "For Year")]
        public string _foryear = DateTime.Now.ToString(Format.DateOnly);
        public string Foryear { get { return _foryear; } set { _foryear = value; } }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        //public List<Hashtable> lstCCSummary { get; set; }
        public List<ArrayList> lstCCAccuracy { get; set; }
        public List<ArrayList> dataSummaryAccuracyChart { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

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
        #endregion

        #region Methods 
        public bool GetList(string programId, string programName, string no, string calender)
        {
            cDAL oDAL = new cDAL("INIT");
            string query = string.Empty;
            query = @"
SELECT ProgramID,
       ProgramName,
	   Site, 
	   SUM(TotalCount) as TotalCount, 
	   SUM(Pass) as Pass, 
	   SUM(Fail) as Fail,
<calender>
FROM rpt.CycleCountAccuracy
WHERE
";
            if (programId != "0")
            {
                query += "ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (calender == "Days")
            {
                query += "AND ForDate >= DATEADD(DAY,- " + no + ",GETDATE()) And  ForDate <=  GETDATE() ";
                query += @"GROUP BY ProgramID,
                           ProgramName,
                           Site, 
                           <calender> ";
                query += "ORDER BY <calender> DESC ";
                query = query.Replace("<calender>", "FORMAT(CONVERT(datetime, ForDate), 'yyyy.MM.dd') ");
            }
            else if (calender == "Weeks")
            {
                query += "AND FORMAT(DATEADD(week, DATEDIFF(week, 0, CreateDate- 1), 0) , 'yyyy.MM.dd') >= DATEADD(WEEK,- " + no + ",GETDATE()) And FORMAT(DATEADD(week, DATEDIFF(week, 0, CreateDate- 1), 0) , 'yyyy.MM.dd')<= GETDATE() ";
                query += @"GROUP BY ProgramID,
                           ProgramName,
                           Site, 
                           <calender> ";
                query += "ORDER BY <calender> DESC ";
                query = query.Replace("<calender>", "FORMAT(DATEADD(week, DATEDIFF(week, 0, CreateDate- 1), 0) , 'yyyy.MM.dd') ");
            }
            else if (calender == "Months")
            {
                query += "AND ForDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) -" + no + ", 0) And  ForDate <=  GETDATE() ";
                query += @"GROUP BY ProgramID,
                           ProgramName,
                           Site, 
                           <calender> ";
                query += "ORDER BY <calender> DESC ";
                query = query.Replace("<calender>", "FORMAT (convert(DATETIME, ForDate), 'yyyy.MM') ");
            }

            DataTable dt = oDAL.GetData(query);


            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            if (!string.IsNullOrEmpty(no))
                filterString += " | Go Back = '" + no + " " + calender + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("110", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstCCAccuracy = cCommon.ConvertDtToArrayList(dt);
                if (calender == "Days")
                {
                    query = @"SELECT 
       FORMAT(CONVERT(datetime, ForDate), 'yyyy.MM.dd') ForDate,
       SUM(Pass) as Pass, 
	   SUM(Fail) as Fail,
       SUM(TotalCount) as TotalCount
FROM   rpt.CycleCountAccuracy WHERE ";
                    if (programId != "0")
                    {
                        query += "ProgramID = '" + programId + "' ";
                    }
                    else
                    {
                        query += "ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                    }

                    query += "AND ForDate >= DATEADD(DAY,- " + no + ",GETDATE()) And  ForDate <=  GETDATE()  ";
                    query += "GROUP BY  FORMAT(CONVERT(datetime, ForDate), 'yyyy.MM.dd') ";
                    query += "ORDER BY ForDate DESC ";

                    DataTable dtForDate = oDAL.GetData(query);
                    dtForDate.Columns.Add("Percentage");
                    foreach (DataRow item in dtForDate.Rows)
                    {
                        double pass = Convert.ToDouble(item["Pass"]);
                        double totalCount = Convert.ToDouble(item["TotalCount"]);
                        double percentage = (pass / totalCount) * 100;
                        percentage = Math.Round(percentage, 0);
                        item["Percentage"] = percentage;
                    }

                    dataSummaryAccuracyChart = cCommon.ConvertDtToArrayList(cCommon.GenerateTransposedTable(dtForDate));
                }

                else if (calender == "Weeks")
                {
                    query = @"SELECT 
FORMAT(DATEADD(week, DATEDIFF(week, 0, CreateDate- 1), 0) , 'yyyy.MM.dd') WeekStart,
       SUM(Pass) as Pass, 
	   SUM(Fail) as Fail,
       SUM(TotalCount) as TotalCount
FROM   rpt.CycleCountAccuracy WHERE ";
                    if (programId != "0")
                    {
                        query += "ProgramID = '" + programId + "' ";
                    }
                    else
                    {
                        query += "ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                    }

                    query += "AND FORMAT(DATEADD(week, DATEDIFF(week, 0, CreateDate- 1), 0) , 'yyyy.MM.dd') >= DATEADD(WEEK,- " + no + ",GETDATE()) And FORMAT(DATEADD(week, DATEDIFF(week, 0, CreateDate- 1), 0) , 'yyyy.MM.dd')<= GETDATE() ";
                    query += "GROUP BY FORMAT(DATEADD(week, DATEDIFF(week, 0, CreateDate- 1), 0) , 'yyyy.MM.dd') ";
                    query += "ORDER BY FORMAT(DATEADD(week, DATEDIFF(week, 0, CreateDate- 1), 0) , 'yyyy.MM.dd') DESC ";

                    DataTable dtWeek = oDAL.GetData(query);
                    dtWeek.Columns.Add("Percentage");
                    foreach (DataRow item in dtWeek.Rows)
                    {
                        double pass = Convert.ToDouble(item["Pass"]);
                        double totalCount = Convert.ToDouble(item["TotalCount"]);
                        double percentage = (pass / totalCount) * 100;
                        percentage = Math.Round(percentage, 0);
                        item["Percentage"] = percentage;
                    }

                    dataSummaryAccuracyChart = cCommon.ConvertDtToArrayList(cCommon.GenerateTransposedTable(dtWeek));
                }

                else if (calender == "Months")
                {
                    query = @"SELECT 
 FORMAT (convert(DATETIME, ForDate), 'yyyy.MM') as MonthYear,
       SUM(Pass) as Pass, 
	   SUM(Fail) as Fail,
       SUM(TotalCount) as TotalCount

FROM   rpt.CycleCountAccuracy WHERE ";
                    if (programId != "0")
                    {
                        query += "ProgramID = '" + programId + "' ";
                    }
                    else
                    {
                        query += "ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                    }

                    query += "AND ForDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) -" + no + ", 0) And  ForDate <=  GETDATE() ";
                    query += "GROUP BY FORMAT (convert(DATETIME, ForDate), 'yyyy.MM') ";
                    query += "ORDER BY FORMAT (convert(DATETIME, ForDate), 'yyyy.MM') DESC ";



                    DataTable dtMonth = oDAL.GetData(query);
                    dtMonth.Columns.Add("Percentage");
                    foreach (DataRow item in dtMonth.Rows)
                    {
                        double pass = Convert.ToDouble(item["Pass"]);
                        double totalCount = Convert.ToDouble(item["TotalCount"]);
                        double percentage = (pass / totalCount) * 100;
                         percentage = Math.Round(percentage, 0);
                        item["Percentage"] = percentage;
                    }

                    dataSummaryAccuracyChart = cCommon.ConvertDtToArrayList(cCommon.GenerateTransposedTable(dtMonth));
                }

                return true;

            }


        }

        #endregion
    }
}