using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class WOWipReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public bool isAllDate { get; set; }
        public string WorkOrderType { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
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
        public List<Hashtable> lstWOWipReport { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, bool isAllDate, string WorkOrderType, string WorkOrderTypeText, string programId, string ProgramName)
        {
            cDAL oDAL1 = new cDAL("ACTIVE");
            string ipAddress = HttpContext.Current.Session["RemoteAddr"].ToString();
            //string deleteQuery = @"DELETE FROM PlusRS.rpt.WOWipValve WHERE IpAddress = '" + ipAddress + "'";
            //oDAL1.Execute(deleteQuery);


            if (WorkOrderTypeText == "All")
            {
                WorkOrderTypeText = null;
            }
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
IF OBJECT_ID('tempdb.dbo.#WOWip') IS NULL
BEGIN
CREATE TABLE #WOWip
(
 ProgramId int,
 WOHeaderId int,
 SerialNo varchar(50),
 PartNo varchar(50),
 PrevWS varchar(50),
 CrntWS varchar(50),
 WorkOrderType varchar(50),
 StatusId smallint,
 ReturnOrderNo int,
 ROCustRef varchar(50),
 ReturnOrderType varchar(50),
 ReturnReason varchar(MAX),
 HoldReason varchar(MAX),
 IncomingPartNo varchar(50),
 InWarranty varchar(50),
 CreatedBy varchar(50),
 CreatedOn smalldatetime,
 LastActivityOn smalldatetime,
 TAT smallint,
 RepeatReturn char(1) DEFAULT 'N',
 ReturnCount tinyint,
 RepeatReturnTAT smallint,
 ROHeaderId int,
 ROUnitId int,
 MaxRcvdOn smalldatetime,
 MaxShippedOn smalldatetime,
 PrevShippedOn smalldatetime,
 RepairTypeId smallint
)
END

INSERT INTO #WOWip (ProgramId, WOHeaderId, SerialNo, PartNo, PrevWS, CrntWS, WorkOrderType, StatusId, ReturnOrderNo, ROCustRef, ReturnOrderType, ROHeaderId, ROUnitId, TAT, CreatedBy, CreatedOn, LastActivityOn, RepairTypeId)
SELECT ps.ProgramId
, ps.WOHeaderID
, woh.SerialNo
, PS.PartNo 
, woh.WorkStationIDPrevious AS PrevWS
, woh.WorkStationID AS CrntWS
, crt.Description AS OrderType 
, woh.StatusId
, roh.Id AS ReturnOrderNo
, roh.CustomerReference AS ROCustRef
, cod.Description AS ReturnOrderType
, ps.ROHeaderID
, rou.ID AS ROUnitId
, Abs(DateDiff(Day, WOH.LastActivityDate, WOH.CreateDate)) AS TAT
, u.Username
, woh.CreateDate
, woh.LastActivityDate
, woh.RepairTypeId
FROM pls.WOHeader woh
INNER JOIN pls.PartSerial ps  ON ps.ProgramId = woh.ProgramId and ps.WOHeaderID = WOH.ID AND ps.PartNo = woh.PartNo AND ps.SerialNo = woh.SerialNo
INNER JOIN pls.ROHeader roh ON roh.ID = ps.ROHeaderID

INNER JOIN pls.ROLine rol ON rol.ROHeaderID = roh.ID AND rol.PartNo = PS.PartNo AND rol.StatusID <> 3

INNER JOIN pls.ROUnit rou ON rou.ROLineID = rol.ID AND rou.SerialNo = woh.SerialNo
INNER JOIN pls.CodeRepairType crt ON crt.ID = woh.RepairTypeID
INNER JOIN pls.CodeOrderType cod ON cod.ID = roh.OrderTypeID
INNER JOIN pls.[User] U ON U.ID = woh.UserID
WHERE ";
            //INNER JOIN pls.ROLine rol ON rol.ROHeaderID = roh.ID AND rol.PartNo = PS.PartNo  --line 126 removed. line 127 added
            if (programId != "0")
            {
                query += "WOH.ProgramId = '" + programId + "' ";
            }
            else
            {
                query += "WOH.ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (isAllDate != true)
            {
                query += "AND CONVERT(Date, WOH.CreateDate) >= '<frmDt>' AND CONVERT(Date, WOH.CreateDate) <= '<toDt>' ";
            }
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            //query = query.Replace("<IpAddress>", ipAddress);

            query += "AND WOH.StatusID IN (28,19) ";

            if (!string.IsNullOrEmpty(WorkOrderTypeText))
                query += "AND crt.Description LIKE '%" + WorkOrderTypeText + "%' ";

            query += @"UPDATE wip SET wip.PrevWS = CASE WHEN cwsd.Code IS NULL THEN cws.Description ELSE cwsd.Description END
FROM #WOWip wip 
INNER JOIN pls.CodeWorkStation cws ON cws.Id = wip.PrevWS
LEFT JOIN pls.CodeWorkStationCustomDescription cwsd ON cwsd.ProgramId = wip.ProgramId AND cwsd.RepairTypeId = wip.RepairTypeId AND cwsd.CodeWorkStationId = cws.Id

-- update current work station
UPDATE wip SET wip.CrntWS = CASE WHEN cwsd.Code IS NULL THEN cws.Description ELSE cwsd.Description END
FROM #WOWip wip 
INNER JOIN pls.CodeWorkStation cws ON cws.Id = wip.CrntWS
LEFT JOIN pls.CodeWorkStationCustomDescription cwsd ON cwsd.ProgramId = wip.ProgramId AND cwsd.RepairTypeId = wip.RepairTypeId AND cwsd.CodeWorkStationId = cws.Id

-- Return Reason = pls.ROUnitAttribute.RepairNotes if null then get pls.ROHeaderAttribute.ReturnReason
UPDATE wip SET wip.ReturnReason = ISNULL(roua.Value, roha.Value)
FROM #WOWip wip 
LEFT JOIN pls.CodeAttribute caRepair ON caRepair.AttributeName = 'REPAIRNOTES'
LEFT JOIN pls.ROUnitAttribute roua ON roua.ROUnitID = wip.ROUnitId AND roua.AttributeID = caRepair.ID
LEFT JOIN pls.CodeAttribute ca ON ca.AttributeName = 'ReturnReason'
LEFT JOIN pls.ROHeaderAttribute roha ON roha.ROHeaderID = wip.ROHeaderId AND roha.AttributeID = ca.ID

-- PartNo = WO Part number(incoming partnumber attribute of RO if unit not RE-ID)
UPDATE wip SET IncomingPartNo = roua.Value
FROM #WOWip wip 
INNER JOIN pls.CodeAttribute ca ON ca.AttributeName = 'INCOMINGPARTNO'
INNER JOIN pls.ROUnitAttribute roua ON roua.ROUnitID = wip.ROUnitId AND roua.AttributeID = ca.ID

-- InWarranty attribute
UPDATE wip SET wip.InWarranty = roua.Value
FROM #WOWip wip 
INNER JOIN pls.CodeAttribute ca ON ca.AttributeName = 'INWARRANTY'
INNER JOIN pls.ROUnitAttribute roua ON roua.ROUnitID = wip.ROUnitId AND roua.AttributeID = ca.ID

-- HoldReason - when a unit is on hold in woheader
UPDATE wip SET wip.HoldReason =
(
    SELECT TOP 1 pt.Reason
    FROM pls.PartTransaction pt

    WHERE pt.ProgramId = wip.ProgramId AND pt.PartTransactionId = 12  AND pt.SerialNo = wip.SerialNo

    ORDER BY ID DESC
)
FROM #WOWip wip
WHERE wip.StatusId = 28

-- ReturnCount = How many times a unit is return after shipped
UPDATE wip SET ReturnCount =
   (
      SELECT COUNT(id) - 1
       FROM pls.PartTransaction PT
       WHERE  PT.ProgramId = wip.ProgramID  
             AND  PT.PartTransactionID = 1
           AND  PT.SerialNo = wip.SerialNo
    )
FROM #WOWip wip 

-- Repeat Return = Y if return count > 0
UPDATE #WOWip 
SET RepeatReturn = 'Y'
WHERE ReturnCount > 0

-- Get latest Recieved Date
UPDATE wip SET MaxRcvdOn =
(
    SELECT MAX(CreateDate)
    FROM pls.PartTransaction pt
    WHERE pt.ProgramId = wip.ProgramId AND PartTransactionId = 1
          AND SerialNo = wip.SerialNo
)
FROM #WOWip wip
WHERE wip.RepeatReturn = 'Y'

-- Get latest Shipped Date
UPDATE wip SET MaxShippedOn =
(
    SELECT MAX(CreateDate)
    FROM pls.PartTransaction pt
    WHERE pt.ProgramId = wip.ProgramId AND PartTransactionId = 18
          AND SerialNo = wip.SerialNo
)
FROM #WOWip wip
WHERE wip.RepeatReturn = 'Y'

-- Get previous shipped date
UPDATE wip SET PrevShippedOn =
(
    SELECT ForDate
    FROM pls.PartTransaction pt
    WHERE pt.ProgramId = wip.ProgramId AND PartTransactionId = 18
          AND SerialNo = wip.SerialNo
    ORDER BY ID DESC
    OFFSET 1 ROW
    FETCH FIRST 1 ROW ONLY
)
FROM #WOWip wip
WHERE wip.RepeatReturn = 'Y'

--Calculating Repeat Turn Arround When unit didn't return back after last shipped while returned back after previous shipped:)
UPDATE wip SET RepeatReturnTAT = DATEDIFF(DAY, PrevShippedOn, MaxRcvdOn)
FROM #WOWip wip 
WHERE wip.RepeatReturn = 'Y'
      AND MaxShippedOn >= MaxRcvdOn-- It means unit didn't return back after last shipped

--Calculating Repeat Turn Arround When unit returned back after last shipped
UPDATE wip SET RepeatReturnTAT = DATEDIFF(DAY, MaxShippedOn, MaxRcvdOn)
FROM #WOWip wip 
WHERE wip.RepeatReturn = 'Y'
      AND MaxRcvdOn >= MaxShippedOn-- It means unit return back after last shipped

SELECT* FROM #WOWip 
DROP TABLE #WOWip  ";
    //        query += @"AND ROH.ID = (SELECT  MAX(ROH.ID)
                //FROM pls.Roheader ROH
                //INNER JOIN pls.ROLine ROL ON ROL.ROHeaderID = ROH.ID
                //INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.id
                //INNER JOIN Pls.PartSerial PSe ON PSe.ROHeaderID = Roh.id
                //WHERE PS.SerialNo = ROU.SerialNo
                //)";

            //        if (!string.IsNullOrEmpty(WorkOrderTypeText))
            //            query += "AND CRT.Description LIKE '%" + WorkOrderTypeText + "%' ";


            //if (WorkOrderType == "B2C")
            //{
            //    query += "AND CRT.Description LIKE '%" + WorkOrderType + "%' ";
            //}

            //if (WorkOrderType == "B2B")
            //{
            //    query += "AND CRT.Description LIKE '%" + WorkOrderType + "%' ";
            //}

           

            DataTable dt = oDAL1.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";
            if (isAllDate != true)
            {
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            }
            if (!string.IsNullOrEmpty(WorkOrderTypeText))
                filterString += " | Work Order Type = '" + WorkOrderTypeText + "' ";
            if (WorkOrderTypeText == null)
            {
                filterString += " | Work Order Type = 'All' ";
            }


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("144", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWOWipReport = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}