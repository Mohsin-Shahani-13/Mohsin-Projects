
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class InvoiceValve
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        [Display(Name = "Program:")]
        public string program { get; set; }

        [Display(Name = "Program:")]
        public string program_Id { get; set; }

        [Display(Name = "Part No.:")]
        public string PartNo { get; set; }

        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstData { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

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

        #endregion
        public bool GetList(string fromDt, string toDt, string programId, string ProgramName)
        {


            cDAL oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            string ipAddress = HttpContext.Current.Session["RemoteAddr"].ToString();
            //string deleteQuery = @"DELETE FROM PlusRS.rpt.Invoice_WOClosed  WHERE IpAddress = '" + ipAddress + "'";
            //oDAL.Execute(deleteQuery);
            query = @" WITH RepairLevelCTE AS (
    SELECT 
        t.ProgramID,
        t.OrderHeaderID,
        t.serialno,
        roh.CustomerReference,
        t.rmarec,
        t.woclose,
        ISNULL(ISNULL(rohret.value, rouanote.value), 'N/A') AS retreason,
        t.partno AS model,
        rouainc.value AS incpart,
        t.RepairTypeDescription,
        CASE
            WHEN wshcid.workstationID = 29 THEN 'Customer Induced Damage'
            ELSE ISNULL(cid.reason, 'N')
        END AS cid,
        t.repairstatus,
        rouawarr.value AS warr,
        COALESCE(rouawarrgng.value, rouawarr.value) AS InWarrantyOutGoing,
        wosa.value AS reid,
        (DATEDIFF(dd, t.rmarec, t.woclose) + 1) 
        - (DATEDIFF(wk, t.rmarec, t.woclose) * 2) 
        - CASE WHEN DATENAME(dw, t.rmarec) = 'Sunday' THEN 1 ELSE 0 END 
        - CASE WHEN DATENAME(dw, t.woclose) = 'Saturday' THEN 1 ELSE 0 END AS tat,
        t.multi,
        t.closedrep - 1 AS closedrep,
        CASE WHEN t.closedrep > 1 THEN 'Y' ELSE 'N' END AS repreturn,
        t.ro,
        MAX(CONVERT(VARCHAR, pna.id) + '_' + pna.Value) AS repairLvl,
        t.ship,
        t.prevship,
        CASE WHEN t.closedrep > 1 THEN DATEDIFF(DAY, t.prevship, t.rmarec) END AS rettat
    FROM (
        SELECT
            pt.programid,
            pt.partno,
            pt.serialno,
            pt.OrderHeaderID,
            FORMAT(pt.CreateDate, 'yyyy/MM/dd HH:mm') AS woclose,
            pt.ForYear,
            pt.ForMonth,
            ISNULL(psh.roheaderid, ps.roheaderid) AS ro,
            crt.Description AS RepairTypeDescription,
            wol.ComponentPartNo,
            wol.QtyConsumed,
            FORMAT(woh.CreateDate, 'yyyy/MM/dd HH:mm') AS rmarec,
            SUBSTRING(pl.LocationNo, 1, 3) AS repairstatus,
            (
                SELECT COUNT(*)
                FROM pls.parttransaction WITH (NOLOCK)
                WHERE serialno = pt.serialno
                  AND ProgramID = pt.ProgramID
                  AND PartTransactionID = 1
            ) AS multi,
            (
                SELECT SUM(inr.qty) - ISNULL(( 
                    SELECT SUM(ino.qty)
                    FROM pls.parttransaction ino WITH (NOLOCK)
                    WHERE ino.serialno = pt.SerialNo
                      AND ino.ProgramID = pt.ProgramID
                      AND ino.CreateDate < pt.CreateDate
                      AND ino.PartTransactionID = 2
                ), 0)
                FROM pls.parttransaction inr WITH (NOLOCK)
                WHERE inr.serialno = pt.SerialNo
                  AND inr.ProgramID = pt.ProgramID
                  AND inr.CreateDate < pt.CreateDate
                  AND inr.PartTransactionID = 1
            ) AS closedrep,
            (
                SELECT MIN(ship.createdate)
                FROM pls.parttransaction ship WITH (NOLOCK)
                WHERE ship.serialno = pt.serialno
                  AND ship.programid = pt.programid
                  AND ship.createdate > pt.createdate
                  AND ship.PartTransactionID = 18
            ) AS ship,
            (
                SELECT MAX(ship.createdate)
                FROM pls.parttransaction ship WITH (NOLOCK)
                WHERE ship.serialno = pt.serialno
                  AND ship.programid = pt.programid
                  AND ship.createdate < pt.createdate
                  AND ship.PartTransactionID = 18
            ) AS prevship,
            wcd.description
        FROM pls.parttransaction pt WITH (NOLOCK)
        INNER JOIN pls.WOHeader woh WITH (NOLOCK) ON woh.id = pt.OrderHeaderID
        LEFT JOIN pls.PartLocation pl WITH (NOLOCK) ON pl.ID = woh.DefaultLocationID
        LEFT JOIN pls.CodeRepairType crt WITH (NOLOCK) ON crt.ID = woh.RepairTypeID
        LEFT JOIN pls.codeworkstationcustomdescription wcd WITH (NOLOCK) ON wcd.RepairTypeID = woh.RepairTypeID
            AND wcd.CodeWorkStationID = woh.WorkStationID
        LEFT JOIN pls.WOLine wol WITH (NOLOCK) ON wol.WOHeaderID = woh.ID AND wol.statusid = 14 AND wol.ComponentPartNo != woh.PartNo
        LEFT JOIN pls.WOUnit wou WITH (NOLOCK) ON wou.WOLineID = wol.ID
        LEFT JOIN pls.PartSerial ps WITH (NOLOCK) ON ps.woheaderid = pt.orderheaderid AND ps.ProgramID = pt.ProgramID
        LEFT JOIN pls.PartSerialHistory psh WITH (NOLOCK) ON psh.woheaderid = pt.orderheaderid AND psh.ProgramID = pt.ProgramID
            AND psh.id = (SELECT MAX(id) FROM pls.PartSerialHistory pshck WITH (NOLOCK) WHERE psh.WOHeaderID = pshck.woheaderid AND pshck.ProgramID = psh.ProgramID)
        WHERE pt.PartTransactionID = 7

";
            if (programId != "0" && programId != null)
            {
                query += " AND pt.ProgramID = '" + programId + "' ";
                query = query.Replace("<programId>", programId);
            }

            query += "AND CONVERT(Date, pt.ForDate) >= '<frmDt>' AND CONVERT(Date, pt.ForDate) <= '<toDt>' ";

            query = query.Replace("<frmDt>", fromDt);
            query = query.Replace("<toDt>", toDt);
            query = query.Replace("<IpAddress>", ipAddress);


            query += @"
 AND pt.id = (SELECT MAX(ck.id) FROM pls.PartTransaction ck WITH (NOLOCK)
                       WHERE ck.OrderHeaderID = pt.OrderHeaderID AND ck.PartTransactionID = 7 AND ck.ProgramID = pt.ProgramID AND ck.ForMonth = pt.ForMonth)
          AND NOT EXISTS (
              SELECT rop.id
              FROM pls.parttransaction rop WITH (NOLOCK)
              WHERE rop.id > pt.id
                AND pt.orderheaderid = rop.orderheaderid
                AND rop.PartTransactionID = 9
                AND rop.ProgramID = pt.ProgramID
          )
    ) t
    INNER JOIN pls.ROHeader roh WITH (NOLOCK) ON roh.id = t.ro AND roh.ProgramID = t.programid
    INNER JOIN pls.ROLine rol WITH (NOLOCK) ON rol.ROHeaderID = roh.ID AND rol.PartNo = t.PartNo
    INNER JOIN pls.ROUnit rou WITH (NOLOCK) ON rou.ROLineID = rol.ID AND rou.SerialNo = t.SerialNo
    LEFT JOIN pls.vROUnitAttribute rouainc WITH (NOLOCK) ON rouainc.ROUnitID = rou.Id AND rouainc.AttributeName = 'INCOMINGPARTNO'
    LEFT JOIN pls.vROHeaderAttribute rohret WITH (NOLOCK) ON rohret.ROHeaderID = roh.Id AND rohret.AttributeName = 'ReturnReason'
    LEFT JOIN pls.vROUnitAttribute rouanote WITH (NOLOCK) ON rouanote.ROUnitID = rou.Id AND rouanote.AttributeName = 'REPAIRNOTES'
    LEFT JOIN pls.vROUnitAttribute rouawarr WITH (NOLOCK) ON rouawarr.ROUnitID = rou.Id AND rouawarr.AttributeName = 'INWARRANTY'
    LEFT JOIN pls.vROUnitAttribute rouawarrGng WITH (NOLOCK) ON rouawarrGng.ROUnitID = rou.Id AND rouawarrGng.AttributeName = 'INWARRANTYOUTGOING'
    LEFT JOIN pls.vPartNoAttribute pna WITH (NOLOCK) ON pna.ProgramID = roh.ProgramID AND pna.PartNo = t.componentPartNo AND pna.AttributeName = 'REPAIR LEVEL'
    LEFT JOIN pls.WOStationHistory WSH WITH (NOLOCK) ON WSH.WOHeaderID = t.OrderHeaderID AND WSH.WorkStationID = '17'
    LEFT JOIN pls.vWOStationAttribute WOSA WITH (NOLOCK) ON WOSA.Attribute = 'ReIDPartNumber' AND WOSA.WOStationHistoryID = WSH.ID
	LEFT JOIN pls.WOStationHistory WSHCID with (nolock)
  ON WSHCID.WOHeaderID = t.OrderHeaderID
  AND WSHCID.WorkStationID = '29'  -- cid confirm
  AND WSHCID.ID = (SELECT
    MAX(wshc.id)
  FROM pls.WOStationHistory wshc with (nolock)
  WHERE WSHc.WOHeaderID = t.OrderHeaderID
  AND wshc.WorkStationID = '29')
	LEFT JOIN pls.PartTransaction cid with (nolock)
  ON cid.PartTransactionID = 12
  AND cid.SerialNo = t.serialno
  AND t.OrderHeaderID = cid.OrderHeaderID
  AND cid.ProgramID = t.ProgramID
  AND cid.Reason = 'Customer Induced Damage'
    GROUP BY 
        t.ProgramID, t.OrderHeaderId, t.serialno, roh.CustomerReference, 
        t.rmarec, t.woclose, rohret.Value, rouainc.Value, rouanote.Value, 
        t.PartNo, t.RepairTypeDescription, wshcid.workstationID, cid.Reason, 
        t.repairstatus, rouawarr.Value, rouawarrgng.value, wosa.value, t.multi, 
        t.closedrep, t.ro, t.ship, t.prevship
)
SELECT *,
       SUBSTRING(repairlvl, CHARINDEX('_', repairlvl) + 1, 25) AS RepairLevel
FROM RepairLevelCTE
ORDER BY woclose, serialno;

";

            query = query.Replace("<IpAddress>", ipAddress);
            DataTable dt = oDAL.GetDataForGeneric(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(PartNo))
                filterString += "| Part No. Like '" + PartNo + "' ";

            filterString += " | From = '" + fromDt + "' To = '" + toDt + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("132", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstData = cCommon.ConvertDtToHashTable(dt);
                return true;

            }

        }

        public bool GetDetail(string fromDt, string toDt, string programId, string ProgramName)
        {

            string query = "detail";
            query = @"
select t.ProgramID, t.OrderHeaderID,t.serialno, roh.CustomerReference, 
FORMAT(t.rmarec, 'yyyy/MM/dd HH:mm') AS rmarec ,
FORMAT(t.woclose, 'yyyy/MM/dd HH:mm') AS woclose ,
isnull(isnull(rohret.value,rouanote.value),'N\A') as retreason, 
t.partno as model, 
rouainc.value as incpart, 
t.RepairTypeDescription ,
(case when wshcid.workstationID =  29 then 'Customer Induced Damage' else isnull(cid.reason,'N') end)cid,
t.repairstatus, 
rouawarr.value as warr,
--CASE WHEN rouawarrgng.value is null THEN rouawarr.value ELSE rouawarrgng.value END AS InWarrantyoutGoing,
   COALESCE(rouawarrgng.value, rouawarr.value) AS InWarrantyoutGoing,
wosa.value as reid,
(DATEDIFF(dd, t.rmarec, t.woclose) + 1)- (DATEDIFF(wk, t.rmarec, t.woclose) * 2)- (CASE WHEN DATENAME(dw, t.rmarec) = 'Sunday' THEN 1 ELSE 0 END) -(CASE WHEN DATENAME(dw, t.woclose) = 'Saturday' THEN 1 ELSE 0 END)tat,
  t.multi, t.closedrep-1 as closedrep, (case when t.closedrep > 1 then 'Y' else 'N' end)repreturn, t.ro, t.componentpartno,
  (case when t.componentpartno is null  then 
  (case when t.description != 'RE ID unit' then t.description
   when t.description = 'RE ID unit' then 'NFF' 
   else  'NFF'
      end )else t.faults end)faults, 
      t.repairs, pna.value as repairlevel, t.ship, t.prevship,
  (case when t.closedrep > 1 then datediff(day, t.prevship, t.rmarec) end)rettat
 
from (
SELECT pt.programid, pt.partno, pt.serialno, pt.OrderHeaderID, pt.CreateDate woclose, pt.ForYear, pt.ForMonth, isnull(psh.roheaderid,ps.roheaderid) ro,crt.Description as RepairTypeDescription, wol.ComponentPartNo, wol.QtyConsumed, woh.CreateDate rmarec,
substring(pl.LocationNo,1,3)repairstatus, cf.Description AS Faults,cr.Description AS Repairs,
(select count(*) from pls.parttransaction  with (nolock) where serialno = pt.serialno and ProgramID = pt.ProgramID and PartTransactionID = 1 )multi,
(select sum(inr.qty)- isnull((select sum(ino.qty) from pls.parttransaction ino with (nolock) where ino.serialno = pt.SerialNo and ino.ProgramID = <programId> and ino.CreateDate < pt.CreateDate and ino.PartTransactionID = 2 ),0)
from pls.parttransaction inr with (nolock)
where inr.serialno = pt.SerialNo and inr.ProgramID = <programId> and inr.CreateDate < pt.CreateDate and inr.PartTransactionID = 1)closedrep,
(select min(ship.createdate)from pls.parttransaction ship with (nolock) where ship.serialno = pt.serialno and ship.programid = pt.programid and ship.createdate > pt.createdate  and ship.PartTransactionID = 18)ship,
(select max(ship.createdate)from pls.parttransaction ship with (nolock) where ship.serialno = pt.serialno and ship.programid = pt.programid  and ship.createdate < pt.createdate and ship.PartTransactionID = 18)prevship,
wcd.description
 from pls.parttransaction pt with (nolock)
inner join pls.WOHeader woh with (nolock) on woh.id = pt.OrderHeaderID
LEFT JOIN pls.PartLocation pl with (nolock) ON pl.ID = woh.DefaultLocationID
LEFT JOIN pls.CodeRepairType crt with (nolock) ON crt.ID = woh.RepairTypeID
LEFT JOIN pls.codeworkstationcustomdescription wcd with (nolock) on wcd.RepairTypeID = woh.RepairTypeID and wcd.CodeWorkStationID = woh.WorkStationID
LEFT JOIN pls.WOLine wol with (nolock) ON wol.WOHeaderID = woh.ID and wol.statusid = 14 and wol.ComponentPartNo != woh.PartNo
LEFT JOIN pls.WOUnit wou with (nolock) ON wou.WOLineID = wol.ID
LEFT JOIN pls.WOUnitCodes wouc with (nolock) ON wouc.WOUnitID = wou.ID
LEFT JOIN pls.CodeFault cf with (nolock) ON cf.ID = wouc.FaultID
LEFT JOIN pls.CodeRepair cr with (nolock) ON cr.ID = wouc.RepairID
left join pls.PartSerial ps with (nolock) on ps.woheaderid = pt.orderheaderid and  ps.ProgramID = pt.ProgramID AND ISNULL(woh.SourcePartNo , woh.PartNo)=ps.PartNo
left join pls.PartSerialHistory psh with (nolock) on psh.woheaderid = pt.orderheaderid and  psh.ProgramID = pt.ProgramID and psh.id = (select max(id) from pls.PartSerialHistory pshck where
 psh.WOHeaderID = pshck.woheaderid and pshck.ProgramID = psh.ProgramID)
   where pt.PartTransactionID = 7
";
            query += "AND CONVERT(Date, pt.ForDate) >= '<frmDt>' AND CONVERT(Date, pt.ForDate) <= '<toDt>' ";
            query = query.Replace("<frmDt>", fromDt);
            query = query.Replace("<toDt>", toDt);


            if (programId != null && programId != "0")
            {
                query += "AND pt.ProgramId = '" + programId + "' ";
                query = query.Replace("<programId>", programId);
            }

            query = query.Replace("<frmDt>", fromDt);
            query = query.Replace("<toDt>", toDt);

            query += @"
and pt.id = (select max(ck.id) from pls.PartTransaction ck with (nolock) where ck.OrderHeaderID = pt.OrderHeaderID and ck.PartTransactionID = 7 and ck.ProgramID = pt.ProgramID and ck.ForMonth = pt.ForMonth
 )
 and not exists (select rop.id from pls.parttransaction rop with (nolock) where rop.id > pt.id and pt.orderheaderid = rop.orderheaderid and rop.PartTransactionID = 9 and rop.ProgramID = pt.ProgramID )
)t
INNER JOIN pls.ROHeader roh with (nolock) ON roh.id = t.ro and roh.ProgramID = t.programid
INNER JOIN pls.ROLine rol with (nolock) ON rol.ROHeaderID = roh.ID AND rol.PartNo = t.PartNo
INNER JOIN pls.ROUnit rou with (nolock) ON rou.ROLineID = rol.ID AND rou.SerialNo = t.SerialNo
LEFT JOIN pls.vROUnitAttribute rouainc with (nolock) ON rouainc.ROUnitID = rou.Id and rouainc.AttributeName = 'INCOMINGPARTNO'
left JOIN pls.vROHeaderAttribute rohret with (nolock) ON rohret.ROHeaderID = roh.Id and rohret.AttributeName = 'ReturnReason'
left JOIN pls.vROUnitAttribute rouanote with (nolock) ON rouanote.ROUnitID = rou.Id and rouanote.AttributeName = 'REPAIRNOTES'
left JOIN pls.vROUnitAttribute rouawarr with (nolock) ON rouawarr.ROUnitID = rou.Id and rouawarr.AttributeName = 'INWARRANTY'
left JOIN pls.vROUnitAttribute rouawarrGng with (nolock) ON rouawarrGng.ROUnitID = rou.Id and rouawarrGng.AttributeName = 'INWARRANTYOUTGOING'
left join pls.vPartNoAttribute pna  with (nolock) on pna.ProgramID = roh.ProgramID and pna.PartNo = t.componentPartNo and pna.AttributeName = 'REPAIR LEVEL'
left JOIN pls.WOStationHistory WSH with (nolock) ON WSH.WOHeaderID = t.OrderHeaderID and wsh.WorkStationID = 17
and wsh.ID = (select max(wshc.id) from pls.WOStationHistory wshc with (nolock) where WSHc.WOHeaderID = t.OrderHeaderID and wshc.WorkStationID = 17)
left join pls.vWOStationAttribute WOSA with (nolock) on WOSA.Attribute = 'ReIDPartNumber' and WOSA.WOStationHistoryID = WSH.ID
left JOIN pls.WOStationHistory WSHCID with (nolock) ON WSHCID.WOHeaderID = t.OrderHeaderID and WSHCID.WorkStationID = 29 
and WSHCID.ID = (select max(wshc.id) from pls.WOStationHistory wshc with (nolock) where WSHc.WOHeaderID = t.OrderHeaderID and wshc.WorkStationID = 29)
left join pls.PartTransaction cid with (nolock) on cid.PartTransactionID = 12 and cid.SerialNo = t.serialno and t.OrderHeaderID = cid.OrderHeaderID and cid.ProgramID = t.ProgramID and cid.Reason = 'Customer Induced Damage'
ORDER BY t.woclose,t.serialno
  ";


            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            //if (!string.IsNullOrEmpty(PartNo))
            //    filterString += "| Part No. Like '" + PartNo + "' ";

            filterString += " | From = '" + fromDt + "' To = '" + toDt + "' ";



            DataTable dt = oDAL.GetDataForGeneric(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("132", query, "detail", false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstData = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

    }
}