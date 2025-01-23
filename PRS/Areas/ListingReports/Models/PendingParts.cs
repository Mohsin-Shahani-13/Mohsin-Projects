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
    public class PendingParts
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        public List<Hashtable> lstPendingParts { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string programId, string ProgramName, string partNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            if (sites == "PRAGUE")
            {
                query = @"
CREATE TABLE #IFSTable (
    part_no VARCHAR(255),
    total_quantity INT
);
 
INSERT INTO #IFSTable (part_no, total_quantity)
SELECT *
FROM OPENQUERY(EMEA, '
    SELECT 
part_no, SUM(qty_onhand - qty_reserved) 
FROM ifsapp.inventory_part_in_stock b 
WHERE b.contract=''12536''
AND qty_onhand - qty_reserved > 0 AND configuration_id IN (''GOOD'',''GOODC'',''HARVEST'') 
AND location_no NOT LIKE ''PLUS%''
AND location_no NOT LIKE ''QUAR%''
GROUP BY part_no
');
 
SELECT 
  ps.ProgramID,
  P.Name,
  pt.Reason,
  wh.ID as WorkOrderNo,
  roh.ID,
  roh.CustomerReference as ROCustomerRef, 
  ps.PartNo,
  (SELECT CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
   FROM pls.PartSerial
   WHERE SerialNo = ps.SerialNo AND ProgramID = ps.ProgramID) AS HAS_SN,
  ps.SerialNo, 
  crt.Description as RepairType, 
  wl.ComponentPartNo, 
  pn.Description as CompPartNoDesc, 
  usr.Username as [By], 
  ps.RODate as ReceiveDate,
  CASE WHEN ca.AttributeName is not null then ifst.total_quantity -- If available in the #IFSTable table
    ELSE 
      (SELECT SUM(pq.AvailableQty) 
       FROM pls.partqty PQ 
       INNER JOIN pls.PartLocation PL ON pq.ProgramID = pl.ProgramID 
                                      AND pq.LocationID = pl.ID 
                                      AND pl.Warehouse IN ('FGI', 'FLOORSTOCK') 
                                      AND PQ.programID = PS.programID 
                                      AND PQ.PartNo = wl.ComponentPartNo)
  END AS availqty,
  max(pt.CreateDate) as HoldDate, 
  DATEDIFF(DD, max(pt.CreateDate), GETDATE()) as DaysinHold
FROM 
  pls.PartSerial ps 
  INNER JOIN pls.PartTransaction pt ON ps.SerialNo = pt.SerialNo 
             AND pt.OrderType = 'WO' 
             AND ps.ProgramID = pt.ProgramID 
             AND ps.UserID = pt.UserID 
             AND CONVERT(DATETIME2(0), pt.CreateDate) = CONVERT(DATETIME2(0), ps.LastActivityDate) 
  INNER JOIN pls.[User] usr ON pt.UserID = usr.ID 
  LEFT OUTER JOIN pls.ROHeader roh ON ps.ROHeaderID = roh.ID 
             AND ps.ProgramID = roh.ProgramID 
  INNER JOIN pls.woheader wh ON pt.orderheaderid = wh.id 
  INNER JOIN pls.woline wl ON wh.id = wl.woheaderid 
             AND wl.ComponentPartNo <> ps.PartNo 
             AND wl.StatusID = 7 --New
  INNER JOIN pls.PartNo pn ON wl.ComponentPartNo = pn.PartNo 
  INNER JOIN pls.CodeRepairType crt ON wh.RepairTypeID = crt.ID
  INNER JOIN pls.Program P ON P.ID = PS.ProgramID
   LEFT JOIN pls.codeAttribute ca on ca.AttributeName = 'ERPProgramId'
  LEFT JOIN pls.ProgramAttribute PA ON PA.ProgramID = ps.ProgramID and pa.AttributeId = ca.Id
  LEFT JOIN #IFSTable ifst ON wl.ComponentPartNo = ifst.part_no
WHERE 
  (ps.StatusID = 28) 
  AND ((pt.Reason = 'Pendiente de Partes / Pending Parts') OR (pt.Reason LIKE 'Awaiting for Parts%'))
";

            }
            else
            {
                query = @"CREATE TABLE #ERPTable (
    part_no VARCHAR(255),
    total_quantity INT
);
                INSERT INTO #ERPTable (part_no, total_quantity)
SELECT*
FROM OPENQUERY(E2DB1, '
    SELECT
    PartNum,
    SUM(OnhandQty)
FROM
    [E2DB1].[cwdw].[ERP].[PartBin]
WHERE
    Company = ''VT02''

    AND OnhandQty > 0
    AND WarehouseCode IN(''JZPURCH'', ''JZPLUS'')
    AND BinNum IN(''ValveWIP'', ''WHSTOCK'', ''PLUSWIP'')
    AND PartNum IN(
        SELECT PartNum
        FROM[cwdw].[Erp].[Part]
        WHERE Company = ''VT02''
        AND ClassID = ''VLVP''
    )

    GROUP BY PartNum

    ;
                ');
SELECT
  ps.ProgramID,
  P.Name,
  pt.Reason,
  wh.ID as WorkOrderNo,
  roh.ID,
  roh.CustomerReference as ROCustomerRef, 
  ps.PartNo,
  (SELECT CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
   FROM pls.PartSerial
   WHERE SerialNo = ps.SerialNo AND ProgramID = ps.ProgramID) AS HAS_SN,
  ps.SerialNo, 
  crt.Description as RepairType, 
  wl.ComponentPartNo, 
  pn.Description as CompPartNoDesc, 
  usr.Username as [By], 
  ps.RODate as ReceiveDate,
  CASE WHEN ca.AttributeName is not null then ERPt.total_quantity-- If available in the #IFSTable table
    ELSE
     (SELECT SUM(pq.AvailableQty)
      FROM pls.partqty PQ

      INNER JOIN pls.PartLocation PL ON pq.ProgramID = pl.ProgramID

                                     AND pq.LocationID = pl.ID

                                     AND pl.Warehouse IN ('FGI', 'FLOORSTOCK') 
                                      AND PQ.programID = PS.programID
                                      AND PQ.PartNo = wl.ComponentPartNo)
  END AS availqty,
  max(pt.CreateDate) as HoldDate, 
  DATEDIFF(DD, max(pt.CreateDate), GETDATE()) as DaysinHold
FROM
  pls.PartSerial ps
  INNER JOIN pls.PartTransaction pt ON ps.SerialNo = pt.SerialNo
             AND pt.OrderType = 'WO'
             AND ps.ProgramID = pt.ProgramID
             AND ps.UserID = pt.UserID
             AND CONVERT(DATETIME2(0), pt.CreateDate) = CONVERT(DATETIME2(0), ps.LastActivityDate)
  INNER JOIN pls.[User] usr ON pt.UserID = usr.ID
  LEFT OUTER JOIN pls.ROHeader roh ON ps.ROHeaderID = roh.ID
             AND ps.ProgramID = roh.ProgramID
  INNER JOIN pls.woheader wh ON pt.orderheaderid = wh.id
  INNER JOIN pls.woline wl ON wh.id = wl.woheaderid
             AND wl.ComponentPartNo<> ps.PartNo
            AND wl.StatusID = 7--New
INNER JOIN pls.PartNo pn ON wl.ComponentPartNo = pn.PartNo
  INNER JOIN pls.CodeRepairType crt ON wh.RepairTypeID = crt.ID
  INNER JOIN pls.Program P ON P.ID = PS.ProgramID
   LEFT JOIN pls.codeAttribute ca on ca.AttributeName = 'ERPProgramId'
  LEFT JOIN pls.ProgramAttribute PA ON PA.ProgramID = ps.ProgramID and pa.AttributeId = ca.Id
  LEFT JOIN #ERPTable ERPt ON wl.ComponentPartNo = ERPt.part_no
WHERE
  (ps.StatusID = 28)
  AND((pt.Reason = 'Pendiente de Partes / Pending Parts') OR(pt.Reason LIKE 'Awaiting for Parts%') OR (pt.Reason LIKE 'Awaiting Parts%')) ";
            }
            if (!string.IsNullOrEmpty(partNo))
                query += "AND ps.PartNo LIKE '%" + partNo + "%'";

            if (programId != "0")
            {
                query += "AND ps.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "AND ps.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (sites == "PRAGUE")
            {
                query += @" GROUP BY 
                          ps.ProgramID,
                          p.Name,
                          pt.Reason,
                          wh.ID, 
                          roh.Id,
                          roh.CustomerReference, 
                          ps.PartNo, 
                          ps.SerialNo,
                          crt.Description, 
                          wl.ComponentPartNo, 
                          pn.Description, 
                          usr.Username, 
                          ps.RODate,
                          ca.AttributeName,
                          ifst.total_quantity";
            }
            else
            {
                query += @" GROUP BY 
                          ps.ProgramID,
                          p.Name,
                          pt.Reason,
                          wh.ID, 
                          roh.Id,
                          roh.CustomerReference, 
                          ps.PartNo, 
                          ps.SerialNo,
                          crt.Description, 
                          wl.ComponentPartNo, 
                          pn.Description, 
                          usr.Username, 
                          ps.RODate,
                          ca.AttributeName,
                          ERPt.total_quantity";
            }
            query += "\n Order by DaysInHold desc, SerialNo  ";
            if (sites == "PRAGUE")
            {
                query += "\n DROP TABLE #IFSTable;";
            }
            else
            {
                query += "\n DROP TABLE #ERPTable";
            }

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(partNo))
                filterString += "| Part No. Like '" + partNo + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("028", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPendingParts = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}