using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class AvailableInventory
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields        
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string filterString { get; set; }       
        public string ReportTitle { get; set; }

        public List<Hashtable> lstAvailableInventory { get; set; }
        
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
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
        public bool GetList(string programID, string programName)
        {
            string query = string.Empty;
            query = @"
WITH
  aa
  AS
  (
    SELECT
	ps.ProgramID,
      sh.CustomerReference AS Ship_Order_No,
      rh.CustomerReference AS Receipt_Order_No,
      cs.[Description] AS status,
      ps.PartNo,
      ps.SerialNo,
      ps.StatusID,
      su.FromLocationID,
      pl.LocationNo,
      (select wa.[Value]
      from [pls].[WOStationAttribute] wa
        JOIN [pls].[CodeAttribute] ca1 ON ca1.ID = wa.AttributeID AND ca1.AttributeName IN ('GRADE')
      where  wa.WOStationHistoryID = wh.ID) as GRADE ,
      (select wa.[Value]
      from [pls].[WOStationAttribute] wa
        JOIN [pls].[CodeAttribute] ca1 ON ca1.ID = wa.AttributeID AND ca1.AttributeName IN ('ReIDSerialNumber')
      where  wa.WOStationHistoryID = wh.ID) as New_SerialNo ,
      pnn.[Description],
      ps.RODate AS RO_Date,
      wh.LastActivityDate AS WO_ClosedDate,
      sh.LastActivityDate AS SO_ReservedDate
    FROM [pls].[PartSerial] ps
      LEFT JOIN [pls].[ROUnit] ru ON ru.SerialNo = ps.SerialNo
      LEFT JOIN [pls].[CodeStatus] cs ON cs.ID = ps.StatusID
      LEFT JOIN [pls].[PartNoAttribute] pn ON pn.PartNo = ps.PartNo
      LEFT JOIN [pls].[CodeAttribute] ca ON ca.ID = pn.AttributeID
      LEFT JOIN [pls].[PartNo] pnn ON pnn.PartNo = ps.PartNo
      LEFT JOIN [pls].[ROHeader] rh ON ps.ROHeaderID = rh.ID
      LEFT JOIN [pls].[WOStationHistory] wh ON wh.WOHeaderID = ps.WOHeaderID
      LEFT JOIN [pls].[SOUnit] su ON su.SerialNo = ps.SerialNo
      left join [pls].[PartLocation] pl on su.FromLocationID is not null and pl.ID=su.FromLocationID and pl.ProgramID=10045 and pl.LocationNo like '%.pic.%'
      LEFT JOIN [pls].[SOLine] sl ON sl.ID = su.SOLineID
      LEFT JOIN [pls].[SOHeader] sh ON sl.SOHeaderID = sh.ID
    WHERE ps.ProgramID = '<programID>'
      and pl.LocationNo is null
      AND (
      cs.[Description] IN ('Repair', 'SCRAP', 'Reid', 'Reserved')
      OR (cs.[Description] = 'RECEIVED' AND ca.AttributeName = 'AUTO_WO' AND pn.[Value] = 'NO')
    )
  ),
  bb
  AS
  (
    SELECT
      SerialNo AS old_SerialNo,
      New_SerialNo AS SerialNo
    FROM aa
    GROUP BY SerialNo, New_SerialNo
    HAVING New_SerialNo IS NOT NULL AND SerialNo != New_SerialNo
  )
SELECT
ProgramID,
  Ship_Order_No,
  Receipt_Order_No,
  PartNo,
  aa.SerialNo,
  aa.[Description],
  MAX(bb.old_SerialNo) AS Old_SerialNo,
  max(aa.status) AS status,
  COALESCE(MAX(aa.GRADE), 
           CASE 
             WHEN aa.status = 'RECEIVED' THEN 'Received without Repair'
             WHEN aa.status = 'RESERVED' THEN 'SCRAP'
             ELSE aa.status
           END) AS Grade,
  MAX(RO_Date) AS Receipt_Order_Date,
  MAX(WO_ClosedDate) AS Work_Order_Closed_Date,
  MAX(SO_ReservedDate) AS Ship_Order_Reserved_Date
FROM aa
  LEFT JOIN bb ON aa.SerialNo = bb.SerialNo
GROUP BY 
ProgramID,
  Ship_Order_No, 
  Receipt_Order_No, 
  PartNo, 
  aa.SerialNo, 
  aa.[Description], 
  aa.STATUS
HAVING aa.STATUS IN ('Repair', 'SCRAP', 'RECEIVED', 'RESERVED')
ORDER BY Work_Order_Closed_Date DESC
       ";

            query = query.Replace("<programID>", programID);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(programName))
                filterString = "> Program = '" + programName + "' ";


            cLog oLog = new cLog();
            oLog.AddSqlQuery("196", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstAvailableInventory = cCommon.ConvertDtToHashTable(dt);
                return true;

            }


        }
    }
}