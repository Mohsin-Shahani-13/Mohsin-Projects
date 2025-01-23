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
    public class GoProInventoryWeb
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
       
        [Display(Name = "Program:")]
        public string program { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("Active");
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
        public List<Hashtable> lstGoProInventoryWeb { get; set; }
        #endregion
        #region Methods 
        public bool GetList(string programId, string programName)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
		DECLARE @ProgramID SMALLINT = '<ProgramId>'  
DECLARE @ProgramDateTime DATETIME  = (SELECT IIF(p.TimeZone != '', (SELECT CONVERT(DATETIME, GETUTCDATE() AT TIME ZONE 'UTC' AT TIME ZONE p.TimeZone, 0)), GETDATE())   FROM pls.Program p WHERE p.ID = @ProgramID)
SET @ProgramDateTime = CONVERT(DATE,@ProgramDateTime)
SELECT 
	@ProgramDateTime as SnapshotDate,
	@ProgramID as ProgramID,
	pn.PartNo, 
	replace(pn.[Description],',','') AS PartDescription, 
	cpt.[Description] AS PartTypeDescription, 
	pl.LocationNo, 
	CASE 
		WHEN pl.Warehouse LIKE 'SCR%' OR pl.Warehouse LIKE 'SCRAP%' THEN 'SCR' 
		WHEN pl.Warehouse LIKE 'RIN%' THEN 'RIN'  
		WHEN pl.Warehouse LIKE 'MRB%' THEN 'MRB'  
		WHEN pl.Warehouse LIKE 'BFG%' THEN 'BFG'  
		WHEN pl.Warehouse LIKE 'TST%' THEN 'TST'  
		WHEN pl.Warehouse LIKE 'TDN%' THEN 'TDN' 
		WHEN pl.Warehouse LIKE 'WIP%' OR pl.Warehouse LIKE 'FLOORSTOCK%' OR pl.Warehouse LIKE 'ISSUE%' OR pl.Warehouse LIKE 'FGI%' THEN 'WIP' 
		ELSE pl.LocationNo END AS GoProLocation, 
	1 AS Quantity, 
	ps.SerialNo 
FROM pls.vPartSerial ps (NOLOCK)
	LEFT JOIN pls.vPartTransaction pt (NOLOCK) ON pt.ProgramID = ps.ProgramID 
		AND pt.PartNo = ps.PartNo 
		AND pt.SerialNo = ps.SerialNo 
		AND ps.StatusDescription = 'CONSUMED' 
		AND pt.PartTransaction = 'WO-CONSUMECOMPONENTS' 
	INNER JOIN pls.PartNo pn (NOLOCK) ON pn.PartNo = ps.PartNo 
	INNER JOIN pls.CodePartType cpt (NOLOCK) ON cpt.ID = pn.PartTypeID 
	INNER JOIN pls.PartLocation pl (NOLOCK) ON pl.ProgramID = ps.ProgramID 
		AND pl.LocationNo = ps.PartLocationNo 
WHERE ps.ProgramID = '<ProgramId>' 
	AND ps.StatusDescription NOT IN ('SHIPPED', 'UNRECEIVED', 'REID') 
	AND ps.StatusDescription NOT IN ('WIP', 'RESERVED') 
	AND pt.ID IS NULL 
UNION ALL
SELECT
	@ProgramDateTime as SnapshotDate,
	@ProgramID as ProgramID,
	pn.PartNo, 
	replace(pn.[Description],',','') AS PartDescription, 
	cpt.[Description] AS PartTypeDescription, 
	ps.WorkstationDescription, 
	CASE 
		WHEN ps.WorkstationDescription = 'gTask0' THEN 'MRB' 
		WHEN ps.WorkstationDescription IN ('gTest0', 'Cosmetic') THEN 'TDN' 
		WHEN ps.WorkstationDescription = 'Scrap' THEN 'SCR' 
		WHEN ps.WorkstationDescription IN ('Inspection', 'Refurbish', 'Audit') OR ps.WorkstationDescription LIKE 'gTest%' THEN 'TST' 
		WHEN ps.WorkstationDescription = 'Kitting' THEN 'WIP' 
		ELSE ps.PartLocationNo END AS GoProLocation, 
	1 AS Quantity, 
	ps.SerialNo 
FROM pls.vPartSerial ps (NOLOCK)
	INNER JOIN pls.PartNo pn (NOLOCK) ON pn.PartNo = ps.PartNo 
	INNER JOIN pls.CodePartType cpt (NOLOCK) ON cpt.ID = pn.PartTypeID 
	INNER JOIN pls.PartLocation pl (NOLOCK) ON pl.ProgramID = ps.ProgramID 
		AND pl.LocationNo = ps.PartLocationNo 
WHERE ps.ProgramID = '<ProgramId>' 
	AND ps.StatusDescription IN ('WIP') 
UNION ALL
SELECT 
	@ProgramDateTime as SnapshotDate,
	@ProgramID as ProgramID,
	pn.PartNo, 
	replace(pn.[Description],',','') AS PartDescription, 
	cpt.[Description] AS PartTypeDescription, 
	ps.PartLocationNo,
	CASE 
		WHEN pl.Warehouse LIKE 'SCR%' OR pl.Warehouse LIKE 'SCRAP%' THEN 'SCR' 
		WHEN pl.Warehouse LIKE 'RIN%' THEN 'RIN'  
		WHEN pl.Warehouse LIKE 'MRB%' THEN 'MRB'  
		WHEN pl.Warehouse LIKE 'BFG%' THEN 'BFG'  
		WHEN pl.Warehouse LIKE 'TST%' THEN 'TST'  
		WHEN pl.Warehouse LIKE 'TDN%' THEN 'TDN' 
		WHEN pl.Warehouse LIKE 'WIP%' OR pl.Warehouse LIKE 'FLOORSTOCK%' OR pl.Warehouse LIKE 'ISSUE%' OR pl.Warehouse LIKE 'FGI%' THEN 'WIP' 
		ELSE pl.LocationNo END AS GoProLocation, 
	1 AS Quantity, 
	ps.SerialNo 
FROM pls.vPartSerial ps (NOLOCK)
	INNER JOIN pls.PartNo pn (NOLOCK) ON pn.PartNo = ps.PartNo 
	INNER JOIN pls.CodePartType cpt (NOLOCK) ON cpt.ID = pn.PartTypeID 
	INNER JOIN pls.PartTransaction pt (NOLOCK) ON pt.ID = ( 
		SELECT MAX(ptr.ID) 
		FROM pls.PartTransaction ptr (NOLOCK)
		WHERE ptr.ProgramID = ps.ProgramID 
			AND ptr.PartNo = ps.PartNo 
			AND ptr.SerialNo = ps.SerialNo) 
	INNER JOIN pls.PartLocation pl (NOLOCK) ON pl.ProgramID = pt.ProgramID 
		AND pl.LocationNo = pt.[Location] 
WHERE ps.ProgramID = '<ProgramId>' 
	AND ps.StatusDescription IN ('RESERVED') 
UNION ALL
SELECT 
	@ProgramDateTime as SnapshotDate,
	@ProgramID as ProgramID,
	pn.PartNo, 
	replace(pn.[Description],',','') AS PartDescription, 
	cpt.[Description] AS PartTypeDescription, 
	pl.LocationNo, 
	CASE 
		WHEN pl.Warehouse LIKE 'SCR%' OR pl.Warehouse LIKE 'SCRAP%' THEN 'SCR' 
		WHEN pl.Warehouse LIKE 'RIN%' THEN 'RIN'  
		WHEN pl.Warehouse LIKE 'MRB%' THEN 'MRB'  
		WHEN pl.Warehouse LIKE 'BFG%' THEN 'BFG'  
		WHEN pl.Warehouse LIKE 'TST%' THEN 'TST'  
		WHEN pl.Warehouse LIKE 'TDN%' THEN 'TDN' 
		WHEN pl.Warehouse LIKE 'WIP%' OR pl.Warehouse LIKE 'FLOORSTOCK%' OR pl.Warehouse LIKE 'ISSUE%' OR pl.Warehouse LIKE 'FGI%' THEN 'WIP' 
		ELSE pl.LocationNo END AS GoProLocation, 
	SUM(pq.AvailableQty) AS Quantity, 
	'*' AS SerialNo 
FROM pls.PartQty pq (NOLOCK)
	INNER JOIN pls.PartNo pn (NOLOCK) ON pn.PartNo = pq.PartNo 
		AND pn.SerialFlag = 0 
	INNER JOIN pls.CodePartType cpt (NOLOCK) ON cpt.ID = pn.PartTypeID 
	INNER JOIN pls.PartLocation pl (NOLOCK) ON pl.ID = pq.LocationID 
WHERE pq.ProgramID = '<ProgramId>' 
	AND pq.AvailableQty > 0 
	AND pl.LocationNo != 'RESERVE.10003.0.0.0' 
GROUP BY pn.PartNo, 
	replace(pn.[Description],',',''), 
	cpt.[Description], 
	pl.LocationNo, 
	CASE 
		WHEN pl.Warehouse LIKE 'SCR%' OR pl.Warehouse LIKE 'SCRAP%' THEN 'SCR' 
		WHEN pl.Warehouse LIKE 'RIN%' THEN 'RIN'  
		WHEN pl.Warehouse LIKE 'MRB%' THEN 'MRB'  
		WHEN pl.Warehouse LIKE 'BFG%' THEN 'BFG'  
		WHEN pl.Warehouse LIKE 'TST%' THEN 'TST'  
		WHEN pl.Warehouse LIKE 'TDN%' THEN 'TDN' 
		WHEN pl.Warehouse LIKE 'WIP%' OR pl.Warehouse LIKE 'FLOORSTOCK%' OR pl.Warehouse LIKE 'ISSUE%' OR pl.Warehouse LIKE 'FGI%' THEN 'WIP' 
		ELSE pl.LocationNo END 
UNION ALL 
SELECT 
	@ProgramDateTime as SnapshotDate,
	@ProgramID as ProgramID,
	pn.PartNo, 
	replace(pn.[Description],',','') AS PartDescription, 
	cpt.[Description] AS PartTypeDescription, 
	ppl.LocationNo, 	
	CASE 
		WHEN pl.Warehouse LIKE 'SCR%' OR pl.Warehouse LIKE 'SCRAP%' THEN 'SCR' 
		WHEN pl.Warehouse LIKE 'RIN%' THEN 'RIN'  
		WHEN pl.Warehouse LIKE 'MRB%' THEN 'MRB'  
		WHEN pl.Warehouse LIKE 'BFG%' THEN 'BFG'  
		WHEN pl.Warehouse LIKE 'TST%' THEN 'TST'  
		WHEN pl.Warehouse LIKE 'TDN%' THEN 'TDN' 
		WHEN pl.Warehouse LIKE 'WIP%' OR pl.Warehouse LIKE 'FLOORSTOCK%' OR pl.Warehouse LIKE 'ISSUE%' OR pl.Warehouse LIKE 'FGI%' THEN 'WIP' 
		ELSE pl.LocationNo END AS GoProLocation, 
	SUM(sou.QtyReserved) AS Quantity, 
	'*' AS SerialNo 
FROM pls.SOHeader soh (NOLOCK)
	INNER JOIN pls.SOLine sol (NOLOCK) ON soh.ID = sol.SOHeaderID 
	INNER JOIN pls.PartNo pn (NOLOCK) ON pn.PartNo = sol.PartNo 
		AND pn.SerialFlag = 0 
	INNER JOIN pls.CodePartType cpt (NOLOCK) ON cpt.ID = pn.PartTypeID 
	INNER JOIN pls.SOUnit sou (NOLOCK) ON sol.ID = sou.SOLineID 
	INNER JOIN pls.PartLocation pl (NOLOCK) ON pl.ID = sou.FromLocationID 
	INNER JOIN pls.PartLocation ppl (NOLOCK) on sou.LocationID = ppl.ID
WHERE soh.ProgramID = '<ProgramId>' 
	AND sou.StatusID = 12 
GROUP BY pn.PartNo, 
	replace(pn.[Description],',',''), 
	cpt.[Description], 
	ppl.LocationNo, 
	CASE 
		WHEN pl.Warehouse LIKE 'SCR%' OR pl.Warehouse LIKE 'SCRAP%' THEN 'SCR' 
		WHEN pl.Warehouse LIKE 'RIN%' THEN 'RIN'  
		WHEN pl.Warehouse LIKE 'MRB%' THEN 'MRB'  
		WHEN pl.Warehouse LIKE 'BFG%' THEN 'BFG'  
		WHEN pl.Warehouse LIKE 'TST%' THEN 'TST'  
		WHEN pl.Warehouse LIKE 'TDN%' THEN 'TDN' 
		WHEN pl.Warehouse LIKE 'WIP%' OR pl.Warehouse LIKE 'FLOORSTOCK%' OR pl.Warehouse LIKE 'ISSUE%' OR pl.Warehouse LIKE 'FGI%' THEN 'WIP' 
		ELSE pl.LocationNo END ";


			query = query.Replace("<ProgramId>", programId);

			if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";


            DataTable dt = oDAL.GetData(query);



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("200", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstGoProInventoryWeb = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}