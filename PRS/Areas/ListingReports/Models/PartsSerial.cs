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
    public class PartsSerial
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string WOId { get; set; }
        public string SOId { get; set; }
        public string ROId { get; set; }
        public string ProgramID { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Serial No.: Like (Please Enter min. of 3 chracters)")]
        public string serialNo { get; set; }
        //[Display(Name = "Serial No.:")]
        //public string serialNo { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Parent Serial No.:")]
        public string parentserialno { get; set; }
        [Display(Name = "Location No.:")]
        public string locationNo { get; set; }
        [Display(Name = "Status:")]
        public string status { get; set; }
        [Display(Name = "RO No.:")]
        public string ROHcustomerreference { get; set; }
        [Display(Name = "RO Date:")]
        public string rodate { get; set; }
        [Display(Name = "WO No.:")]
        public string WOHNo { get; set; }
        [Display(Name = "WO Station:")]
        public string station { get; set; }
        [Display(Name = "WO Start Date:")]
        public string wostartdate { get; set; }
        [Display(Name = "WO End Date:")]
        public string woenddate { get; set; }
        [Display(Name = "Description:")]
        public string description { get; set; }
        [Display(Name = "SO No.:")]
        public string SONo { get; set; }
        [Display(Name = "SO Date:")]
        public string sodate { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstPartsSerial { get; set; }
        public List<Hashtable> lstTransDetail { get; set; }

        public List<Hashtable> lstShopfloorDetail { get; set; }
        public List<Hashtable> lstComponentDetail { get; set; }
        public List<Hashtable> lstDetail { get; set; }
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
        public bool GetList(string frmDt, string toDt, bool isAllDate, string serialNo, string programId, string ProgramName, string partNo, string locNo, string palletBoxNo, string config, string originFrom)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT  
        PS.ProgramID,
        PS.SerialNo,   
        PS.PalletBoxNo,
		PS.ParentSerialNo,
        PN.PartNo,		 
		PL.LocationNo,
        P.Name As Program,
		CS.Description AS Status, 		
		U.Username, 
		FORMAT(PS.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn,
        FORMAT(PS.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
INNER JOIN pls.PartNo PN ON PN.PartNo = PS.PartNo
INNER JOIN pls.PartLocation PL ON PL.id = PS.LocationID
INNER JOIN pls.CodeStatus CS ON CS.ID = PS.StatusID
INNER JOIN pls.[User] U ON U.ID = PS.UserID 
";

            if (originFrom == "PartsQty")
            {
                query += @"LEFT JOIN pls.PartTransaction pt ON pt.ProgramID = ps.ProgramID
                                                                AND pt.PartNo = ps.PartNo 
                                                                AND pt.SerialNo = ps.SerialNo 
                                                                AND pt.PartTransactionId = 35 
                                                                AND ps.StatusId = 14
";
            }


            if (programId != "0")
                query += "WHERE P.ID = '" + programId + "' ";
            else
                query += "WHERE P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";


            if (originFrom == "PartsQty")
                query += "AND ps.StatusID NOT IN (18, 8, 32) AND pt.ID IS NULL "; // excluded consumed and shipped records

            //if (originFrom != "PartsQty" && isAllDate == false)
            //{
            //    query += "AND CONVERT(Date, PS.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, PS.LastActivityDate) <= '<toDt>'";
            //}
            //query = query.Replace("<frmDt>", frmDt);
            //query = query.Replace("<toDt>", toDt);

            if (!string.IsNullOrEmpty(serialNo))
                query += "AND PS.SerialNo LIKE '%" + serialNo + "%' ";
            if (!string.IsNullOrEmpty(partNo))
                query += "AND ps.PartNo = '" + partNo + "' ";
            if (!string.IsNullOrEmpty(locNo))
                query += "AND ps.LocationId = '" + locNo + "' ";
            if (!string.IsNullOrEmpty(palletBoxNo))
                query += "AND PS.PalletBoxNo = '" + palletBoxNo + "' ";
            if (!string.IsNullOrEmpty(config))
                query += "AND PS.ConfigurationId = '" + config + "' ";

            query += "Order BY PS.LastActivityDate DESC";


            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            //if (conType == "PROD")
            //{
            //    query = query.Replace("PalletBoxNo", "PalletNo");
            //}
            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            //if (originFrom != "PartsQty" && isAllDate == false)
            //    filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
                if (!string.IsNullOrEmpty(serialNo))
                filterString += "| Serial No. Like '" + serialNo + "' ";


            //For SQL Documentation
        
            cLog oLog = new cLog();
            oLog.AddSqlQuery("007", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPartsSerial = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
        public bool GetDetail(string serialNo, string programId,string PartNo)
        {
            string query = string.Empty;

            #region Header

            query = @"
SELECT 
        PS.ProgramID,
       ROH.ID AS ROId,
       WOH.ID AS WOId,
       SO.ID AS SOId,
       P.Name Program,
       PS.serialNo,
       PS.partno,
       PS.parentserialno,
       PL.LocationNo,
       CS.Description AS Status,
       ROH.customerreference AS ROHNo,
       PS.rodate,
       WOH.customerreference WOHNo,
        CASE
            WHEN wsd.Code IS NULL THEN cws.Description
            ELSE wsd.Description
        END
       AS Station,
       PS.wostartdate,
       PS.woenddate,
       CWS.description,
       SO.customerreference AS SONo,
       PS.sodate
FROM   pls.partserial PS
INNER JOIN pls.Program P ON p.ID = PS.ProgramID
INNER JOIN pls.PartLocation PL ON PL.ID = PS.LocationID
INNER JOIN pls.CodeStatus CS ON CS.ID = PS.StatusID
LEFT JOIN pls.roheader ROH ON ROH.id = PS.roheaderid
LEFT JOIN pls.woheader WOH ON WOH.id = PS.woheaderid
LEFT JOIN pls.codeworkstation CWS ON CWS.id = PS.workstationid 
LEFT JOIN pls.CodeWorkStationCustomDescription wsd ON
				   wsd.ProgramID = woh.ProgramID 
				   AND wsd.RepairTypeID = woh.RepairTypeID
				   AND wsd.CodeWorkStationID = ps.WorkStationID
LEFT JOIN pls.soheader SO ON SO.id = PS.soheaderid
WHERE  ps.serialno = '<serialNo>' AND PS.ProgramID = '<programId>' AND PS.partno = '<PartNo>'  ";


            //if (!string.IsNullOrEmpty(PartNo))
            //    query += "AND ps.PartNo = '" + PartNo + "' ";

            query = query.Replace("<serialNo>", serialNo);
            query = query.Replace("<programId>", programId);
            query = query.Replace("<PartNo>", PartNo);


            filterString = "Serial No. = '" + serialNo + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("007-1", query, "Header", false);

            //oDAL = new cDAL(HttpContext.Current.Request["DB"]);
            DataTable dtHeader = oDAL.GetData(query);

            if (dtHeader.Rows.Count > 0)
            {
                DataRow dr = dtHeader.Rows[0];
                serialNo = serialNo;
                ProgramID = dr["ProgramID"].ToString();
                ROId = dr["ROId"].ToString();
                WOId = dr["WOId"].ToString();
                SOId = dr["SOId"].ToString();
                program = dr["Program"].ToString();
                partNo = dr["partno"].ToString();
                parentserialno = dr["parentserialno"].ToString();
                locationNo = dr["LocationNo"].ToString();
                status = dr["Status"].ToString();
                ROHcustomerreference = dr["ROHNo"].ToString();              
                rodate = dr["rodate"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["rodate"]).ToString("yyyy.MM.dd HH:mm");
                WOHNo = dr["WOHNo"].ToString();
                station = dr["Station"].ToString();
                wostartdate = dr["wostartdate"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["wostartdate"]).ToString("yyyy.MM.dd HH:mm");
                woenddate = dr["woenddate"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["woenddate"]).ToString("yyyy.MM.dd HH:mm");
                description = dr["description"].ToString();
                SONo = dr["SONo"].ToString();
                sodate = dr["sodate"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["sodate"]).ToString("yyyy.MM.dd HH:mm");              
                //lastActivityOn = dr["LastActivityDate"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["LastActivityDate"]).ToString("yyyy.MM.dd HH:mm:ss");

            }
            #endregion

            #region
            query = @"


---Work Order Transaction---
SELECT 
      CASE WHEN CPT.Description = 'WO-WIP' THEN  WIP.CreatedOn ELSE FORMAT(PT.CREATEDATE, 'yyyy.MM.dd HH:mm:ss:fff') END AS CREATEDATE, 
       CPT.Description AS PartTransaction, 
	   WIP.WorkStation,
       QTY, 
       SOURCE, 
       CONDITION, 
       CONFIGURATION, 
       LOCATION, 
       TOLOCATION, 
	   CASE WHEN CPT.Description = 'WO-WIP' THEN WIP.Reason ELSE PT.Reason END AS Reason,
	   CASE WHEN CPT.Description = 'WO-WIP' THEN WIP.CreatedBy ELSE U.Username END AS CreatedBy,
	   WIP.Iteration,
	   WIP.IsPass,
	   WIP.Fault
FROM PLS.PARTTRANSACTION AS PT
INNER JOIN Pls.Program P ON P.ID = PT.programId
INNER JOIN pls.CodePartTransaction CPT ON CPT.ID = PT.PARTTRANSACTIONID
INNER JOIN Pls.[User] U ON U.ID = PT.USERID
LEFT OUTER JOIN
				(SELECT  wsh.WOHeaderID,  
				CASE WHEN wsd.Code IS NULL THEN cws.Description ELSE wsd.Code END AS WorkStation
					   , wsh.Iteration
					   , CASE WHEN wsh.IsPass = 1 THEN 'Y' WHEN wsh.IsPass = 0 THEN 'N' ELSE NULL END IsPass   
					   , cft.Description AS Reason
					   , cf.description AS Fault
					   , usr.FirstName + ' ' + usr.LastName AS CreatedBy
					   , FORMAT(wsh.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn
				FROM pls.WOStationHistory wsh
				INNER JOIN pls.WOHeader woh ON
						   woh.ID = wsh.WOHeaderID
				INNER JOIN pls.CodeWorkStation cws ON
						   cws.ID = wsh.WorkStationID
				LEFT JOIN pls.CodeWorkStationCustomDescription wsd ON
						   wsd.ProgramID = woh.ProgramID
						   AND wsd.RepairTypeID = woh.RepairTypeID
						   AND wsd.CodeWorkStationID = wsh.WorkStationID
				LEFT JOIN pls.WOStationHistoryFailReasons whf ON
						   whf.WOStationHistoryId = wsh.ID
				LEFT JOIN pls.CodeFault cft ON
						   cft.ID = whf.FaultID
				INNER JOIN pls.WOLine WOL ON WOL.WOHeaderID = WOH.ID and wol.ComponentPartNo = woh.PartNo
                   LEFT JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID
                   LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = WOU.ID
                   LEFT JOIN pls.CodeFault CF ON CF.ID = WUC.FaultID
				INNER JOIN pls.[User] usr ON
						  usr.Id = wsh.UserId
				WHERE WOH.ProgramId = '<programId>'
					  AND WOH.SerialNo = '<serialNo>'
					  AND WOH.PartNo = '<PartNo>'
				) AS WIP ON WIP.WOHeaderID = PT.OrderHeaderID AND CPT.Description = 'WO-WIP'


WHERE PT.SerialNo = '<serialNo>' AND PT.ProgramID = '<programId>' AND PT.PartNo = '<PartNo>'
ORDER BY CREATEDATE DESC

-------Component-------

SELECT
 WOH.ProgramID
 , WOL.ComponentPartNo
, CCN.[Description] AS Configuration
, SUM(WOU.QtyIssued) AS QtyIssued
, SUM(WOU.QtyConsumed) AS QtyConsumed
, PLN.LocationNo
, CST.Description AS LineStatus
, WOU.SerialNo
, CFT.Description AS Defects
, CRP.Description AS RepairDesc
, CASE WHEN WUC.RepairID IS NOT NULL THEN USR.Username ELSE NULL END AS RepairedBy
, CASE WHEN WUC.RepairID IS NOT NULL THEN WUC.LastActivityDate ELSE NULL END AS RepairedOn
FROM pls.WOUnit WOU
INNER JOIN pls.WOLine WOL ON WOL.ID = WOU.WOLineID
INNER JOIN pls.WOHeader WOH ON WOH.ID = WOL.WOHeaderID
LEFT JOIN pls.CodeConfiguration CCN ON CCN.ID = WOU.ConfigurationID
LEFT JOIN pls.PartLocation PLN ON PLN.ID = WOU.FromLocationID
LEFT JOIN pls.CodeStatus CST ON CST.ID = WOL.StatusID
LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = WOU.ID
LEFT JOIN pls.CodeFault CFT ON CFT.ID = WUC.FaultID
LEFT JOIN pls.CodeRepair CRP ON CRP.ID = WUC.RepairID
LEFT JOIN pls.[USER] USR ON USR.ID = WUC.UserID
WHERE WOH.PartNo <> WOL.ComponentPartNo
AND WOH.ProgramID = '<programId>'
AND WOH.SerialNo = '<serialNo>'
AND WOH.PartNo = '<PartNo>'
GROUP BY WOH.ProgramID
,WOL.ComponentPartNo
, CCN.Description
, CST.Description
, PLN.LocationNo
, WOU.SerialNo
, CFT.Description
, CRP.Description
, USR.Username
, WUC.RepairID
, WUC.LastActivityDate


";

            query = query.Replace("<programId>", programId);
            query = query.Replace("<serialNo>", serialNo);
            query = query.Replace("<PartNo>", PartNo);

            DataSet DS = oDAL.GetDataSet(query);
            //cLog oLog = new cLog();
            
            oLog.AddSqlQuery("007-2", query, string.Empty, false);
            if (!oDAL.HasErrors)
            {
                List<ArrayList> lstDtl = new List<ArrayList>();

                lstDtl = cCommon.ConvertDtToArrayList(DS.Tables[0]);
                lstShopfloorDetail = cCommon.ConvertDtToHashTable(DS.Tables[0]);

                lstDtl = cCommon.ConvertDtToArrayList(DS.Tables[1]);
                lstComponentDetail = cCommon.ConvertDtToHashTable(DS.Tables[1]);

                //lstDtl = cCommon.ConvertDtToArrayList(DS.Tables[2]);
                //lstTransDetail = cCommon.ConvertDtToHashTable(DS.Tables[2]);

                return true;

            }
            else
            {
                return false;
            }

        }
        #endregion
    }
}


