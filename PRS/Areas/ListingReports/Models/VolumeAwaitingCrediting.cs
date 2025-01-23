using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
namespace IP.Areas.ListingReports.Models
{
    public class VolumeAwaitingCrediting
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstVolumeAwaitingCrediting { get; set; }
        public List<Hashtable> lstUnits { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList()
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            if (sites == "BYDGOSZCZ")
            {
                query = "DECLARE @ProgramID INT = 10058; ";
            }
            else
            {
                query = "DECLARE @ProgramID INT = 10059; ";
            }
            query += @"


-- Calculate the time zone offset
DECLARE @CurrentUtcOffset NVARCHAR(6);

SET @CurrentUtcOffset = (
    SELECT current_utc_offset
    FROM sys.time_zone_info
    WHERE name = 
        CASE 
            WHEN @ProgramID = 10058 THEN 'Central Europe Standard Time'
            WHEN @ProgramID = 10059 THEN 'Pacific Standard Time'
            ELSE 'UTC'
        END
);

-- First Part: Aggregating by Area
SELECT 
    ROA.Value AS Area, 
  COUNT(DISTINCT RO.CustomerReference) AS Orders,
    SUM(ROL.QtyToReceive - ROL.QtyReceived) AS Units,
    SUM(CASE 
            WHEN DATEDIFF(DAY, DL.CreateDate, SWITCHOFFSET(SYSDATETIMEOFFSET(), @CurrentUtcOffset)) BETWEEN 0 AND 30 THEN (ROL.QtyToReceive - ROL.QtyReceived) 
            ELSE 0 
        END) AS '0-30',
    SUM(CASE 
            WHEN DATEDIFF(DAY, DL.CreateDate, SWITCHOFFSET(SYSDATETIMEOFFSET(), @CurrentUtcOffset)) > 30 THEN (ROL.QtyToReceive - ROL.QtyReceived) 
            ELSE 0 
        END) AS '30+ Days'
FROM 
    pls.RODockLog DL
JOIN 
    pls.ROHeader RO 
    ON 
        RO.ID = DL.ROHeaderID 
        AND RO.StatusID = 7
JOIN 
    pls.ROLine ROL 
    ON 
        ROL.ROHeaderID = RO.ID 
        AND ROL.StatusID = 7
JOIN 
    pls.ROHeaderAttribute ROA 
    ON 
        ROA.ROHeaderID = RO.ID 
        AND ROA.AttributeID = 2
INNER JOIN 
    pls.ROHeaderAttribute ROHA 
    ON 
        ROHA.ROHeaderID = RO.ID 
        AND ROHA.Value IN ('RETURN', 'EXCHANGE')
INNER JOIN 
    pls.CodeAttribute CAH 
    ON 
        CAH.ID = ROHA.AttributeID 
        AND CAH.AttributeName = 'PROCESS_TYPE'
WHERE 
    DL.ProgramID = @ProgramID
GROUP BY 
    ROA.Value

UNION

-- Second Part: Discrepancy Units
SELECT 
    'Discrepancy Units' AS Area, 
  COUNT(DISTINCT RO.CustomerReference) AS Orders,
    COUNT(C.ID) AS Units,
    SUM(CASE 
            WHEN DATEDIFF(DAY, C.CreateDate, SWITCHOFFSET(SYSDATETIMEOFFSET(), @CurrentUtcOffset)) BETWEEN 0 AND 30 THEN 1
            ELSE 0 
        END) AS '0-30',
    SUM(CASE 
            WHEN DATEDIFF(DAY, C.CreateDate, SWITCHOFFSET(SYSDATETIMEOFFSET(), @CurrentUtcOffset)) > 30 THEN 1
            ELSE 0 
        END) AS '30+ Days'
FROM 
    pls.CaseMgt C
INNER JOIN 
    pls.ROHeader RO 
    ON 
        RO.ProgramID = @ProgramID 
        AND C.CustomerReference = RO.CustomerReference 
        AND RO.StatusID = 7
JOIN 
    pls.ROLine ROL 
    ON 
        ROL.ROHeaderID = RO.ID 
        AND ROL.StatusID = 7
JOIN 
    pls.ROHeaderAttribute ROA 
    ON 
        ROA.ROHeaderID = RO.ID 
        AND ROA.AttributeID = 2
INNER JOIN 
    pls.ROHeaderAttribute ROHA 
    ON 
        ROHA.ROHeaderID = RO.ID 
        AND ROHA.Value IN ('RETURN', 'EXCHANGE')
INNER JOIN 
    pls.CodeAttribute CAH 
    ON 
        CAH.ID = ROHA.AttributeID 
        AND CAH.AttributeName = 'PROCESS_TYPE'
WHERE 
    C.ProgramID = @ProgramID
    AND C.StatusID <> 24

UNION

-- Third Part: Product Information
SELECT 
    'PRODUCT ' + ISNULL(PNA.Value, 'OTHER') AS Area, 
  COUNT(DISTINCT RO.CustomerReference) AS Orders,
    SUM(ROL.QtyToReceive - ROL.QtyReceived) AS Units,
    SUM(CASE 
            WHEN DATEDIFF(DAY, ROD.CreateDate, SWITCHOFFSET(SYSDATETIMEOFFSET(), @CurrentUtcOffset)) BETWEEN 0 AND 30 THEN (ROL.QtyToReceive - ROL.QtyReceived) 
            ELSE 0 
        END) AS '0-30',
    SUM(CASE 
            WHEN DATEDIFF(DAY, ROD.CreateDate, SWITCHOFFSET(SYSDATETIMEOFFSET(), @CurrentUtcOffset)) > 30 THEN (ROL.QtyToReceive - ROL.QtyReceived) 
            ELSE 0 
        END) AS '30+ Days'
FROM 
    pls.RODockLog ROD
JOIN 
    pls.ROHeader RO 
    ON 
        RO.ID = ROD.ROHeaderID 
        AND RO.StatusID = 7
JOIN 
    pls.ROLine ROL 
    ON 
        ROL.ROHeaderID = RO.ID 
        AND ROL.StatusID = 7
JOIN 
    pls.ROHeaderAttribute ROA 
    ON 
        ROA.ROHeaderID = RO.ID 
        AND ROA.AttributeID = 2
JOIN 
    pls.ROHeaderAttribute ROB 
    ON 
        ROB.ROHeaderID = RO.ID 
        AND ROB.AttributeID = 986 
        AND ROB.Value IN ('RETURN', 'EXCHANGE')
LEFT JOIN 
    pls.PartNoAttribute PNA 
    ON 
        PNA.AttributeID = 1005 
        AND PNA.PartNo = ROL.PartNo 
        AND PNA.ProgramID = RO.ProgramID
WHERE 
    ROD.ProgramID = @ProgramID
GROUP BY 
    PNA.Value; ";
            DataTable dt = oDAL.GetData(query);

            filterString += "> Program = BOSE";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("236", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstVolumeAwaitingCrediting = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }

        public bool GetUnits(string programId, string Area, string Range)
        {
            string query = string.Empty;
            var conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            if (sites == "BYDGOSZCZ")
            {
                query = "DECLARE @ProgramID INT = 10058; ";
            }
            else
            {
                query = "DECLARE @ProgramID INT = 10059; ";
            }

            query += @"
DECLARE @AttributeValue NVARCHAR(50) = '<Area>';  -- Replace with the desired attribute value or leave NULL for all

-- Calculate the time zone offset
DECLARE @CurrentUtcOffset NVARCHAR(6);

SET @CurrentUtcOffset = (
    SELECT current_utc_offset
    FROM sys.time_zone_info
    WHERE name = 
        CASE 
            WHEN @ProgramID = 10058 THEN 'Central Europe Standard Time'
            WHEN @ProgramID = 10059 THEN 'Pacific Standard Time'
            ELSE 'UTC'
        END
);

SELECT 
    Area,
	 ProgramID,
    OrderHeaderID,
    CustomerReference,
    PartNo,
   CreateDate
FROM (

-- Filtered Query for Area = 'B2B'
SELECT 
    ROA.Value AS Area, 
RO.ProgramID,
RO.ID AS OrderHeaderID,
	RO.CustomerReference,
    ROL.PartNo AS PartNo, 
	DL.CreateDate AS CreateDate
FROM 
    pls.RODockLog DL
JOIN 
    pls.ROHeader RO 
    ON 
        RO.ID = DL.ROHeaderID 
        AND RO.StatusID = 7
JOIN 
    pls.ROLine ROL 
    ON 
        ROL.ROHeaderID = RO.ID 
        AND ROL.StatusID = 7
JOIN 
    pls.ROHeaderAttribute ROA 
    ON 
        ROA.ROHeaderID = RO.ID 
        AND ROA.AttributeID = 2
INNER JOIN 
    pls.ROHeaderAttribute ROHA 
    ON 
        ROHA.ROHeaderID = RO.ID 
        AND ROHA.Value IN ('RETURN', 'EXCHANGE')
INNER JOIN 
    pls.CodeAttribute CAH 
    ON 
        CAH.ID = ROHA.AttributeID 
        AND CAH.AttributeName = 'PROCESS_TYPE'
WHERE 
    DL.ProgramID = @ProgramID
    AND ROA.Value = @AttributeValue -- Filter for B2B only

UNION ALL

SELECT 
    'Discrepancy Units' AS Area, 
RO.ProgramID,
RO.ID AS OrderHeaderID,
	RO.CustomerReference,
    ROL.PartNo AS PartNo, 
	C.CreateDate AS CreateDate
FROM 
    pls.CaseMgt C
INNER JOIN 
    pls.ROHeader RO 
    ON 
        RO.ProgramID = @ProgramID 
        AND C.CustomerReference = RO.CustomerReference 
        AND RO.StatusID = 7
JOIN 
    pls.ROLine ROL 
    ON 
        ROL.ROHeaderID = RO.ID 
        AND ROL.StatusID = 7
JOIN 
    pls.ROHeaderAttribute ROA 
    ON 
        ROA.ROHeaderID = RO.ID 
        AND ROA.AttributeID = 2
INNER JOIN 
    pls.ROHeaderAttribute ROHA 
    ON 
        ROHA.ROHeaderID = RO.ID 
        AND ROHA.Value IN ('RETURN', 'EXCHANGE')
INNER JOIN 
    pls.CodeAttribute CAH 
    ON 
        CAH.ID = ROHA.AttributeID 
        AND CAH.AttributeName = 'PROCESS_TYPE'
WHERE 
    C.ProgramID = @ProgramID
    AND C.StatusID <> 24
    AND 'Discrepancy Units' = @AttributeValue -- No match but kept for uniformity

UNION ALL

SELECT 
    'PRODUCT ' + ISNULL(PNA.Value, 'OTHER') AS Area, 
RO.ProgramID,
RO.ID AS OrderHeaderID,
	RO.CustomerReference,
    ROL.PartNo AS PartNo, 
	ROD.CreateDate AS CreateDate
FROM 
    pls.RODockLog ROD
JOIN 
    pls.ROHeader RO 
    ON 
        RO.ID = ROD.ROHeaderID 
        AND RO.StatusID = 7
JOIN 
    pls.ROLine ROL 
    ON 
        ROL.ROHeaderID = RO.ID 
        AND ROL.StatusID = 7
JOIN 
    pls.ROHeaderAttribute ROA 
    ON 
        ROA.ROHeaderID = RO.ID 
        AND ROA.AttributeID = 2
JOIN 
    pls.ROHeaderAttribute ROB 
    ON 
        ROB.ROHeaderID = RO.ID 
        AND ROB.AttributeID = 986 
        AND ROB.Value IN ('RETURN', 'EXCHANGE')
LEFT JOIN 
    pls.PartNoAttribute PNA 
    ON 
        PNA.AttributeID = 1005 
        AND PNA.PartNo = ROL.PartNo 
        AND PNA.ProgramID = RO.ProgramID
		) AS CombinedData
WHERE 
    ProgramID = @ProgramID
	  AND Area = @AttributeValue ";

            
                // Use DL.CreateDate for non-'Problem Units' area
                if (Range == "0-30 Days")
                {
                    query += " AND DATEDIFF(DAY, CreateDate, SWITCHOFFSET(SYSDATETIMEOFFSET(), @CurrentUtcOffset)) BETWEEN 0 AND 30;  -- Filter for 0-30 days ";
                }
                else
                {
                    query += " AND DATEDIFF(DAY, CreateDate, SWITCHOFFSET(SYSDATETIMEOFFSET(), @CurrentUtcOffset)) > 30;  -- Filter for more than 30 days ";
                }
            

            // Replacing placeholders with parameter values
            query = query.Replace("<programId>", programId);
            query = query.Replace("<Area>", Area);

            DataTable dt = oDAL.GetData(query);

            // For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("201-1", query, "---Serial No.---", false);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstUnits = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }

    }
}