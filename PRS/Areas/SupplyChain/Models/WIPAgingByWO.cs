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
    public class WIPAgingByWO
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstWipAgingByWO { get; set; }
        public List<Hashtable> lstWipAgingByWOUnit { get; set; }
        // public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"

-- Delete records for current user
DELETE FROM PlusRS.rpt.WIPAgingByOrder WHERE UserId = '@USER_ID'

-- Add data for open work orders only of current user
INSERT INTO PlusRS.rpt.WIPAgingByOrder
SELECT woh.ProgramId
	  , P.Name AS Program
	  , WOH.ID
	  , WOH.CustomerReference
	  , WOH.PartNo
	  , WOH.SerialNo
	  , CRT.Description As RepairType
	  , CASE WHEN wsd.Code IS NULL THEN cws.ID ELSE wsd.ID END AS WorkstationID
	  , CASE WHEN wsd.Code IS NULL THEN cws.Description ELSE wsd.Description END As Workstation
	  , CS.Description AS Status 
	  , DATEDIFF(DAY, WOH.CreateDate, GETDATE()) AS Aging
	  , U.Username AS CreatedBy
	  , WOH.CreateDate AS CreatedOn
	  , WOH.LastActivityDate AS LastActivityOn
	  , '@USER_ID'
FROM pls.WOHeader WOH
INNER JOIN pls.[User] U ON U.ID = WOH.UserID
INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
LEFT OUTER JOIN pls.CodeRepairType CRT ON CRT.ID = WOH.RepairTypeID
LEFT OUTER JOIN pls.CodeWorkStation CWS ON CWS.ID = WOH.WorkstationID
LEFT JOIN pls.CodeWorkStationCustomDescription wsd ON wsd.ProgramID = WOH.ProgramID
AND wsd.RepairTypeID = WOH.RepairTypeID
AND wsd.CodeWorkStationID = WOH.WorkStationID
LEFT OUTER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID
WHERE WOH.StatusID IN (19,28) --AND DATEDIFF(DAY, WOH.CreateDate, GETDATE()) > 0
      AND wOH.@PROGRAM_CLAUSE  

-- Start Pivot logic here

SELECT Workstation,[NoAging], [1Day],[2Days],[3Days],[4Days],[5Days],[6-9Days],[10-15Days],[16-30Days],[>30Days],
ISNULL([NoAging],0) + ISNULL([1Day],0) + ISNULL([2Days],0)+ ISNULL([3Days],0)+ISNULL([4Days],0)+ISNULL([5Days],0)+
            ISNULL([6-9Days],0)+ISNULL([10-15Days],0)+ISNULL([16-30Days],0)++ISNULL([>30Days],0) Total FROM
(
SELECT Distinct Workstation
, OpenOrders
, CASE
WHEN WipDays = 0 THEN 'NoAging'
WHEN WipDays = 1 THEN '1Day'
WHEN WipDays = 2 THEN '2Days'
WHEN WipDays = 3 THEN '3Days'
WHEN WipDays = 4 THEN '4Days'
WHEN WipDays = 5 THEN '5Days'
WHEN WipDays BETWEEN '6' AND '9' THEN '6-9Days'
WHEN WipDays BETWEEN '10' AND '15' THEN '10-15Days'
WHEN WipDays BETWEEN '16' AND '30' THEN '16-30Days'
WHEN WipDays > 30 THEN '>30Days'
END AS WipColumns

FROM
(
SELECT
    CASE WHEN wp.status = 'Hold' THEN wp.Status ELSE wp.Workstation END WorkStation
, wp.OrderId as OpenOrders
, DATEDIFF(DAY, wp.CreatedOn,GETDATE()) AS WipDays
FROM  plusrs.rpt.WIPAgingByOrder wp
WHERE UserId = '@USER_ID' AND wp.@PROGRAM_CLAUSE



) WIP_DATA ) X
Pivot
(
COUNT(OpenOrders)
FOR
WipColumns IN (NoAging,[1Day],[2Days],[3Days],[4Days],[5Days],[6-9Days],[10-15Days],[16-30Days],[>30Days])) as PivotTable
ORDER BY CASE WHEN Workstation = 'Hold' THEN 1 ELSE 0 END 
";
            query = query.Replace("@USER_ID", HttpContext.Current.Session["LogonUser"].ToString());

            if (programId != "0")
                query = query.Replace("@PROGRAM_CLAUSE", "ProgramId = '" + programId + "'");
            else
                query = query.Replace("@PROGRAM_CLAUSE", "ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ")");

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString = "> Program = '" + ProgramName + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("102", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWipAgingByWO = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool GetWOUnit(string workStation, string frmDt, string toDt, string programId)
        {
            string query = string.Empty;
            query = @"
SELECT ProgramId
       ,Program
       ,orderId
       ,DATEDIFF(DAY, CreatedOn,GETDATE()) as Days
	   ,orderNum As Customerreference 
	   ,PartNo
	   ,SerialNo
	   ,RepairType
	   ,WorkstationID
	   ,Workstation
	   ,Status
	   ,CreatedBy
	   ,CreatedOn
	   ,Aging
	   ,LastActivityOn
       ,UserId
FROM PLUSRS.RPT.WIPAgingByOrder 
WHERE UserId = '@USER_ID'";

            if (programId != "0")
            {
                query += " AND ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            //else
            //{
            //    query += " AND ProgramID = '" + programId + "' ";
            //}

            if (workStation == "null" && frmDt != null && toDt != null)
            {
                query += " AND DATEDIFF(DAY, CreatedOn,GETDATE()) between '<frmDt>' AND '<toDt>' ";

            }
            else if (workStation == "HOLD" && frmDt != null && toDt != null)
            {
                query += " AND Status = '<workStation>' AND DATEDIFF(DAY, CreatedOn,GETDATE()) between '<frmDt>' AND '<toDt>' ";
            }

            else if (workStation != "HOLD" && frmDt != null && toDt != null)
            {
                query += " AND Workstation = '<workStation>' AND DATEDIFF(DAY, CreatedOn,GETDATE()) between '<frmDt>' AND '<toDt>' AND Status = 'WIP' ";
            }

            query = query.Replace("<workStation>", workStation);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            query = query.Replace("@USER_ID", HttpContext.Current.Session["LogonUser"].ToString());

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("102-1", query, "---Work Orders WIP---", false);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstWipAgingByWOUnit = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}