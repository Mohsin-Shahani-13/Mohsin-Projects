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
    public class DriveDetails
    {
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Received From:")]
        public string _RecfromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string RecfromDt { get { return _RecfromDt; } set { _RecfromDt = value; } }
        [Display(Name = "Received To:")]
        public string _RectoDt = DateTime.Now.ToString(Format.DateOnly);
        public string RectoDt { get { return _RectoDt; } set { _RectoDt = value; } }
        public bool isAllDate { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Customer Ref.:")]
        public string custRef { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public List<Hashtable> lstDriveDetail { get; set; }
        public string ErrorMessage { get; set; }


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

        public bool GetList(string fromDt, string toDt, bool isAllDate, string programId, string programName, string SerialNo, string custReference)
        {

            string query = string.Empty;
            string _SerialNo = GetInValue(SerialNo);
            string _ReferenceNo = GetInValue(custReference);
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
           
                query = @"
                 
IF OBJECT_ID('tempdb.dbo.#DrvDtl') IS NULL
BEGIN
CREATE TABLE #DrvDtl
(
 ProgramName varchar(50),
 LocalRMANo varchar(50),
 Product varchar(50),
 FDCode varchar(50),
 Category varchar(50),
 RegionSite varchar(50) Default 'TELEPLAN',
 Customer varchar(200),
 FailDate date,
 FailWeek tinyint,
 FailMonth tinyint,
 CheckDate date,
 CustomerPartNo varchar(50) Default 'NA',
 TSBModel varchar(50),
 FactoryCode varchar(50) Default 'NA',
 CustomerCode varchar(50) Default 'NA',
 TSBSN varchar(50),
 SubTSB varchar(50) Default '',
 MCode varchar(50),
 COO varchar(50),
 PlatForms varchar(50) Default 'NA',
 FailStation varchar(50) Default 'NA',
 ClaimedFail varchar(50) Default 'NA',
 TSBJudg varchar(50),
 ErrorCode Nvarchar(200),
 DSDFResult Nvarchar(200),
 Res varchar(50),
 Actions varchar(50),
 Carton varchar(50) Default 'NA',
 RespPerson varchar(50) Default 'Xuan.Liang',
 RFCNo varchar(50),
 ShipDate date,
 Remark varchar(100),
 NGDrivesDate date,
 Operator varchar(50),
 PalletNo varchar(200),
 BoxNo varchar(50),
 SecondReturn varchar(50),
 cProgramId int,
 cROHeaderId int,
 cROUnitId int,
 cWOHeaderId int,
 cWSHId int,
 cWOEndDate date
)
END

INSERT INTO #DrvDtl (ProgramName,LocalRMANo, TSBSN, TSBModel, FailDate, ShipDate, Operator, PalletNo, BoxNo, cProgramId, cROHeaderId, cROUnitId, cWOHeaderId, cWSHId, cWOEndDate)

SELECT  
  P.Name AS ProgramName
, ROH.CustomerReference AS LocalRMANo
, ROU.SerialNo AS TSBSN
, PN.ModelNo AS TSBModel
, CASE WHEN WOH.StatusId <> 3 AND PS.WOEndDate IS NOT NULL THEN FORMAT(PS.WOEndDate, 'dd-MMM-yyyy') ELSE PSH.WOEndDate /* 2nd logic in update statement*/ END FailDate
, PS.SODate AS ShipDate
, USR.Username AS Operator
, PPBA.Value as PalletNo
, PS.PalletBoxNo AS BoxNo
, ROH.ProgramId
, ROH.Id AS ROHeaderId
, ROU.ID AS ROUnitId
, PS.WOHeaderId 
, MAX(WSH.ID) AS WSHId
, PS.WOEndDate
FROM pls.ROHeader roh
INNER JOIN pls.Program P ON P.ID = ROH.ProgramID
INNER JOIN pls.ROLine ROL ON ROL.ROHeaderId = roh.ID
INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID
INNER JOIN pls.PartNo PN ON PN.PartNo = ROL.PartNo
LEFT JOIN pls.PartSerial PS ON PS.ProgramId = roh.ProgramId AND PS.PartNo = ROL.PartNo AND PS.SerialNo = ROU.SerialNo
LEFT JOIN pls.PartSerialHistory PSH ON PS.ProgramId = roh.ProgramId AND PSH.PartNo = ROL.PartNo AND PSH.SerialNo = ROU.SerialNo
LEFT JOIN pls.PartLocation PL ON PL.ID = PS.LocationID
LEFT JOIN pls.WOHeader WOH ON WOH.Id = PS.WOHeaderId
LEFT JOIN pls.WOStationHistory WSH ON WSH.WOHeaderID = WOH.ID AND WSH.WorkStationId = WOH.WorkStationId
LEFT JOIN pls.vPartPalletBoxNoAttribute PPBA on PPBA.AttributeName = 'PalletNo' AND PPBA.CustomPalletBoxNo = PS.PalletBoxNo
LEFT JOIN pls.vPartPalletBoxNoAttribute PPBAH on PPBAH.AttributeName = 'PalletNo' AND PPBAH.CustomPalletBoxNo = PSH.PalletBoxNo
INNER JOIN pls.[User] USR ON USR.Id = roh.UserId
WHERE 
 ";

                if (programId != "0")
                {
                    query += " ROH.ProgramID = '" + programId + "' ";
                }
                else
                {
                    query += " ROH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }
                if (isAllDate != true)
                {
                    query += "AND CONVERT(Date, ROH.CreateDate) >= '<fromDt>' AND CONVERT(Date, ROH.CreateDate) <= '<toDt>'";
                }
                if (string.IsNullOrEmpty(SerialNo) && string.IsNullOrEmpty(custReference))
                {
                    query += " AND ROU.StatusID NOT IN (3, 7)";
                }

                if (!string.IsNullOrEmpty(SerialNo))
                    query += "AND ROU.SerialNo IN (" + _SerialNo + ")";

                if (!string.IsNullOrEmpty(custReference))
                    query += "AND ROH.CustomerReference IN (" + _ReferenceNo + ")";

                query = query.Replace("<fromDt>", fromDt);
                query = query.Replace("<toDt>", toDt);


                query += @"GROUP BY  P.Name 
,ROH.CustomerReference
, ROU.SerialNo
, WOH.StatusId
, PN.ModelNo
, PS.SODate
, USR.Username
, PL.LocationNo
, PPBA.Value
, PS.PalletBoxNo
, PSH.PalletBoxNo
, ROH.ProgramId
, ROH.Id
, ROU.ID
, PS.WOHeaderId
, PS.WOEndDate
, PSH.WOEndDate

--ShipDate logic
UPDATE DD SET ShipDate = CASE WHEN PSH.Sodate > pt.CreateDate THEN PSH.SODate ELSE null end
FROM #DrvDtl DD
INNER JOIN pls.PartSerialHistory PSH ON PSH.SerialNo = DD.TSBSN
INNER JOIN pls.PartTransaction PT ON PT.OrderHeaderID = DD.cROHeaderId AND PT.SerialNo = DD.TSBSN AND DD.cProgramId =PT.ProgramID

--Box NO logic
UPDATE DD SET BoxNo = CASE WHEN PSH.Sodate > pt.CreateDate THEN PSH.PalletBoxNo ELSE PS.PalletBoxNo end
FROM #DrvDtl DD
INNER JOIN pls.PartSerial PS ON PS.SerialNo = DD.TSBSN
INNER JOIN pls.PartSerialHistory PSH ON PSH.SerialNo = DD.TSBSN
INNER JOIN pls.PartTransaction PT ON PT.OrderHeaderID = DD.cROHeaderId AND PT.SerialNo = DD.TSBSN AND DD.cProgramId =PT.ProgramID

-- Fail Date 2nd logic
UPDATE DD SET FailDate = PT.ForDate
FROM #DrvDtl DD
INNER JOIN pls.PartTransaction PT ON PT.PartTransactionId = 1 AND PT.[Source] = 'DataEntry - PreAlert Receiving TOSHIBA' AND PT.SerialNo = DD.TSBSN
WHERE FailDate IS NULL

--Getting FailMonth and FailWeek
UPDATE DD SET FailMonth = Month(FailDate)
			, FailWeek = DATEPART(ISO_WEEK, FailDate)
FROM #DrvDtl DD
WHERE FailDate IS NOT NULL

--CheckDate same as FailDate (Need to make sure why)
UPDATE DD SET CheckDate = FailDate
FROM #DrvDtl DD
WHERE FailDate IS NOT NULL

--importing pls.ROUnitAttribute data for further use
SELECT  CA.AttributeName, RUA.ROUnitId, RUA.Value
INTO #ROUnitAttr
FROM pls.CodeAttribute CA
INNER JOIN pls.ROUnitAttribute RUA ON RUA.AttributeId = CA.ID
WHERE CA.AttributeName IN('FD_CODE', 'DRIVE_TYPE', 'MNA', 'ODM', 'MCODE', 'COO', 'VMI_RESULT', 'VMI_RES', 'VMI_ACTION', 'RFC_NO', 'RETURN_COUNT', 'VMI_RES')
AND RUA.ROUnitId IN(SELECT cROUnitId FROM #DrvDtl)

-- importing pls.WOStationAttribute data for further use
SELECT  CA.AttributeName, WSA.WOStationHistoryID, WSA.Value
INTO #WOStationAttr
FROM pls.CodeAttribute CA
INNER JOIN pls.WOStationAttribute WSA ON WSA.AttributeId = CA.ID
LEFT JOIN pls.WOStationHistory WSH ON WSH.ID = WSA.WOStationHistoryID
LEFT JOIN pls.PartSerialHistory PSH ON PSH.WOHeaderID = WSH.WOHeaderID
WHERE CA.AttributeName IN('NDF_RESULT', 'DNR_RESULT', 'TMDT_RESULT', 'RES', 'ACTION', 'REMARK')
AND WSA.WOStationHistoryID IN(SELECT cWSHId FROM #DrvDtl)


-- FDCode
UPDATE DD SET FDCode = RUA.Value
FROM #DrvDtl DD 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId 
WHERE RUA.AttributeName = 'FD_CODE'

-- Category
UPDATE DD SET Category = RUA.Value
FROM #DrvDtl DD 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId 
WHERE RUA.AttributeName = 'DRIVE_TYPE'

-- Mcode
UPDATE DD SET MCode = RUA.Value
FROM #DrvDtl DD 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId 
WHERE RUA.AttributeName = 'MCODE'
	
-- COO
UPDATE DD SET COO = RUA.Value
FROM #DrvDtl DD
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId
WHERE RUA.AttributeName = 'COO'

-- Second Return
UPDATE DD SET SecondReturn = RUA.Value
FROM #DrvDtl DD 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId 
WHERE RUA.AttributeName = 'RETURN_COUNT'

-- RFC No
UPDATE DD SET RFCNo = RUA.Value
FROM #DrvDtl DD 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId 
WHERE RUA.AttributeName = 'RFC_NO'


-- MNA + ODM concatenated values
UPDATE DD SET Customer =
(
    SELECT STRING_AGG(RUA.Value, '-')

    FROM #ROunitAttr RUA
	WHERE RUA.AttributeName IN('MNA', 'ODM')

          AND RUA.ROUnitId = DD.cROUnitId
)
FROM #DrvDtl DD 

-- RES from WOStationAttr
UPDATE DD SET RES = WSA.Value
FROM #DrvDtl DD 
INNER JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId
WHERE WSA.AttributeName = 'RES'

-- RES from ROUnitAttr if not in WOStationAttr
UPDATE DD SET RES = RUA.Value
FROM #DrvDtl DD 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitID = DD.cROUnitId
WHERE RUA.AttributeName = 'VMI_RES' AND DD.RES IS NULL


-- ACTION from WOStationAttr
UPDATE DD SET Actions = WSA.Value
FROM #DrvDtl DD 
INNER JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId
WHERE WSA.AttributeName = 'ACTION'

-- ACTION from ROUnitAttr if not in WOStationAttr
UPDATE DD SET Actions = RUA.Value
FROM #DrvDtl DD 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitID = DD.cROUnitId
WHERE RUA.AttributeName = 'VMI_ACTION' AND DD.Actions IS NULL


-- Remark
/*
WOStationAttribute		REMARK	    if exist (not blank)
WOStationAttribute		DNR_RESULT	else if exist (not blank)
WOStationAttribute		TMDT_RESULT	else if exist (not blank)
WOStationAttribute		NDF_RESULT	else if exist (not blank)
ROUnitAttribute		    VMI_RESULT	else if exist (not blank)
*/
--Update remark for 'REMARK'
UPDATE DD SET Remark =  Case when ShipDate is null then WSA.Value Else null END
FROM #DrvDtl DD 
INNER JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName IN ('REMARK')  

--Update remark for 'DNR_RESULT'
UPDATE DD SET Remark =  WSA.Value
FROM #DrvDtl DD 
INNER JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName IN ('DNR_RESULT')
AND (Remark IS NULL OR Remark = '')


--Update remark for 'VMI_RESULT'
UPDATE DD SET Remark =  RUA.Value
FROM #DrvDtl DD 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId AND RUA.AttributeName = 'VMI_RESULT'
AND (Remark IS NULL OR Remark = '')

--Update remark for 'TMDT_RESULT'
UPDATE DD SET Remark =  '' --WSA.Value
FROM #DrvDtl DD 
INNER JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName IN ('TMDT_RESULT')
AND (Remark <> 'RTV')
 
--Update remark for 'NDF_RESULT'
UPDATE DD SET Remark =  '' --WSA.Value
FROM #DrvDtl DD 
INNER JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName IN ('NDF_RESULT')
AND (Remark <> 'RTV')



-- NG Drives back to OEM(Date)
UPDATE DD SET NGDrivesDate = CASE WHEN WSA.Value = 'CID' OR WSA.Value = 'NTF' THEN FORMAT(DD.cWOEndDate, 'dd-MMM-yyyy') END
FROM #DrvDtl DD 
INNER JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId 
WHERE WSA.AttributeName IN('Action') 

--Getting Product
UPDATE DD SET Product = SubString(CIL.Item, CharIndex('-', CIL.Item) + 1, LEN(CIL.Item))
FROM #DrvDtl DD 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId 
INNER JOIN pls.CodeItemList CIL ON CIL.Name = 'FDCODE_LIST' AND SubString(CIL.Item, 0, CharIndex('-', CIL.Item)) = RUA.Value
WHERE RUA.AttributeName = 'FD_CODE'

UPDATE DD SET TSBJudg =  WSA.Value
						
FROM #DrvDtl DD 
LEFT JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName = 'NDF_RESULT'


-- TSB Jusgement
UPDATE DD SET TSBJudg = SubString(CIL.Item, CharIndex('-', CIL.Item) + 1, LEN(CIL.Item))
FROM #DrvDtl DD 
INNER JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId 
INNER JOIN pls.CodeItemList CIL ON CIL.Name = 'NDF_ERROR_CODE' AND SubString(CIL.Item, 0, CharIndex('-', CIL.Item)) = WSA.Value
WHERE WSA.AttributeName = 'NDF_RESULT'


UPDATE DD SET ErrorCode = DT.Chinese
FROM #DrvDtl DD 
INNER JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId 
INNER JOIN pls.vDataTranslation DT ON LTRIM(SubString(DT.English, CharIndex('-', DT.English)+1, LEN(DT.English))) = LTRIM(WSA.Value)
WHERE DT.English like 'DNR_TEST_CODE%' AND WSA.AttributeName = 'DNR_RESULT'

-- Error Code -condition 2 to 8
UPDATE DD SET ErrorCode = CASE WHEN WSA.Value = 'PASS' AND RUA.Value = 1 THEN 'TMDT-PASS'

                               WHEN WSA.Value = 'PASS' AND RUA.Value > 1 THEN 'TMDT-PASS(2nd/Rec)'

                               WHEN WSA.Value = 'FAIL' THEN 'TMDT-FAIL'

                          END
FROM #DrvDtl DD 
LEFT JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId AND RUA.AttributeName = 'RETURN_COUNT'
LEFT JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName = 'TMDT_RESULT'
WHERE ErrorCode IS NULL

UPDATE DD SET ErrorCode = CASE WHEN WSA.AttributeName = 'NDF_RESULT'  AND RUA.AttributeName = 'RETURN_COUNT' AND WSA.Value = '0000' AND RUA.Value > 1 THEN 'NDF-Test Code 0000 (2nd/Rec)'

                               WHEN WSA.AttributeName = 'NDF_RESULT'  AND RUA.AttributeName = 'RETURN_COUNT' AND WSA.Value = '0000' AND RUA.Value = 1 THEN 'NDF–0000'

                               WHEN WSA.AttributeName = 'NDF_RESULT'  AND RUA.AttributeName = 'RETURN_COUNT' AND WSA.Value <> '0000' THEN WSA.Value
                          END
FROM #DrvDtl DD 
LEFT JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId AND RUA.AttributeName = 'RETURN_COUNT'
LEFT JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName = 'NDF_RESULT'
WHERE ErrorCode IS NULL

--Error Code - condition 9						  ---MADE THIS CHANGE---
UPDATE DD SET ErrorCode = --RUA.Value
						CASE 
							  
							  WHEN RUA.AttributeName = 'VMI_RESULT' AND RUA.Value = 'PASS'  THEN 'VMI-PASS'
							  WHEN RUA.AttributeName = 'VMI_RESULT' AND RUA.Value <> 'PASS'  THEN CONCAT('VMI - Remark(', RUA.Value, ')')
                        END
						
FROM #DrvDtl DD
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId AND RUA.AttributeName = 'VMI_RESULT'
WHERE ErrorCode IS NULL
--Error Code - condition 9 Last
UPDATE DD SET ErrorCode = CASE 
							  WHEN RUA.AttributeName = 'VMI_RESULT'  AND RUA.Value = 'PASS' THEN 'VMI-PASS'
                              WHEN RUA.AttributeName = 'VMI_RESULT' AND RUA.Value <> 'PASS' THEN CONCAT('VMI - Remark(', DT.Chinese, ')')	  
                          END
FROM #DrvDtl DD
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId AND RUA.AttributeName = 'VMI_RESULT'
INNER JOIN pls.vDataTranslation DT ON LTRIM(SubString(DT.English, CharIndex('-', DT.English)+1, LEN(DT.English))) = LTRIM(RUA.Value)
AND DT.English like 'VMI_RESULT%' AND RUA.AttributeName = 'VMI_RESULT' AND RUA.Value <> 'PASS'

-- DS / DF RESULT
  /*
   DNR_RESULT	if exist (not blank) with value DNR_RECEIVE, display ‘N/A’
   DNR_RESULT	else if exist (not blank) with value not DNR_RECEIVE, display ‘REJECT’
  TMDT_RESULT	else if exist (not blank) with value PASS and ROUnitAttribute.AttributeName = RETURN_COUNT is > 1, display ‘N/A’
  TMDT_RESULT	else RESULT if exist (not blank) with value PASS and ROUnitAttribute.AttributeName = RETURN_COUNT is = 1, display ‘OK’
  TMDT_RESULT	else if exist (not blank) with value FAIL, display ‘N/A’
   NDF_RESULT	else if exist (not blank) with value 0000 and ROUnitAttribute.AttributeName = RETURN_COUNT is > 1, display ‘N/A’
   NDF_RESULT	else RESULT if exist (not blank) with value 0000 and ROUnitAttribute.AttributeName = RETURN_COUNT is = 1, display ‘OK’
   NDF_RESULT	else if exist (not blank) with value not 0000, display ‘N/A’
   VMI_RESULT	else if exist (not blank) with value PASS, display 'N/A'
   VMI_RESULT	else if exist (not blank) with value not PASS, display ‘REJECT’

  */

  UPDATE DD SET DSDFResult = CASE WHEN WSA.AttributeName = 'DNR_RESULT' AND WSA.Value = 'DNR_RECEIVE' THEN 'N/A'

                                WHEN WSA.AttributeName = 'DNR_RESULT' AND WSA.Value <> 'DNR_RECEIVE' THEN 'REJECT'

                           END
FROM #DrvDtl DD 
LEFT JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId AND RUA.AttributeName IN ('RETURN_COUNT')
LEFT JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName IN ('DNR_RESULT')

UPDATE DD SET DSDFResult = CASE WHEN WSA.AttributeName = 'TMDT_RESULT' AND WSA.Value = 'PASS' AND RUA.AttributeName = 'RETURN_COUNT' AND RUA.Value > 1 THEN 'N/A'

                                WHEN WSA.AttributeName = 'TMDT_RESULT' AND WSA.Value = 'PASS' AND RUA.AttributeName = 'RETURN_COUNT' AND RUA.Value = 1 THEN 'OK'

                                WHEN WSA.AttributeName = 'TMDT_RESULT' AND WSA.Value = 'FAIL' THEN 'N/A'

                           END
FROM #DrvDtl DD 
LEFT JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId AND RUA.AttributeName IN ('RETURN_COUNT')
LEFT JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName IN ('TMDT_RESULT')
WHERE DD.DsdfResult IS NULL

UPDATE DD SET DSDFResult = CASE   WHEN WSA.AttributeName = 'NDF_RESULT' AND WSA.Value = '0000' AND RUA.AttributeName = 'RETURN_COUNT' AND RUA.Value > 1 THEN 'N/A'

                                  WHEN WSA.AttributeName = 'NDF_RESULT' AND WSA.Value = '0000' AND RUA.AttributeName = 'RETURN_COUNT' AND RUA.Value = 1 THEN 'OK'

                                  WHEN WSA.AttributeName = 'NDF_RESULT' AND WSA.Value <> '0000' THEN 'N/A'

                           END
FROM #DrvDtl DD 
LEFT JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId AND RUA.AttributeName IN ('RETURN_COUNT')
LEFT JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName IN ('NDF_RESULT')
WHERE DD.DsdfResult IS NULL

UPDATE DD SET DSDFResult = CASE   WHEN RUA.AttributeName = 'VMI_RESULT' AND RUA.Value = 'PASS' THEN 'N/A'

                                  WHEN RUA.AttributeName = 'VMI_RESULT' AND RUA.Value <> 'PASS' THEN 'REJECT'

                           END
FROM #DrvDtl DD 
INNER JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId AND RUA.AttributeName IN ('VMI_RESULT')
WHERE DD.DsdfResult IS NULL


/*
UPDATE DD SET DSDFResult = CASE WHEN WSA.AttributeName = 'DNR_RESULT' AND WSA.Value = 'DNR_RECEIVE' THEN 'N/A'
							    WHEN WSA.AttributeName = 'DNR_RESULT' AND WSA.Value <> 'DNR_RECEIVE' THEN 'REJECT'
								WHEN WSA.AttributeName = 'TMDT_RESULT' AND WSA.Value = 'PASS' AND RUA.AttributeName = 'RETURN_COUNT' AND RUA.Value > 1 THEN 'N/A'
								WHEN WSA.AttributeName = 'TMDT_RESULT' AND WSA.Value = 'PASS' AND RUA.AttributeName = 'RETURN_COUNT' AND RUA.Value = 1 THEN 'OK'
								WHEN WSA.AttributeName = 'TMDT_RESULT' AND WSA.Value = 'FAIL' THEN 'N/A'
								WHEN WSA.AttributeName = 'NDF_RESULT' AND WSA.Value = '0000' AND RUA.AttributeName = 'RETURN_COUNT' AND RUA.Value > 1 THEN 'N/A'
								WHEN WSA.AttributeName = 'NDF_RESULT' AND WSA.Value = '0000' AND RUA.AttributeName = 'RETURN_COUNT' AND RUA.Value = 1 THEN 'OK'
								WHEN WSA.AttributeName = 'NDF_RESULT' AND WSA.Value <> '0000' THEN 'N/A'
								WHEN RUA.AttributeName = 'VMI_RESULT' AND RUA.Value = 'PASS' THEN 'N/A'
								WHEN RUA.AttributeName = 'VMI_RESULT' AND RUA.Value <> 'PASS' THEN 'REJECT'
						   END
FROM #DrvDtl DD 
LEFT JOIN #ROUnitAttr RUA ON RUA.ROUnitId = DD.cROUnitId AND RUA.AttributeName IN ('RETURN_COUNT', 'VMI_RESULT')
LEFT JOIN #WOStationAttr WSA ON WSA.WOStationHistoryID = DD.cWSHId AND WSA.AttributeName IN ('DNR_RESULT', 'TMDT_RESULT', 'NDF_RESULT')
*/

SELECT* FROM #DrvDtl

DROP TABLE #DrvDtl
DROP TABLE #ROUnitAttr
DROP TABLE #WOStationAttr

 ";
            
            

            DataTable dt = oDAL.GetData(query);

            ///////////FILTER SRINGS/////////

            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            if (isAllDate != true)
            {
                filterString += " | From = '" + fromDt + "' To = '" + toDt + "' ";
            }

            if (isAllDate == true)
                filterString += " | Date Range = All ";

            if (!string.IsNullOrEmpty(SerialNo))
                filterString += " | Serial No. = '" + _SerialNo + "'";

            if (!string.IsNullOrEmpty(custReference))
                filterString += " | Customer Ref. = '" + _ReferenceNo + "'";

            //filterString += " | Rec. From = '" + fromDt + "' To = '" + toDt + "'";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("143", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDriveDetail = cCommon.ConvertDtToHashTable(dt);
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