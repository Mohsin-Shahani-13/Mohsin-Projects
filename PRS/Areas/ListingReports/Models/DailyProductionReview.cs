using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.ListingReports.Models
{
    public class DailyProductionReview
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Workstation:")]
        public string Workstation { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable GetProgramBySite()
        {
            //oDAL = new cDAL("ACTIVE");
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
        public List<Hashtable> lstDailyProductionReview { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public DataTable Program() // onHand warehouse method
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetList(string frmDt, string toDt, string Workstation, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");

            string query = string.Empty;
            //DateTime toDate = Convert.ToDateTime(toDt);
            //toDate.ToString(Format.DateOnly);
            //DateTime aaj = toDate.AddDays(1);

            //string to_date = toDate.ToString(Format.DateOnly);

            if (ProgramName == "BOSE")
            {
                query = @"

-- Step 1: Existing Query with ROW_NUMBER
WITH CTE AS (
    SELECT WOH.ProgramID,
           WOSH.woheaderid,
           WOH.serialno,
           ra.value ORDER_TYPE,
           (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
            FROM pls.partserial PS
            WHERE PS.SerialNo = woh.SerialNo AND PS.ProgramID = woh.ProgramID AND ps.PartNo = woh.PartNo ) HAS_SN,
           CASE WHEN WOSH.IsPass = 1 THEN 'Y' ELSE 'N' END AS Ispass,
           WOSH.iteration,
           WOSH.createdate,
           WOSH.lastactivitydate,
           CASE
             WHEN wsd.code IS NULL THEN cws.description
             ELSE wsd.description
           END AS Workstation,
           CASE 
              WHEN CWSD.Code IS NULL THEN CWOS.Description 
              ELSE CWSD.Description 
           END AS ToWorkStation,
           (
            SELECT TOP 1 CASE WOH.RepairTypeID WHEN 42 THEN '1' WHEN 71 THEN '3' ELSE MAX(pna.Value) END
            FROM pls.woline wl 
            LEFT JOIN pls.PartNoAttribute pna ON WOH.ProgramID = pna.ProgramID and wl.ComponentPartNo = pna.PartNo and pna.AttributeID = 149
            WHERE wl.WOHeaderID = WOH.ID and wl.StatusID = 14
           ) as RepairLevel,
           usr.Username AS UserName,
           U.username AS Technician,
           CONVERT(DATE, WOSH.lastactivitydate) AS [Day],
           FORMAT(WOSH.lastactivitydate, 'hh:mm:ss tt') AS [Time],
           CASE
             WHEN ( CAST(WOSH.lastactivitydate AS TIME) >= '06:00:00' )
                  AND ( CAST(WOSH.lastactivitydate AS TIME) <= '15:30:00' ) THEN 1
             ELSE 2
           END AS [Shift],
           UPPER(woh.partno) AS Model,
           (SELECT TOP 1 value
            FROM   pls.partnoattribute
            WHERE  programid = WOH.programid
                   AND partno = woh.partno
                   AND attributeid = 278) AS Family,
           (SELECT TOP 1 value
            FROM   pls.partnoattribute
            WHERE  programid = WOH.programid
                   AND partno = woh.partno
                   AND attributeid = 279) AS Technology,
           DATEPART(hour, WOSH.lastactivitydate) AS [Hour],
           CONCAT(WOSH.woheaderid, WOH.serialno) AS [RMA/SN],
           CASE WHEN wsd.Code IS NULL THEN cws.ID ELSE wsd.CodeWorkStationId END AS wsId,
           WOU.ID AS wouId,
           CASE WHEN WSH.IsPass = 1 THEN 'PASS' ELSE 'FAIL' END As Rout,
           cf.id,
           CASE WHEN WOSH.IsPass = 0 THEN CF.Code+'-'+CF.Description ELSE NULL END AS FAULT,
           ROW_NUMBER() OVER (PARTITION BY WOH.serialno, WOSH.workstationid, WOSH.IsPass ORDER BY WOSH.lastactivitydate DESC) AS rn
    FROM   pls.woheader WOH
           INNER JOIN pls.wostationhistory WOSH ON WOH.id = WOSH.woheaderid 
           LEFT JOIN pls.codeworkstation CWS ON CWS.id = WOSH.workstationid AND cws.passfail = 1
           LEFT JOIN pls.codeworkstationcustomdescription wsd ON wsd.programid = WOH.programid AND wsd.repairtypeid = WOH.repairtypeid AND wsd.codeworkstationid = WOSH.workstationid
           LEFT JOIN pls.CodeWorkStation CWOS ON CWOS.ID = WOSH.toWorkStationID
           LEFT JOIN pls.CodeWorkStationCustomDescription CWSD ON CWSD.ProgramID = WOH.ProgramID AND CWSD.RepairTypeID = WOH.RepairTypeID AND CWSD.CodeWorkStationID = WOSH.ToWorkStationID
           INNER JOIN pls.[User] U ON U.ID = WOSH.UserID
           LEFT JOIN pls.WOStationHistory WSH ON WSH.WOHeaderID = WOSH.WOHeaderID AND WSH.ToWorkStationID = WOSH.WorkStationID AND wsh.LastActivityDate = WOSH.CreateDate
           INNER JOIN pls.WOLine WOL ON WOL.WOHeaderID = WOH.ID and wol.ComponentPartNo = woh.PartNo
           LEFT JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID
           LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = WOU.ID
           LEFT JOIN pls.CodeFault CF ON CF.ID = WUC.FaultID
           LEFT JOIN pls.[User] Usr ON Usr.ID = WSH.UserID
 LEFT OUTER JOIN pls.PartSerial PS ON PS.WOHeaderID = WOH.ID AND PS.PartNo = WOH.PartNo AND PS.SerialNo = WOH.SerialNo
		   LEFT join pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID 
		   left outer join pls.ROHeaderAttribute RA on roh.id = ra.roheaderid and ra.attributeid in (select a.id from pls.CodeAttribute a where a.attributename = 'PROCESS_TYPE')
    WHERE CONVERT(Date, WOSH.lastactivitydate) >= '<frmDt>' AND CONVERT(Date, WOSH.lastactivitydate) <= '<toDt>'
       AND WOSH.ispass IS NOT NULL
 ";
                if (!string.IsNullOrEmpty(Workstation))
                    query += "AND wsd.Description LIKE '%" + Workstation + "%' ";

                if (programId != "0")
                {
                    query += "AND WOH.programid = '" + programId + "' ";
                }
                else
                {
                    query += "AND WOH.programid IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }
                query += @")
-- Step 2: Create the Temporary Table and Insert Data
SELECT
    CTE.wouId AS WOUnitId,
    CTE.wsId AS WcId,
    CTE.Rout AS RoutResult,
    CTE.*,
    CTE.Ispass as Pass,
    CAST(NULL AS VARCHAR(2000)) AS FCM1,
    CAST(NULL AS VARCHAR(2000)) AS FCM2,
    CAST(NULL AS VARCHAR(2000)) AS FCM3,
    CAST(NULL AS VARCHAR(2000)) AS FCM4,
    CAST(NULL AS VARCHAR(2000)) AS FCM5
INTO #Yield
FROM CTE
WHERE CTE.rn = 1 OR CTE.IsPass = 'Y'; -- Keep only the first occurrence for IsPass = 'N' and all for IsPass = 'Y'

-- Step 3: Declare Variables and Loop to Update Columns
DECLARE @COUNT AS INT = 1;
DECLARE @QUERY AS VARCHAR(1000);

WHILE @COUNT <= 5
BEGIN
    -- Construct the dynamic SQL query to update the FCM columns
    SET @QUERY = '
    UPDATE MSO
    SET FCM' + CAST(@COUNT AS VARCHAR) + ' = (
        SELECT TOP 1 CASE WHEN MSO.IsPass =''N'' THEN CF.Code +'' - ''+ CF.Description ELSE null END
        FROM (
            SELECT WUC.FaultId, ROW_NUMBER() OVER (ORDER BY WUC.Id) AS rowNo
            FROM pls.WOUnitCodes WUC
            WHERE WUC.WOUnitId = MSO.WOUnitId
        ) FCM
        INNER JOIN pls.CodeFault CF ON CF.ID = FaultId
        WHERE rowNo = ' + CAST(@COUNT AS VARCHAR) + '
    )
    FROM #Yield MSO
    WHERE MSO.Pass = ''N'';
    ';
    EXEC(@QUERY);
    
    -- Increment the counter
    SET @COUNT = @COUNT + 1;
END;

-- Optional: Select data from the temporary table to see the results
SELECT * FROM #Yield
ORDER BY #Yield.lastactivitydate DESC;

-- Cleanup: Drop the temporary table if no longer needed
DROP TABLE #Yield; ";
            }
            else
            {
                query = @"

SELECT WOH.ProgramID,
       WOSH.woheaderid,
       WOH.serialno,
       (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
	   FROM pls.partserial PS
	   WHERE PS.SerialNo = woh.SerialNo AND PS.ProgramID = woh.ProgramID AND ps.PartNo = woh.PartNo ) HAS_SN,
       CASE WHEN WOSH.IsPass = 1 THEN 'Y' ELSE 'N' END AS Ispass,
       WOSH.iteration,
       WOSH.createdate,
       WOSH.lastactivitydate,
       CASE
         WHEN wsd.code IS NULL THEN cws.description
         ELSE wsd.description
       END AS Workstation,
       CASE 
          WHEN CWSD.Code IS NULL THEN CWOS.Description 
	      ELSE CWSD.Description 
	   END AS ToWorkStation,
        (
	    select CASE WOH.RepairTypeID WHEN 42 THEN '1' WHEN 71 THEN '3' ELSE MAX(pna.Value) END
        from pls.woline wl 
        LEFT JOIN pls.PartNoAttribute pna ON WOH.ProgramID = pna.ProgramID and wl.ComponentPartNo = pna.PartNo and pna.AttributeID = 149
        where wl.WOHeaderID = WOH.ID and wl.StatusID = 14
	   ) as RepairLevel,
       usr.Username AS UserName,
       U.username AS Technician,
       CONVERT(DATE, WOSH.lastactivitydate) AS [Day],
       Format(WOSH.lastactivitydate, 'hh:mm:ss tt') AS [Time],
       CASE
         WHEN ( Cast(WOSH.lastactivitydate AS TIME) >= '06:00:00' )
              AND ( Cast(WOSH.lastactivitydate AS TIME) <= '15:30:00' ) THEN 1
         ELSE 2
       END AS [Shift],
       Upper(woh.partno) AS Model,
       (SELECT value
        FROM   pls.partnoattribute
        WHERE  programid = WOH.programid
               AND partno = woh.partno
               AND attributeid = 278) AS Family,
       (SELECT value
        FROM   pls.partnoattribute
        WHERE  programid = WOH.programid
               AND partno = woh.partno
               AND attributeid = 279) AS Technology,
       Datepart(hour, WOSH.lastactivitydate) AS [Hour],
       Concat(WOSH.woheaderid, WOH.serialno) AS [RMA/SN]
FROM   pls.woheader WOH
       INNER JOIN pls.wostationhistory WOSH
               ON WOH.id = WOSH.woheaderid 
       LEFT JOIN pls.codeworkstation CWS
              ON CWS.id = WOSH.workstationid
                 AND cws.passfail = 1
       LEFT JOIN pls.codeworkstationcustomdescription wsd
              ON wsd.programid = WOH.programid
                 AND wsd.repairtypeid = WOH.repairtypeid
                 AND wsd.codeworkstationid = WOSH.workstationid

        LEFT JOIN pls.CodeWorkStation CWOS ON CWOS.ID = WOSH.toWorkStationID
	   LEFT JOIN pls.CodeWorkStationCustomDescription CWSD ON CWSD.ProgramID = WOH.ProgramID
				AND CWSD.RepairTypeID = WOH.RepairTypeID
				AND CWSD.CodeWorkStationID = WOSH.ToWorkStationID         


       INNER JOIN pls.[User] U ON U.ID = WOSH.UserID

       LEFT JOIN pls.WOStationHistory WSH ON WSH.WOHeaderID = WOSH.WOHeaderID 
	             AND WSH.ToWorkStationID = WOSH.WorkStationID  
                 AND wsh.LastActivityDate = WOSH.CreateDate 
	   LEFT JOIN pls.[User] Usr ON Usr.ID = WSH.UserID

      

WHERE CONVERT(Date, WOSH.lastactivitydate) >= '<frmDt>' AND CONVERT(Date, WOSH.lastactivitydate) <= '<toDt>'
       AND WOSH.ispass IS NOT NULL
 ";
                if (!string.IsNullOrEmpty(Workstation))
                    query += "AND wsd.Description LIKE '%" + Workstation + "%' ";

                if (programId != "0")
                {
                    query += "AND WOH.programid = '" + programId + "' ";
                }
                else
                {
                    query += "AND WOH.programid IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }

                query += "ORDER BY WOSH.lastactivitydate DESC";
            }



            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            if (!string.IsNullOrEmpty(Workstation))
                filterString += " | Workstation Like '" + Workstation + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("106", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDailyProductionReview = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}