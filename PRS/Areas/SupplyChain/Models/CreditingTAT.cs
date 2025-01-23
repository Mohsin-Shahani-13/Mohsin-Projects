using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
namespace IP.Areas.SupplyChain.Models
{
    public class CreditingTAT
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
        [Display(Name = "Program:")]
        public string program { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("Active");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
AND Name = 'BOSE'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);


            return dt;
        }
        public List<Hashtable> lstCreditingTAT { get; set; }
        public List<Hashtable> lstDetail { get; set; }
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string programId, string programName)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
		WITH DiffCalculation AS (
    SELECT 
        ROD.ProgramID,
        ROHA.Value AS ReturnReason,
        (
            SELECT MIN(PT.CreateDate) 
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = 'RO' 
                AND PT.PartTransactionID = 1 
                AND PT.OrderHeaderID = ROD.ROHeaderID
                AND PT.ProgramID = ROD.ProgramID
        ) AS TransactionDate,
        -- Calculate TotalHours excluding weekends
        DATEDIFF(HOUR, ROD.CreateDate, (
            SELECT MIN(PT.CreateDate) 
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = 'RO' 
                AND PT.PartTransactionID = 1 
                AND PT.OrderHeaderID = ROD.ROHeaderID
                AND PT.ProgramID = ROD.ProgramID
        )) 
        - (DATEDIFF(WEEK, ROD.CreateDate, (
            SELECT MIN(PT.CreateDate) 
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = 'RO' 
                AND PT.PartTransactionID = 1 
                AND PT.OrderHeaderID = ROD.ROHeaderID
                AND PT.ProgramID = ROD.ProgramID
        )) * 48) AS TotalHours  -- Subtract 48 hours per weekend
    FROM pls.RODockLog ROD
    INNER JOIN pls.Program P ON P.ID = ROD.ProgramID
    INNER JOIN pls.ROHeader ROH ON ROH.ID = ROD.ROHeaderID 
        AND ROH.StatusID IN (6, 9)
    LEFT JOIN pls.ROHeaderAttribute ROHA ON ROHA.ROHeaderID = ROD.ROHeaderID
        AND ROHA.Value IN ('B2B', 'B2C')  
INNER JOIN pls.ROHeaderAttribute ROHC ON ROHC.ROHeaderID = ROD.ROHeaderID
AND ROHC.Value in ('RETURN','EXCHANGE') and rohc.AttributeID=986
";

            if (programId != "0")
            {
                query += "WHERE ROD.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "WHERE ROD.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (!string.IsNullOrEmpty(frmDt) && !string.IsNullOrEmpty(toDt))
                // query += "AND Format(Cast(CD.MMDDYYYY as date), 'yyyy.MM.dd') = '" + OrdCreatOnFrm + "'";
                query += "AND ROD.CreateDate > '" + frmDt + "' " + "AND ROD.CreateDate < '" + toDt + "' ";

            query += @" 
   GROUP BY ROD.ProgramID, ROHA.Value, ROD.ROHeaderID, ROD.ProgramID, ROD.CreateDate
) 
SELECT 
    ProgramID,
    ReturnReason,
    SUM(TotalHours) AS TotalHours,
  -- Add buckets for NDB and transactions within 48 hours
    SUM(CASE WHEN TotalHours <= 24 THEN 1 ELSE 0 END) AS NDB,
    CONCAT(CAST(ROUND(
        100.0 * SUM(CASE WHEN TotalHours <= 48 THEN 1 ELSE 0 END) 
        / COUNT(*), 2
    ) AS INT), '%') AS PercentageWithin48Hours
FROM DiffCalculation
GROUP BY ProgramID, ReturnReason;

";


            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";


            DataTable dt = oDAL.GetData(query);



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("199", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstCreditingTAT = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        public bool GetDetail(string programId, string ReturnReason, string frmDt, string toDt)
        {


            string query = string.Empty;
            var conType = @HttpContext.Current.Session["CONN_TYPE"].ToString();

            query = @"	WITH DiffCalculation AS (
    SELECT
        ROD.ProgramID,
        ROHA.Value AS ReturnReason,
        ROD.ROHeaderID,
        ROH.CustomerReference,
CAD.Name AS CustName,
        ROD.CreateDate AS DocklogDate,
        (
            SELECT MIN(PT.CreateDate) 
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = 'RO' 
                AND PT.PartTransactionID = 1 
                AND PT.OrderHeaderID = ROD.ROHeaderID
                AND PT.ProgramID = ROD.ProgramID
        ) AS TransactionDate,
        -- Calculate TotalHours excluding weekends
        DATEDIFF(HOUR, ROD.CreateDate, (
            SELECT MIN(PT.CreateDate) 
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = 'RO' 
                AND PT.PartTransactionID = 1 
                AND PT.OrderHeaderID = ROD.ROHeaderID
                AND PT.ProgramID = ROD.ProgramID
        )) 
        - (DATEDIFF(WEEK, ROD.CreateDate, (
            SELECT MIN(PT.CreateDate) 
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = 'RO' 
                AND PT.PartTransactionID = 1 
                AND PT.OrderHeaderID = ROD.ROHeaderID
                AND PT.ProgramID = ROD.ProgramID
        )) * 48) AS TotalHours  -- Subtract 48 hours for each weekend
    FROM pls.RODockLog ROD
    INNER JOIN pls.Program P ON P.ID = ROD.ProgramID
    INNER JOIN pls.ROHeader ROH ON ROH.ID = ROD.ROHeaderID 
        AND ROH.StatusID IN (6, 9)
LEFT JOIN pls.CodeAddressDetails CAD on CAD.AddressID = ROH.AddressID AND CAD.AddressType = 'Billto'
    LEFT JOIN pls.ROHeaderAttribute ROHA ON ROHA.ROHeaderID = ROD.ROHeaderID
AND ROHA.Value IN ('<ReturnReason>')
	WHERE ROD.ProgramID = '<programId>'
	AND ROD.CreateDate > '<frmDt>' AND ROD.CreateDate < '<toDt>'
	)
SELECT 
    ProgramID,
    ROHeaderID,
    CustomerReference,
CustName,
    ReturnReason, 
    TotalHours,
    CASE 
          WHEN TotalHours > 48 THEN '0%' 
          ELSE '100%' 
      END AS PercentageWithin48Hours
FROM DiffCalculation
WHERE ReturnReason IS NOT NULL; 

 ";
            
            
            

            query = query.Replace("<programId>", programId);
            query = query.Replace("<ReturnReason>", ReturnReason);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);


            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("199-1", query, "Detail", false);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstDetail = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}