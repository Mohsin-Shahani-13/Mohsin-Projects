using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class WIP
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Customer Ref.:")]
        public string custRef { get; set; }

        public int totalIndex { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public String ProgramforSite { get; set; }
        public string ProgramBySite { get; set; }
        public List<ArrayList> lstWip { get; set; }
        public List<Hashtable> lstWOUnit { get; set; }

        public List<ArrayList> lstDataColumn { get; set; }

        #endregion
        #region "Methods"
        public bool GetList(string programId, string programName)
        {
            string query = string.Empty;
            string programForWIP = GetProgramBysite(HttpContext.Current.Session["DefaultSite"].ToString());
            var conType = @HttpContext.Current.Session["CONN_TYPE"].ToString();


            if (programName == "BOSE")
            {
                query = @"
--Wip Query--

DECLARE @SQL_QUERY AS NVARCHAR(MAX)
DECLARE @COLUMNS AS NVARCHAR(MAX)
 
 
SELECT @COLUMNS = COALESCE(@COLUMNS + ',', '') + QUOTENAME(WorkstationDescription)
FROM
(
    SELECT WorkStationDescription
	FROM 
	(
		SELECT WO.ProgramID , WO.PartNo
, PN.Description
, CASE WHEN WO.StatusID = 19 THEN -- FOR WIP
		CASE WHEN WSD.Code IS NULL THEN cws.Description ELSE WSD.CODE END
  ELSE -- FOR HOLD
		CS.Description
END AS WorkstationDescription
, count(SerialNo) SN
FROM pls.WOHeader WO
INNER JOIN pls.PartNo PN ON PN.PartNo = WO.PartNo
INNER JOIN pls.CodeStatus cs ON cs.ID = WO.StatusID
INNER JOIN pls.CodeWorkStation cws ON cws.ID = WO.WorkStationId
LEFT JOIN pls.CodeWorkStationCustomDescription WSD ON
WSD.ProgramID = WO.ProgramID
AND WSD.RepairTypeID = WO.RepairTypeID
AND WSD.CodeWorkStationID = WO.WorkStationID
WHERE @PROGRAM_WHERECLAUSE AND WO.StatusID IN (19, 28)
GROUP BY   WO.ProgramID, WO.PartNo, WSD.Code, cws.Description, PN.Description, cs.Description, WO.StatusID
	) TMP
	GROUP BY WorkStationDescription
    ORDER BY WorkStationDescription OFFSET 0 ROWS
 
 
) AS PIVOT_COLUMNS
 
SET @SQL_QUERY = 
N'SELECT * FROM 
(
    SELECT ProgramID, PartNo, Description, WorkStationDescription,ROHA_Value, SUM(SN) AS SN
	FROM 
	(
		SELECT WO.ProgramID, WO.PartNo
, PN.Description
, CASE WHEN WO.StatusID = 19 THEN -- FOR WIP
		CASE WHEN WSD.Code IS NULL THEN cws.Description ELSE WSD.CODE END
  ELSE -- FOR HOLD
		CS.Description
END AS WorkstationDescription
,ROHA.Value AS ROHA_Value
, count(WO.SerialNo) SN
FROM pls.WOHeader WO
INNER JOIN pls.PartNo PN ON PN.PartNo = WO.PartNo
INNER JOIN pls.CodeStatus cs ON cs.ID = WO.StatusID
INNER JOIN pls.CodeWorkStation cws ON cws.ID = WO.WorkStationId
INNER JOIN pls.PartSerial PS ON PS.WOHeaderID = WO.ID 
        INNER JOIN pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID AND ROH.ProgramID = PS.ProgramID
        LEFT JOIN pls.CodeAttribute CA ON AttributeName = ''PROCESS_TYPE''
        LEFT JOIN pls.ROHeaderAttribute ROHA ON ROHA.ROHeaderID = ROH.ID AND ROHA.AttributeID = CA.ID
LEFT JOIN pls.CodeWorkStationCustomDescription WSD ON
WSD.ProgramID = WO.ProgramID
AND WSD.RepairTypeID = WO.RepairTypeID
AND WSD.CodeWorkStationID = WO.WorkStationID
WHERE @PROGRAM_WHERECLAUSE AND WO.StatusID IN (19, 28)
GROUP BY  WO.ProgramID, WO.PartNo, WSD.Code, cws.Description, PN.Description, cs.Description, WO.StatusID, ROHA.Value
	) TMP
	GROUP BY  ProgramID, PartNo, Description, WorkStationDescription, ROHA_Value
  ) A
PIVOT
(
    SUM(SN)
    FOR WorkstationDescription
    IN('+ @COLUMNS +')
)
AS PIVOT_TABLE
ORDER BY PartNo
    '
    EXEC sp_executesql @SQL_QUERY

";

            }
            else
            {
                query = @"
--Wip Query--
DECLARE @SQL_QUERY AS NVARCHAR(MAX)
DECLARE @COLUMNS AS NVARCHAR(MAX)

 

SELECT @COLUMNS = COALESCE(@COLUMNS + ',', '') + QUOTENAME(WorkstationDescription)
FROM
(
    SELECT WorkStationDescription
	FROM 
	(
		SELECT WO.ProgramID , WO.PartNo
, PN.Description
, CASE WHEN WO.StatusID = 19 THEN -- FOR WIP
		CASE WHEN WSD.Code IS NULL THEN cws.Description ELSE WSD.CODE END
  ELSE -- FOR HOLD
		CS.Description
END AS WorkstationDescription
, count(SerialNo) SN
FROM pls.WOHeader WO
INNER JOIN pls.PartNo PN ON PN.PartNo = WO.PartNo
INNER JOIN pls.CodeStatus cs ON cs.ID = WO.StatusID
INNER JOIN pls.CodeWorkStation cws ON cws.ID = WO.WorkStationId
LEFT JOIN pls.CodeWorkStationCustomDescription WSD ON
WSD.ProgramID = WO.ProgramID
AND WSD.RepairTypeID = WO.RepairTypeID
AND WSD.CodeWorkStationID = WO.WorkStationID
WHERE @PROGRAM_WHERECLAUSE AND WO.StatusID IN (19, 28)
GROUP BY   WO.ProgramID, WO.PartNo, WSD.Code, cws.Description, PN.Description, cs.Description, WO.StatusID
	) TMP
	GROUP BY WorkStationDescription
    ORDER BY WorkStationDescription OFFSET 0 ROWS


) AS PIVOT_COLUMNS

SET @SQL_QUERY = 
N'SELECT * FROM 
(
    SELECT ProgramID, PartNo, Description, WorkStationDescription, SUM(SN) AS SN
	FROM 
	(
		SELECT WO.ProgramID, WO.PartNo
, PN.Description
, CASE WHEN WO.StatusID = 19 THEN -- FOR WIP
		CASE WHEN WSD.Code IS NULL THEN cws.Description ELSE WSD.CODE END
  ELSE -- FOR HOLD
		CS.Description
END AS WorkstationDescription
, count(SerialNo) SN
FROM pls.WOHeader WO
INNER JOIN pls.PartNo PN ON PN.PartNo = WO.PartNo
INNER JOIN pls.CodeStatus cs ON cs.ID = WO.StatusID
INNER JOIN pls.CodeWorkStation cws ON cws.ID = WO.WorkStationId
LEFT JOIN pls.CodeWorkStationCustomDescription WSD ON
WSD.ProgramID = WO.ProgramID
AND WSD.RepairTypeID = WO.RepairTypeID
AND WSD.CodeWorkStationID = WO.WorkStationID
WHERE @PROGRAM_WHERECLAUSE AND WO.StatusID IN (19, 28)
GROUP BY  WO.ProgramID, WO.PartNo, WSD.Code, cws.Description, PN.Description, cs.Description, WO.StatusID
	) TMP
	GROUP BY  ProgramID, PartNo, Description, WorkStationDescription 
          
  ) A
PIVOT
(
    SUM(SN)
    FOR WorkstationDescription
    IN('+ @COLUMNS +')
)
AS PIVOT_TABLE
 ORDER BY PartNo
    '
    EXEC sp_executesql @SQL_QUERY

";
            }
            if (programId != "0" && programId != null)
                query = query.Replace("@PROGRAM_WHERECLAUSE", "WO.PROGRAMID = " + programId);
            else
                query = query.Replace("@PROGRAM_WHERECLAUSE", "WO.PROGRAMID IN (" + programForWIP + ")");

            DataTable dt = oDAL.GetData(query);
            if (programName == "BOSE")
            {
                dt.Columns.Add("Total");
                foreach (DataRow row in dt.Rows)
                {
                    decimal total = 0;
                    for (int i = 4; i < dt.Columns.Count - 1; i++)
                    {
                        decimal value = 0;
                        if (row[i] != DBNull.Value)
                            value = Convert.ToDecimal(row[i]);

                        total += value;
                    }


                    row["TOTAL"] = total;
                }
            }
            else
            {
                dt.Columns.Add("Total");
                foreach (DataRow row in dt.Rows)
                {
                    decimal total = 0;
                    for (int i = 3; i < dt.Columns.Count - 1; i++)
                    {
                        decimal value = 0;
                        if (row[i] != DBNull.Value)
                            value = Convert.ToDecimal(row[i]);

                        total += value;
                    }


                    row["TOTAL"] = total;
                }
            }
            

            DataTable dtList = dt.Clone();
            DataTable dtColHeader = cCommon.GenerateTransposedTable(dtList);

            if (!string.IsNullOrEmpty(programName))
                filterString += " Program = '" + programName + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("011", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWip = cCommon.ConvertDtToArrayList(dt);
                lstDataColumn = cCommon.ConvertDtToArrayList(dtColHeader);

                return true;

            }
        }

        public bool GetWOUnit(string partNo, string workStation, string programId, string statusId, string attribute)
        {


            string query = string.Empty;
            string programForWIP = GetProgramBysite(HttpContext.Current.Session["DefaultSite"].ToString());
            var conType = @HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (programId == "10058")
            {
                query = @"SELECT WOH.SerialNo, woh.ProgramID,WOH.PartNo, ROHA.Value FROM pls.WOHeader WOH
INNER JOIN pls.PartSerial PS ON PS.WOHeaderID = WOH.ID 
  INNER JOIN pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID AND ROH.ProgramID = PS.ProgramID
LEFT JOIN pls.CodeAttribute CA ON AttributeName = 'PROCESS_TYPE'
        LEFT JOIN pls.ROHeaderAttribute ROHA ON ROHA.ROHeaderID = ROH.ID AND ROHA.AttributeID = CA.ID
INNER JOIN pls.CodeWorkStation CWS ON CWS.ID = WOH.WorkStationID
LEFT  JOIN pls.CodeWorkStationCustomDescription WSD ON
			   WSD.ProgramID = WOH.ProgramID 
			   AND WSD.RepairTypeID = WOH.RepairTypeID
			   AND WSD.CodeWorkStationID = WOH.WorkStationID
WHERE  @PROGRAM_WHERECLAUSE  ";
            }

            else
            {
                query = @"SELECT SerialNo, woh.ProgramID,WOH.PartNo FROM pls.WOHeader WOH
INNER JOIN pls.CodeWorkStation CWS ON CWS.ID = WOH.WorkStationID
LEFT  JOIN pls.CodeWorkStationCustomDescription WSD ON
			   WSD.ProgramID = WOH.ProgramID 
			   AND WSD.RepairTypeID = WOH.RepairTypeID
			   AND WSD.CodeWorkStationID = WOH.WorkStationID
WHERE  @PROGRAM_WHERECLAUSE  ";
            }
                

            if (programId != "0" && programId != null)
                query = query.Replace("@PROGRAM_WHERECLAUSE", "WOH.PROGRAMID = " + programId);
            else
                query = query.Replace("@PROGRAM_WHERECLAUSE", "WOH.PROGRAMID IN (" + programForWIP + ")");

            if (!string.IsNullOrEmpty(attribute))
                query += "AND ROHA.Value = '<attribute>' ";

            if (!string.IsNullOrEmpty(partNo))
                query += "AND WOH.PartNo = '<partNo>' ";

            if (workStation == "HOLD")
                query += " AND WOH.StatusID IN (28)";
            else if (workStation == "Total")
                query += " AND WOH.StatusID IN (28,19)";
            else
                query += " AND WOH.StatusID IN (19)";


            if (!string.IsNullOrEmpty(workStation) && workStation != "Undefined" && workStation != "Total" && workStation != "HOLD")
            {
                if (workStation == "Close" || workStation == "Kitting" || workStation == "Scrap")
                    query += " AND CWS.Description = '<workStation>' AND ( WSD.Code = '<workStation>' OR WSD.Code IS NULL) ";
                else
                    query += "AND (CWS.Description = '<workStation>' OR WSD.Code = '<workStation>' ) ";

            }

            query = query.Replace("<programId>", programId);
            query = query.Replace("<partNo>", partNo);
            query = query.Replace("<workStation>", workStation);
            query = query.Replace("<statusId>", statusId);
            query = query.Replace("<attribute>", attribute);

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("011-1", query, "---Serial No.---", false);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstWOUnit = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }

        public string GetProgramBysite(string site)
        {
            cDAL oDAL = new cDAL("ACTIVE");
            string query = "SELECT Id  FROM pls.Program WHERE Site = '<Site>'";
            query = query.Replace("<Site>", site);
            DataTable dt = oDAL.GetData(query);

            ProgramBySite = dt.Rows[0]["Id"].ToString();
            var ProgramList = (from p in dt.AsEnumerable()
                               select p.Field<object>("ID")).ToList().Distinct();
            ProgramforSite = String.Join(",", ProgramList).Insert(0, "").Insert(String.Join(",", ProgramList).Insert(0, "").Length, "");
            return ProgramforSite;
        }
        #endregion
    }
}