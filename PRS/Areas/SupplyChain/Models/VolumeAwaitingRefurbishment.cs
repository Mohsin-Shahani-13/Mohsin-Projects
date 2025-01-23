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
    public class VolumeAwaitingRefurbishment
    {

        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
       
        [Display(Name = "Program:")]
        public string program { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstVolumeAwaitingRefurbishment { get; set; }
        public List<Hashtable> lstUnits { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("Active");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
                      AND Name = 'BOSE'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);


            return dt;
        }
      
        #endregion
        #region Methods 
        public bool GetList(string programId, string programName)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            query = @"
	SELECT 
    PS.ProgramID,
    CC.Description AS AreaCell,
    COUNT(PS.SerialNo) AS TotalSerialNo,
    SUM(CASE 
            WHEN DATEDIFF(DAY, WOH.CreateDate, GETDATE()) BETWEEN 0 AND 30 THEN 1 
            ELSE 0 
        END) AS '0-30',
    SUM(CASE 
            WHEN DATEDIFF(DAY, WOH.CreateDate, GETDATE()) BETWEEN 31 AND 60 THEN 1 
            ELSE 0 
        END) AS [31-60],
    SUM(CASE 
            WHEN DATEDIFF(DAY, WOH.CreateDate, GETDATE()) BETWEEN 61 AND 90 THEN 1 
            ELSE 0 
        END) AS [61-90],
    SUM(CASE 
            WHEN DATEDIFF(DAY, WOH.CreateDate, GETDATE()) > 90 THEN 1 
            ELSE 0 
        END) AS [>90]
FROM pls.PartSerial PS
INNER JOIN pls.PartNo PN 
    ON PN.PartNo = PS.PartNo
INNER JOIN pls.CodeCommodity CC 
    ON PN.PrimaryCommodityID = CC.ID
INNER JOIN pls.WOHeader WOH 
    ON WOH.ID = PS.WOHeaderID ";
            if (sites == "BYDGOSZCZ")
            {
                query += " WHERE PS.ProgramID = 10058 ";
            }
            else
            {
                query += " WHERE PS.ProgramID = 10059 ";
            }
            query += @"AND WOH.ID NOT IN (SELECT WOHeaderID from pls.WOStationHistory WOSH where WOSH.WorkStationID = 4)
   AND WOH.WorkStationIDPrevious IS NULL 
                        AND WOH.StatusID = 19
                        GROUP BY PS.ProgramID, CC.Description ";


            query = query.Replace("<ProgramId>", programId);

            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            DataTable dt = oDAL.GetData(query);


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("201", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstVolumeAwaitingRefurbishment = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool GetUnits(string programId, string AreaCell, string Range)
        {


            string query = string.Empty;
            var conType = @HttpContext.Current.Session["CONN_TYPE"].ToString();
            
                query = @"-- Declare variables to filter by area cell and date range if needed.
DECLARE @AreaCell VARCHAR(50) = '<AreaCell>';  -- Replace with the specific AreaCell name
DECLARE @programId int = '<programId>'

SELECT  Distinct
 PS.ProgramID,
 ROH.ID,
 ROH.CustomerReference,
PS.PartNo,
    PS.SerialNo
FROM pls.PartSerial PS
INNER JOIN pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID
INNER JOIN pls.PartNo PN 
    ON PN.PartNo = PS.PartNo
INNER JOIN pls.CodeCommodity CC 
    ON PN.PrimaryCommodityID = CC.ID
INNER JOIN pls.WOHeader WOH 
    ON WOH.ID = PS.WOHeaderID  
WHERE 
    PS.ProgramID = @programId
    AND WOH.WorkStationIDPrevious IS NULL 
    AND WOH.StatusID = 19
    AND CC.Description = @AreaCell 
 ";

            if (Range == "0-30")
            {
                query += " AND DATEDIFF(DAY, WOH.CreateDate, GETDATE()) BETWEEN 0 AND 30;  -- Filter for 0-30 days ";
            }

            else if(Range == "31-60")
            {
                query += " AND DATEDIFF(DAY, WOH.CreateDate, GETDATE()) BETWEEN 31 AND 60;  -- Filter for 31-60 days ";
            }
            else if (Range == "61-90")
            {
                query += " AND DATEDIFF(DAY, WOH.CreateDate, GETDATE()) BETWEEN 61 AND 90;  -- Filter for 61-90 days ";
            }


            else
            {
                query += "  AND DATEDIFF(DAY, WOH.CreateDate, GETDATE()) > 90;  -- Filter for more than 90 days ";
            }



            query = query.Replace("<programId>", programId);
            query = query.Replace("<AreaCell>", AreaCell);
          

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("201-1", query, "---Serial No.---", false);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstUnits = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }


        #endregion
    }
}