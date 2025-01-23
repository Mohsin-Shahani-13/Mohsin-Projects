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
    public class RepairTAT
    {
		cDAL oDAL = new cDAL("ACTIVE");
		#region Fields
		public bool isAllDate { get; set; }
		[Display(Name = "From:")]
		public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
		public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
		[Display(Name = "To:")]
		public string _toDt = DateTime.Now.ToString(Format.DateOnly);
		public string toDt { get { return _toDt; } set { _toDt = value; } }
		public string program { get; set; }

		public string ReportTitle { get; set; }
		public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
		public List<Hashtable> lstRepairTAT { get; set; }

		public string filterString { get; set; }
		public string ErrorMessage { get; set; }
		#endregion
		#region Methods 
		public DataTable Program() // onHand warehouse method
		{
			cDAL oDAL = new cDAL("ACTIVE");
			string sites = HttpContext.Current.Session["DefaultSite"].ToString();
			string query = string.Empty;
			query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

			query = query.Replace("<site>", sites);
			DataTable dt = oDAL.GetData(query);
			return dt;
		}
		public bool GetList(string programId, string ProgramName, string frmDt, string toDt)
		{
			oDAL = new cDAL("ACTIVE");
			string query = string.Empty;
			query = @"
WITH DiffCalculation AS (
    SELECT
        ROHA.Value AS ProcessType,  
        DATEDIFF(HOUR, ROD.CreateDate, PT.MinCreateDate) AS TotalHours,
        DATEDIFF(DAY, ROD.CreateDate, PT.MinCreateDate) AS TotalDays
    FROM pls.RODockLog ROD
    INNER JOIN pls.Program P ON P.ID = ROD.ProgramID
    INNER JOIN pls.ROHeader ROH ON ROH.ID = ROD.ROHeaderID AND ROH.StatusID = 6
    LEFT JOIN pls.ROHeaderAttribute ROHA ON ROHA.ROHeaderID = ROD.ROHeaderID
    OUTER APPLY (
        SELECT MIN(PT.CreateDate) AS MinCreateDate
        FROM pls.PartTransaction PT WITH (NOLOCK)
        WHERE PT.OrderType = 'RO'
          AND PT.PartTransactionID = 1
          AND PT.OrderHeaderID = ROD.ROHeaderID
          AND PT.ProgramID = ROD.ProgramID
    ) PT
    WHERE 
        ROD.ProgramID =  '<programId>'       
        AND ROHA.Value IN ('Repair')
          AND convert(date, ROD.CreateDate) >= '<frmDt>' 
        AND convert(date, ROD.CreateDate) <= '<toDt>' 
),
CompletionCalculation AS (
    SELECT 
        AVG(DATEDIFF(DAY, ROD.CreateDate, PS.SODate)) AS AvgTotalDays,
        100.0 * SUM(CASE WHEN DATEDIFF(DAY, ROD.CreateDate, PS.SODate) <= 6 THEN 1 ELSE 0 END) / COUNT(*) AS PercentageWithin6Days
    FROM pls.PartSerial PS
    INNER JOIN pls.RODockLog ROD ON ROD.ROHeaderID = PS.ROHeaderID AND ROD.ProgramID = PS.ProgramID
    INNER JOIN pls.SOHeader SOH ON SOH.ID = PS.SOHeaderID AND SOH.ProgramID = PS.ProgramID
    WHERE 
        PS.ProgramID = '<programId>'    
        AND PS.StatusID = 12  -- Reserved
   AND convert(date, ROD.CreateDate) >= '<frmDt>' 
        AND convert(date, ROD.CreateDate) <= '<toDt>' 
)
SELECT 
    '% Units received in within 24 hours' AS Focus,
    '100%' AS Target,
    CONCAT(ISNULL(CAST(ROUND(100.0 * SUM(CASE WHEN TotalHours <= 24 THEN 1 ELSE 0 END) / COUNT(*), 2) AS INT), 0), '%') AS Actual
FROM DiffCalculation
UNION ALL
SELECT 
    'TAT (days)',
    '6' AS Target,
    CAST(ISNULL(ROUND(AvgTotalDays, 2), 0) AS VARCHAR) AS Actual
FROM CompletionCalculation
UNION ALL
SELECT 
    '% Units completed within 6 days',
    '90%' AS Target,
    CONCAT(ISNULL(CAST(ROUND(PercentageWithin6Days, 2) AS INT), 0), '%') AS Actual
FROM CompletionCalculation;


";
			query = query.Replace("<programId>", programId);
			query = query.Replace("<frmDt>", frmDt);
			query = query.Replace("<toDt>", toDt);


			DataTable dt = oDAL.GetData(query);
			filterString += " > Program = '" + ProgramName + "' ";
			filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

			




			//For SQL Documentation
			cLog oLog = new cLog();
			oLog.AddSqlQuery("245", query, string.Empty, false);

			if (oDAL.HasErrors)
			{
				ErrorMessage = oDAL.ErrMessage;
				return false;
			}
			else
			{
				if (dt.Rows.Count > 0)
					lstRepairTAT = cCommon.ConvertDtToHashTable(dt);
				return true;

			}
		}

		#endregion
	}
}