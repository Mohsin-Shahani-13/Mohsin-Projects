using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
namespace IP.Areas.ListingReports.Models
{
    public class BoseCreditingTATByWeek
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        public int totalIndex { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public String ProgramforSite { get; set; }
        public string ProgramBySite { get; set; }
        public List<ArrayList> lstBoseCreditingTATByWeek { get; set; }
        public List<ArrayList> lstData { get; set; }

        #endregion
        #region "Methods"
        public DataTable Program() // onHand warehouse method
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetList(string fDate, string tDate, string programId, string programName)
        {
            string query = string.Empty;
            var conType = @HttpContext.Current.Session["CONN_TYPE"].ToString();



            query = @"

DECLARE @StartDate DATE = '<fDate>'; -- Parameter for the start of the date range
DECLARE @EndDate DATE = '<tDate>';   -- Parameter for the end of the date range

-- Calculate the first Monday on or after the @StartDate
DECLARE @FirstMonday DATE = 
    CASE 
        WHEN DATENAME(WEEKDAY, @StartDate) = 'Monday' THEN @StartDate
        ELSE DATEADD(DAY, (7 - DATEPART(WEEKDAY, @StartDate) + 2) % 7, @StartDate)
    END;

-- Temporary table to store dynamic week start dates
CREATE TABLE #DateRange (
    WeekNumber INT,
    WeekStartDate DATE,
    WeekEndDate DATE
);

-- Populate the temporary table with weekly ranges
WITH DateRange AS (
    SELECT 
        WeekNumber = ROW_NUMBER() OVER (ORDER BY T.StartDate),
        WeekStartDate = T.StartDate,
        WeekEndDate = DATEADD(DAY, 6, T.StartDate)
    FROM (
        SELECT DATEADD(DAY, (N - 1) * 7, @FirstMonday) AS StartDate
        FROM (SELECT TOP (DATEDIFF(DAY, @FirstMonday, @EndDate) / 7 + 1) 
              ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N 
              FROM master.dbo.spt_values) AS Numbers
    ) T
    WHERE T.StartDate <= @EndDate
)
INSERT INTO #DateRange
SELECT * FROM DateRange;

-- Build dynamic columns for each week's start date
DECLARE @DynamicColumns NVARCHAR(MAX) = '';
DECLARE @PivotColumns NVARCHAR(MAX) = '';
SELECT 
    @DynamicColumns += QUOTENAME(CONVERT(VARCHAR, WeekStartDate, 23)) + ' AS ' 
                       + QUOTENAME('Week of ' + CONVERT(VARCHAR, WeekStartDate, 23)) + ',',
    @PivotColumns += QUOTENAME(CONVERT(VARCHAR, WeekStartDate, 23)) + ','
FROM #DateRange;

-- Remove trailing commas
SET @DynamicColumns = LEFT(@DynamicColumns, LEN(@DynamicColumns) - 1);
SET @PivotColumns = LEFT(@PivotColumns, LEN(@PivotColumns) - 1);

-- Dynamic SQL to create the pivot table
DECLARE @Sql NVARCHAR(MAX) = '
WITH DiffCalculation AS (
    SELECT 
        ROD.ProgramID,
        ROHA.Value AS ReturnReason,
        ROD.CreateDate,
        (
            SELECT MIN(PT.CreateDate)
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = ''RO''
                AND PT.PartTransactionID = 1
                AND PT.OrderHeaderID = ROD.ROHeaderID
                AND PT.ProgramID = ROD.ProgramID
        ) AS TransactionDate,
        DATEDIFF(HOUR, ROD.CreateDate, (
            SELECT MIN(PT.CreateDate)
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = ''RO''
                AND PT.PartTransactionID = 1
                AND PT.OrderHeaderID = ROD.ROHeaderID
                AND PT.ProgramID = ROD.ProgramID
        ))
        - (DATEDIFF(WEEK, ROD.CreateDate, (
            SELECT MIN(PT.CreateDate)
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = ''RO''
                AND PT.PartTransactionID = 1
                AND PT.OrderHeaderID = ROD.ROHeaderID
                AND PT.ProgramID = ROD.ProgramID
        )) * 48) AS TotalHours
    FROM pls.RODockLog ROD
    INNER JOIN pls.Program P ON P.ID = ROD.ProgramID
INNER JOIN pls.ROHeader ROH ON ROH.ID = ROD.ROHeaderID 
        AND ROH.StatusID IN (6, 9)
    LEFT JOIN pls.ROHeaderAttribute ROHA ON ROHA.ROHeaderID = ROD.ROHeaderID
        AND ROHA.Value IN (''B2B'', ''B2C'')
INNER JOIN pls.ROHeaderAttribute ROHC ON ROHC.ROHeaderID = ROD.ROHeaderID
AND ROHC.Value in (''RETURN'',''EXCHANGE'') and rohc.AttributeID=986
    WHERE ROD.ProgramID = '<programId>'
        AND ROD.CreateDate BETWEEN @StartDate AND @EndDate
),
WeeklyData AS (
    SELECT 
        D.ReturnReason,
        DR.WeekStartDate,
        COUNT(*) AS TotalRecords,
        SUM(CASE WHEN D.TotalHours <= 48 THEN 1 ELSE 0 END) AS RecordsWithin48Hours
    FROM DiffCalculation D
    INNER JOIN #DateRange DR 
        ON D.CreateDate BETWEEN DR.WeekStartDate AND DR.WeekEndDate
    WHERE D.ReturnReason IS NOT NULL -- Exclude rows where ReturnReason is NULL
    GROUP BY D.ReturnReason, DR.WeekStartDate
)
SELECT 
    ReturnReason, ' + @DynamicColumns + '
FROM (
    SELECT 
        ReturnReason,
        WeekStartDate,
        CONCAT(CAST(ROUND(100.0 * RecordsWithin48Hours / TotalRecords, 2) AS INT), ''%'') AS Percentage
    FROM WeeklyData
) AS SourceTable
PIVOT (
    MAX(Percentage)
    FOR WeekStartDate IN (' + @PivotColumns + ')
) AS PivotTable;
';

-- Execute the dynamic SQL
EXEC sp_executesql @Sql, N'@StartDate DATE, @EndDate DATE', @StartDate = @StartDate, @EndDate = @EndDate;

-- Cleanup
DROP TABLE #DateRange;


            



            ";
            query = query.Replace("'<programId>'", programId);
            query = query.Replace("<fDate>", fDate);
            query = query.Replace("<tDate>", tDate);

            DataTable dt = oDAL.GetData(query);

            DataTable dtList = dt.Clone();
            DataTable dtColHeader = cCommon.GenerateTransposedTable(dtList);
            filterString = " Program = BOSE ";
            filterString += " | From = '" + fDate + "' To = '" + tDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("255", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstBoseCreditingTATByWeek = cCommon.ConvertDtToArrayList(dt);
                    lstData = cCommon.ConvertDtToArrayList(dtColHeader);

                return true;

            }
        }
        #endregion
    }
}