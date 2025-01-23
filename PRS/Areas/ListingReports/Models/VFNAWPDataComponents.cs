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
    public class VFNAWPDataComponents
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
		public List<Hashtable> lstVFNAWPDataComponents { get; set; }
		#endregion
		#region Methods 
		public bool GetList()
		{
			// oDAL = new cDAL("ACTIVE", "ST");
			string query = string.Empty;
			query = @"	   			  
						  select 
distinct p.[WO Header], p.ProgramID, p.Reason, p.[Unit PN], p.SerialNo, p.Username, p.[Location], max(p.[ON HOLD DATE]) [HOLD DATE], p.Comp1,p.Comp2,p.Comp3,p.Comp4,p.Comp5,p.Comp6 from (select wh.ID [WO Header] , pt.ProgramID, pt.Reason, pt.PartNo [Unit PN], pt.SerialNo, 
( SELECT U.Username from(
    select  ROW_NUMBER() OVER (ORDER BY wl.WOHeaderID) AS 'RowNumber', wl.UserID from 
    pls.woline wl with (nolock)
    where  wl.WOHeaderID = wh.ID and wl.StatusID = 7
    ) e 
	LEFT JOIN PLS.[User] U ON U.ID = e.UserID
	where e.RowNumber =1
) [Username],
pt.[Location] , pt.CreateDate [ON HOLD DATE], 
( SELECT e.ComponentPartNo from(
    select  ROW_NUMBER() OVER (ORDER BY wl.WOHeaderID) AS 'RowNumber', wl.ComponentPartNo from 
    pls.woline wl with (nolock)
    where  wl.WOHeaderID = wh.ID and wl.StatusID = 7
    ) e where e.RowNumber = 1
) [Comp1],
( SELECT e.ComponentPartNo from(
    select  ROW_NUMBER() OVER (ORDER BY wl.WOHeaderID) AS 'RowNumber', wl.ComponentPartNo from 
    pls.woline wl with (nolock)
    where  wl.WOHeaderID = wh.ID and wl.StatusID = 7
    ) e where e.RowNumber =2
) [Comp2],
( SELECT e.ComponentPartNo from(
    select  ROW_NUMBER() OVER (ORDER BY wl.WOHeaderID) AS 'RowNumber', wl.ComponentPartNo from 
    pls.woline wl with (nolock)
    where  wl.WOHeaderID = wh.ID and wl.StatusID = 7
    ) e where e.RowNumber =3
) [Comp3],
( SELECT e.ComponentPartNo from(
    select  ROW_NUMBER() OVER (ORDER BY wl.WOHeaderID) AS 'RowNumber', wl.ComponentPartNo from 
    pls.woline wl with (nolock)
    where  wl.WOHeaderID = wh.ID and wl.StatusID = 7
    ) e where e.RowNumber =4
) [Comp4],
( SELECT e.ComponentPartNo from(
    select  ROW_NUMBER() OVER (ORDER BY wl.WOHeaderID) AS 'RowNumber', wl.ComponentPartNo from 
    pls.woline wl with (nolock)
    where  wl.WOHeaderID = wh.ID and wl.StatusID = 7
    ) e where e.RowNumber =5
) [Comp5],
( SELECT e.ComponentPartNo from(
    select  ROW_NUMBER() OVER (ORDER BY wl.WOHeaderID) AS 'RowNumber', wl.ComponentPartNo from 
    pls.woline wl with (nolock)
    where  wl.WOHeaderID = wh.ID and wl.StatusID = 7
    ) e where e.RowNumber =6
) [Comp6]
from 
pls.PartTransaction pt with (nolock)
INNER JOIN pls.woheader wh with (nolock) on pt.OrderHeaderID = wh.ID  and wh.StatusID = 28
where 
pt.OrderType ='WO' 
and PartTransactionID = 12
and pt.Reason ='On hold for parts'
and pt.ProgramID = 10010
) P
group by   p.[WO Header], p.ProgramID, p.Reason, p.[Unit PN], p.SerialNo, p.Username, p.[Location], 
p.Comp1,p.Comp2,p.Comp3,p.Comp4,p.Comp5,p.Comp6

";

			//string programID = HttpContext.Current.Session["ProgramForSite"].ToString();

			//query = query.Replace("@frmDt", frmDt);
			//query = query.Replace("@toDt", toDt);
			//query = query.Replace("@programID", programID);


			DataTable dt = oDAL.GetData(query);

			//if (!string.IsNullOrEmpty(ProgramName))
			//	filterString += "> Program = '" + ProgramName + "' ";

			//filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";


			//For SQL Documentation
			cLog oLog = new cLog();
			oLog.AddSqlQuery("228", query, string.Empty, false);

			if (oDAL.HasErrors)
			{
				ErrorMessage = oDAL.ErrMessage;
				return false;
			}
			else
			{
				if (dt.Rows.Count > 0)
					lstVFNAWPDataComponents = cCommon.ConvertDtToHashTable(dt);
				return true;

			}
		}
		#endregion
	}
}