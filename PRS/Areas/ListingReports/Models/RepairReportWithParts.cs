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
    public class RepairReportWithParts
    {
		cDAL oDAL = new cDAL("ACTIVE");
		#region Fields

		[Display(Name = "From:")]
		public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
		public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

		[Display(Name = "To:")]
		public string _toDt = DateTime.Now.ToString(Format.DateOnly);
		public string toDt { get { return _toDt; } set { _toDt = value; } }
		public string ReportTitle { get; set; }
		public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
		public string filterString { get; set; }
		public string ErrorMessage { get; set; }
		public List<Hashtable> lstRepairReportWithParts { get; set; }
		#endregion
		#region Methods 
		public bool GetList(string frmDt, string toDt, string programId, string ProgramName)
		{
			// oDAL = new cDAL("ACTIVE", "ST");
			string query = string.Empty;
			query = @"	   			  
SELECT * FROM (
SELECT DISTINCT
		p.ID,
	   p.Name,
	   SOH.CustomerReference,
	   SOH.ThirdPartyReference,
       PS.SerialNo,
       PS.PartNo,
       MainPN.ModelNo,
       FORMAT(PS.RODate, 'dd/MM/yyyy') AS RODate,
       FORMAT(SOSI.ShipmentDate, 'dd/MM/yyyy') AS ShipmentDate,
       CF.Description AS FaultDescription,
       CR.Description AS RepairDescription,
       WSA.Value AS Remark,
       WOL.ComponentPartNo,
       PN.Description 
FROM pls.PartSerial PS
INNER JOIN pls.SOShipmentInfo SOSI ON SOSI.SOHeaderID = PS.SOHeaderID and SOSI.StatusID = 18
LEFT JOIN pls.SOHeader SOH ON SOH.ID = SOSI.SOHeaderID
INNER JOIN pls.WOHeader WOH ON WOH.ID = PS.WOHeaderID
INNER JOIN pls.WOLine WOL ON WOL.WOHeaderID = PS.WOHeaderID
INNER JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID 
LEFT JOIN pls.PartNo PN ON PN.PartNo = WOL.ComponentPartNo
LEFT JOIN pls.PartNo MainPN On MainPN.PartNo = PS.PartNo
LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = (SELECT MIN(WU.ID) FROM pls.WOLine WL
											     INNER JOIN pls.WOUnit WU ON WU.WOLineID = WL.ID
												 WHERE WL.WOHeaderID = PS.WOHeaderID)
LEFT JOIN pls.CodeFault CF ON CF.ID = WUC.FaultID
LEFT JOIN pls.CodeRepair CR ON CR.ID = WUC.RepairID
LEFT JOIN pls.WOStationAttribute WSA ON WSA.WOStationHistoryID = (Select MAX(ID) from pls.WOStationHistory where WOHeaderID = WOH.ID AND WorkStationID = 13)
LEFT JOIN pls.CodeAttribute CA ON WSA.AttributeID = CA.ID AND CA.AttributeName = 'REMARK'
INNER JOIN pls.program p on p.ID = PS.ProgramID
WHERE WOL.ComponentPartNo <> PS.PartNo
AND PS.ProgramID = @programID 
AND CONVERT(Date, SOSI.ShipmentDate) >= '@frmDt' AND CONVERT(Date, SOSI.ShipmentDate) <= '@toDt'
GROUP BY 
p.ID,p.Name,SOH.CustomerReference, SOH.ThirdPartyReference,PS.SerialNo, PS.PartNo, PS.RODate, MainPN.ModelNo, PN.Description, 
SOSI.ShipmentDate, CF.Description, CR.Description, WSA.Value,
WOL.ComponentPartNo, PN.Description

UNION ALL

SELECT DISTINCT
		p.ID,
	   p.Name,
	   SOH.CustomerReference,
	   SOH.ThirdPartyReference,
       PSH.SerialNo,
       PSH.PartNo,
       MainPN.ModelNo,
       FORMAT(PSH.RODate, 'dd/MM/yyyy') AS RODate,
       FORMAT(SOSI.ShipmentDate, 'dd/MM/yyyy') AS ShipmentDate,
       CF.Description AS FaultDescription,
       CR.Description AS RepairDescription,
       WSA.Value AS Remark,
       WOL.ComponentPartNo,
       PN.Description
FROM pls.PartSerialHistory PSH
INNER JOIN pls.SOShipmentInfo SOSI ON SOSI.SOHeaderID = PSH.SOHeaderID and SOSI.StatusID = 18
LEFT JOIN pls.SOHeader SOH ON SOH.ID = SOSI.SOHeaderID
INNER JOIN pls.WOHeader WOH ON WOH.ID = PSH.WOHeaderID
INNER JOIN pls.WOLine WOL ON WOL.WOHeaderID = PSH.WOHeaderID 
INNER JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID 
LEFT JOIN pls.PartNo PN ON PN.PartNo = WOL.ComponentPartNo
LEFT JOIN pls.PartNo MainPN On MainPN.PartNo = PSH.PartNo
LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = (SELECT MIN(WU.ID) FROM pls.WOLine WL
											     INNER JOIN pls.WOUnit WU ON WU.WOLineID = WL.ID
												 WHERE WL.WOHeaderID = PSH.WOHeaderID)
LEFT JOIN pls.CodeFault CF ON CF.ID = WUC.FaultID
LEFT JOIN pls.CodeRepair CR ON CR.ID = WUC.RepairID
LEFT JOIN pls.WOStationAttribute WSA ON WSA.WOStationHistoryID = (Select MAX(ID) from pls.WOStationHistory where WOHeaderID = WOH.ID AND WorkStationID = 13)
LEFT JOIN pls.CodeAttribute CA ON WSA.AttributeID = CA.ID AND CA.AttributeName = 'REMARK'
INNER JOIN pls.program p on p.ID = PSH.ProgramID
WHERE WOL.ComponentPartNo <> PSH.PartNo
AND PSH.ProgramID = @programID 
AND CONVERT(Date, SOSI.ShipmentDate) >= '@frmDt' AND CONVERT(Date, SOSI.ShipmentDate) <= '@toDt'
GROUP BY 
p.ID,p.Name,SOH.CustomerReference, SOH.ThirdPartyReference,PSH.SerialNo, PSH.PartNo, PSH.RODate, MainPN.ModelNo, PN.Description, 
SOSI.ShipmentDate, CF.Description, CR.Description, WSA.Value,
WOL.ComponentPartNo, PN.Description) a
ORDER BY ShipmentDate DESC";

			string programID = HttpContext.Current.Session["ProgramForSite"].ToString();

			query = query.Replace("@frmDt", frmDt);
			query = query.Replace("@toDt", toDt);
			query = query.Replace("@programID", programID);


			DataTable dt = oDAL.GetData(query);

			if (!string.IsNullOrEmpty(ProgramName))
				filterString += "> Program = '" + ProgramName + "' ";

			filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";


			//For SQL Documentation
			cLog oLog = new cLog();
			oLog.AddSqlQuery("225", query, string.Empty, false);

			if (oDAL.HasErrors)
			{
				ErrorMessage = oDAL.ErrMessage;
				return false;
			}
			else
			{
				if (dt.Rows.Count > 0)
					lstRepairReportWithParts = cCommon.ConvertDtToHashTable(dt);
				return true;

			}
		}
		#endregion
	}
}