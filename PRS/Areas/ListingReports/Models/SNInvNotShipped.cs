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
    public class SNInvNotShipped
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Status:")]
        public string description { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public bool ischecked2 { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Status:")]
        public string Status { get; set; }
        public List<ArrayList> lstStatus { get; set; }
        public List<Hashtable> lstSNInvNotShipped { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetStatus()
        {
            string query = string.Empty;
            query = @"SELECT DISTINCT CS.ID,
		        CS.Description		
                FROM pls.CodeStatus CS
                Inner Join pls.PartSerial PS On CS.ID = PS.StatusID
                WHERE (PS.StatusID <> 18)
                ORDER BY CS.Description	ASC";
            //query = @"SELECT DISTINCT Id, Description FROM pls.CodeStatus ORDER BY Description ";
            DataTable dt = oDAL.GetData(query);
            lstStatus = cCommon.ConvertDtToArrayList(dt);
            if (!oDAL.HasErrors)
                return true;
            else
                return false;
        }
        public bool GetList(string programId, string ProgramName, string status, string statusId)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"

SELECT        
              PS.ProgramID,
              P.Name As Program,
              ROH.ID,
              WOH.ID AS WO_ID,
              PS.PartNo, 
              PS.SerialNo,
              PL.LocationNo,
              PS.RODate,
              PS.CreateDate,
              PS.WOStartDate, 
              PS.WOEndDate,
              PS.WOPass,
              CS.Description,
              ROH.CustomerReference, 
              WOH.CustomerReference AS WO_Number, 
              CASE WHEN CWSD.Code IS NULL THEN CWS.Description ELSE CWSD.Description END AS Workstation
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID 
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID 
LEFT OUTER JOIN pls.WOHeader WOH ON PS.WOHeaderID = WOH.ID 
LEFT OUTER JOIN pls.CodeWorkStation CWS ON PS.WorkStationID = CWS.ID 
LEFT JOIN pls.CodeWorkStationCustomDescription CWSD ON
				   CWSD.ProgramID = WOH.ProgramID 
				   AND CWSD.RepairTypeID = WOH.RepairTypeID
				   AND CWSD.CodeWorkStationID = PS.WorkStationID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID <> 18)
";

            if (programId != "0")
            {
                query += "AND PS.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "AND PS.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (!status.Equals("All"))
            {
                query += "AND CS.ID  IN (" + statusId + ") ";
            }
            //query = query.Replace("<programId>",programId);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' | Status = '" + status + "'";

            //if (!string.IsNullOrEmpty(status))
            //    filterString = "| Status = '" + status + "'";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("017", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSNInvNotShipped = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}