using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.Logistics.Models
{
    public class CrossDock
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
        public List<Hashtable> lstCrossDock { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
select 
	CD.ID,
	P.Name As Program,
	PL.LocationNo,
	CD.TrackingNo,
	CD.CarrierName,
	CD.FromSite,
	CD.ToSite,
	CS.Description AS Status,
	U.Username AS CreatedBy,
	R.Username AS ReceivedBy,
	FORMAT(CD.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn,
       FORMAT(CD.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
from pls.CrossDock CD
INNER JOIN pls.Program P ON P.ID= CD.ProgramID
LEFT JOIN pls.PartLocation PL ON PL.ID = CD.LocationID
INNER JOIN [pls].[CodeStatus] CS ON CS.ID = CD.StatusID
INNER JOIN pls.[User] U ON U.ID = CD.UserID
INNER JOIN pls.[User] R ON R.ID = CD.ReceivedByUserID
WHERE CONVERT(Date, CD.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, CD.LastActivityDate) <= '<toDt>'
 ";


            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            if (programId != "0")
            {
                query += "AND P.ID = '" + programId + "' ";
            }
            else
            {
                query += "AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            query += "ORDER BY CD.LastActivityDate DESC";

            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("010", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstCrossDock = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}