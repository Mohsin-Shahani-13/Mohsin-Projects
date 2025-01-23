using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class DailyPulse
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstDailyPulse { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList()
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"

WITH DAYTAT AS (
    SELECT pts.serialno, 
           ptr.CreateDate AS REC, 
           pts.CreateDate AS REP,
           DATEDIFF(DAY, ptr.CreateDate, pts.CreateDate) AS TAT,
           case when datediff(DAY,ptr.CreateDate,pts.CreateDate) <=5 then 1 else 0 end INTAT

    FROM pls.parttransaction pts
    JOIN pls.parttransaction ptr 
        ON ptr.serialno = pts.serialno 
        AND ptr.programid = pts.programid 
        AND ptr.parttransactionid = 1
    WHERE pts.ProgramID = 10064
      AND pts.PartTransactionID = 7
      AND pts.CreateDate >= DATEADD(DAY, DATEDIFF(DAY, 5, GETDATE()), 0)
)

select CONVERT(CHAR(10), GETDATE(), 101) 'Date', 'Docking Stations' 'Commodity',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = 10064 AND WO.StatusID IN (19, 28)) 'Current WIP Units',

'0' 'OCB Qty <= 5 Days',
'0' 'OCB Qty > 5 Days',

(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = 10064 AND WO.StatusID IN (19, 28)
and wo.CreateDate >= dateadd(day,datediff(day,5,GETDATE()),0)) 'Units <= 5 Days',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = 10064 AND WO.StatusID IN (19, 28)
and wo.CreateDate < dateadd(day,datediff(day,5,GETDATE()),0)) 'Units > 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
WHERE PS.StatusID =19
AND PS.ProgramID = '10064'  and ps.LastActivityDate >= dateadd(day,datediff(day,5,GETDATE()),0)) 'Workable WIP <= 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
WHERE PS.StatusID =19
AND PS.ProgramID = '10064'  and ps.LastActivityDate < dateadd(day,datediff(day,5,GETDATE()),0)) 'Workable WIP > 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'NPI Vendor' and ps.LastActivityDate >= dateadd(day,datediff(day,5,GETDATE()),0)) 'NPI Hold TRP <= 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'NPI Vendor' and ps.LastActivityDate < dateadd(day,datediff(day,5,GETDATE()),0)) 'NPI Hold TRP > 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'NPI DELL' and ps.LastActivityDate >= dateadd(day,datediff(day,5,GETDATE()),0)) 'NPI Hold Dell <= 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'NPI DELL' and ps.LastActivityDate < dateadd(day,datediff(day,5,GETDATE()),0)) 'NPI Hold Dell > 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'Awaiting Parts' and ps.LastActivityDate > dateadd(day,datediff(day,1,GETDATE()),0)) 'Material Hold in 1 day',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'Awaiting Parts' and ps.LastActivityDate between dateadd(day,datediff(day,1,GETDATE()),0) and dateadd(day,datediff(day,3,GETDATE()),0)) 'Material Hold in 2-3 days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'Awaiting Parts' and ps.LastActivityDate between dateadd(day,datediff(day,3,GETDATE()),0) and dateadd(day,datediff(day,5,GETDATE()),0)) 'Material Hold in 4-5 days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'Awaiting Parts' and ps.LastActivityDate < dateadd(day,datediff(day,5,GETDATE()),0)) 'Material Hold > 5 days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'Engineering Vendor' and ps.LastActivityDate >= dateadd(day,datediff(day,5,GETDATE()),0)) 'Engineering Hold TRP <= 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'Engineering Vendor' and ps.LastActivityDate < dateadd(day,datediff(day,5,GETDATE()),0)) 'Engineering Hold TRP > 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'Engineering DELL' and ps.LastActivityDate >= dateadd(day,datediff(day,5,GETDATE()),0)) 'Engineering Hold Dell <= 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'Engineering DELL' and ps.LastActivityDate < dateadd(day,datediff(day,5,GETDATE()),0)) 'Engineering Hold Dell > 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'OEM' and ps.LastActivityDate >= dateadd(day,datediff(day,5,GETDATE()),0)) 'OEM Hold <= 5 Days',
(SELECT
count(*) 
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
AND PS.ProgramID = '10064' and pt.reason = 'OEM' and ps.LastActivityDate < dateadd(day,datediff(day,5,GETDATE()),0)) 'OEM Hold > 5 Days',
0 'Unit Capacity',
(select count(*) from pls.PartTransaction pt where pt.ProgramID = '10064' and pt.PartTransactionID = '1' and pt.CreateDate >= dateadd(day,datediff(day,1,GETDATE()),0) and pt.CreateDate < dateadd(day,datediff(day,0,GETDATE()),0)) 'Volume Received',
(select count(*) from pls.PartTransaction pt where pt.ProgramID = '10064' and pt.PartTransactionID = '18' and pt.CreateDate >= dateadd(day,datediff(day,1,GETDATE()),0) and pt.CreateDate < dateadd(day,datediff(day,0,GETDATE()),0)) 'Volume Shipped',

(select SUM(INTAT) from DAYTAT) 'TAT Performance to 5 Days Qty',

(select count(*) from pls.woheader wo
join pls.WOStationHistory wsh1 on wsh1.WOHeaderID = wo.id and wsh1.WorkStationID = 21 and wsh1.ToWorkStationID = 22
join pls.WOStationHistory wsh2 on wsh2.WOHeaderID = wo.id and wsh2.WorkStationID = 22 and wsh2.ToWorkStationID = 19
join pls.WOStationHistory wsh3 on wsh3.WOHeaderID = wo.id and wsh3.WorkStationID = 19 and wsh3.ToWorkStationID = 20
WHERE WO.PROGRAMID = 10064 AND WO.StatusID =15
and wo.LastActivityDate >= dateadd(day,datediff(day,1,GETDATE()),0))   'NFF Qty',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = '10064' AND WO.StatusID = '15'
and wo.LastActivityDate >= dateadd(day,datediff(day,0,GETDATE()),0))  'Repaired Qty',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = '10064' AND WO.StatusID = '17'
and wo.LastActivityDate >= dateadd(day,datediff(day,0,GETDATE()),0))   'Scrapped Qty'  ";


            DataTable dt = oDAL.GetData(query);

            filterString += "> Program = DELL";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("215", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDailyPulse = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}