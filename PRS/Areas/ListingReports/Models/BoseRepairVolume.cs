using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class BoseRepairVolume
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstBoseRepairVolume { get; set; }
        public List<Hashtable> lstUnits { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>' AND Name = 'BOSE'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }

        #endregion
        #region Methods 
        public bool GetList(string programId, string programName, string fDate, string tDate)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            query = @"WITH DateInfo AS (
    SELECT distinct
        PS.SerialNo,
        PS.ROHeaderID,
        PS.ProgramID,
        PNA.Value AS CodeName,
        CONCAT(
            CASE
                WHEN PNA.Value IN ('ARIZONA', 'LIPTON', 'MINNOW', 'PHELPS', 'SKIPPER', 'PROFESSOR BB') THEN 'AIO CELL'
                WHEN PNA.Value IN ('EDDIE', 'LANCOME', 'LANCOME PLUS', 'M3', 'TAYLOR') THEN 'BT CELL'
                WHEN PNA.Value IN ('DURAN', 'GOODYEAR', 'LONE STARR', 'PRINCE') THEN 'HEADSET CELL'
                WHEN PNA.Value IN ('SCOTTY', 'SMALLS') THEN 'INEAR CELL'
                WHEN PNA.Value IN ('ANGUS', 'BABY YODA', 'BENTO GILLIGAN', 'CHIBI', 'GILLIGAN PREMIUM', 'GINGER CHEEVERS', 'MALCOLM', 'SAN DIEGO', 'STEVIE', 'ZAKIM') THEN 'SYSTEM CELL'
                WHEN PNA.Value = 'EDELMAN' THEN 'NPI - AIO CELL'
                WHEN PNA.Value = 'SERENA' THEN 'NPI - INEAR CELL'
            END, ' - ', PNA.Value
        ) AS Line,
        -- Calculate TransactionDate and TotalDays here in CTE
        (
            SELECT MIN(PT.CreateDate) 
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = 'RO' 
                AND PT.PartTransactionID = 1 
                AND PT.OrderHeaderID = PS.ROHeaderID
                AND PT.ProgramID = PS.ProgramID
        ) AS ReceiptDate,
        DATEDIFF(
            DAY,
            (
                SELECT MIN(PT.CreateDate) 
                FROM pls.PartTransaction PT WITH (NOLOCK)
                WHERE PT.OrderType = 'RO' 
                    AND PT.PartTransactionID = 1 
                    AND PT.OrderHeaderID = PS.ROHeaderID
                    AND PT.ProgramID = PS.ProgramID
            ),
            GETDATE()
        ) AS DaysTAT
    FROM pls.PartSerial PS with (nolock)
    INNER JOIN pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID
    INNER JOIN pls.ROLine ROL ON ROL.ROHeaderID = ROH.ID AND ROL.PartNo = PS.PartNo
    INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID
    LEFT JOIN pls.CodeAttribute CA ON CA.AttributeName = 'CODE_NAME'
    LEFT JOIN pls.PartNoAttribute PNA ON PNA.AttributeID = CA.ID AND PNA.PartNo = PS.PartNo
    LEFT JOIN pls.CodeAttribute CA1 ON CA1.AttributeName = 'PROCESS_TYPE'
    LEFT JOIN pls.ROHeaderAttribute ROHA ON ROHA.ROHeaderID = ROH.ID AND ROHA.AttributeID = CA1.ID
    WHERE PS.ProgramID = '<ProgramId>'
    AND PS.StatusID NOT IN (17, 18)  -- excluding scrap and ship statuses
	AND ROHA.Value = 'REPAIR'
)
-- Now perform the aggregation on the results of the CTE
SELECT distinct
    ProgramID,
    Line AS AreaCell,
    COUNT(SerialNo) AS Units,
 
    -- Transaction Date
    MIN(ReceiptDate) AS ReceiptDate,
 
    -- Total Days
    MAX(DaysTAT) AS 'Days(TAT)',
 
    -- Units in 0-30 Days
    SUM(CASE 
        WHEN DaysTAT BETWEEN 0 AND 30 THEN 1
        ELSE 0
    END) AS '0-30 Days',

      -- Units in 31 - 60 Days
    SUM(CASE
        WHEN DaysTAT BETWEEN 31 AND 60 THEN 1
        ELSE 0
    END) AS '31-60 Days',
 
    --Units in 61 - 90 Days
    SUM(CASE
        WHEN DaysTAT BETWEEN 61 AND 90 THEN 1
        ELSE 0
    END) AS '61-90 Days',
 
    --Units in 90 + Days
    SUM(CASE
        WHEN DaysTAT > 90 THEN 1
        ELSE 0
    END) AS '90+ Days'
FROM DateInfo
WHERE CONVERT(Date, ReceiptDate) >= '<frmDt>' AND CONVERT(Date, ReceiptDate) <= '<toDt>'
GROUP BY Line, ProgramID
ORDER BY Line ";

            
   query = query.Replace("<ProgramId>", programId);
            query = query.Replace("<frmDt>", fDate);
            query = query.Replace("<toDt>", tDate);

            //if (!string.IsNullOrEmpty(programName))
            filterString += "> Program = '" + programName + "' ";

            filterString += " | From = '" + fDate + "' To = '" + tDate + "' ";


            DataTable dt = oDAL.GetData(query);


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("246", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstBoseRepairVolume = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool GetUnits(string programId, string AreaCell, string frmDt, string toDt, string Range)
        {
            string query = string.Empty;
            var conType = @HttpContext.Current.Session["CONN_TYPE"].ToString();
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            query = @"
						WITH DateInfo AS (
    SELECT 
        PS.SerialNo,
		PS.PartNo,
        PS.ROHeaderID,
        PS.ProgramID,
        PNA.Value AS CodeName,
        CONCAT(
            CASE
                WHEN PNA.Value IN ('ARIZONA', 'LIPTON', 'MINNOW', 'PHELPS', 'SKIPPER', 'PROFESSOR BB') THEN 'AIO CELL'
                WHEN PNA.Value IN ('EDDIE', 'LANCOME', 'LANCOME PLUS', 'M3', 'TAYLOR') THEN 'BT CELL'
                WHEN PNA.Value IN ('DURAN', 'GOODYEAR', 'LONE STARR', 'PRINCE') THEN 'HEADSET CELL'
                WHEN PNA.Value IN ('SCOTTY', 'SMALLS') THEN 'INEAR CELL'
                WHEN PNA.Value IN ('ANGUS', 'BABY YODA', 'BENTO GILLIGAN', 'CHIBI', 'GILLIGAN PREMIUM', 'GINGER CHEEVERS', 'MALCOLM', 'SAN DIEGO', 'STEVIE', 'ZAKIM') THEN 'SYSTEM CELL'
                WHEN PNA.Value = 'EDELMAN' THEN 'NPI - AIO CELL'
                WHEN PNA.Value = 'SERENA' THEN 'NPI - INEAR CELL'
            END, ' - ', PNA.Value
        ) AS Line,
        -- Calculate TransactionDate and TotalDays here in CTE
        (
            SELECT MIN(PT.CreateDate) 
            FROM pls.PartTransaction PT WITH (NOLOCK)
            WHERE PT.OrderType = 'RO' 
                AND PT.PartTransactionID = 1 
                AND PT.OrderHeaderID = PS.ROHeaderID
                AND PT.ProgramID = PS.ProgramID
        ) AS ReceiptDate,
        DATEDIFF(
            DAY,
            (
                SELECT MIN(PT.CreateDate) 
                FROM pls.PartTransaction PT WITH (NOLOCK)
                WHERE PT.OrderType = 'RO' 
                    AND PT.PartTransactionID = 1 
                    AND PT.OrderHeaderID = PS.ROHeaderID
                    AND PT.ProgramID = PS.ProgramID
            ),
            GETDATE()
        ) AS DaysTAT
    FROM pls.PartSerial PS with (nolock)
    INNER JOIN pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID
    INNER JOIN pls.ROLine ROL ON ROL.ROHeaderID = ROH.ID AND ROL.PartNo = PS.PartNo
    INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID
    LEFT JOIN pls.CodeAttribute CA ON CA.AttributeName = 'CODE_NAME'
    LEFT JOIN pls.PartNoAttribute PNA ON PNA.AttributeID = CA.ID AND PNA.PartNo = PS.PartNo
    LEFT JOIN pls.CodeAttribute CA1 ON CA1.AttributeName = 'PROCESS_TYPE'
    LEFT JOIN pls.ROHeaderAttribute ROHA ON ROHA.ROHeaderID = ROH.ID AND ROHA.AttributeID = CA1.ID 
            WHERE PS.ProgramID = '<ProgramId>' ";


            if (sites == "BYDGOSZCZ")
            {
                //query += " WHERE PS.ProgramID = 10058 ";
                query = query.Replace("<ProgramId>", "10058");
            }
            else
            {
                //query += " WHERE PS.ProgramID = 10059 ";
                query = query.Replace("<ProgramId>", "10059");
            }

    query += @"AND PS.StatusID NOT IN (17, 18)  -- excluding scrap and ship statuses
	AND ROHA.Value = 'REPAIR'

)
-- Now perform the aggregation on the results of the CTE
SELECT distinct
ProgramID,
Line,
ReceiptDate,
PartNo,
SerialNo
FROM DateInfo
WHERE 
ProgramID = '<programId>'
AND CONVERT(Date, ReceiptDate) >= '<frmDt>'  AND CONVERT(Date, ReceiptDate) <='<toDt>' 
AND Line = '<AreaCell>'
 ";

            if (Range == "0-30")
            {
                query += " AND DaysTAT BETWEEN 0 AND 30;  -- Filter for 0-30 days ";
            }

            else if (Range == "31-60")
            {
                query += " AND DaysTAT BETWEEN 31 AND 60;  -- Filter for 31-60 days ";
            }
            else if (Range == "61-90")
            {
                query += " AND DaysTAT BETWEEN 61 AND 90;  -- Filter for 61-90 days ";
            }


            else
            {
                query += "  AND DaysTAT > 90;  -- Filter for more than 90 days ";
            }

            query = query.Replace("<programId>", programId);
            query = query.Replace("<AreaCell>", AreaCell);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);


            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("246-1", query, "---Serial No.---", false);

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


        #endregion
    }
}