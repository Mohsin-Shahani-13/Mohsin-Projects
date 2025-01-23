using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Web;


namespace IP.Areas.ListingReports.Models
{
    public class DtaUpload
	{
		cDAL oDAL = new cDAL("ACTIVE");
		#region Fields
		[Display(Name = "Program:")]
		public string program { get; set; }
		//[Display(Name = "From:")]
		//public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
		//public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
		//[Display(Name = "To:")]
		//public string _toDt = DateTime.Now.ToString(Format.DateOnly);
		//public string toDt { get { return _toDt; } set { _toDt = value; } }
		public string ReportTitle { get; set; }
		public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
		public string filterString { get; set; }
		public string ErrorMessage { get; set; }
		public string ReportName { get; set; }
		public List<Hashtable> lstDtaUpload { get; set; }

		public DataTable Program()
		{
			string sites = HttpContext.Current.Session["DefaultSite"].ToString();
			string query = string.Empty;
			query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM
                              WHERE SITE = '<site>'
                      ORDER BY NAME";
			query = query.Replace("<site>", sites);
			DataTable dt = oDAL.GetData(query);
			return dt;
		}
		public bool GetList(string ProgramID, string ProgramName, string rptName)
		{
			string query = string.Empty;
			if (rptName == "DataUpload")
			{
				query = @"SELECT 
     DU.ID,
     DU.ProgramID,
     DU.DataUploadScriptID,
     DU.DataUploadScriptName,
     DU.BatchID,
     DU.StatusDescription,
	 DU.Message,
	 DU.UserName as UserID,
	 DU.C01,
	 DU.C02,
	 DU.C03,
	 DU.C04,
	 DU.C05,
	 DU.C06,
	 DU.C07,
	 DU.C08,
	 DU.C09,
	 DU.C10,
	 DU.C11,
	 DU.C12,
	 DU.C13,
	 DU.C14,
	 DU.C15,
	 DU.C16,
	 DU.C17,
	 DU.C18,
	 DU.C19,
     DU.CreateDate,
	 DU.LastActivityDate
FROM pls.vDataUpload DU 
WHERE 1=1
";
				if (!string.IsNullOrEmpty(ProgramID))
					query += "AND DU.ProgramID = '" + ProgramID + "'";

			}
			else if (rptName == "DataUploadSchedule")
			{
				query = @"SELECT 
     DUS.ID,
     DUS.ProgramID,
     DUS.DataUploadScriptID,
     DUS.DataUploadScript as DataUploadScriptName,
     DUS.BatchID,
     DUS.Status as StatusID,
	 DUS.Message,
	 DUS.UserName as UserID,
	 DUS.C01,
	 DUS.C02,
	 DUS.C03,
	 DUS.C04,
	 DUS.C05,
	 DUS.C06,
	 DUS.C07,
	 DUS.C08,
	 DUS.C09,
	 DUS.C10,
	 DUS.C11,
	 DUS.C12,
	 DUS.C13,
	 DUS.C14,
	 DUS.C15,
	 DUS.C16,
	 DUS.C17,
	 DUS.C18,
	 DUS.C19,
     DUS.CreateDate,
	 DUS.LastActivityDate
FROM pls.vDataUploadSchedule DUS
WHERE 1=1
";

				if (!string.IsNullOrEmpty(ProgramID))
					query += "AND DUS.ProgramID = '" + ProgramID + "'";
			}
			else if (rptName == "DataUploadArchive")
			{
				query = @"SELECT 
     DUA.ID,
	 DUA.ProgramID,
	 DUA.DataUploadScriptID,
	 DUA.BatchID,
	 DUA.StatusID,
	 DUA.Message,
	 DUA.UserID,
	 DUA.C01,
	 DUA.C02,
	 DUA.C03,
	 DUA.C04,
	 DUA.C05,
	 DUA.C06,
	 DUA.C07,
	 DUA.C08,
	 DUA.C09,
	 DUA.C10,
	 DUA.C11,
	 DUA.C12,
	 DUA.C13,
	 DUA.C14,
	 DUA.C15,
	 DUA.C16,
	 DUA.C17,
	 DUA.C18,
	 DUA.C19,
     DUA.CreateDate,
	 DUA.LastActivityDate
FROM PlusExt.pls.DataUploadArchive DUA
WHERE 1=1
";
				if (!string.IsNullOrEmpty(ProgramID))
					query += "AND DUA.ProgramID = '" + ProgramID + "'";
			}
			else if (rptName == "DataUploadScheduleArchive")
			{
				query = @"SELECT
     DUSA.ID,
	 DUSA.ProgramID,
	 DUSA.DataUploadScriptID,
	 DUSA.BatchID,
	 DUSA.StatusID,
	 DUSA.Message,
	 DUSA.UserID,
	 DUSA.C01,
	 DUSA.C02,
	 DUSA.C03,
	 DUSA.C04,
	 DUSA.C05,
	 DUSA.C06,
	 DUSA.C07,
	 DUSA.C08,
	 DUSA.C09,
	 DUSA.C10,
	 DUSA.C11,
	 DUSA.C12,
	 DUSA.C13,
	 DUSA.C14,
	 DUSA.C15,
	 DUSA.C16,
	 DUSA.C17,
	 DUSA.C18,
	 DUSA.C19,
     DUSA.CreateDate,
	 DUSA.LastActivityDate
FROM PlusExt.pls.DataUploadScheduleArchive DUSA
WHERE 1=1
";

				if (!string.IsNullOrEmpty(ProgramID))
					query += "AND ProgramID = '" + ProgramID + "'";
			}
			DataTable dt = oDAL.GetData(query);
         

            if (!string.IsNullOrEmpty(ProgramName))
				filterString += " > Program = '" + ProgramName + "'";
			if (!string.IsNullOrEmpty(rptName))
				filterString += " | Report Type = '" + rptName + "'";

			//For SQL Documentation
			cLog oLog = new cLog();
			oLog.AddSqlQuery("145", query, string.Empty, true);

			if (oDAL.HasErrors)
			{
				ErrorMessage = oDAL.ErrMessage;
				return false;
			}
			else
			{
				if (dt.Rows.Count > 0)
					lstDtaUpload = cCommon.ConvertDtToHashTable(dt);
				return true;

			}
		}
	}
}
#endregion