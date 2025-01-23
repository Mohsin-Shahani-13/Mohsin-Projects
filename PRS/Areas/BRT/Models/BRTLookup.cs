using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Data.SqlClient;


namespace IP.Areas.BRT.Models
{
    public class BRTLookup
    {

        cDAL oDAL = new cDAL("ACTIVE");
        #region fields
        [Display(Name = "Building:")]
        public string building { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<ArrayList> lstBRTLookup { get; set; }
        public List<ArrayList> lstData { get; set; }
        public List<ArrayList> lstDataColumn { get; set; }
        public List<ArrayList> lstlocation { get; set; }
        public List<ArrayList> lstSerial { get; set; }
        public List<ArrayList> lstLocationByPart { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion

        #region Methods 
        //public DataTable Program() // onHand warehouse method
        //{
        //    string query = string.Empty;
        //    query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program";

        //    DataTable dt = oDAL.GetData(query);
        //    return dt;
        //}
        public DataTable Building(string programId, string ProgramName) // onHand warehouse method
        {
            string query = string.Empty;
            query = @"SELECT DISTINCT Building 
                      FROM [pls].[PartLocation] PL
                      INNER JOIN pls.PartQty PQ ON 
                                 PQ.ProgramID = PL.ProgramID AND 
                                 PQ.LocationID = PL.ID
					   INNER JOIN pls.PartLocationWarehouse PLW ON 
					   PLW.Warehouse = PL.Warehouse 
                       INNER JOIN Pls.Program P On P.ID = PL.ProgramID ";
            if (programId != "0" && programId != null)
                {
                query += " WHERE P.ID = '" + programId + "' ";
            }
                else
            {
                query += " WHERE P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += "ORDER BY Building ";

            DataTable dt = oDAL.GetData(query);
            return dt;
        }

        public bool GetList(string building)
        {
            cDAL oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"SELECT Bay
                      FROM [pls].[PartLocation] PL
                      INNER JOIN pls.PartQty PQ ON 
                                 PQ.ProgramID = PL.ProgramID AND 
                                 PQ.LocationID = PL.ID
					   INNER JOIN pls.PartLocationWarehouse PLW ON 
					   PLW.Warehouse = PL.Warehouse
                      WHERE Building = '<building>' 
                      GROUP BY Bay
                      ORDER BY LEN(Bay), Bay ";

            query = query.Replace("<building>", building);

            DataTable dt = oDAL.GetData(query);

            filterString += "Bldg = '" + building + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("0043", query, string.Empty, false);



            if (!oDAL.HasErrors)
            {
                lstBRTLookup = cCommon.ConvertDtToArrayList(dt);

                return true;
            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
        }

        public bool GetRowList(string bay, string building)
        {
            cDAL oDAL = new cDAL("ACTIVE");
            string query = string.Empty;

            query = @"DECLARE   @SQLQuery AS NVARCHAR(MAX)
DECLARE   @PivotColumns AS NVARCHAR(MAX)
--Get unique values of pivot column  
SELECT  @PivotColumns= COALESCE(@PivotColumns + ',','') + QUOTENAME(Row)
FROM 
(
    SELECT Row FROM pls.PartLocation WHERE Bay = '<bay>' AND Building = '<building>' GROUP BY Row ORDER BY LEN(Row), Row OFFSET 0 ROWS
) AS PivotExample 
-- Main report query with PIVOT implementation
SET   @SQLQuery = 
N'WITH CTE AS 
(
 SELECT PL.Tier,  PL.ROW,  SUM(PQ.AvailableQty) QTY
 FROM pls.PartLocation PL
  INNER JOIN pls.PartQty PQ ON 
                                 PQ.ProgramID = PL.ProgramID AND 
                                 PQ.LocationID = PL.ID
					             INNER JOIN pls.PartLocationWarehouse PLW ON 
					             PLW.Warehouse = PL.Warehouse
 WHERE PL.Bay = ''<bay>'' AND
       PL.Building = ''<building>''
 GROUP BY PL.Tier, PL.Row
)
SELECT Tier, '+ @PivotColumns +'
FROM CTE
PIVOT(SUM(QTY)   
FOR Row IN ('+ @PivotColumns +')) AS P
ORDER BY LEN(TIER) DESC, TIER DESC ' 
EXEC sp_executesql @SQLQuery ";

            query = query.Replace("<bay>", bay);
            query = query.Replace("<building>", building);

            DataTable dtMaster = oDAL.GetData(query);

            DataTable dtList = dtMaster.Clone();
            DataTable dtColHeader = cCommon.GenerateTransposedTable(dtList);

            filterString += "BRT Lookup For Building = '" + building + "' | Bay = '" + bay + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("0043-1", query, "Get Detail", false);


            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dtMaster.Rows.Count > 0)
                {
                    lstData = cCommon.ConvertDtToArrayList(dtMaster);
                    lstDataColumn = cCommon.ConvertDtToArrayList(dtColHeader);

                }
                return true;

            }
        }

        public bool GetLocaton(string building, string bay, string tier, string row)
        {
            string query = string.Empty;
            query = @"SELECT PL.LocationNo, PQ.PartNo, SUM(PQ.AvailableQty ) AS  QTY, PQ.LocationID,
(SELECT CASE WHEN COUNT(*) > 0 THEN 'Y' ELSE 'N' END FROM pls.PartSerial PS where PS.PartNo = PQ.PartNo ) AS IsSerialExist
FROM pls.PartLocation PL
INNER JOIN pls.PartQty PQ ON PL.ProgramID = PQ.ProgramID AND PL.ID = PQ.LocationID
INNER JOIN pls.PartLocationWarehouse PLW ON PLW.Warehouse = PL.Warehouse
WHERE PL.Building = '<building>' AND
      PL.Bay = '<bay>' AND ";
            if (!string.IsNullOrEmpty(tier))
                query += "PL.Tier = '<tier>' AND ";


            query += @"PL.Row = '<row>'
GROUP BY PL.LocationNo, PQ.PartNo, PQ.LocationID ";

            query = query.Replace("<building>", building);
            query = query.Replace("<bay>", bay);
            query = query.Replace("<tier>", tier);
            query = query.Replace("<row>", row);

            DataTable dt = oDAL.GetData(query);
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("0043-2", query, string.Empty, false);

            if (!string.IsNullOrEmpty(tier))
                filterString += "Location For Row = '" + row + "' | Tier = '" + tier + "' ";

            if (!oDAL.HasErrors)
            {
                lstlocation = cCommon.ConvertDtToArrayList(dt);

                return true;
            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
        }

        public bool GetSerial(string locationId, string partNo)
        {
            string query = string.Empty;
            query = @"SELECT PS.ProgramId, SerialNo, SUM(AvailableQty) AS AvailQty
FROM pls.PartSerial PS
INNER JOIN pls.PartQty PQ ON PQ.PartNo = PS.PartNo 
INNER JOIN pls.PartLocation PL ON PL.ID = Ps.LocationID
WHERE PL.ID = '<locationId>' AND
      PQ.PartNo = '<partNo>'
GROUP BY  PS.ProgramId, SerialNo";

            query = query.Replace("<locationId>", locationId);
            query = query.Replace("<partNo>", partNo);

            DataTable dt = oDAL.GetData(query);
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("0043-3", query, string.Empty, false);



            if (!oDAL.HasErrors)
            {
                lstSerial = cCommon.ConvertDtToArrayList(dt);

                return true;
            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
        }

        public bool GetLocationByPart(string locationId, string partNo)
        {
            string query = string.Empty;
            query = @"SELECT PL.LocationNo, SUM(AvailableQty) QtyOH
FROM pls.PartLocation PL
INNER JOIN pls.PartQty PQ ON PL.ProgramID = PQ.ProgramID AND PL.ID = PQ.LocationID
WHERE  PL.ID = '<locationId>' AND PQ.PartNo = '<partNo>'
GROUP BY PL.LocationNo ";
            query = query.Replace("<locationId>", locationId);
            query = query.Replace("<partNo>", partNo);

            DataTable dt = oDAL.GetData(query);
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("0043-3", query, string.Empty, false);



            if (!oDAL.HasErrors)
            {
                lstLocationByPart = cCommon.ConvertDtToArrayList(dt);

                return true;
            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
        }
        #endregion
    }
}