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
    public class SOPickList
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Customer Ref. / PR Header:")]
        public string custRef { get; set; }
        public List<Hashtable> lstSOPickList { get; set; }

        public List<Hashtable> lstERSPickList { get; set; }
        public List<Hashtable> lstSOPickListNotReserved { get; set; }
        public List<Hashtable> lstCleanToBuild { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion

        #region Methods 
        public bool GetList(string programId, string ProgramName, string custRef)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @" 
SELECT sol.ID, 
       soh.programId,
       P.Name,
       soh.CustomerReference,
	   soh.ID AS SOHeaderID,
       CONCAT(pn.PartNo,  ' - ', pn.[Description], ' - [', cc.[Description], ']') AS [PartNo - Description - Configuration],
       CONCAT(CONVERT(VARCHAR, sol.QtyReserved), '/', CONVERT(VARCHAR, sol.QtyToShip)) AS [Reserved/To Ship],
       ISNULL(REPLACE(STUFF((
	   SELECT '|' + CONCAT(pl.LocationNo, ' - [', FORMAT(pq.AvailableQty, 'N0'), ']')
FROM pls.PartQty pq
INNER JOIN pls.PartLocation pl ON pl.ID = pq.LocationID
INNER JOIN pls.CodeLocationGroup clg ON clg.ID = pl.LocationGroupID AND clg.[Description] IN ('DGI','FGI','SCRAP')
LEFT JOIN ( SELECT ps.ProgramID,
                   ps.PartNo,
                   ps.ConfigurationID,
                   ps.LocationID,
                   MIN(ps.CreateDate) AS minReceipt
FROM pls.PartSerial ps
WHERE ps.PalletBoxNo = '0'
GROUP BY ps.ProgramID,
         ps.PartNo,
         ps.ConfigurationID,
         ps.LocationID ) oldestReceipt ON pq.ProgramID = oldestReceipt.ProgramID
AND pq.LocationID = oldestReceipt.LocationID
AND pq.ConfigurationID = oldestReceipt.ConfigurationID
AND pq.PartNo = oldestReceipt.PartNo
WHERE pq.ProgramID = soh.ProgramID
AND pq.ConfigurationID = sol.ConfigurationID
AND pq.PartNo = sol.PartNo
AND pq.PalletBoxNo = '0'
AND pq.AvailableQty > 0
ORDER BY oldestReceipt.minReceipt
FOR XML PATH ('')), 1, 1, ''), '|', '<br/>'), 'N/A') AS [Location - QtyAvailable],
cs.[Description] AS [Status]
FROM pls.SOHeader soh
INNER JOIN pls.Program P ON P.ID = soh.ProgramID
INNER JOIN pls.SOLine sol ON soh.ID = sol.SOHeaderID
INNER JOIN pls.PartNo pn ON pn.PartNo = sol.PartNo
INNER JOIN pls.CodeConfiguration cc ON cc.ID = sol.ConfigurationID
INNER JOIN pls.CodeStatus cs ON cs.ID = sol.StatusID
--WHERE soh.ID = 45
WHERE cs.[Description] <> 'Shipped'
";

            if (!string.IsNullOrEmpty(custRef))
                query += "AND SOH.CustomerReference LIKE '%" + custRef + "%'";

            if (programId != "0")
            {
                query += "AND soh.ProgramId = '" + programId + "' ";
            }
            else
            {
                query += "AND soh.ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }


            query += "ORDER BY sol.ID";
            //query = query.Replace("<programId>",programId);


            DataTable dt = oDAL.GetData(query);
            
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(custRef))
                filterString += "| Customer Ref. Like '" + custRef + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("020", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSOPickList = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool GetNotReserved(string programId, string ProgramName, string custRef)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
 
      select * from (
Select SOH.ProgramID,SOH.CustomerReference, 
(
SELECT value
FROM pls.SOLineAttribute
WHERE SOLineID = SOL.ID
and attributeid = 48
) PRDetailsNo
,sol.PartNo,
(
SELECT replace(value, ' ','')
FROM pls.SOLineAttribute
WHERE SOLineID = SOL.ID
and attributeid = 225
) Warehouse,
(
SELECT value
FROM pls.SOLineAttribute
WHERE SOLineID = SOL.ID
and attributeid = 209
) RequestType
,(
SELECT value
FROM pls.SOLineAttribute
WHERE SOLineID = SOL.ID
and attributeid = 225
) SupplyCow
, (
SELECT value
FROM pls.SOHeaderAttribute
WHERE SOHeaderID = SOH.ID
and attributeid = 211
) AS ShipVia
,(
SELECT value
FROM pls.SOLineAttribute
WHERE SOLineID = SOL.ID
and attributeid = 161
) MRACode
, sol.QtyReserved
, sol.QtyToShip
, CS.Description AS SOStatus
from pls.SOHeader SOH
INNER JOIN pls.SOLine SOL ON SOH.ID = sol.SOHeaderID
LEFT OUTER JOIN pls.SOUnit SOU ON SOU.SOLineID = SOL.ID
LEFT OUTER JOIN pls.PartLocation PL ON PL.ID = SOU.LocationID
LEFT OUTER JOIN pls.PartLocationAttribute PLA ON PLA.PartLocationId = PL.ID
INNER JOIN pls.CodeStatus CS ON CS.ID = SOH.StatusID
LEFT OUTER JOIN pls.SOLineAttribute SOLA on SOLA.SOLineID = SOL.ID
--WHERE LocationGroupId = 2 AND
--PLA.AttributeId = 290 -- COW
)tmp
Where SOStatus NOT IN ( 'RESERVED','SHIPPED', 'CANCELED')
AND
RequestType IN ('Order-Deployment','Advance Exchange','Advance Exch. Scrap','NS-from-Stock','Exchange','Exchange-NS','')
";

            if (!string.IsNullOrEmpty(custRef))
                query += "AND CustomerReference LIKE '%" + custRef + "%'";

            if (programId != "0")
            {
                query += "AND ProgramId = '" + programId + "' ";
            }
            else
            {
                query += "AND ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += @"
GROUP BY 
ProgramID,
CustomerReference,
PRDetailsNo,
PartNo,
RequestType,
SupplyCow,
ShipVia,
MRACode,
SOStatus,
QtyToShip,
QtyReserved,
Warehouse
ORDER BY CustomerReference

";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(custRef))
                filterString += "| Customer Ref. Like '" + custRef + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("020", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSOPickListNotReserved = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool GetClearToBuild(string programId, string ProgramName, string custRef)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
 Select
P.ID AS ProgramId
,P.Name
,SOH.ID
,SOH.CreateDate AS PR_DATE
,(SELECT value FROM pls.SOLineAttribute WHERE SOLineID = SOL.ID and attributeid = 209) RequestType
,SOH.CustomerReference + '-' + ISNULL(CAST(SOL.ID AS varchar), '0') AS PR_Number
,SOH.CustomerReference AS PR_Header
,SOL.PartNo AS Part_Number
,PN.Description AS Part_Description
--,SOU.FromLocationID
,SOL.QtyToShip AS Quantity
--, SUM(PQ.AvailableQty) AS QtyAvailable
,(SELECT value FROM pls.SOHeaderAttribute WHERE SOHeaderID = SOH.ID and attributeid = 203) Customer_Name
,SOLA.value AS Supply_COW
,CASE WHEN SOL.QtyToShip <= SUM(PQ.AvailableQty) AND REPLACE(SOLA.Value, ' ', '') = PL.Warehouse THEN 'CTB' ELSE 'HOLD' END AS Status
from pls.SOHeader SOH
INNER JOIN pls.Program P ON P.ID = SOH.ProgramId
LEFT OUTER JOIN pls.SOLine SOL ON sol.SOHeaderID = SOH.ID
LEFT OUTER JOIN pls.SoUnit SOU ON SOU.SOLineID = SOL.ID
LEFT OUTER JOIN pls.PartNo PN ON PN.PartNo = SOL.PartNo
LEFT OUTER JOIN pls.PartQty PQ ON PQ.ProgramID = SOH.ProgramID AND PQ.LocationID = SOU.FromLocationID AND PQ.ConfigurationID = SOL.ConfigurationID AND PQ.PartNo = SOL.PartNo
LEFT OUTER JOIN pls.PartLocation pl ON pl.ID = pq.LocationID
LEFT OUTER JOIN pls.SOLineAttribute SOLA ON SOLA.SOLineID = SOL.ID and SOLA.AttributeID = 225
WHERE SOH.CustomerReference NOT LIKE '%REY%' AND SOH.StatusID IN (7, 31)
";

            if (!string.IsNullOrEmpty(custRef))
                query += "AND CustomerReference LIKE '%" + custRef + "%'";

            if (programId != "0")
            {
                query += "AND SOH.ProgramId = '" + programId + "' ";
            }
            else
            {
                query += "AND SOH.ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += @"
GROUP BY P.ID, P.Name, SOH.ID, SOH.CreateDate, SOH.CustomerReference, SOL.ID, SOL.PartNo, PN.Description, SOU.FromLocationID, QtyToShip, SOLA.Value, PL.Warehouse, SOH.ID
ORDER BY SOH.CreateDate
";

            //query = query.Replace("<programId>",programId);


            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(custRef))
                filterString += "| Customer Ref. Like '" + custRef + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("020", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstCleanToBuild = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool GetERSPickList(string cusRef)
        {
            string query = string.Empty;
          

            query = @"SELECT DISTINCT
                        '' AS Download
                       ,SOH.ID
                      ,SOH.ProgramID
                      ,SOH.customerreference
                      ,(
                      SELECT value
                      FROM pls.SoheaderAttribute
                      WHERE SOHeaderId = SOH.ID
                      and attributeid = 203
                      ) AS SHIPTOSITENAME 
                      ,(
                      SELECT value
                      FROM pls.SoheaderAttribute
                      WHERE SOHeaderId = SOH.ID
                      and attributeid = 204
                      ) AS SHIPTOSITEID 
                      ,(
                      SELECT FORMAT( CAST(value AS date), 'yyyy.MM.dd') Value
                      FROM pls.SoheaderAttribute
                      WHERE SOHeaderId = SOH.ID
                      and attributeid = 216
                      ) AS PRCREATIONDATE 
                      ,(
                      SELECT value
                      FROM pls.SoheaderAttribute
                      WHERE SOHeaderId = SOH.ID
                      and attributeid = 210
                      ) AS PRDETAILNOTES
                      FROM [pls].soheader AS SOH
                      INNER JOIN pls.CodeStatus AS CS
                      ON SOH.statusid = CS.id
                      INNER JOIN Pls.SOLine AS SOL
                      ON SOH.id = SOL.soheaderid
                      AND CS.id = SOL.statusid
                      INNER JOIN pls.PartNo PN ON PN.PartNo = SOL.PartNo
                      WHERE SOH.statusid IN ( 13, 7) AND SOH.customerreference IN('<CustRef>')";
                      
            query = query.Replace("<CustRef>", cusRef);
            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("127", query, string.Empty, false);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                    lstERSPickList = cCommon.ConvertDtToHashTable(dt);

                return true;
            }
            else
                return false;
        }
        #endregion
    }
}