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
    public class BoseLookupTool
    {
		cDAL oDAL = new cDAL("ACTIVE");
		#region Fields
		[Display(Name = "Program:")]
		public string program_Id { get; set; }
		[Display(Name = "Program:")]
		public string program { get; set; }

		[Display(Name = "Serial No.:")]
		public string SerialNo { get; set; }

		[Display(Name = "RMA No.:")]
		public string custRef { get; set; }

		[Display(Name = "Inbound Tracking No.:")]
		public string InboundTracking { get; set; }
		public string ReportTitle { get; set; }
		public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
		public List<Hashtable> lstBoseLookupTool { get; set; }

		public string filterString { get; set; }
		public string ErrorMessage { get; set; }
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

		public bool GetList(string programId, string programName, string custRef, string SerialNo, string InboundTracking)
		{
			oDAL = new cDAL("ACTIVE");
			string query = string.Empty;
			query = @"
select PS.ProgramID, PS.PARTNO CURRENT_SKU, isnull(psao.[Value], ps.PartNo) ORIGINAL_SKU, ps.SerialNo, roha.[Value] ORDER_TYPE, 
rohb.[Value] RETURN_REASON, 
(case when roha.[Value] = 'REPAIR' and cro.[Description] = 'FINS' then 'FINS'
     when roha.[Value] != 'REPAIR' and cro.[Description] is not null then cro.[Description]
     when roha.[Value] != 'REPAIR' and crs.[Description] is not null then crs.[Description]
     when roha.[Value] != 'REPAIR' and crp.[Description] is not null then crp.[Description]
     ELSE 'n/a' END) BLACKLIST,
rohc.[Value] SYMPTOM, 
(case when psad.[Value] is null and roha.[Value] = 'REPAIR' then 'n/a'
   when psad.[Value] is null and roha.[Value] != 'REPAIR' then 'REMAN'
   else psad.[Value] END) CREDIT_DISPOSITION, cs.[Description] LAST_STATUS,
   cl.locationno LAST_LOCATION, 
roh.CustomerReference RMA_NO, isnull(dock.CreateDate,ps.RODate) DOCK_DATE, ps.RODate RECEIPT_DATE, psat.[Value] INBOUND_TRACKING ,
wscd.[Description] CURRENT_REPAIR_STEP, pthold.CreateDate LATEST_HOLD_DATE, pthold.Reason LATEST_HOLD_REASON, ps.WOEndDate REPAIR_COMPLETE_DATE, 
soh.CustomerReference SHIP_ORDER_NO, si.TrackingNo OUTBOUND_TRACKING
from pls.PartSerial ps
join pls.ROHeaderAttribute roha on roha.ROHeaderID = ps.ROHeaderID and roha.AttributeID = 986
join pls.ROHeaderAttribute rohb on rohb.ROHeaderID = ps.ROHeaderID and rohb.AttributeID = 2
left join pls.ROHeaderAttribute rohc on rohc.ROHeaderID = ps.ROHeaderID and rohc.AttributeID = 144
join pls.ROHeader ROH on roh.id = ps.ROHeaderID and roh.ProgramID = ps.ProgramID
left join pls.SOHeader soh on soh.id = ps.SOHeaderID and soh.programID = ps.ProgramID
left join pls.SOShipmentInfo si on si.SOHeaderID = soh.id
join pls.CodeStatus cs on cs.id = ps.StatusID
left join pls.WOHeader wo on wo.id = ps.WOHeaderID and wo.ProgramID = ps.ProgramID
left join pls.CodeWorkStationCustomDescription wscd on wscd.CodeWorkStationID = ps.WorkStationID and wscd.ProgramID = ps.ProgramID and wscd.RepairTypeID = wo.RepairTypeID
left join pls.PartTransaction pthold on pthold.ProgramId = ps.ProgramId and pthold.PartTransactionID = 12 and pthold.SerialNo = ps.SerialNo
  and pthold.id = (select max(id) from pls.parttransaction where ProgramID = ps.ProgramId and PartTransactionID = 12 and SerialNo = ps.SerialNo)
left join pls.PartSerialAttribute psat on psat.AttributeID = 92 and psat.PartSerialID = ps.id
left join pls.PartSerialAttribute psao on psao.AttributeID = 1010 and psao.PartSerialID = ps.id
left join pls.PartSerialAttribute psad on psad.AttributeID = 973 and psad.PartSerialID = ps.id 
left join pls.RODockLog dock on dock.ROHeaderID = roh.id and dock.TrackingNo = psat.[Value] and dock.ProgramID = ps.ProgramID
left join pls.ROBlackList BLSS on blsS.ProgramID = ps.ProgramID and blss.AttributeID = 981 and blss.[Value] = ps.SerialNo left join pls.CodeReason crs on crs.id = blss.ReasonID
left join pls.ROBlackList BLSp on blsp.ProgramID = ps.ProgramID and blsp.AttributeID = 1091 and blsp.[Value] = ps.PartNo left join pls.CodeReason crp on crp.id = blsp.ReasonID
left join pls.ROBlackList BLSo on blso.ProgramID = ps.ProgramID and blso.AttributeID = 91 and blso.[Value] = roh.CustomerReference left join pls.CodeReason cro on cro.id = blso.ReasonID
left join pls.partlocation cl on cl.id = ps.locationid and cl.programid = ps.programid
where ps.ProgramID = '<programId>'
";

			query = query.Replace("<programId>", programId);

			if (!string.IsNullOrEmpty(custRef))
				query += "AND roh.CustomerReference LIKE '%" + custRef + "%' ";

			if (!string.IsNullOrEmpty(SerialNo))
				query += "AND ps.SerialNo LIKE '%" + SerialNo + "%' ";

			if (!string.IsNullOrEmpty(InboundTracking))
				query += "AND psat.[Value] = '" + InboundTracking + "' ";

			DataTable dt = oDAL.GetData(query);

			filterString += " > Program = '" + programName + "' ";

			if (!string.IsNullOrEmpty(SerialNo))
				filterString += " | Serial No. LIKE '" + SerialNo + "' ";

			if (!string.IsNullOrEmpty(custRef))
				filterString += " | RMA No = '" + custRef + "' ";

			if (!string.IsNullOrEmpty(InboundTracking))
				filterString += " | Inbound Tracking No. = '" + InboundTracking + "' ";





			//For SQL Documentation
			cLog oLog = new cLog();
			oLog.AddSqlQuery("253", query, string.Empty, false);

			if (oDAL.HasErrors)
			{
				ErrorMessage = oDAL.ErrMessage;
				return false;
			}
			else
			{
				if (dt.Rows.Count > 0)
					lstBoseLookupTool = cCommon.ConvertDtToHashTable(dt);
				return true;

			}
		}

		#endregion
	}
}