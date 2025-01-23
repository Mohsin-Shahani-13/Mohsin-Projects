using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class WO
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
        [Display(Name = "Status:")]
        public string description { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }

        //[Display(Name = "Status:")]
        //public string statusId { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "Program:")]
        public string ProgramID { get; set; }
    
        [Display(Name = "Repair Type:")]
        public string returnReason { get; set; }
        [Display(Name = "Subtitute Part:")]
        public string SubstitutePart { get; set; }

        [Display(Name = "Substitute Serial:")]
        public string SubstituteSerial { get; set; }

        [Display(Name = "Created On:")]
        public string createDate { get; set; }
        [Display(Name = "Last Activity On:")]
        public string lastActivityDate { get; set; }

        [Display(Name = "Workstation:")]
        public string workstation { get; set; }

        [Display(Name = "Location No.:")]
        public string locationNo { get; set; }

        [Display(Name = "Created By:")]
        public string username { get; set; }
        public bool ischecked { get; set; }
        public bool ischecked2 { get; set; }
        public bool ischecked3 { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstWO { get; set; }
        public List<Hashtable> lstDetail { get; set; }
        public List<Hashtable> lstWOUnit { get; set; }
        public List<Hashtable> lstRepairedUnits { get; set; }

        [Display(Name = "Status:")]
        public string Status { get; set; }
        public List<ArrayList> lstStatus { get; set; }
        [Display(Name = "Repair Type:")]
        public string Repair { get; set; }
        public bool widget { get; set; }
        public List<ArrayList> lstRepair { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion

        #region Methods 
        public DataTable Program()
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

        //public DataTable Status()
        //{
        //    string query = string.Empty;
        //    query = @"SELECT DISTINCT CS.ID,
        //  CS.Description		
        //        FROM pls.CodeStatus CS
        //        Inner Join pls.WOHeader On CS.ID = StatusID 
        //        ORDER BY CS.Description	ASC";
        //    //query = @"SELECT DISTINCT Id, Description FROM pls.CodeStatus ORDER BY Description ";
        //    DataTable dt = oDAL.GetData(query);
        //    return dt;
        //}

        public bool GetStatus()
        {
            string query = string.Empty;
            query = @"SELECT DISTINCT CS.ID,
		        CS.Description		
                FROM pls.CodeStatus CS
                Inner Join pls.WOHeader On CS.ID = StatusID 
                ORDER BY CS.Description	ASC";
            //query = @"SELECT DISTINCT Id, Description FROM pls.CodeStatus ORDER BY Description ";
            DataTable dt = oDAL.GetData(query);
            lstStatus = cCommon.ConvertDtToArrayList(dt);
            if (!oDAL.HasErrors)
                return true;
            else
                return false;
        }
        public bool Repairtype()
        {
            string programid = HttpContext.Current.Session["ProgramForSite"].ToString();
            string query = string.Empty;
            query = @"SELECT DISTINCT	   CRT.ID,
            	   CRT.Description 	   
                   FROM   pls.WOHeader WOH
                   INNER JOIN pls.CodeRepairType CRT ON CRT.ID = WOH.RepairTypeID 
              WHERE WOH.ProgramID IN (<programid>) ";

            query = query.Replace("<programid>", programid);
            DataTable dt = oDAL.GetData(query);
            lstRepair = cCommon.ConvertDtToArrayList(dt);
            if (!oDAL.HasErrors)
                return true;
            else
                return false;
        }

        public bool GetWO(string Id, string frmDt, string toDt, bool isAllDate, bool ischecked3, string custRef, string status, string statusId, string Repair, string RepairTypeID, bool ischecked, string type, string ProgramID, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT WOH.ID, 
       WOH.ProgramID,
       P.Name AS Program,
       WOH.CustomerReference,   
       (select CustomerReference
        from pls.ROHeader where id = (select ROHeaderID
        from pls.ROLine where id = (select max(ROLineID)
        from pls.rounit where SerialNo = WOH.SerialNo))) AS ROCustomerReference,
       WOH.PartNo,
       WOH.SerialNo,
       (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
       FROM pls.partserial PS
       WHERE PS.SerialNo = WOH.SerialNo AND PS.ProgramID = WOH.ProgramID ) HAS_SN,
       CRT.Description As RepairType,
       (select value
        from pls.PartNoAttribute PNA
        where pna.ProgramID = WOH.ProgramID and PNA.PartNo = WOH.PartNo and pna.AttributeID = 278) AS FamilyAttribute,
       CASE WHEN wsd.Code IS NULL THEN cws.ID ELSE wsd.ID END AS WorkstationID,
       CASE WHEN wsd.Code IS NULL THEN cws.Description ELSE wsd.Description END As Workstation,
       CS.Description AS Status,
       SUM(CAST(QtyRequested AS bigint)) AS QtyRequested,
       SUM(CAST(QtyConsumed AS bigint)) AS QtyConsumed,
      
       U.Username AS CreatedBy, 
       FORMAT(WOH.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn, 
       FORMAT(WOH.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
FROM   pls.WOHeader WOH
INNER JOIN pls.[User] U ON U.ID = WOH.UserID 
INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
LEFT JOIN pls.WOLine WOL ON WOL.WOHeaderID = WOH.ID
LEFT OUTER JOIN pls.CodeRepairType CRT ON CRT.ID = WOH.RepairTypeID
LEFT OUTER JOIN pls.CodeWorkStation CWS ON CWS.ID = WOH.WorkstationID
LEFT JOIN pls.CodeWorkStationCustomDescription wsd ON
                   wsd.ProgramID = WOH.ProgramID 
                   AND wsd.RepairTypeID = WOH.RepairTypeID
                   AND wsd.CodeWorkStationID = WOH.WorkStationID
LEFT OUTER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID
";
            if (ProgramID != "0" && ProgramID != null)
            {
                query += " WHERE P.ID = '" + ProgramID + "' ";
            }
            else
            {
                query += " WHERE P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (ischecked3 != true && !type.Equals("REPAIR WO"))
            {
                query += "AND CONVERT(Date, WOH.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, WOH.LastActivityDate) <= '<toDt>' ";
            }
            if (!string.IsNullOrEmpty(statusId) && type.Equals("REPAIR WO") || type.Equals("RESERVED WO") || type.Equals("PRD WO") && ischecked ==true  || ischecked2 == true && ischecked3 != true)
            
            {

                query += " AND CS.ID  IN ('<ID>') ";
                //query += " WHERE CRT.ID  IN ('<RepairTypeID>') ";

                
                query = query.Replace("<ID>", statusId);
                //query = query.Replace("<RepairTypeID>", RepairTypeID);
                query += @"GROUP BY
       WOH.ID, 
       WOH.ProgramID,
       P.Name, 
       WOH.CustomerReference,
       CRT.Description,
       U.Username,
       WOH.CreateDate,
       WOH.PartNo,
       WOH.SerialNo,
       wsd.Code,cws.ID,
       wsd.ID,
       cws.Description,
       wsd.Description,
       CS.Description,
       WOH.LastActivityDate ";

                query += "ORDER BY CreatedOn DESC";
                //filterString = "Status = '" + status + "'";
                if (!string.IsNullOrEmpty(ProgramName))
                {
                    filterString = " Program = '" + ProgramName + "' | Open work orders ";
                }
                else
                {
                    filterString = " Status = '"+status+"' ";
                }
                
            }
            else
            {

                if (ischecked3 != false && isAllDate !=true)
                {
                    query += "AND CONVERT(Date, WOH.CreateDate) >= '<frmDt>' AND CONVERT(Date, WOH.CreateDate) <= '<toDt>' ";
                }

               

                if (ischecked3 != false )
                {

                    query += "AND CS.ID IN (19,28)";
                }


                query = query.Replace("<frmDt>", frmDt);
                    query = query.Replace("<toDt>", toDt);

                if (!string.IsNullOrEmpty(custRef))
                    query += "AND WOH.CustomerReference LIKE '%" + custRef + "%'";

                if (!status.Equals("All") && ischecked == true || ischecked2 == true)
                    query += "AND CS.ID  IN (" + statusId + ") ";

                if (!status.Equals("All") && ischecked == false && ischecked2 == false && ischecked3 == false)
                    query += "AND CS.ID  IN (" + statusId + ") ";

                if (!Repair.Equals("All") && RepairTypeID != "" )
                    query += "AND CRT.ID IN (" + RepairTypeID + ") ";

                

                if (ischecked == true)
                {
                    query += "AND WOH.WorkStationID = 4 AND WOH.StatusID = 19 ";
                }

                query += @"GROUP BY
       WOH.ID, 
       WOH.ProgramID,
       P.Name, 
       WOH.CustomerReference,
       CRT.Description,
       U.Username,
       WOH.CreateDate,
       WOH.PartNo,
       WOH.SerialNo,
       wsd.Code,cws.ID,
       wsd.ID,
       cws.Description,
       wsd.Description,
       CS.Description,
       WOH.LastActivityDate ";

                query += "ORDER BY CreatedOn DESC";

                //if (!string.IsNullOrEmpty(ProgramName))
                //    filterString = "> Program = '" + ProgramName + "' ";

                if (string.IsNullOrEmpty(custRef) && !string.IsNullOrEmpty(ProgramName) && ischecked3 != true)
                    filterString = " Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDt + "' | Status = '" + status + "'" + "' | Repair = '" + Repair + "'";

                else if (!string.IsNullOrEmpty(custRef) && !string.IsNullOrEmpty(ProgramName) && ischecked3 != false)
                 
                filterString = " Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDt + "' | Customer Ref. Like '" + custRef + "' | Open Work Order ";

                else if (!string.IsNullOrEmpty(ProgramName) && ischecked3 != false && isAllDate != true)
                    filterString = " Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDt + "' | Open Work Order ";
                else if (!string.IsNullOrEmpty(custRef) && !string.IsNullOrEmpty(ProgramName) && ischecked3 != true)
                    filterString = "Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDt + "'  | Customer Ref. Like '" + custRef + "'   | Status = '" + status + "'" + "' | Repair = '" + Repair + "'";

                else if (!string.IsNullOrEmpty(custRef) && !string.IsNullOrEmpty(ProgramName) && ischecked3 != false)
                    filterString = " Program = '" + ProgramName + "'  | From = '" + frmDt + "' To = '" + toDt + "' | Customer Ref. Like '" + custRef + "'  | Open Work Order ";

                else if (!string.IsNullOrEmpty(ProgramName) && ischecked3 != false && isAllDate != false)
                    filterString = "Program = '" + ProgramName + "' | Open Work Order ";

                else
                    filterString = "Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDt + "' | Customer Ref. Like '" + custRef + "' ";

                if (ischecked == true)
                {
                    filterString += " | Showing units at close station but not yet closed ";
                }
            }
            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("008", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWO = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool GetRepairedUnit(string frmDt, string toDate, string custRef, string status, string statusid, string Repair, string RepairTypeID, string programId, string ProgramName, bool ischecked2)
        {
            
            string query = string.Empty;
            query = @"
-- 10001 Repair
SELECT * 
FROM (
SELECT distinct WOH.ID as WOID, 
       WOH.ProgramID, 
       P.Name Program,
	   WOH.SerialNo,
       (select CustomerReference
		from pls.ROHeader where id = (select ROHeaderID
		from pls.ROLine where id = (select max(ROLineID)
		from pls.rounit where SerialNo = WOH.SerialNo))) AS RMA,
	   WOH.PartNo AS Model,
	   CRT.Description As RepairType,
	   FORMAT(WOSH.CreateDate, 'yyyy.MM.dd HH:mm') AS RepairDate,
	   CASE woh.RepairTypeID WHEN 42 THEN '1' WHEN 71 THEN '3' ELSE MAX(pna.Value) OVER (PARTITION BY WOH.id) END RepairLevel,    
	   (select value
	    from pls.PartNoAttribute PNA
		where pna.ProgramID = WOH.ProgramID and PNA.PartNo = WOH.PartNo and pna.AttributeID = 354) AS InvoiceFamily
FROM   pls.WOHeader WOH
INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
INNER JOIN pls.WOLine WOL ON WOH.ID = WOL.WOHeaderID
INNER JOIN pls.CodeRepairType CRT ON CRT.ID = WOH.RepairTypeID
INNER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID
INNER JOIN pls.wostationhistory WOSH
     ON WOH.id = WOSH.woheaderid
	 AND WOSH.WorkStationID = 12
	 AND WOSH.Iteration = 1 
LEFT JOIN pls.PartNoAttribute pna
	ON	WOH.ProgramID = pna.ProgramID
	and WOL.ComponentPartNo = pna.PartNo
	and pna.AttributeID = 149
";
            if (programId != "0" && programId != null)
            {
                query += "WHERE WOH.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "WHERE WOH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += "AND WOH.RepairTypeID = 41 ";
            query += "AND WOH.StatusID != 3 ";

            query += "AND CONVERT(Date, WOSH.CreateDate) >= '<frmDt>' AND CONVERT(Date, WOSH.CreateDate) <= '<toDt>'";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            if (!string.IsNullOrEmpty(custRef))
                query += "AND WOH.CustomerReference LIKE '%" + custRef + "%'";

            
            query += @"UNION
-- 10001 Kitting
SELECT distinct WOH.ID as WOID, 
       WOH.ProgramID,  
       P.Name Program,
	   WOH.SerialNo,
       (select CustomerReference
		from pls.ROHeader where id = (select ROHeaderID
		from pls.ROLine where id = (select max(ROLineID)
		from pls.rounit where SerialNo = WOH.SerialNo))) AS RMA,
	   WOH.PartNo AS Model,
	   CRT.Description As RepairType,
	   FORMAT(WOH.LastActivityDate, 'yyyy.MM.dd HH:mm') AS RepairDate,
	   CASE woh.RepairTypeID WHEN 42 THEN '1' WHEN 71 THEN '3' ELSE MAX(pna.Value) OVER (PARTITION BY WOH.id) END RepairLevel,    
	   (select value
	    from pls.PartNoAttribute PNA
		where pna.ProgramID = WOH.ProgramID and PNA.PartNo = WOH.PartNo and pna.AttributeID = 354) AS InvoiceFamily
FROM   pls.WOHeader WOH
INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
INNER JOIN pls.WOLine WOL ON WOH.ID = WOL.WOHeaderID
INNER JOIN pls.CodeRepairType CRT ON CRT.ID = WOH.RepairTypeID
INNER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID
LEFT JOIN pls.PartNoAttribute pna
	ON	WOH.ProgramID = pna.ProgramID
	and WOL.ComponentPartNo = pna.PartNo
	and pna.AttributeID = 149 ";

            if (programId != "0" && programId != null)
            {
                query += "WHERE WOH.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "WHERE WOH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += "AND WOH.RepairTypeID = 42 ";
            query += "AND CS.ID = 15 ";

            query += "AND CONVERT(Date, WOH.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, WOH.LastActivityDate) <= '<toDt>' ";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            if (!string.IsNullOrEmpty(custRef))
                query += "AND WOH.CustomerReference LIKE '%" + custRef + "%' ";
            query += ")TEMP where TEMP.RMA not like '%CAVPC%' ";

            //////////FILTER STRINGS///////


            //if (string.IsNullOrEmpty(custRef) && !string.IsNullOrEmpty(ProgramName))
            //    filterString = "| Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDate + "'";

            //else
            //    filterString = "| Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDate + "' | Customer Ref. Like '" + custRef + "' ";


            if (string.IsNullOrEmpty(custRef) && !string.IsNullOrEmpty(ProgramName))
                filterString = "Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDt + "'" ;
            else
                filterString = "Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDt + "' | Customer Ref. Like '" + custRef + "' ";
            DataTable dt = oDAL.GetData(query);

            if (ischecked2 == true)
            {
                filterString += " | Repaired Units ";
            }

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("008", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRepairedUnits = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion

        public bool GetDetail(string Id, string custRef)
        {
            //oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);
            string query = string.Empty;

            #region Header

            query = @"
SELECT  P.ID,
        P.Name AS Program,
        CS.Description AS Status,
	    WOH.PartNo,
	    WOH.SerialNo, 
	    CRT.Description As RepairType, 
	    CASE WHEN wsd.Code IS NULL THEN CWS.Description ELSE wsd.Description END AS WorkStation,
	    PL.LocationNo AS Location,
        U.Username, 
	    FORMAT(WOH.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn, 
	    FORMAT(WOH.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
FROM   pls.WOHeader WOH
INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
INNER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID
LEFT OUTER JOIN pls.CodeRepairType CRT ON CRT.ID = WOH.RepairTypeID
INNER JOIN pls.CodeWorkStation CWS ON CWS.ID = WOH.WorkStationID
LEFT JOIN pls.CodeWorkStationCustomDescription wsd ON
				   wsd.ProgramID = WOH.ProgramID 
				   AND wsd.RepairTypeID = WOH.RepairTypeID
				   AND wsd.CodeWorkStationID = WOH.WorkStationID
INNER JOIN pls.[User] U ON U.ID = WOH.UserID
INNER JOIN pls.PartLocation PL ON PL.ID = WOH.DefaultLocationID
WHERE WOH.ID = '<Id>'
ORDER BY CreatedOn DESC
";
            query = query.Replace("<Id>", Id);
            filterString = "Customer Ref. = '" + custRef + "' ";
            cLog oLog = new cLog();
            oLog.AddSqlQuery("008-1", query, "Header", false);
            DataTable dtHeader = oDAL.GetData(query);

            if (dtHeader.Rows.Count > 0)
            {
                DataRow dr = dtHeader.Rows[0];
                ProgramID = dr["ID"].ToString();
                program = dr["Program"].ToString();
                partNo = dr["PartNo"].ToString();
                serialNo = dr["SerialNo"].ToString();
                description = dr["Status"].ToString();
                returnReason = dr["RepairType"].ToString();
                username = dr["Username"].ToString();
                workstation = dr["Workstation"].ToString();
                locationNo = dr["Location"].ToString();
                createDate = dr["CreatedOn"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["CreatedOn"]).ToString("yyyy.MM.dd HH:mm");
                lastActivityDate = dr["LastActivityOn"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["LastActivityOn"]).ToString("yyyy.MM.dd HH:mm");

            }
            #endregion


            query = @"
------WO TRANSACTION------

SELECT  WOH.ProgramID,
        WOL.Id, 
        WOL.ComponentPartNo AS PartNo, 
        WOL.QtyRequested,
        WOL.QtyConsumed,
        CS.Description AS Status,
        U.Username,
        FORMAT(WOL.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn, 
	    FORMAT(WOL.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
FROM   pls.WOLine WOL
INNER JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID
LEFT JOIN pls.CodeStatus CS ON CS.ID = WOL.StatusID
INNER JOIN pls.[User] U ON U.ID = WOL.UserID
INNER JOIN pls.WOHeader WOH ON WOH.ID = WOL.WOHeaderID
WHERE WOL.WOHeaderId = '<Id>' 
ORDER BY CreatedOn DESC;

------UNIT TRANSACTION------

SELECT WOU.ID, 
        WOH.PartNo,
(SELECT  CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
        FROM pls.PartSerial
        WHERE SerialNo = WOU.SerialNo) AS HAS_SN,
        PL.ProgramID,
	    WOU.SerialNo,
        WOU.QtyIssued, 
        WOU.QtyConsumed,
	    PL.LocationNo AS Location,
	    U.Username,
        FORMAT(WOU.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn, 
	    FORMAT(WOU.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
FROM pls.WOUnit WOU
--INNER JOIN [pls].[WOUnitAttribute] WUA ON WUA.WOUnitID = WOU.ID
INNER JOIN pls.WOLine WOL ON WOL.ID = WOU.WOLineID
INNER JOIN pls.WOHeader WOH ON WOH.ID = WOL.WOHeaderID
INNER JOIN pls.[User] U ON U.ID = WOU.UserID
LEFT JOIN pls.PartLocation PL ON PL.ID = WOU.FromLocationID
WHERE WOL.WOHeaderID = <Id>
  ";

            query = query.Replace("<Id>", Id);
            DataSet DS = oDAL.GetDataSet(query);
            oLog.AddSqlQuery("008-2", query, string.Empty, false);
            if (!oDAL.HasErrors)
            {
                List<ArrayList> lstDtl = new List<ArrayList>();
                lstDtl = cCommon.ConvertDtToArrayList(DS.Tables[0]);
                lstDetail = cCommon.ConvertDtToHashTable(DS.Tables[0]);

                lstDtl = cCommon.ConvertDtToArrayList(DS.Tables[1]);
                lstWOUnit = cCommon.ConvertDtToHashTable(DS.Tables[1]);
                return true;
            }
            else
            {
                return false;
            }
        }
        //        public bool GetDetail(string Id, string custRef)
        //        {
        //            string query = string.Empty;

        //            #region Header

        //            query = @"
        //SELECT P.Name AS Program,
        //       CS.Description AS Status,
        //	   WOH.PartNo,
        //	   WOH.SerialNo, 
        //	   COT.Description AS OrderType, 
        //	   --WOA.SubstitutePart, 
        //       -- WOA.SubstituteSerial, 
        //	   CWS.Description AS WorkStation,
        //	   PL.LocationNo AS Location,
        //       U.Username, 
        //	   FORMAT(WOH.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn, 
        //	   FORMAT(WOH.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
        //FROM   pls.WOHeader WOH
        //--INNER JOIN pls.WOHeaderAttribute WOA ON WOH.ID = WOA.WOHeaderID
        //INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
        //INNER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID
        //INNER JOIN pls.CodeOrderType COT ON COT.ID = WOH.RepairTypeID
        //INNER JOIN pls.CodeWorkStation CWS ON CWS.ID = WOH.WorkStationID
        //--LEFT JOIN pls.CodeAddress CA ON CA.ID = WOH.WorkStationID
        //INNER JOIN pls.[User] U ON U.ID = WOH.UserID
        //INNER JOIN pls.PartLocation PL ON PL.ID = WOH.DefaultLocationID
        //WHERE WOH.ID = '<Id>'
        //ORDER BY LastActivityOn DESC ";
        //            query = query.Replace("<Id>", Id);

        //            filterString = "Customer Ref. = '" + custRef + "' ";

        //            //For SQL Documentation
        //            cLog oLog = new cLog();
        //            oLog.AddSqlQuery("008-1", query, "Header", false);

        //            // oDAL = new cDAL(HttpContext.Current.Request["DB"]);
        //            DataTable dtHeader = oDAL.GetData(query);

        //            if (dtHeader.Rows.Count > 0)
        //            {
        //                DataRow dr = dtHeader.Rows[0];
        //                custRef = custRef;

        //                program = dr["Program"].ToString();
        //                partNo = dr["PartNo"].ToString();
        //                serialNo = dr["SerialNo"].ToString();
        //                description = dr["Status"].ToString();
        //                returnReason = dr["OrderType"].ToString();
        //                username = dr["Username"].ToString();
        //                workstation = dr["Workstation"].ToString();
        //                locationNo = dr["Location"].ToString();
        //                createDate = dr["CreatedOn"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["CreatedOn"]).ToString("yyyy.MM.dd HH:mm:ss");
        //                lastActivityDate = dr["LastActivityOn"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["LastActivityOn"]).ToString("yyyy.MM.dd HH:mm:ss");

        //            }
        //            #endregion

        //            #region
        //            query = @"
        //SELECT     WOL.Id, 
        //        -- Comp. Part No.
        //       WOL.ComponentPartNo AS PartNo, 
        //        -- CC.Description AS Configuration,
        //       WOL.QtyRequested,
        //       WOL.QtyConsumed,
        //        -- WLA.OrderCode,
        //        -- WLA.SupplyCode,
        //        -- WLA.RootCause,
        //       CS.Description AS Status,
        //       U.Username,
        //        FORMAT(WOL.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn, 
        //	   FORMAT(WOL.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
        //FROM   pls.WOLine WOL
        //INNER JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID
        //--LEFT JOIN pls.WOLineAttribute WLA ON WLA.WOLineID = WOL.ID
        //--LEFT JOIN pls.CodeConfiguration CC ON CC.ID = WOL.ConfigurationID 
        //INNER JOIN pls.CodeStatus CS ON CS.ID = WOL.StatusID
        //INNER JOIN pls.[User] U ON U.ID = WOL.UserID
        //WHERE WOL.WOHeaderId = '<Id>' 
        //ORDER BY LastActivityOn DESC ";

        //            query = query.Replace("<Id>", Id);
        //            //For SQL Documentation
        //            oLog.AddSqlQuery("008-2", query, "---Detail---", false);
        //            // oDAL = new cDAL(HttpContext.Current.Request["DB"]);
        //            DataTable dt = oDAL.GetData(query);

        //            if (!oDAL.HasErrors)
        //            {
        //                if (dt.Rows.Count > 0)
        //                {
        //                    lstDetail = cCommon.ConvertDtToHashTable(dt);
        //                    return true;
        //                }
        //            }

        //            return false;
        //        }
        //        #endregion
        //        public bool GetWOUnit(string Id, string statusId)
        //        {
        //            string query = string.Empty;
        //            query = @"
        //SELECT WOU.ID, 
        //(SELECT  CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
        //        FROM pls.PartSerial
        //        WHERE SerialNo = WOU.SerialNo) AS HAS_SN,
        //	   WOU.SerialNo,
        //       WOU.QtyIssued, 
        //       WOU.QtyConsumed,
        //	   PL.LocationNo AS Location,
        //	   CF.Description AS Fault,
        //	   CR.Description AS Repair,
        //	   CS.Description AS Section,
        //	   U.Username,
        //       FORMAT(WOU.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn, 
        //	   FORMAT(WOU.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
        //FROM pls.WOUnit WOU
        //--INNER JOIN [pls].[WOUnitAttribute] WUA ON WUA.WOUnitID = WOU.ID
        //INNER JOIN pls.WOLine WOL ON WOL.ID = WOU.WOLineID
        //INNER JOIN pls.[User] U ON U.ID = WOU.UserID
        //INNER JOIN pls.PartLocation PL ON PL.ID = WOU.LocationID
        //INNER JOIN pls.CodeFault CF ON CF.ID = WOU.FaultID
        //INNER JOIN pls.CodeRepair CR ON CR.ID = WOU.RepairID
        //INNER JOIN [pls].[CodeSection] CS ON CS.ID = WOU.SectionID
        //WHERE WOL.WOHeaderID = <Id>";

        //            query = query.Replace("<Id>", Id);

        //            if (!string.IsNullOrEmpty(statusId))
        //                query += "AND WOL.StatusID = '" + statusId + "' ";


        //            //For SQL Documentation
        //            cLog oLog = new cLog();
        //            oLog.AddSqlQuery("008-3", query, "---WO Unit---", false);



        //            DataTable dt = oDAL.GetData(query);

        //            if (!oDAL.HasErrors)
        //            {
        //                if (dt.Rows.Count > 0)
        //                {
        //                    lstWOUnit = cCommon.ConvertDtToHashTable(dt);
        //                }
        //                return true;
        //            }
        //            return false;
        //        }
    }


}
