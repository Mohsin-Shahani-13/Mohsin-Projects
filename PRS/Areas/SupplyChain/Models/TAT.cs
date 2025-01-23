using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class TAT
    {
        cDAL oDAL = new cDAL("ACTIVE");

        #region Fields
        [Display(Name = "Program:")]
        public string ProgramName { get; set; }
        [Display(Name = "Program:")]
        public string ProgramID { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstTAT { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
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
        public DataTable Program() // onHand warehouse method
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        #endregion
        public bool GetList(string frmDt, string toDt, string ProgramId, string ProgramName)
        {
            string query = string.Empty;
            query = @"WITH CTE_TransactionDates AS (
  SELECT 
    pt.Id AS TransactionId, 
    pt.ProgramID, 
    CPT.Description AS PartTransaction, 
    pt.SerialNo, 
    pt.PartNo, 
    pt.CustomerReference, 
    ROUA.Value AS ROAttributeValue, 
    SOUA.Value AS SOAttributeValue, 
    DE.C05 AS MovePartSerialize, 
    pt.Location AS PreviousLocation, 
    pt.ToLocation AS CurrentLocation, 
    U.Username AS TranBy,

    -- Get the ReceiptDate (only once here)
    (
      SELECT MAX(CreateDate)
      FROM pls.PartTransaction ptt WITH (NOLOCK)
      WHERE ptt.OrderType = 'RO' 
      AND ptt.ProgramId = '<ProgramId>'
      AND ptt.PartTransactionID = 1 -- 'RO-RECEIVE'
      AND ptt.SerialNo = pt.SerialNo
    ) AS ReceiptDate,

	 -- Get the PreviousTransactionDate (using LAG)
    LAG(pt.CreateDate, 1) OVER(
      PARTITION BY pt.SerialNo 
      ORDER BY pt.ID
    ) AS PreviousTransactionDate,

    -- Get the TranDate (only once here)
    (
      SELECT MAX(CreateDate)
      FROM pls.PartTransaction ptt WITH (NOLOCK)
      WHERE ptt.ProgramId = '<ProgramId>'
      AND ptt.SerialNo = pt.SerialNo
    ) AS TranDate,

    -- Count the number of transactions for each SerialNo
    COUNT(*) OVER (PARTITION BY pt.SerialNo) AS SerialTransactionCount
    
  FROM pls.PartTransaction pt WITH (NOLOCK)
    LEFT JOIN pls.CodePartTransaction CPT ON CPT.ID = pt.PartTransactionID
    LEFT JOIN pls.[User] U ON U.ID = pt.UserID
    LEFT JOIN pls.ROHeader ROH WITH (NOLOCK) ON ROH.ID = pt.OrderHeaderID 
    AND pt.OrderType = 'RO' 
    AND ROH.ProgramID = pt.ProgramID 
    LEFT JOIN pls.ROLine ROL WITH (NOLOCK) ON pt.OrderLineID = ROL.ID 
    AND ROH.ID = ROL.ROHeaderID 
    LEFT JOIN pls.ROUnit ROU WITH (NOLOCK) ON ROU.ROLineID = ROL.ID 
    AND ROU.SerialNo = pt.SerialNo 
    LEFT JOIN pls.CodeAttribute CA WITH (NOLOCK) ON CA.AttributeName = 'FA FLAG (RECEIPT)' 
    LEFT JOIN pls.ROUnitAttribute ROUA WITH (NOLOCK) ON ROUA.ROUnitID = ROU.ID 
    AND ROUA.AttributeID = CA.ID 
    LEFT JOIN pls.SOHeader SOH WITH (NOLOCK) ON SOH.ID = pt.OrderHeaderID 
    AND pt.OrderType = 'SO' 
    AND SOH.ProgramID = pt.ProgramID 
    LEFT JOIN pls.SOLine SOL WITH (NOLOCK) ON SOL.ID = pt.OrderLineID 
    AND SOH.ID = SOL.SOHeaderID 
    LEFT JOIN pls.SOUnit SOU WITH (NOLOCK) ON SOU.SOLineID = SOL.ID 
    AND SOU.SerialNo = pt.SerialNo 
    LEFT JOIN pls.CodeAttribute CA2 WITH (NOLOCK) ON CA2.AttributeName = 'FA FLAG (SHIP)' 
    LEFT JOIN pls.SOUnitAttribute SOUA WITH (NOLOCK) ON SOUA.SOUnitID = SOU.ID 
    AND SOUA.AttributeID = CA2.ID 
    LEFT JOIN PlusExt.pls.DataEntryArchive DE WITH (NOLOCK) ON DE.DataEntryScriptID = 485 
    AND pt.SerialNo = DE.C03 
    AND DE.C04 = pt.PartNo 
  WHERE pt.ForDate >= '<frmDt>' 
    AND pt.ForDate <= '<toDt>'  
    AND pt.ProgramID = '<ProgramId>'
    AND pt.SerialNo <> ''
	AND pt.SerialNo <> '*'
),

CTE_TAT_Calculations AS (
  SELECT
    TransactionId, 
    ProgramID, 
    PartTransaction, 
    SerialNo, 
    PartNo, 
    CustomerReference, 
    ROAttributeValue, 
    SOAttributeValue, 
    MovePartSerialize, 
    PreviousLocation, 
    CurrentLocation, 
    TranBy, 
    ReceiptDate, 
	PreviousTransactionDate,
    TranDate, 
    SerialTransactionCount, -- Including the transaction count

    -- Calculate TATDays (excludes weekends)
    CASE 
      WHEN PreviousTransactionDate IS NOT NULL 
      THEN DATEDIFF(DAY, PreviousTransactionDate, TranDate) 
           - (DATEDIFF(WEEK, PreviousTransactionDate, TranDate) * 2)
      ELSE 0
    END AS TATDays,

    -- Calculate TotalTATDays based on movement or no movement
    CASE 
      -- Condition: No movement (same location and serial count is 1)
      WHEN PreviousLocation = CurrentLocation OR SerialTransactionCount = 1 
      THEN DATEDIFF(DAY, TranDate, GETDATE()) 
           - (DATEDIFF(WEEK, TranDate, GETDATE()) * 2)

      -- Condition: Serial has movement
      ELSE DATEDIFF(DAY, ReceiptDate, TranDate) 
           - (DATEDIFF(WEEK, ReceiptDate, TranDate) * 2)
    END AS TotalTATDays

  FROM CTE_TransactionDates
)

SELECT 
  TransactionId, 
  ProgramID, 
  PartTransaction, 
  SerialNo, 
  PartNo, 
  CustomerReference, 
  ROAttributeValue, 
  SOAttributeValue, 
  MovePartSerialize, 
  PreviousLocation, 
  CurrentLocation, 
  TranBy,  
  ReceiptDate, 
  PreviousTransactionDate, 
  TranDate, 
  TATDays, 
  TotalTATDays
FROM CTE_TAT_Calculations
ORDER BY SerialNo;



";

            query = query.Replace("<ProgramId>", ProgramId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);


            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("174", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstTAT = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }

    }
}