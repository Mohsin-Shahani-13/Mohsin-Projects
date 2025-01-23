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
    public class RepairReportNSN
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
		public List<Hashtable> lstRepairReportNSN { get; set; }
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
	   ROUA.Value AS CustomerID,
	   ROUAT.Value AS RMR,
	   ROUATFR.Value AS FR,
	   ROUATDN.Value AS DN,
       FORMAT(PS.RODate, 'dd/MM/yyyy') AS RODate,
       FORMAT(SOSI.ShipmentDate, 'dd/MM/yyyy') AS ShipmentDate,
	   ROUATRC.Value AS ReturnCount,
	   CASE WHEN ROUATWS.Value = 'YES' THEN 'SRR'ELSE 'NOT' END AS WarrantyStatus,
       CF.Description AS FaultDescription,
       CR.Description AS RepairDescription,
       WSA.Value AS Remark,
	   PSAPOO.Value As NokiaPONo,
	   PSADD.Value AS NokiaDispatchDate,
	   PSARCT.Value AS RCTNo,
	   PSASN.Value AS NokiaShipmentNo,
	   PSARDN.Value AS ReconextDN,
	   -- Calculate the number of weekdays (TAT)
        (DATEDIFF(DAY, PS.RODate, SOSI.ShipmentDate) 
            -- Subtract weekends (Saturdays and Sundays)
            - (DATEDIFF(WEEK, PS.RODate, SOSI.ShipmentDate) * 2)
            -- Adjust if the period starts or ends on a weekend
            + CASE WHEN DATENAME(WEEKDAY, PS.RODate) = 'Sunday' THEN 1 ELSE 0 END
            + CASE WHEN DATENAME(WEEKDAY, SOSI.ShipmentDate) = 'Saturday' THEN 1 ELSE 0 END
        ) + 1 AS TAT -- Add 1 day as requested 
FROM pls.PartSerial PS
INNER JOIN pls.SOShipmentInfo SOSI ON SOSI.SOHeaderID = PS.SOHeaderID and SOSI.StatusID = 18
LEFT JOIN pls.SOHeader SOH ON SOH.ID = SOSI.SOHeaderID
INNER JOIN pls.WOHeader WOH ON WOH.ID = PS.WOHeaderID
INNER JOIN pls.WOLine WOL ON WOL.WOHeaderID = PS.WOHeaderID
INNER JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID 
LEFT JOIN pls.PartNo MainPN On MainPN.PartNo = PS.PartNo
INNER JOIN pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID
INNER JOIN pls.ROLine ROL ON ROL.ROHeaderId = ROH.Id AND ROL.PartNo = PS.PartNo
INNER JOIN pls.ROUnit ROU ON ROU.ROLineId = ROL.Id AND ROU.SerialNo = PS.SerialNo
LEFT JOIN pls.CodeAttribute CA_INCOMINGPARTNO ON  CA_INCOMINGPARTNO.AttributeName = 'CUSTOMER_NAME'
LEFT JOIN pls.ROUnitAttribute ROUA ON ROUA.ROUnitID = ROU.ID AND ROUA.AttributeID = CA_INCOMINGPARTNO.ID 
LEFT JOIN pls.CodeAttribute CARMR ON CARMR.AttributeName = 'RMR'
LEFT JOIN pls.ROUnitAttribute ROUAT ON ROUAT.ROUnitID = ROU.ID AND ROUAT.AttributeID = CARMR.ID
LEFT JOIN pls.CodeAttribute CAFR ON CAFR.AttributeName = 'FR'
LEFT JOIN pls.ROUnitAttribute ROUATFR ON ROUATFR.ROUnitID = ROU.ID AND ROUATFR.AttributeID = CAFR.ID
LEFT JOIN pls.CodeAttribute CADN ON CADN.AttributeName = 'DN'
LEFT JOIN pls.ROUnitAttribute ROUATDN ON ROUATDN.ROUnitID = ROU.ID AND ROUATDN.AttributeID = CADN.ID
LEFT JOIN pls.CodeAttribute CARC ON CARC.AttributeName = 'RETURN_COUNT'
LEFT JOIN pls.ROUnitAttribute ROUATRC ON ROUATRC.ROUnitID = ROU.ID AND ROUATRC.AttributeID = CARC.ID
LEFT JOIN pls.CodeAttribute CAWS ON CAWS.AttributeName = 'SRR'
LEFT JOIN pls.ROUnitAttribute ROUATWS ON ROUATWS.ROUnitID = ROU.ID AND ROUATWS.AttributeID = CAWS.ID
LEFT JOIN pls.CodeAttribute CAPOO ON CAPOO.AttributeName = 'POORSCRAPNUM'
LEFT JOIN pls.PartSerialAttribute PSAPOO ON PSAPOO.AttributeID = CAPOO.ID AND PSAPOO.PartSerialID = PS.ID
LEFT JOIN pls.CodeAttribute CADD ON CADD.AttributeName = 'RCTDISPATCHDATE'
LEFT JOIN pls.PartSerialAttribute PSADD ON PSADD.AttributeID = CADD.ID AND PSADD.PartSerialID = PS.ID
LEFT JOIN pls.CodeAttribute CARCT ON CARCT.AttributeName = 'RCTNO'
LEFT JOIN pls.PartSerialAttribute PSARCT ON PSARCT.AttributeID = CARCT.ID AND PSARCT.PartSerialID = PS.ID
LEFT JOIN pls.CodeAttribute CASN ON CASN.AttributeName = 'RCTSHIPMENTNUM'
LEFT JOIN pls.PartSerialAttribute PSASN ON PSASN.AttributeID = CASN.ID AND PSASN.PartSerialID = PS.ID
LEFT JOIN pls.CodeAttribute CARDN ON CARDN.AttributeName = 'RECONEXTDN'
LEFT JOIN pls.PartSerialAttribute PSARDN ON PSARDN.AttributeID = CARDN.ID AND PSARDN.PartSerialID = PS.ID
LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = WOU.ID
LEFT JOIN pls.CodeFault CF ON CF.ID = WUC.FaultID
LEFT JOIN pls.CodeRepair CR ON CR.ID = WUC.RepairID
LEFT JOIN pls.WOStationHistory WOSH ON WOSH.WOHeaderID = WOH.ID AND WOSH.WorkStationID = 13
LEFT JOIN pls.WOStationAttribute WSA ON WSA.WOStationHistoryID =  
	(SELECT MAX(ID) FROM pls.WOStationHistory  
														WHERE WOHeaderID = WOH.ID  AND WorkStationID = 13)
LEFT JOIN pls.CodeAttribute CA ON WSA.AttributeID = CA.ID AND CA.AttributeName = 'REMARK'
INNER JOIN pls.program p on p.ID = PS.ProgramID
WHERE PS.ProgramID = @programID
AND CONVERT(Date, SOSI.ShipmentDate) >= '@frmDt' AND CONVERT(Date, SOSI.ShipmentDate) <= '@toDt'
AND WOL.ComponentPartNo = PS.PartNo
GROUP BY 
p.ID,p.Name,SOH.CustomerReference, SOH.ThirdPartyReference,PS.SerialNo, PS.PartNo, PS.RODate, MainPN.ModelNo, 
ROUA.Value, ROUAT.Value, ROUATFR.Value, ROUATDN.Value, 
SOSI.ShipmentDate, ROUATRC.Value, ROUATWS.Value, CF.Description, CR.Description, WSA.Value,
PSAPOO.Value, PSADD.Value, PSARCT.Value,
PSASN.Value, PSARDN.Value
UNION ALL
SELECT DISTINCT
		p.ID,
	   p.Name,
	   SOH.CustomerReference,
	   SOH.ThirdPartyReference,
       PSH.SerialNo,
       PSH.PartNo,
       MainPN.ModelNo,
	   ROUA.Value AS CustomerID,
	   ROUAT.Value AS RMR,
	   ROUATFR.Value AS FR,
	   ROUATDN.Value AS DN,
       FORMAT(PSH.RODate, 'dd/MM/yyyy') AS RODate,
       FORMAT(SOSI.ShipmentDate, 'dd/MM/yyyy') AS ShipmentDate,
	   ROUATRC.Value AS ReturnCount,
	   CASE WHEN ROUATWS.Value = 'YES' THEN 'SRR'ELSE 'NOT' END AS WarrantyStatus,
       CF.Description AS FaultDescription,
       CR.Description AS RepairDescription,
       WSA.Value AS Remark,
	   PSAPOO.Value As NokiaPONo,
	   PSADD.Value AS NokiaDispatchDate,
	   PSARCT.Value AS RCTNo,
	   PSASN.Value AS NokiaShipmentNo,
	   PSARDN.Value AS ReconextDN,
	   -- Calculate the number of weekdays (TAT)
        (DATEDIFF(DAY, PSH.RODate, SOSI.ShipmentDate) 
            -- Subtract weekends (Saturdays and Sundays)
            - (DATEDIFF(WEEK, PSH.RODate, SOSI.ShipmentDate) * 2)
            -- Adjust if the period starts or ends on a weekend
            + CASE WHEN DATENAME(WEEKDAY, PSH.RODate) = 'Sunday' THEN 1 ELSE 0 END
            + CASE WHEN DATENAME(WEEKDAY, SOSI.ShipmentDate) = 'Saturday' THEN 1 ELSE 0 END
        ) + 1 AS TAT -- Add 1 day as requested
FROM pls.PartSerialHistory PSH
INNER JOIN pls.SOShipmentInfo SOSI ON SOSI.SOHeaderID = PSH.SOHeaderID and SOSI.StatusID = 18
LEFT JOIN pls.SOHeader SOH ON SOH.ID = SOSI.SOHeaderID
INNER JOIN pls.WOHeader WOH ON WOH.ID = PSH.WOHeaderID
INNER JOIN pls.WOLine WOL ON WOL.WOHeaderID = PSH.WOHeaderID 
INNER JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID 
LEFT JOIN pls.PartNo MainPN On MainPN.PartNo = PSH.PartNo
INNER JOIN pls.ROHeader ROH ON ROH.ID = PSH.ROHeaderID
INNER JOIN pls.ROLine ROL ON ROL.ROHeaderId = ROH.Id AND ROL.PartNo = PSH.PartNo
INNER JOIN pls.ROUnit ROU ON ROU.ROLineId = ROL.Id AND ROU.SerialNo = PSH.SerialNo
LEFT JOIN pls.CodeAttribute CA_INCOMINGPARTNO ON  CA_INCOMINGPARTNO.AttributeName = 'CUSTOMER_NAME'
LEFT JOIN pls.ROUnitAttribute ROUA ON ROUA.ROUnitID = ROU.ID AND ROUA.AttributeID = CA_INCOMINGPARTNO.ID 
LEFT JOIN pls.CodeAttribute CARMR ON CARMR.AttributeName = 'RMR'
LEFT JOIN pls.ROUnitAttribute ROUAT ON ROUAT.ROUnitID = ROU.ID AND ROUAT.AttributeID = CARMR.ID
LEFT JOIN pls.CodeAttribute CAFR ON CAFR.AttributeName = 'FR'
LEFT JOIN pls.ROUnitAttribute ROUATFR ON ROUATFR.ROUnitID = ROU.ID AND ROUATFR.AttributeID = CAFR.ID
LEFT JOIN pls.CodeAttribute CADN ON CADN.AttributeName = 'DN'
LEFT JOIN pls.ROUnitAttribute ROUATDN ON ROUATDN.ROUnitID = ROU.ID AND ROUATDN.AttributeID = CADN.ID
LEFT JOIN pls.CodeAttribute CARC ON CARC.AttributeName = 'RETURN_COUNT'
LEFT JOIN pls.ROUnitAttribute ROUATRC ON ROUATRC.ROUnitID = ROU.ID AND ROUATRC.AttributeID = CARC.ID
LEFT JOIN pls.CodeAttribute CAWS ON CAWS.AttributeName = 'SRR'
LEFT JOIN pls.ROUnitAttribute ROUATWS ON ROUATWS.ROUnitID = ROU.ID AND ROUATWS.AttributeID = CAWS.ID
LEFT JOIN pls.CodeAttribute CAPOO ON CAPOO.AttributeName = 'POORSCRAPNUM'
LEFT JOIN pls.PartSerialAttributeHistory PSAPOO ON PSAPOO.AttributeID = CAPOO.ID AND PSAPOO.PartSerialHistoryID = PSH.ID
LEFT JOIN pls.CodeAttribute CADD ON CADD.AttributeName = 'RCTDISPATCHDATE'
LEFT JOIN pls.PartSerialAttributeHistory PSADD ON PSADD.AttributeID = CADD.ID AND PSADD.PartSerialHistoryID = PSH.ID
LEFT JOIN pls.CodeAttribute CARCT ON CARCT.AttributeName = 'RCTNO'
LEFT JOIN pls.PartSerialAttributeHistory PSARCT ON PSARCT.AttributeID = CARCT.ID AND PSARCT.PartSerialHistoryID = PSH.ID
LEFT JOIN pls.CodeAttribute CASN ON CASN.AttributeName = 'RCTSHIPMENTNUM'
LEFT JOIN pls.PartSerialAttributeHistory PSASN ON PSASN.AttributeID = CASN.ID AND PSASN.PartSerialHistoryID = PSH.ID
LEFT JOIN pls.CodeAttribute CARDN ON CARDN.AttributeName = 'RECONEXTDN'
LEFT JOIN pls.PartSerialAttributeHistory PSARDN ON PSARDN.AttributeID = CARDN.ID AND PSARDN.PartSerialHistoryID = PSH.ID
LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = WOU.ID
LEFT JOIN pls.CodeFault CF ON CF.ID = WUC.FaultID
LEFT JOIN pls.CodeRepair CR ON CR.ID = WUC.RepairID
LEFT JOIN pls.WOStationHistory WOSH ON WOSH.WOHeaderID = WOH.ID AND WOSH.WorkStationID = 13
LEFT JOIN pls.WOStationAttribute WSA ON WSA.WOStationHistoryID = 
	(SELECT MAX(ID) FROM pls.WOStationHistory  
														WHERE WOHeaderID = WOH.ID  AND WorkStationID = 13)
LEFT JOIN pls.CodeAttribute CA ON WSA.AttributeID = CA.ID AND CA.AttributeName = 'REMARK'
INNER JOIN pls.program p on p.ID = PSH.ProgramID
WHERE PSH.ProgramID = @programID
AND CONVERT(Date, SOSI.ShipmentDate) >= '@frmDt' AND CONVERT(Date, SOSI.ShipmentDate) <= '@toDt'
AND WOL.ComponentPartNo = PSH.PartNo
GROUP BY 
p.ID,p.Name,SOH.CustomerReference, SOH.ThirdPartyReference,PSH.SerialNo, PSH.PartNo, PSH.RODate, MainPN.ModelNo, 
ROUA.Value, ROUAT.Value, ROUATFR.Value, ROUATDN.Value, 
SOSI.ShipmentDate, ROUATRC.Value, ROUATWS.Value, CF.Description, CR.Description, WSA.Value, PSAPOO.Value, 
PSADD.Value, PSARCT.Value, PSASN.Value, PSARDN.Value
) a
ORDER BY ShipmentDate DESC
";

			string programID = HttpContext.Current.Session["ProgramForSite"].ToString();

			query = query.Replace("@frmDt", frmDt);
			query = query.Replace("@toDt", toDt);
			query = query.Replace("@programID", programID);


			DataTable dt = oDAL.GetData(query);

			//if (!string.IsNullOrEmpty(ProgramName))
			//	filterString += "> Program = '" + ProgramName + "' ";

			filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";


			//For SQL Documentation
			cLog oLog = new cLog();
			oLog.AddSqlQuery("227", query, string.Empty, false);

			if (oDAL.HasErrors)
			{
				ErrorMessage = oDAL.ErrMessage;
				return false;
			}
			else
			{
				if (dt.Rows.Count > 0)
					lstRepairReportNSN = cCommon.ConvertDtToHashTable(dt);
				return true;

			}
		}
		#endregion
	}
}