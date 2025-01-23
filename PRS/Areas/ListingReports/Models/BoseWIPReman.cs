using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class BoseWIPReman
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstBoseWIPReman { get; set; }
        public List<Hashtable> lstUnit { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList()
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"
SELECT 
    xx.Line,  
    xx.CodeName, 
    xx.ProgramID,

    -- Total sum of all positions
    SUM(CASE WHEN xx.Position IN ('FRESH', 'WIP', 'HOLD', 'PACKING', 'FGI') THEN 1 ELSE 0 END) AS REMAN,

    SUM(CASE WHEN xx.Position = 'FRESH' THEN 1 ELSE 0 END) AS FRESH,
    SUM(CASE WHEN xx.Position = 'WIP' THEN 1 ELSE 0 END) AS WIP,
    SUM(CASE WHEN xx.Position = 'HOLD' THEN 1 ELSE 0 END) AS HOLD,
    SUM(CASE WHEN xx.Position = 'PACKING' THEN 1 ELSE 0 END) AS PACKING,
    SUM(CASE WHEN xx.Position = 'FGI' THEN 1 ELSE 0 END) AS FGI,
    
    SUM(CASE WHEN xx.StatusDesc = 'REPAIR' THEN 1 ELSE 0 END) AS QTY

FROM
(
    SELECT concat(
            CASE
                WHEN x.CodeName IN ('ARIZONA', 'LIPTON', 'MINNOW', 'PHELPS', 'SKIPPER', 'PROFESSOR BB') THEN 'AIO CELL'
                WHEN x.CodeName IN ('EDDIE', 'LANCOME', 'LANCOME PLUS', 'M3', 'TAYLOR') THEN 'BT CELL'
                WHEN x.CodeName IN ('DURAN', 'GOODYEAR', 'LONE STARR', 'PRINCE') THEN 'HEADSET CELL'
                WHEN x.CodeName IN ('SCOTTY', 'SMALLS') THEN 'INEAR CELL'
                WHEN x.CodeName IN ('ANGUS', 'BABY YODA', 'BENTO GILLIGAN', 'CHIBI', 'GILLIGAN PREMIUM', 'GINGER CHEEVERS', 'MALCOLM', 'SAN DIEGO', 'STEVIE', 'ZAKIM') THEN 'SYSTEM CELL'
                WHEN x.CodeName = 'EDELMAN' THEN 'NPI - AIO CELL'
                WHEN x.CodeName = 'SERENA' THEN 'NPI - INEAR CELL'
            END, ' - ', x.CodeName) AS Line,
           x.PartNo,
           x.ProgramID, -- Added ProgramID column
           CASE 
               WHEN x.Position = 'FGI' AND x.BoxNo = '0' THEN 'FGI - Shop Floor' 
               WHEN x.Position = 'FGI' AND x.BoxNo != '0' THEN 'FGI - Storage'
               ELSE x.Position
           END AS Position,
           x.CodeName,
           x.StatusDesc
    FROM
    (
        SELECT woh.PartNo,
               woh.SerialNo,
               woh.ProgramID, -- Added ProgramID column
               woh.CreateDate,
               woh.CustomerReference,
               woh.LastActivityDate,
               (SELECT cs.Description
                  FROM pls.CodeStatus cs
                 WHERE cs.ID = pt.PartTransactionID) AS ship_status,
               ISNULL(wsd.Code, cws.Description) AS WorkStation,
               CASE
                   WHEN UPPER(ISNULL(wsd.Code, cws.Description)) = 'A1030' AND cs.Description = 'WIP' THEN 'FAIL'
                   WHEN UPPER(ISNULL(wsd.Code, cws.Description)) IN ('A1200', 'CLOSE') THEN 'FGI'
                   WHEN cs.Description = 'HOLD' THEN 'HOLD'
                   WHEN UPPER(ISNULL(wsd.Code, cws.Description)) = 'WFFA' THEN 'FRESH'
                   WHEN UPPER(ISNULL(wsd.Code, cws.Description)) = 'SCRAP' THEN 'SCRAP'
                   WHEN UPPER(ISNULL(wsd.Code, cws.Description)) = 'A1090' THEN 'PACKING'
                   ELSE 'WIP'
               END AS Position,
               (SELECT UPPER(MAX(pna.Value))
                FROM pls.PartNoAttribute pna, pls.CodeAttribute ca1
                WHERE ca1.ID = pna.AttributeID
                      AND ca1.AttributeName = 'CODE_NAME'
                      AND pna.PartNo = woh.PartNo) AS CodeName,
               cs.Description AS StatusDesc,
               pl.LocationNo,
               (SELECT CASE
                          WHEN rha1.Value IN ('RETURN', 'EXCHANGE') THEN 'REMAN'
                          ELSE rha1.Value
                      END
                FROM pls.ROHeaderAttribute rha1, pls.CodeAttribute ca1
                WHERE ca1.ID = rha1.AttributeID
                      AND ca1.AttributeName = 'PROCESS_TYPE'
                      AND rha1.ROHeaderID = ps.ROHeaderID) AS PROCESS_TYPE,
               (SELECT TOP 1 ro.CustomerReference
                FROM pls.ROHeader ro
                WHERE ro.ID = ps.ROHeaderID) AS RMA,
               ISNULL((SELECT TOP 1 psa.Value
                       FROM pls.PartSerialAttribute psa, pls.CodeAttribute ca
                       WHERE ca.ID = psa.AttributeID
                             AND ca.AttributeName = 'RECEIVING_MANUAL_DISPOSITION_OVERRIDE'
                             AND psa.PartSerialID = ps.ID), '0') AS MANUAL_DISPO,
               (SELECT ps.PalletBoxNo
                FROM pls.PartSerial ps
                LEFT JOIN pls.PartLocation pl ON pl.ID = ps.LocationID
                WHERE ps.ProgramID = '10058'
                      AND ps.PartNo = woh.PartNo
                      AND ps.SerialNo = woh.SerialNo) AS BoxNo        
        FROM pls.WOHeader woh
        INNER JOIN pls.PartSerial ps ON ps.WOHeaderID = woh.ID
                                        AND ps.SerialNo = woh.SerialNo
                                        AND woh.PartNo = ps.PartNo
        LEFT JOIN pls.CodeWorkStationCustomDescription WSD ON WSD.ProgramID = woh.ProgramID
                                                             AND WSD.RepairTypeID = woh.RepairTypeID
                                                             AND WSD.CodeWorkStationID = woh.WorkStationID
        LEFT JOIN pls.CodeStatus cs ON cs.ID = woh.StatusID
        LEFT JOIN pls.CodeWorkStation cws ON cws.ID = ps.WorkStationID
        LEFT JOIN pls.PartLocation pl ON pl.ID = ps.LocationID
        LEFT JOIN pls.PartTransaction pt ON pt.ProgramID = woh.ProgramID
                                           AND pt.PartNo = woh.PartNo
                                           AND pt.SerialNo = woh.SerialNo
                                           AND pt.PartTransactionID = 18
        WHERE woh.ProgramID = '<programId>'
    ) x
) xx
GROUP BY 
    xx.Line,  
    xx.CodeName, 
    xx.ProgramID -- Grouping only at the Cell level
ORDER BY 
    xx.Line;


 ";
            


                    if (sites == "BYDGOSZCZ")
            {
                query = query.Replace("<programId>", "10058");
            }

            else
            {
                query = query.Replace("<programId>", "10059");
            }
            DataTable dt = oDAL.GetData(query);

            filterString += "> Program = Bose";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("238", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstBoseWIPReman = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }

        public bool GetUnits(string programId, string Line, string Code)
        {


            string query = string.Empty;
            var conType = @HttpContext.Current.Session["CONN_TYPE"].ToString();

            query = @"SELECT distinct
    xx.Line,  
    xx.CodeName, 
    xx.ProgramID,
    xx.PartNo
FROM
(
    SELECT concat(
            CASE
                WHEN x.CodeName IN ('ARIZONA', 'LIPTON', 'MINNOW', 'PHELPS', 'SKIPPER', 'PROFESSOR BB') THEN 'AIO CELL'
                WHEN x.CodeName IN ('EDDIE', 'LANCOME', 'LANCOME PLUS', 'M3', 'TAYLOR') THEN 'BT CELL'
                WHEN x.CodeName IN ('DURAN', 'GOODYEAR', 'LONE STARR', 'PRINCE') THEN 'HEADSET CELL'
                WHEN x.CodeName IN ('SCOTTY', 'SMALLS') THEN 'INEAR CELL'
                WHEN x.CodeName IN ('ANGUS', 'BABY YODA', 'BENTO GILLIGAN', 'CHIBI', 'GILLIGAN PREMIUM', 'GINGER CHEEVERS', 'MALCOLM', 'SAN DIEGO', 'STEVIE', 'ZAKIM') THEN 'SYSTEM CELL'
                WHEN x.CodeName = 'EDELMAN' THEN 'NPI - AIO CELL'
                WHEN x.CodeName = 'SERENA' THEN 'NPI - INEAR CELL'
            END, ' - ', x.CodeName) AS Line,
           x.PartNo,
           x.ProgramID, -- Added ProgramID column
           CASE 
               WHEN x.Position = 'FGI' AND x.BoxNo = '0' THEN 'FGI - Shop Floor' 
               WHEN x.Position = 'FGI' AND x.BoxNo != '0' THEN 'FGI - Storage'
               ELSE x.Position
           END AS Position,
           x.CodeName,
           x.StatusDesc
    FROM
    (
        SELECT woh.PartNo,
               woh.SerialNo,
               woh.ProgramID, -- Added ProgramID column
               woh.CreateDate,
               woh.CustomerReference,
               woh.LastActivityDate,
               (SELECT cs.Description
                  FROM pls.CodeStatus cs
                 WHERE cs.ID = pt.PartTransactionID) AS ship_status,
               ISNULL(wsd.Code, cws.Description) AS WorkStation,
               CASE
                   WHEN UPPER(ISNULL(wsd.Code, cws.Description)) = 'A1030' AND cs.Description = 'WIP' THEN 'FAIL'
                   WHEN UPPER(ISNULL(wsd.Code, cws.Description)) IN ('A1200', 'CLOSE') THEN 'FGI'
                   WHEN cs.Description = 'HOLD' THEN 'HOLD'
                   WHEN UPPER(ISNULL(wsd.Code, cws.Description)) = 'WFFA' THEN 'FRESH'
                   WHEN UPPER(ISNULL(wsd.Code, cws.Description)) = 'SCRAP' THEN 'SCRAP'
                   WHEN UPPER(ISNULL(wsd.Code, cws.Description)) = 'A1090' THEN 'PACKING'
                   ELSE 'WIP'
               END AS Position,
               (SELECT UPPER(MAX(pna.Value))
                FROM pls.PartNoAttribute pna, pls.CodeAttribute ca1
                WHERE ca1.ID = pna.AttributeID
                      AND ca1.AttributeName = 'CODE_NAME'
                      AND pna.PartNo = woh.PartNo) AS CodeName,
               cs.Description AS StatusDesc,
               pl.LocationNo,
               (SELECT CASE
                          WHEN rha1.Value IN ('RETURN', 'EXCHANGE') THEN 'REMAN'
                          ELSE rha1.Value
                      END
                FROM pls.ROHeaderAttribute rha1, pls.CodeAttribute ca1
                WHERE ca1.ID = rha1.AttributeID
                      AND ca1.AttributeName = 'PROCESS_TYPE'
                      AND rha1.ROHeaderID = ps.ROHeaderID) AS PROCESS_TYPE,
               (SELECT TOP 1 ro.CustomerReference
                FROM pls.ROHeader ro
                WHERE ro.ID = ps.ROHeaderID) AS RMA,
               ISNULL((SELECT TOP 1 psa.Value
                       FROM pls.PartSerialAttribute psa, pls.CodeAttribute ca
                       WHERE ca.ID = psa.AttributeID
                             AND ca.AttributeName = 'RECEIVING_MANUAL_DISPOSITION_OVERRIDE'
                             AND psa.PartSerialID = ps.ID), '0') AS MANUAL_DISPO,
               (SELECT ps.PalletBoxNo
                FROM pls.PartSerial ps
                LEFT JOIN pls.PartLocation pl ON pl.ID = ps.LocationID
                WHERE ps.ProgramID ='<programId>'
                      AND ps.PartNo = woh.PartNo
                      AND ps.SerialNo = woh.SerialNo) AS BoxNo        
        FROM pls.WOHeader woh
        INNER JOIN pls.PartSerial ps ON ps.WOHeaderID = woh.ID
                                        AND ps.SerialNo = woh.SerialNo
                                        AND woh.PartNo = ps.PartNo
        LEFT JOIN pls.CodeWorkStationCustomDescription WSD ON WSD.ProgramID = woh.ProgramID
                                                             AND WSD.RepairTypeID = woh.RepairTypeID
                                                             AND WSD.CodeWorkStationID = woh.WorkStationID
        LEFT JOIN pls.CodeStatus cs ON cs.ID = woh.StatusID
        LEFT JOIN pls.CodeWorkStation cws ON cws.ID = ps.WorkStationID
        LEFT JOIN pls.PartLocation pl ON pl.ID = ps.LocationID
        LEFT JOIN pls.PartTransaction pt ON pt.ProgramID = woh.ProgramID
                                           AND pt.PartNo = woh.PartNo
                                           AND pt.SerialNo = woh.SerialNo
                                           AND pt.PartTransactionID = 18
        WHERE woh.ProgramID ='<programId>'
    ) x
) xx
WHERE 
    xx.Line = '<Line>'
    AND xx.CodeName = '<Code>'
    AND xx.ProgramID = '<programId>'
ORDER BY 
    xx.PartNo;

 
 ";



            query = query.Replace("<programId>", programId);
            query = query.Replace("<Line>", Line);
            query = query.Replace("<Code>", Code);


            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("238", query, "---Part No.---", false);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstUnit = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }
    }
}