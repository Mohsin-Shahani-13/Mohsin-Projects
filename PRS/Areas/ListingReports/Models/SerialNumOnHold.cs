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
    public class SerialNumOnHold
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstSerialNumOnHold { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT
PS.ProgramID,
P.Name As Program,
ROH.ID,
PS.PartNo,
PS.SerialNo,
PL.LocationNo,
PS.RODate,
PS.CreateDate,
CS.Description,
ROH.CustomerReference,
PT.Reason

FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
LEFT OUTER JOIN pls.PartTransaction PT ON (PT.PartTransactionID = 12) AND PS.SerialNo = PT.SerialNo
LEFT OUTER JOIN pls.PartLocation PL ON PS.ProgramID = PL.ProgramID AND PS.LocationID = PL.ID
LEFT OUTER JOIN pls.CodeStatus CS ON PS.StatusID = CS.ID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID
WHERE (PS.StatusID = 28) AND (NOT (PT.Reason IS NULL)) AND PL.LocationNo = PT.ToLocation
";

            if (programId != "0")
            {
                query += "AND PS.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "AND PS.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            query += "Group by PS.ProgramID,P.Name,ROH.ID,PS.PartNo,PS.SerialNo,PL.LocationNo,PS.RODate,PS.CreateDate,CS.Description,ROH.CustomerReference,PT.Reason";
            //query = query.Replace("<programId>",programId);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("016", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSerialNumOnHold = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}