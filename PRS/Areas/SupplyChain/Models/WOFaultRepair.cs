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
    public class WOFaultRepair
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Customer Ref.:")]
        public string custRef { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable Program() // onHand warehouse method
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }

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
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        public bool ischecked { get; set; }
        public List<Hashtable> lstFaultRepair { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string programId, string programName, string serialNo, bool ischecked, string custRef)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            if (ischecked == true)
            {
                query = @"
select     pt.programid,   
           P.Name,
		   pt.PartTransactionID,
           cpt.Description AS TranType,
		   crt.Description RepairType,
           pt.partno,
           (SELECT  CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
           FROM pls.PartSerial
           WHERE SerialNo = PT.SerialNo AND ProgramID = pt.programid) AS HAS_SN,
           pt.serialno,
           pt.orderheaderid,
           wh.customerreference,
           (select CustomerReference
		   from pls.ROHeader where ID = (select ROHeaderID
		   from pls.ROLine where id = (select max(ROLineID)
		   from pls.rounit where SerialNo = wh.SerialNo))) AS ROCustomerReference,
           <symptom>
           wl.componentpartno,
		   pnc.Description CompPartNoDesc,
           pt.Configuration,
		   wl.qtyrequested,
           wl.qtyconsumed,
           cs.description as Status,
		   pt.CreateDate,
           wl.lastactivitydate,
           cf.code AS faultcode,
           cf.description as Fault,
           cr.code AS repaircode,
           cr.description as Repair,
           (select Username from pls.[user] where pls.[user].id =
		   (select top (1) UserID from pls.WOStationHistory wsh
		   where woHeaderId = pt.OrderHeaderID and
		   workStationId in (13,14) order by LastActivityDate desc)) AS RepairedBy,
           u.username, 
		   pt.CreateDate,
		   (
	select top (1) case when IsPass = 1 Then 'Pass' Else 'Fail' End from pls.WOStationHistory wsh where woHeaderId = pt.orderheaderid and workStationId = 13 order by LastActivityDate desc
) AS gTask2
,(
	select top (1) case when IsPass = 1 Then 'Pass' Else 'Fail' End from pls.WOStationHistory wsh where woHeaderId = pt.orderheaderid and workStationId = 14 order by LastActivityDate desc
) AS gTask3
,CASE crt.ID WHEN 42 THEN '1' WHEN 71 THEN '3' ELSE MAX(pna.Value) OVER (PARTITION BY pt.orderheaderid) END RepairLevel
FROM       pls.parttransaction pt
INNER JOIN pls.Program P ON P.ID = pt.ProgramID
INNER JOIN pls.CodePartTransaction cpt ON cpt.ID = pt.PartTransactionId
INNER JOIN pls.woheader wh ON pt.orderheaderid = wh.id AND pt.ProgramID = wh.ProgramID
INNER JOIN pls.woline wl
ON         wh.id = wl.woheaderid
INNER JOIN pls.wounit wu
ON         wl.id = wu.wolineid
INNER JOIN pls.CodeRepairType crt
ON		   wh.RepairTypeID = crt.ID
INNER JOIN pls.PartNo pn
ON		   pn.PartNo = pt.PartNo
INNER JOIN pls.PartNo pnc
ON		   pnc.PartNo = wl.componentpartno
LEFT JOIN pls.wounitcodes wc
ON         wu.id = wc.wounitid
LEFT JOIN pls.coderepair cr
ON         wc.repairid = cr.id
LEFT JOIN pls.codefault cf
ON         wc.faultid = cf.id
LEFT JOIN pls.codestatus cs
ON         wl.statusid = cs.id
LEFT JOIN pls.[User] u
ON         wc.userid = u.id
LEFT JOIN pls.PartNoAttribute pna
ON	pt.ProgramID = pna.ProgramID
and wl.ComponentPartNo = pna.PartNo
and pna.AttributeID = 149

WHERE CONVERT(Date, pt.CreateDate) >= '<frmDt>' AND CONVERT(Date, pt.CreateDate) <= '<toDt>' and PT.OrderType = 'WO' 
AND PT.PartTransactionID = 7
AND cs.ID = '14' ";
            }

            else
            {
                query = @"
select     pt.programid,   
           P.Name,
		   pt.PartTransactionID,
           cpt.Description AS TranType,
		   crt.Description RepairType,
           pt.partno,
           (SELECT  CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
           FROM pls.PartSerial
           WHERE SerialNo = PT.SerialNo AND ProgramID = pt.programid) AS HAS_SN,
           pt.serialno,
           pt.orderheaderid,
           wh.customerreference,
           (select CustomerReference
		   from pls.ROHeader where ID = (select ROHeaderID
		   from pls.ROLine where id = (select max(ROLineID)
		   from pls.rounit where SerialNo = wh.SerialNo))) AS ROCustomerReference,
           <symptom>
           wl.componentpartno,
		   pnc.Description CompPartNoDesc,
		   pt.Configuration,
           wl.qtyrequested,
           wl.qtyconsumed,
           cs.description as Status,
		   pt.CreateDate,
           wl.lastactivitydate,
           cf.code AS faultcode,
           cf.description as Fault,
           cr.code AS repaircode,
           cr.description as Repair,
           (select Username from pls.[user] where pls.[user].id =
		   (select top (1) UserID from pls.WOStationHistory wsh
		   where woHeaderId = pt.OrderHeaderID and
		   workStationId in (13,14) order by LastActivityDate desc)) AS RepairedBy,
           u.username, 
		   pt.CreateDate,
		   (
	select top (1) case when IsPass = 1 Then 'Pass' Else 'Fail' End from pls.WOStationHistory wsh where woHeaderId = pt.orderheaderid and workStationId = 13 order by LastActivityDate desc
) AS gTask2
,(
	select top (1) case when IsPass = 1 Then 'Pass' Else 'Fail' End from pls.WOStationHistory wsh where woHeaderId = pt.orderheaderid and workStationId = 14 order by LastActivityDate desc
) AS gTask3
FROM       pls.parttransaction pt
INNER JOIN pls.Program P ON P.ID = pt.ProgramID
INNER JOIN pls.CodePartTransaction cpt ON cpt.ID = pt.PartTransactionId
INNER JOIN pls.woheader wh ON pt.orderheaderid = wh.id AND pt.ProgramID = wh.ProgramID
INNER JOIN pls.woline wl
ON         wh.id = wl.woheaderid
INNER JOIN pls.wounit wu
ON         wl.id = wu.wolineid
INNER JOIN pls.CodeRepairType crt
ON		   wh.RepairTypeID = crt.ID
INNER JOIN pls.PartNo pn
ON		   pn.PartNo = pt.PartNo
INNER JOIN pls.PartNo pnc
ON		   pnc.PartNo = wl.componentpartno
LEFT JOIN pls.wounitcodes wc
ON         wu.id = wc.wounitid
LEFT JOIN pls.coderepair cr
ON         wc.repairid = cr.id
LEFT JOIN pls.codefault cf
ON         wc.faultid = cf.id
LEFT JOIN pls.codestatus cs
ON         wl.statusid = cs.id
LEFT JOIN pls.[User] u
ON         wc.userid = u.id
WHERE CONVERT(Date, pt.CreateDate) >= '<frmDt>' AND CONVERT(Date, pt.CreateDate) <= '<toDt>' and PT.OrderType = 'WO'  
AND PT.PartTransactionID = 7

";
            }


            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            if (programName == "BOSE")
            {
                string colSql = @"(select [value] from pls.ROHeaderAttribute where AttributeID = 144 and ROHeaderID = (select ID
		                           from pls.ROHeader where ID = (select ROHeaderID
		                           from pls.ROLine where id = (select max(ROLineID)
		                           from pls.rounit where SerialNo = wh.SerialNo )))) AS Symptom,";
                query = query.Replace("<symptom>", colSql);
            }
            else
            {
                query = query.Replace("<symptom>", "");
            }

            if (!string.IsNullOrEmpty(serialNo))
                query += "AND pt.SerialNo LIKE '%" + serialNo + "%'";

            if (!string.IsNullOrEmpty(custRef))
                query += "AND wh.CustomerReference LIKE '%" + custRef + "%'";

            if (programId != "0")
            {
                query += "AND pt.ProgramId = '" + programId + "' ";
            }
            else
            {
                query += "AND pt.ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += "ORDER BY pt.CreateDate DESC";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            filterString += "| From = '" + frmDt + "' To = '" + toDt + "' ";

            if (!string.IsNullOrEmpty(serialNo))
                filterString += "| Serial No. Like '" + serialNo + "' ";

            if (!string.IsNullOrEmpty(custRef))
                filterString += "| Customer Ref. Like '" + custRef + "' ";

            if (ischecked == true)
            {
                filterString += " | Show Consumed Parts Only ";
            }


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("023", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstFaultRepair = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}