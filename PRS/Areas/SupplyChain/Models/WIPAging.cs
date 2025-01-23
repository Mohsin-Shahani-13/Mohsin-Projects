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
    public class WIPAging
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Idle Over:")]
        public string IdleOver { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstWipAging { get; set; }

       // public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string IdleOver, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT   PS.ProgramID
	   , RO.Id AS RO_ID
	   , WOH.Id AS WO_ID
	   , SO.Id AS SO_ID
	   , PS.PartNo
	   , PS.ParentSerialNo
       , PS.SerialNo
       , (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
		FROM pls.partserial PS
		WHERE PS.SerialNo = SerialNo AND PS.ProgramID = ProgramID ) HAS_SN
	   , PL.LocationNo
	   , CLG.Description AS LocationGroup
	   , CS.Description AS Status
	   , RO.CustomerReference AS RONo
       , WOH.CustomerReference AS WONo
	   , CASE WHEN wsd.Code IS NULL THEN cws.Description ELSE wsd.Description END AS WorkStation
	   , PS.WOPass
	   , PS.Shippable
	   , SO.CustomerReference AS SONo
	   , U.Username
	   , WOH.LastActivityDate
FROM pls.WOHeader WOH
INNER JOIN pls.PartSerial PS ON PS.WOHeaderID = WOH.ID
INNER JOIN pls.PartLocation PL ON PL.ID = PS.LocationID
INNER JOIN pls.CodeStatus CS ON CS.ID = PS.StatusID
INNER JOIN pls.[User] U ON U.ID = PS.UserID
LEFT OUTER JOIN PLS.CodeLocationGroup CLG ON CLG.Id = PL.LocationGroupID
LEFT OUTER JOIN pls.ROHeader RO ON RO.Id = PS.ROHeaderID
LEFT OUTER JOIN pls.SOHeader SO ON SO.Id = PS.SOHeaderID
LEFT OUTER JOIN pls.CodeWorkStation CWS ON CWS.ID = WOH.WorkStationID
LEFT JOIN pls.CodeWorkStationCustomDescription WSD ON
				   WSD.ProgramID = WOH.ProgramID 
				   AND WSD.RepairTypeID = WOH.RepairTypeID
				   AND WSD.CodeWorkStationID = WOH.WorkStationID
WHERE WOH.StatusID IN (19, 28)
	  AND DATEDIFF(DAY, WOH.LastActivityDate, GetDate()) +1 > <IdleOver> 
";
            if (programId != "0")
                query += "AND WOH.ProgramID = '" + programId + "' ";
            else
                query += "AND WOH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";

            query += "order by WOH.LastActivityDate desc";
            query = query.Replace("<IdleOver>", IdleOver);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
            filterString = "> Program = '" + ProgramName + "' ";

            filterString += " | Idle Over = '" + IdleOver + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("013", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWipAging = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}