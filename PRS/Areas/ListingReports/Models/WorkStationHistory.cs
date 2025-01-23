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
    public class WorkStationHistory
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Serial No.")]
        public string serialNo { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public bool ischecked { get; set; }
        public List<Hashtable> lstWorkStationHistory { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName, bool ischecked, bool isAllDate, string serialNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            if (ischecked != true)
            {
                query = @" 
SELECT	  Distinct
          WOH.ProgramID
        , P.Name ProgramName
        , WOH.Id
        , WOH.PartNo
        , PN.Description AS PartDesc
		, CR.Description RepairDescription
		, CASE WHEN wsd.Code IS NULL THEN CONCAT(wsd.Code, ' - ', cws.Description) ELSE CONCAT(wsd.Code, ' - ', wsd.Description) END AS FromWorkStation
	    , CASE WHEN CWSD.Code IS NULL THEN CONCAT(CWSD.Code, ' - ', CWOS.Description) ELSE CONCAT(CWSD.Code, ' - ', CWSD.Description) END AS ToWorkStation
		, CASE WHEN woh.IsPass = 0 THEN 'Failed' ELSE 'Pass' END  AS WorkOrderResult 
		, CASE WHEN WSH.IsPass = 0 THEN 'Failed' ELSE 'Pass' END  AS WorkstationResult
		, wsh.Iteration
        , woh.SerialNo
        , (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
			FROM pls.partserial PS
			WHERE PS.SerialNo = woh.SerialNo AND PS.ProgramID = woh.ProgramID AND PS.partno = woh.partno ) HAS_SN
		, woh.CustomerReference	
        ,ROH.CustomerReference AS ROCustRef
		, CF.Code
		, CF.Description
		,CRT.Description As WorkType 
		,WSA.Value As FaultCode1
		,WSA1.Value As FaultCode2
		,WSA2.Value As RelIdPartNumber
			, U.Username AS [By]
		, WSH.CreateDate AS [On]
        , WSH.LastActivityDate 
        , WSH.StartDate
		, WSH.EndDate
FROM pls.WOStationHistory WSH
INNER JOIN pls.WOHeader WOH ON WOH.ID = WSH.WOHeaderID
INNER JOIN pls.Program P ON P.ID = WOH.ProgramId
INNER JOIN pls.PartNo PN ON PN.PartNo = WOH.PartNo
INNER JOIN pls.WOLine WOL ON WOL.WOHeaderID = WOH.ID
left Join pls.PartSerial PS ON PS.WOHeaderID = WOH.ID AND PS.SerialNo = WOH.SerialNo 
Left JOIN pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID
LEFT JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID AND WOU.ConsumeWorkStationID = WSH.WorkStationID

LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = WOU.ID
LEFT OUTER JOIN pls.CodeFault CF ON CF.ID = WUC.FaultID 
LEFT OUTER JOIN pls.CodeRepair CR ON CR.ID = WUC.RepairID
INNER JOIN pls.CodeWorkStation CWS ON
CWS.ID = WOH.WorkStationID
INNER JOIN pls.CodeWorkStationCustomDescription WSD ON wsd.ProgramID = woh.ProgramID
AND WSD.RepairTypeID = WOH.RepairTypeID
AND WSD.CodeWorkStationID = Wsh.WorkStationID
INNER JOIN pls.CodeWorkStation CWOS ON
CWOS.ID = WOH.WorkStationID
INNER JOIN pls.CodeWorkStationCustomDescription CWSD ON CWSD.ProgramID = WOH.ProgramID
AND CWSD.RepairTypeID = WOH.RepairTypeID
AND CWSD.CodeWorkStationID = wsh.ToWorkStationID
INNER JOIN Pls.CodeRepairType CRT ON CRT.ID = WOH.RepairTypeID

--LEFT JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID AND WOU.ConsumeWorkStationID = WSH.WorkStationID
 

LefT Join pls.CodeAttribute CA On CA.AttributeName ='FaultCode1'
Left Join pls.WOStationAttribute WSA On WSA.AttributeID = CA.ID AND WSA.WOStationHistoryID =WSH.ID


LefT Join pls.CodeAttribute CA1 On CA1.AttributeName ='FaultCode2'
Left Join pls.WOStationAttribute WSA1 On WSA1.AttributeID = CA1.ID AND WSA1.WOStationHistoryID =WSH.ID

Left Join pls.CodeAttribute CA2 On CA2.AttributeName='ReIDPartNumber'
Left Join pls.WOStationAttribute WSA2 On WSA2.AttributeID =CA2.ID And WSA2.WOStationHistoryID = WSH.ID

LEFT JOIN pls.[User] U ON U.ID = WSH.UserId
WHERE 1=1
 ";
            }
            else
            {
                query = @" 
SELECT    WOH.ProgramID
        , P.Name ProgramName
        , WOH.Id
        , WOH.PartNo
        , PN.Description AS PartDesc
		, CR.Description RepairDescription
		, CASE WHEN wsd.Code IS NULL THEN cws.Description ELSE wsd.Description END AS FromWorkStation
	    , CASE WHEN CWSD.Code IS NULL THEN CWOS.Description ELSE CWSD.Description END AS ToWorkStation
        , CASE WHEN woh.IsPass = 0 THEN 'Failed' ELSE 'Pass' END  AS WorkOrderResult 
		, CASE WHEN WSH.IsPass = 0 THEN 'Failed' ELSE 'Pass' END  AS WorkstationResult
		, wsh.Iteration
        , woh.SerialNo
		, (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
		   FROM pls.partserial PS
	       WHERE PS.SerialNo = woh.SerialNo AND PS.ProgramID = woh.ProgramID ) HAS_SN
	    , woh.CustomerReference	
        , CF.Code
        , CF.Description
		, U.Username AS [By]
		, WSH.CreateDate AS [On]
        , WSH.LastActivityDate 
        , WSH.StartDate
		, WSH.EndDate
FROM pls.WOStationHistory WSH
INNER JOIN pls.WOHeader WOH ON WOH.ID = WSH.WOHeaderID
INNER JOIN pls.Program P ON P.ID = WOH.ProgramId
INNER JOIN pls.PartNo PN ON PN.PartNo = WOH.PartNo
INNER JOIN pls.WOLine WOL ON WOL.WOHeaderID = WOH.ID	
AND WOL.ComponentPartNo = WOH.PartNo					
INNER JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID		
LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = WOU.ID	
LEFT OUTER JOIN pls.CodeFault CF ON CF.ID = WUC.FaultID	
LEFT OUTER JOIN pls.CodeRepair CR ON CR.ID = WUC.RepairID
AND (WUC.FaultId IS NULL OR WUC.FaultId IN (1019,1023,1053,1054))
INNER JOIN pls.CodeWorkStation CWS ON
CWS.ID = WOH.WorkStationID
INNER JOIN pls.CodeWorkStationCustomDescription WSD ON wsd.ProgramID = woh.ProgramID
AND WSD.RepairTypeID = WOH.RepairTypeID
AND WSD.CodeWorkStationID = Wsh.WorkStationID
INNER JOIN pls.CodeWorkStation CWOS ON
CWOS.ID = WOH.WorkStationID
INNER JOIN pls.CodeWorkStationCustomDescription CWSD ON CWSD.ProgramID = WOH.ProgramID
AND CWSD.RepairTypeID = WOH.RepairTypeID
AND CWSD.CodeWorkStationID = wsh.ToWorkStationID
--LEFT JOIN pls.WOStationHistoryFailReasons WHF ON WHF.WOStationHistoryId = WSH.ID
LEFT JOIN pls.[User] U ON U.ID = WSH.UserId

WHERE WOH.StatusID = 15
AND wsh.WorkStationID = 14 AND Wsh.ToWorkStationID = 13
";          }


            if (isAllDate != true)
            {
                query += "AND CONVERT(Date, WSH.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, WSH.LastActivityDate) <= '<toDt>'";
            }
            //if (!string.IsNullOrEmpty(ProgramName))
            //    filterString += "> Program = '" + ProgramName + "' ";

            //filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            if (programId != "0")
            {
                query += "AND WOH.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "AND WOH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (!string.IsNullOrEmpty(serialNo))
            {
                query += "AND woh.SerialNo LIKE '%" + serialNo + "%'";
            }


            query += "ORDER BY WSH.LastActivityDate DESC";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (ischecked == true)
            {
                filterString += " | No Problem Found ";
            }

            if (isAllDate != true)
            {
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            }
            if (!string.IsNullOrEmpty(serialNo))
                filterString += "| Serial No. Like '" + serialNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("116", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWorkStationHistory = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}