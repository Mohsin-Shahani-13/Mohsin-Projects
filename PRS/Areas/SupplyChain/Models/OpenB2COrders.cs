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
    public class OpenB2COrders
    {
		cDAL oDAL = new cDAL("ACTIVE");
		#region Fields
		[Display(Name = "Program:")]
		public string program { get; set; }
		public string ReportTitle { get; set; }
		public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
		public string filterString { get; set; }
		public string ErrorMessage { get; set; }
		public List<Hashtable> lstOpenB2COrders { get; set; }

		#endregion
		#region Methods 
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
		public DataTable GetStatus()
		{
			oDAL = new cDAL("ACTIVE");
			string sites = HttpContext.Current.Session["DefaultSite"].ToString();

			string query = string.Empty;
			query = @" select distinct 
	                  status 
                FROM tpdc01s209.rdw.rpt.RedmineItems

                ORDER BY status";
			//query = query.Replace("<site>", sites);
			DataTable dt = oDAL.GetData(query);
			return dt;
		}
		public bool GetList(string programId, string programName)
		{
			// oDAL = new cDAL("ACTIVE", "ST");
			//string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
			//string programName = HttpContext.Current.Session["Program"].ToString();
			//string _trackingNo = GetInValue(trackingNo);
			string query = string.Empty;

			query = @"
select prgm.name as program, prgm.id as ProgramID, ro.ID as roheaderId, ro.CustomerReference, ro.CreateDate, ro.AddressID, cad.*
from pls.vROHeader ro with(NoLock)
inner join pls.vROHeaderAttribute roa with(NoLock) on roa.ROHeaderID = ro.ID and roa.AttributeName = 'REQUESTTYPE' and roa.[Value] = 'RMA'
left join pls.vCodeAddressDetails cad with(NoLock) on cad.AddressID = ro.AddressID and cad.AddressType = 'ShipFrom' and cad.AddressID not in ('260300','9431')
inner join pls.program prgm on prgm.ID = ro.ProgramID
where 
ro.[Status] not in ('Received','Canceled')
and cad.AddressID is not null 
";

			if (!string.IsNullOrEmpty(programId))
			{
				query += "AND ro.ProgramId = '"+ programId + "'";
			}
			//query = query.Replace("<programId>", programId);
			//query = query.Replace("<TrackingNo>", _trackingNo);
			//query = query.Replace("<toDt>", tDate);

			DataTable dt = oDAL.GetData(query);

			//filterString += "> Report Type = '" + rptType + "' ";
			//if (!string.IsNullOrEmpty(programName))
			filterString += "> Program = '" + programName + "' ";

			//if (!string.IsNullOrEmpty(trackingNo))
			//	filterString += "> Tracking No. = '" + _trackingNo + "' ";

			//For SQL Documentation
			cLog oLog = new cLog();
			oLog.AddSqlQuery("194", query, string.Empty, false);

			if (oDAL.HasErrors)
			{
				ErrorMessage = oDAL.ErrMessage;
				return false;
			}
			else
			{
				if (dt.Rows.Count > 0)
					lstOpenB2COrders = cCommon.ConvertDtToHashTable(dt);
				return true;
			}
		}
		private string GetInValue(string Value)
		{
			string[] arr = Value.Split(',');
			string _arr = null;
			foreach (var item in arr)
			{
				if (_arr == null)
				{
					_arr = "" + item + "";
				}
				else
				{
					_arr += "," + "" + item + "";
				}

			}
			return _arr;
		}
		#endregion
	}
}