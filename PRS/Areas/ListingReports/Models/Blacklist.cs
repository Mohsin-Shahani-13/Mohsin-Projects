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
    public class Blacklist
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "User:")]
        public string User { get; set; }
        [Display(Name = "Reason:")]
        public string Reason { get; set; }
        [Display(Name = "Attribute:")]
        public string Attribute { get; set; }
        [Display(Name = "Attribute Value:")]
        public string AttributeValue { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }

        public List<Hashtable> lstBlacklist { get; set; }

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
                      WHERE SITE = '<site>' AND Name = 'BOSE'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetList(string programId, string programName, string fDate, string tDate, string user, string reason, string attribute, string attributeValue)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            //string programName = HttpContext.Current.Session["Program"].ToString();

            string query = string.Empty;

            query = @"WITH BlacklistReport AS (
    SELECT  
        ROB.[ID],
        ROB.[ProgramID],
        P.Name AS Program,
        CR.Description,
        CA.AttributeName,
        ROB.[Value],
        CS.Description AS Status,
        U.Username AS [User],
        ROB.[CreateDate],
        ROB.[LastActivityDate]
    FROM [pls].[ROBlackList] ROB
    INNER JOIN pls.Program P ON  P.ID  = ROB.ProgramID
    INNER JOIN pls.CodeStatus CS ON CS.ID = ROB.StatusID
    INNER JOIN Pls.[USER] U On u.ID = ROB.UserID
    INNER JOIN pls.CodeReason CR ON CR.ID = ROB.ReasonID
    LEFT JOIN pls.CodeAttribute CA ON CA.ID = ROB.AttributeID 
    WHERE ROB.[ProgramID] = <programId> 

),
FinsReport AS (
    SELECT 
        t.ProgramID,
        t.OrderHeaderID,
        t.PartTransaction AS PartTransaction,
        t.PartNo AS SKU,
        t.SerialNo AS SERIAL,
        t.qty,
        t.Configuration,
        t.location AS FROM_LOCATION,
        t.tolocation AS TO_LOCATION,
        t.CustomerReference AS ORDER_NO,
        t.CreateDate AS TRANSACTION_DATETIME,
        so.TrackingNo AS SHIPMENT_TRACKING_NO,
        tr.EventDescription AS SHIPMENT_STATUS
    FROM 
        pls.vPartTransaction t
    LEFT JOIN 
        pls.vSOShipmentInfo so ON so.SOHeaderID = t.OrderHeaderID
    LEFT JOIN 
        pls.vTrackingHeader tr ON tr.TrackingNo = so.TrackingNo
    JOIN 
        pls.vProgram p ON t.programID = p.id
    WHERE 
        t.ProgramID = <programId>
        AND t.tolocation LIKE 'FINS%' 
        AND t.PartTransaction = 'RO-RECEIVE'
)
SELECT 
    BR.ID,
    BR.ProgramID,
    BR.Program,
    BR.Description,
    BR.AttributeName,
    BR.Value,
    FR.SERIAL,
    FR.SKU,
    FR.FROM_LOCATION,
    FR.TO_LOCATION,
    FR.TRANSACTION_DATETIME,
    BR.Status,
    BR.[User],
    BR.CreateDate,
    BR.LastActivityDate
  
FROM 
    BlacklistReport BR
LEFT JOIN 
    FinsReport FR ON 
        (BR.AttributeName = 'RMA' AND BR.Value = FR.ORDER_NO) OR 
        (BR.AttributeName = 'Serial' AND BR.Value = FR.SERIAL)
WHERE BR.ProgramID = <programId>";

            query += " \nAND CONVERT(Date, BR.[CreateDate]) >= '<frmDt>' AND CONVERT(Date, BR.[CreateDate]) <= '<toDt>'";
            if (!string.IsNullOrEmpty(user))
                query += "\nAND BR.[User] LIKE '%" + user + "%' ";

            if (!string.IsNullOrEmpty(reason))
                query += "\nAND BR.Description LIKE '%" + reason + "%' ";

            if (!string.IsNullOrEmpty(attribute))
                query += "\nAND BR.AttributeName LIKE '%" + attribute + "%'";

            if (!string.IsNullOrEmpty(attributeValue))
                query += "\nAND BR.[Value] = '" + attributeValue + "'";

            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", fDate);
            query = query.Replace("<toDt>", tDate);

            query += "\nORDER BY BR.[LastActivityDate] desc";
            DataTable dt = oDAL.GetData(query);

            //if (!string.IsNullOrEmpty(programName))
            filterString += "> Program = '" + programName + "' ";

            filterString += " | From = '" + fDate + "' To = '" + tDate + "' ";
            if (!string.IsNullOrEmpty(user))
                filterString += " | User Like '" + user + "' ";
            if (!string.IsNullOrEmpty(reason))
                filterString += " | Reason Like '" + reason + "' ";

            if (!string.IsNullOrEmpty(attribute))
                filterString += " | Attribute Like '" + attribute + "' ";
            if (!string.IsNullOrEmpty(attributeValue))
                filterString += " | Attribute Value = '" + attributeValue + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("196", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstBlacklist = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        #endregion
    }
}