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
    public class DockReceipts
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstDockReceipts { get; set; }
        public List<Hashtable> lstDetail { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT  'View' AS DockLog, 
        ROH.ProgramID,
        P.Name,
        ROH.ID As ROHeaderId,
        ROH.CustomerReference,
        ROH.ThirdPartyReference,
        CS.Description As StatusID,
        ROL.PartNo,
        SUM(ROL.QtyToReceive) As QtyToReceive,
        SUM(ROL.QtyReceived) As QtyReceived,
        ROH.CreateDate As ROCreateDate
From pls.ROHeader ROH
INNER JOIN pls.Program P ON P.ID =ROH.ProgramID
INNER JOIN pls.CodeStatus CS ON ROH.StatusID = CS.ID
INNER JOIN PLS.ROLine ROL ON ROH.ID = ROL.ROHeaderID
where CS.Description <> 'RECEIVED' 
";

            if (programId != "0")
            {
                query += "AND ROH.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "AND ROH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }


            query += @"Group By
        ROH.ProgramID,
        P.Name,
        ROH.ID ,
        ROH.CustomerReference,
        ROH.ThirdPartyReference,
        CS.Description,
        ROL.PartNo,
        ROH.CreateDate
Order By ROH.CreateDate Desc
		";
            //query = query.Replace("<programId>",programId);


            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("021", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDockReceipts = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        public bool GetDetail(string ROHeaderId)
        {
            oDAL = new cDAL("ACTIVE");

            string query = string.Empty;
            query = @"  
SELECT	Distinct  RDL.ID
        , RDL.TrackingNo
		, RDL.CarrierName
		, RDL.Type
		, RDL.Qty
		, CC.Description Condition
		, CS.Description Status
		, U.Username
		, RDL.CreateDate
		, RDL.LastActivityDate
FROM pls.RODockLog RDL
LEFT JOIN Pls.CodeCondition CC ON CC.ID = RDL.ConditionID
INNER JOIN Pls.CodeStatus CS ON CS.ID = RDL.StatusID
INNER JOIN Pls.[User] U ON U.ID = RDL.UserID
WHERE ROHeaderID = '<ROHeaderId>'
";

            query = query.Replace("<ROHeaderId>", ROHeaderId);


            DataTable dt = oDAL.GetData(query);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstDetail = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}