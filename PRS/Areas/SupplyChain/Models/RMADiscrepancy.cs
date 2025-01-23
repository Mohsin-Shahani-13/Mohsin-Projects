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
    public class RMADiscrepancy 
    {
        [Display(Name = "RO Create Date From:")]
        public string _ROCreatefromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string CreatefromDt { get { return _ROCreatefromDt; } set { _ROCreatefromDt = value; } }

        [Display(Name = "RO Create Date To:")]
        public string _ROCreateToDt = DateTime.Now.ToString(Format.DateOnly);
        public string CreateToDt { get { return _ROCreateToDt; } set { _ROCreateToDt = value; } }

        [Display(Name = "ROReceived Date From:")]
        public string _ROReceivedfromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string ReceivedfromDt { get { return _ROReceivedfromDt; } set { _ROReceivedfromDt = value; } }

        [Display(Name = "ROReceived Date To:")]
        public string _ROReceivedToDt = DateTime.Now.ToString(Format.DateOnly);
        public string ReceivedToDt { get { return _ROReceivedToDt; } set { _ROReceivedToDt = value; } }
        public bool isAllDate { get; set; }
        [Display(Name = "Customer Ref. :")]
        public string custRef { get; set; }
        [Display(Name = "Serial No. :")]
        public string serialNo { get; set; }
        [Display(Name = "Box No. :")]
        public string boxNo { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public List<Hashtable> lstRMADiscrepancy { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        public string Report_Name { get; set; }

        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        cDAL oDAL = new cDAL("ACTIVE");
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>' AND NAME ='TOSHIBA'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);

            return dt;
        }

        public bool GetList(string CreateFDate, string CreateTDate, bool isAllDate, string serialNo, string ProgramID, string ProgramName, string custRefer, string boxNo, string rptName)
        {
            string query = string.Empty;

            string _boxNo = GetInValue(boxNo);
            string _serialNo = GetInValue(serialNo);
            string _custRefe = GetInValue(custRefer);


            if (rptName == "Summary")
            {
                query = @"
DROP TABLE IF EXISTS #RMA_SMRY
CREATE TABLE #RMA_SMRY (
    ROHEADER_ID  INT,
    PROGRAM_ID INT,
    ProgramName VARCHAR(50),
    RMA_NO  VARCHAR(50),
    RMA_QTY  INT,
    RMA_SCANNED_QTY  INT,
    SHORTAGE_QTY  INT,
    RCVD_STATUS  VARCHAR(25),
    RMA_CLOSED_COUNT  INT,
    RMA_WO_COUNT  INT,
    RMA_STATUS  VARCHAR(25),
    TAT INT,
    RCVD_DATE SMALLDATETIME,
    CLOSED_DATE SMALLDATETIME,
    REJ_SHIP_COUNT INT,
    RTV_SHIP_COUNT INT,
    RO_DATE SMALLDATETIME,
    ACTION_COUNT INT,
    VMI_ACTION_COUNT INT,
    MAX_WO_CLOSE_DATE DATE,
    MIN_RCVD_DATE DATE,
    MAX_RCVD_DATE DATE
)
INSERT INTO #RMA_SMRY (ROHEADER_ID, Program_ID, ProgramName,  RMA_NO, RMA_QTY, RMA_SCANNED_QTY, RO_DATE, RCVD_DATE)
SELECT ROH.ID AS ROHEADER_ID
     , ROH.ProgramID AS Program_ID  
     , P.Name AS ProgramName
     , ROH.CustomerReference AS RMA_NO
     , 0 AS RMA_QTY
     , 0 AS RMA_SCANNED_QTY
     , ROH.CreateDate AS RO_DATE
     , MIN(PS.CreateDate) AS RCVD_DATE
FROM pls.ROHeader ROH
INNER JOIN pls.Program P ON P.ID = ROH.ProgramID
INNER JOIN pls.ROLine ROL ON ROL.ROHeaderID = ROH.ID
INNER JOIN pls.PartSerial PS ON PS.ROHeaderID = ROH.ID
								AND PS.PartNo = ROL.PartNo

";


                if (serialNo != "" && boxNo == "")
                {
                    query += @" INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID ";

                    query += "AND ROU.SerialNo IN (" + _serialNo + ")";
                    query += @" INNER JOIN pls.PartSerial PTS ON PTS.ProgramID = ROH.ProgramID
                                AND PTS.PartNo = ROL.PartNo
                                AND PTS.SerialNo = ROU.SerialNo ";
                }
                else if (boxNo != "" && serialNo == "")
                {
                    query += @" INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID";
                    query += @" INNER JOIN pls.PartSerial PTS ON PTS.ROHeaderID = ROH.ID
                               AND PTS.PartNo = ROL.PartNo
                              AND PTS.SerialNo = ROU.SerialNo";
                    query += @" AND PTS.PalletBoxNo IN (" + _boxNo + ") ";

                }
                else if (serialNo != "" && boxNo != "")
                {
                    query += @"INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID";
                    query += " AND ROU.SerialNo IN (" + _serialNo + ")";
                    query += @" INNER JOIN pls.PartSerial PTS ON PTS.ROHeaderID = ROH.ID
                               AND PTS.PartNo = ROL.PartNo
                              AND PTS.SerialNo = ROU.SerialNo ";
                    query += @"AND PTS.PalletBoxNo IN (" + _boxNo + ") ";
                }

                if (ProgramID != "0")
                {
                    query += "Where ROH.ProgramID = '" + ProgramID + "' ";
                }
                else
                {
                    query += "where ROH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }



                if (isAllDate != true)
                {
                    query += "AND CONVERT(Datetime, ROH.CreateDate) >= '<CreateFDate>' AND CONVERT(Datetime, ROH.CreateDate) <= '<CreateTDate>'";
                }

                query = query.Replace("<CreateFDate>", CreateFDate);
                query = query.Replace("<CreateTDate>", CreateTDate);

                //if (!string.IsNullOrEmpty(serialNo))
                //    query += "AND ROU.SerialNo IN (" + _serialNo + ") ";

                if (!string.IsNullOrEmpty(custRefer))
                    query += "AND ROH.CustomerReference IN (" + _custRefe + ")";

                //if (!string.IsNullOrEmpty(boxNo))
                //    query += "AND PS.PalletBoxNo IN (" + _boxNo + ")";

                //if (!string.IsNullOrEmpty(currentInventoryLocation))
                //    query += "AND PL.LocationNo LIKE '%" + currentInventoryLocation + "%' ";

                //if (ProgramID != "0")
                //{
                //    query += "AND ROH.ProgramID = '" + ProgramID + "' ";
                //}
                //else
                //{
                //    query += "AND ROH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                //}

                query += @" GROUP BY ROH.ProgramID,
                             P.Name,
                             ROH.ID,
                             ROH.CustomerReference,
                             ROH.CreateDate";

                query += @"
UPDATE RS 
SET RMA_QTY =
(
 SELECT SUM(ROL.QtyToReceive)
 from pls.ROLine ROL
 where ROL.ROHeaderID = RS.ROHEADER_ID
)FROM #RMA_SMRY RS

UPDATE RS 
SET RMA_SCANNED_QTY =
(
 SELECT SUM(ROL.QtyReceived)
 from pls.ROLine ROL
 where ROL.ROHeaderID = RS.ROHEADER_ID
)FROM #RMA_SMRY RS

-- NOW CALCULATING SHORTAGE QTY
UPDATE #RMA_SMRY SET SHORTAGE_QTY = (RMA_QTY - RMA_SCANNED_QTY)
-- GETTING REC STATUS
UPDATE #RMA_SMRY 
SET RCVD_STATUS = (CASE WHEN SHORTAGE_QTY = 0 THEN 'Equal' WHEN SHORTAGE_QTY > 0 THEN 'Shortage' ELSE 'Pending' END)
-- RMA CLOSED COUNT
UPDATE RS 
SET RMA_CLOSED_COUNT =
(
 SELECT COUNT(*)
 FROM pls.ROUnit ROU
 INNER JOIN pls.ROLine ROL ON ROL.ID = ROU.ROLineID
 INNER JOIN pls.WOHeader WOH ON WOH.SerialNo = ROU.SerialNo
 INNER JOIN pls.PartSerial PS on PS.SerialNo = woh.SerialNo AND PS.PartNo = Rol.PartNo AND PS.ProgramID = RS.PROGRAM_ID
 WHERE ROL.ROHeaderID = RS.ROHEADER_ID AND woh.StatusID <> 3  AND PS.WOEndDate is not null
)FROM #RMA_SMRY RS

-- RMA WO COUNT
UPDATE RS 
SET RS.RMA_WO_COUNT =
(
 SELECT COUNT(*)
 FROM pls.ROUnit ROU
 INNER JOIN pls.ROLine ROL ON ROL.ID = ROU.ROLineID
 INNER JOIN pls.WOHeader WOH ON WOH.SerialNo = ROU.SerialNo
 WHERE ROL.ROHeaderID = RS.ROHEADER_ID AND woh.StatusID <> 3 
)
FROM #RMA_SMRY RS
-- RMA STATUS
UPDATE #RMA_SMRY SET RMA_STATUS = (CASE WHEN (RMA_QTY = RMA_SCANNED_QTY AND RMA_WO_COUNT = RMA_CLOSED_COUNT)  THEN 'Closed' ELSE 'Open' END)
-- CLOSED DATE
UPDATE #RMA_SMRY SET CLOSED_DATE =
(
    SELECT MAX(WOEndDate)
    FROM pls.PartSerial 
    WHERE ROHeaderID = ROHEADER_ID 
)
WHERE RMA_SCANNED_QTY <> 0 AND RMA_STATUS = 'Closed'
-- TAT - Condition 1 where rma_scanned_qty = 0
UPDATE #RMA_SMRY SET TAT = 0 WHERE RMA_SCANNED_QTY = 0

--  TAT - Condition 2 
 --   if have WO in the RMA#, ""RMA with WO"" = ""RMA WO Closed"", then get max of ""WO Closed Date"" - min of ""Received On""

-- getting Max WOEndDate from pls.PartSerial
UPDATE RMA SET MAX_WO_CLOSE_DATE =
(
    SELECT MAX(WOEndDate)
    FROM pls.PartSerial ps
    WHERE ps.ROHeaderID = RMA.ROHEADER_ID 
)
FROM #RMA_SMRY RMA
-- get Min Rcvd On from pls.PartTransaction
UPDATE RMA SET MIN_RCVD_DATE =
(
    SELECT MIN(ForDate)
    FROM pls.PartTransaction pt
    WHERE pt.PartTransactionID = 1
          AND pt.OrderHeaderID = RMA.ROHEADER_ID
          AND pt.CustomerReference = RMA.RMA_NO
)
FROM #RMA_SMRY RMA
-- get Max Rcvd On from pls.PartTransaction
UPDATE RMA SET MAX_RCVD_DATE =
(
    SELECT MAX(ForDate)
    FROM pls.PartTransaction pt
    WHERE pt.PartTransactionID = 1
          AND pt.OrderHeaderID = RMA.ROHEADER_ID
          AND pt.CustomerReference = RMA.RMA_NO
)
FROM #RMA_SMRY RMA
-- Condition 2 - Part 1 : if have WO in the RMA#, ""RMA with WO"" = ""RMA WO Closed"", then get max of ""WO Closed Date"" - min of ""Received On""
UPDATE RMA SET TAT = ABS(DATEDIFF(DAY, MAX_WO_CLOSE_DATE, MIN_RCVD_DATE))
FROM #RMA_SMRY RMA
WHERE TAT IS NULL
      AND MAX_WO_CLOSE_DATE IS NOT NULL
      AND RMA_WO_COUNT = RMA_CLOSED_COUNT
-- Condition 2 - Part 2 : if don’t have WO and only have RO in the RMA#, ""RMA Qty"" = ""RMA Scanned Qty"", then get max of ""Received On"" - min of ""Received On"" 
UPDATE RMA SET TAT = ABS(DATEDIFF(DAY, MAX_RCVD_DATE, MIN_RCVD_DATE))
FROM #RMA_SMRY RMA
WHERE TAT IS NULL--TO AVOID 1ST CONDITION
      AND MAX_WO_CLOSE_DATE IS NULL --TO AVOID 2ND PART 1 CONDITION
     AND RMA_QTY = RMA_SCANNED_QTY

-- Condition 3 Part 1 - if ""RMA Scanned Qty"" > 0 then system date - min of ""Received On""
UPDATE RMA SET TAT = ABS(DATEDIFF(DAY, GETDATE(), MIN_RCVD_DATE))
FROM #RMA_SMRY RMA
WHERE TAT IS NULL--TO AVOID 1ST CONDITION
      --AND MAX_WO_CLOSE_DATE IS NULL --TO AVOID 2ND PART 1 CONDITION
     AND RMA_QTY<> RMA_SCANNED_QTY --TO AVOID 2ND PART 2 CONDITION
   AND RMA_SCANNED_QTY > 0
-- Condition 3 Part 2 - if ""RMA Scanned Qty"" > 0 then system date - min of ""Received On""
UPDATE RMA SET TAT = ABS(DATEDIFF(DAY, GETDATE(), MIN_RCVD_DATE))
FROM #RMA_SMRY RMA
WHERE TAT IS NULL--TO AVOID 1ST CONDITION 
     AND RMA_QTY = RMA_SCANNED_QTY --TO AVOID 2ND PART 2 CONDITION
     AND RMA_SCANNED_QTY > 0

UPDATE SMRY SET ACTION_COUNT =
(
    SELECT SUM(CASE WHEN WSA.Value = 'NTF' AND RUA.Value <> CIL.Item THEN 1 
			WHEN WSA.Value <> 'NTF' THEN 1 
			ELSE 0 
	       END)
	FROM pls.PartSerial PS
	INNER JOIN pls.WOHeader WOH ON WOH.ID= ps.WOHeaderID
	INNER JOIN pls.CodeAttribute CA ON CA.AttributeName = 'ACTION'
	INNER JOIN pls.WOStationAttribute WSA ON WSA.AttributeID = CA.ID AND WSA.Value IN ('NTF','CID') 
               AND WSA.WOStationHistoryID = ( Select MAX(WSH.ID)  
                                              from pls.WOStationHistory WSH 
                                              where WSH.WOHeaderID = WOH.ID 
                                                    AND WSH.WorkStationID = WOH.WorkStationID)
	LEFT JOIN pls.ROLine ROL ON ROL.ROHeaderID = PS.ROHeaderID AND ROL.PartNo = PS.PartNo
	LEFT JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID AND ROU.SerialNo = PS.SerialNo
	LEFT JOIN  pls.CodeItemList CIL ON CIL.Name = 'ODM_FOR_NTF'
	LEFT JOIN pls.CodeAttribute CAT ON CAT.AttributeName = 'ODM'
	LEFT JOIN pls.ROUnitAttribute RUA ON RUA.ROUnitID = ROU.ID  AND  RUA.AttributeID = CAT.ID AND RUA.Value <> CIL.Item
	WHERE PS.SerialNo = WOH.SerialNo AND
	      PS.SODate IS NOT NULL
	          AND PS.ProgramID = SMRY.PROGRAM_ID
	          AND PS.ROHeaderID = SMRY.ROHEADER_ID
)
FROM #RMA_SMRY SMRY
-- VMI ACTION COUNT FOR REJECT SHIPPED COUNT
UPDATE SMRY SET VMI_ACTION_COUNT =
(
    SELECT COUNT(*)
    FROM pls.PartSerial PS
    INNER JOIN pls.ROLine ROL ON ROL.ROHeaderID = PS.ROHeaderID
    INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID
    INNER JOIN pls.CodeAttribute CA ON CA.AttributeName = 'VMI_ACTION'
    INNER JOIN pls.ROUnitAttribute RUA ON RUA.ROUnitID = ROU.ID AND RUA.AttributeID = CA.ID AND RUA.Value IN ('NTF', 'CID')
    WHERE PS.SerialNo = ROU.SerialNo AND
          PS.SODate IS NOT NULL
          AND PS.ProgramID = SMRY.PROGRAM_ID
          AND PS.ROHeaderID = SMRY.ROHEADER_ID
)
FROM #RMA_SMRY SMRY
-- UPDATING REJ SHIP COUNT
UPDATE #RMA_SMRY SET REJ_SHIP_COUNT =  ACTION_COUNT + VMI_ACTION_COUNT

--- UPDATE ACTION COUNT TO NULL
 UPDATE SMRY SET ACTION_COUNT = NULL
 FROM #RMA_SMRY SMRY

 --- UPDATE VMI ACTION COUNT TO NULL
 UPDATE SMRY SET VMI_ACTION_COUNT = NULL
 FROM #RMA_SMRY SMRY

-- ACTION COUNT FOR RTV SHIPPED COUNT
UPDATE SMRY SET ACTION_COUNT =
(
    SELECT SUM(CASE WHEN WSA.Value = 'NTF' AND RUA.Value = CIL.Item THEN 1 
			WHEN WSA.Value <> 'NTF' THEN 1 
			ELSE 0 
	       END)
	FROM pls.PartSerial PS
	INNER JOIN pls.WOHeader WOH ON WOH.ID= ps.WOHeaderID
	INNER JOIN pls.CodeAttribute CA ON CA.AttributeName = 'ACTION'
	INNER JOIN pls.WOStationAttribute WSA ON WSA.AttributeID = CA.ID AND WSA.Value IN ('NTF','RTV') 
                  AND WSA.WOStationHistoryID = ( Select MAX(WSH.ID)  
                                              from pls.WOStationHistory WSH 
                                              where WSH.WOHeaderID = WOH.ID 
                                                    AND WSH.WorkStationID = WOH.WorkStationID)
	LEFT JOIN pls.ROLine ROL ON ROL.ROHeaderID = PS.ROHeaderID AND ROL.PartNo = PS.PartNo
	LEFT JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID AND ROU.SerialNo = PS.SerialNo
	LEFT JOIN  pls.CodeItemList CIL ON CIL.Name = 'ODM_FOR_NTF'
	LEFT JOIN pls.CodeAttribute CAT ON CAT.AttributeName = 'ODM'
	LEFT JOIN pls.ROUnitAttribute RUA ON RUA.ROUnitID = ROU.ID  AND  RUA.AttributeID = CAT.ID AND RUA.Value = CIL.Item
	WHERE PS.SerialNo = WOH.SerialNo AND
	      PS.SODate IS NOT NULL
	          AND PS.ProgramID = SMRY.PROGRAM_ID
	          AND PS.ROHeaderID = SMRY.ROHEADER_ID
)
FROM #RMA_SMRY SMRY
-- VMI ACTION COUNT FOR RTV SHIPPED COUNT
UPDATE SMRY SET VMI_ACTION_COUNT =
(
    SELECT COUNT(*)
    FROM pls.PartSerial PS
    INNER JOIN pls.ROLine ROL ON ROL.ROHeaderID = PS.ROHeaderID
    INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID
    INNER JOIN pls.CodeAttribute CA ON CA.AttributeName = 'VMI_ACTION'
    INNER JOIN pls.ROUnitAttribute RUA ON RUA.ROUnitID = ROU.ID AND RUA.AttributeID = CA.ID AND RUA.Value IN ('NTF', 'RTV')
    INNER JOIN  pls.CodeItemList CIL ON CIL.Name = 'ODM_FOR_NTF'
    INNER JOIN pls.CodeAttribute CAT ON CAT.AttributeName = 'ODM'
    INNER JOIN pls.ROUnitAttribute RUAT ON RUAT.ROUnitID = ROU.ID AND  RUAT.AttributeID = CAT.ID AND RUAT.Value = CIL.Item
    WHERE PS.SerialNo = ROU.SerialNo AND
          PS.SODate IS NOT NULL
          AND PS.ProgramID = SMRY.PROGRAM_ID
          AND PS.ROHeaderID = SMRY.ROHEADER_ID
)
FROM #RMA_SMRY SMRY
-- UPDATING RTV SHIP COUNT
UPDATE #RMA_SMRY SET RTV_SHIP_COUNT = CASE WHEN ACTION_COUNT = 0 THEN VMI_ACTION_COUNT ELSE ACTION_COUNT END
select * from #RMA_SMRY ";

            }
            else if (rptName == "Detail")

            {
                query = @"IF OBJECT_ID('tempdb.dbo.#RMADes') IS NULL
BEGIN
CREATE TABLE #RMADes
(
 ProgramId INT,
 ID INT,
 ProgramName VARCHAR(50),
 RMANo VARCHAR(250),
 PartNo VARCHAR(250),
 SerialNo VARCHAR(250),
 PreAlertDate SMALLDATETIME,
 ReceivedDate SMALLDATETIME,
 WOCloseDate SMALLDATETIME,
 BoxNo varchar(150),
 BoxDate SMALLDATETIME,
 FDCode varchar(150),
 ModelNo VARCHAR(200),
 STARTDATE SMALLDATETIME,
 CREDIT_TO VARCHAR(100),
 MNA VARCHAR(50),
 ODM VARCHAR(50),
 WOHID INT,
 cROUnitId INT,
 cWSHId INT,
 UnitStatus VARCHAR(200),
 FinalAction Varchar(50),
 FinalReason VARCHAR(50),
)
END

INSERT INTO #RMADes (ID, ProgramID, ProgramName, RMANo, PartNo, SerialNo, PreAlertDate, ReceivedDate, WOCloseDate, BoxNo, BoxDate
                ,cROUnitId,WOHID, cWSHId, ModelNo, UnitStatus)
SELECT  DISTINCT 
         ROH.ID AS ID
        ,ROH.ProgramID
        ,P.Name AS ProgramName
        ,ROH.CustomerReference RMANo
        ,ROL.PartNo PartNo
        ,ROU.SerialNo SerialNo
        ,FORMAT(ROH.CreateDate,'yyyy.MM.dd') PreAlertDate
        ,FORMAT(PS.CreateDate,'yyyy.MM.dd ') ReceivedDate
        ,FORMAT(PS.WOEndDate,'yyyy.MM.dd ') WOCloseDate
        ,PS.PalletBoxNo BoxNo
        ,FORMAT(PPB.CreateDate,'yyyy.MM.dd') BoxDate
        ,ROU.ID ROunitId
        ,PS.WOHeaderId 
        ,MAX(WSH.ID) AS WSHId
        ,PN.ModelNo ModelNo
        ,CS.[Description] UnitStatus
FROM Pls.ROHeader ROH
INNER JOIN Pls.Program P ON P.ID = ROH.ProgramID
INNER JOIN pls.ROLine ROL on ROL.ROHeaderId = ROH.ID
INNER JOIN pls.ROUnit ROU on ROU.ROLineID = ROL.ID
INNER JOIN pls.PartNo PN on ROL.PartNo = PN.PartNo
LEFT JOIN pls.PartSerial PS on PS.ROHeaderID = ROH.ID 
                                    AND PS.PartNo = PN.PartNo 
                                    AND PS.SerialNo = Rou.SerialNo
LEFT JOIN pls.PartPalletBoxNo PPB on PPB.ProgramID = ROH.ProgramID AND PPB.CustomPalletBoxNo = PS.PalletBoxNo
INNER JOIN pls.CodeStatus CS on CS.ID = PS.StatusId
LEFT JOIN pls.WOHeader WOH ON WOH.Id = PS.WOHeaderId
LEFT JOIN pls.WOStationHistory WSH ON WSH.WOHeaderID = WOH.ID AND WSH.WorkStationId = WOH.WorkStationId


";
                if (ProgramID != "0")
                {
                    query += "WHERE ROH.ProgramID = '" + ProgramID + "' ";
                }
                else
                {
                    query += "WHERE ROH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }
                if (isAllDate != true)
                {
                    query += "AND CONVERT(Datetime, ROH.CreateDate) >= '<CreateFDate>' AND CONVERT(Datetime, ROH.CreateDate) <= '<CreateTDate>'";
                }
                query = query.Replace("<CreateFDate>", CreateFDate);
                query = query.Replace("<CreateTDate>", CreateTDate);

                if (!string.IsNullOrEmpty(serialNo))
                    query += "AND ROU.SerialNo IN (" + _serialNo + ") ";

                if (!string.IsNullOrEmpty(custRefer))
                    query += "AND ROH.CustomerReference IN (" + _custRefe + ")";

                if (!string.IsNullOrEmpty(boxNo))
                    query += "AND PS.PalletBoxNo IN (" + _boxNo + ")";

                //if (!string.IsNullOrEmpty(currentInventoryLocation))
                //    query += "AND PL.LocationNo LIKE '%" + currentInventoryLocation + "%' ";




                query += @"GROUP BY  
             ROH.ID
            ,ROH.ProgramID
            ,P.Name 
            ,ROH.CustomerReference 
            ,ROL.PartNo
            ,ROU.SerialNo
            ,ROH.CreateDate
            ,PS.CreateDate
            ,PS.WOEndDate
            ,PS.PalletBoxNo
            ,PPB.CreateDate
            ,ROU.ID 
            , PS.WOHeaderId
            ,PN.ModelNo 
            ,CS.[Description] 
--importing pls.ROUnitAttribute data for further use
SELECT  CA.AttributeName, RUA.ROUnitId, RUA.Value
INTO #ROUnitAttr
FROM pls.CodeAttribute CA
INNER JOIN pls.ROUnitAttribute RUA ON RUA.AttributeId = CA.ID
WHERE CA.AttributeName IN('FD_CODE', 'START_DATE', 'CREDIT_TO', 'MNA', 'ODM', 'VMI_RES')
AND RUA.ROUnitId IN(SELECT cROUnitId FROM #RMADes)
-- importing pls.WOStationAttribute data for further use
SELECT  CA.AttributeName, WSA.WOStationHistoryID, WSA.Value
INTO #WOStationAttr
FROM pls.CodeAttribute CA
INNER JOIN pls.WOStationAttribute WSA ON WSA.AttributeId = CA.ID
WHERE CA.AttributeName IN('ACTION', 'VMI_ACTION', 'RES')
AND WSA.WOStationHistoryID IN(SELECT cWSHId FROM #RMADes)
-- FDCode
UPDATE RMA SET FDCode = RUA.Value
FROM #RMADes RMA 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = RMA.cROUnitId 
WHERE RUA.AttributeName = 'FD_CODE'
-- STARTDATE
UPDATE RMA SET STARTDATE = RUA.Value
FROM #RMADes RMA 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = RMA.cROUnitId 
WHERE RUA.AttributeName = 'START_DATE'
-- CREDIT_TO
UPDATE RMA SET CREDIT_TO = RUA.Value
FROM #RMADes RMA 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = RMA.cROUnitId 
WHERE RUA.AttributeName = 'CREDIT_TO'
-- MNA
UPDATE RMA SET MNA = RUA.Value
FROM #RMADes RMA 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = RMA.cROUnitId 
WHERE RUA.AttributeName = 'MNA'
-- ODM
UPDATE RMA SET ODM = RUA.Value
FROM #RMADes RMA 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = RMA.cROUnitId 
WHERE RUA.AttributeName = 'ODM'
-- FinalReason from WOStationAttr
UPDATE RMA SET FinalAction = ISNULL(WSA.[Value],RUA.[Value])
FROM #RMADes RMA
LEFT JOIN pls.CodeAttribute CAt ON  CAt.AttributeName = 'VMI_ACTION'
LEFT JOIN pls.ROUnitAttribute RUA on RUA.ROUnitID = RMA.cROUnitId 
                                            AND CAt.ID = RUA.AttributeID
LEFT JOIN pls.CodeAttribute CAA ON CAA.AttributeName = 'ACTION'
LEFT JOIN pls.[WOStationAttribute] WSA on WSA.WoStationHistoryId = RMA.cWSHId
                                               AND CAA.ID = WSA.AttributeID
-- FinalReason from WOStationAttr
UPDATE RMA SET FinalReason = ISNULL(WSA.[Value],RUA.[Value])
FROM #RMADes RMA
LEFT JOIN pls.CodeAttribute CAt ON  CAt.AttributeName = 'VMI_RES'
LEFT JOIN pls.ROUnitAttribute RUA on RUA.ROUnitID = RMA.cROUnitId 
                                            AND CAt.ID = RUA.AttributeID
LEFT JOIN pls.CodeAttribute CAA ON CAA.AttributeName = 'RES'
LEFT JOIN pls.[WOStationAttribute] WSA on WSA.WoStationHistoryId = RMA.cWSHId
                                               AND CAA.ID = WSA.AttributeID
SELECT * FROM #RMADes
DROP TABLE #RMADes                                                                           
DROP TABLE #ROUnitAttr
DROP TABLE #WOStationAttr
";
            }

            DataTable dt = oDAL.GetData(query);

            ///////////FILTER SRINGS/////////

            if (!string.IsNullOrEmpty(rptName))
                filterString += " > Report Type = '" + rptName + "'";


            if (!string.IsNullOrEmpty(ProgramName))
                filterString += " | Program = '" + ProgramName + "' ";

            if (isAllDate != true)
            {
                filterString += " | From = '" + CreateFDate + "' To = '" + CreateTDate + "' ";
            }

            if (isAllDate == true)
                filterString += " | Date Range = All ";

            //filterString += "| RO Create Date From ='" + CreateFDate + "' RO Create Date To = '" + CreateTDate + "' ";
            //filterString += " | From = '" + dockLogFrmDt + "' To = '" + dockLogToDt + "' ";
            if (!string.IsNullOrEmpty(boxNo))
                filterString += "| Box No. = '" + _boxNo + "' ";

            if (!string.IsNullOrEmpty(custRefer))
                filterString += "| Customer Ref. = '" + _custRefe + "' ";

            if (!string.IsNullOrEmpty(serialNo))
                filterString += "| Serial No. = '" + serialNo + "' ";





            //if (!string.IsNullOrEmpty(currentInventoryLocation))
            //    filterString += "> | PartSerial.PartLocationNo = '" + currentInventoryLocation + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("140", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRMADiscrepancy = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }

        private string GetInValue(string Value)
        {
            string[] arr = Value.Split(',');
            string _arr = null;
            foreach (var item in arr)
            {
                if (_arr == null)
                {
                    _arr = "\'" + item + "\'";
                }
                else
                {
                    _arr += "," + "\'" + item + "\'";
                }

            }
            return _arr;
        }

    }
}