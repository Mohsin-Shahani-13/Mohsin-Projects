using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class VRSOutMessaging
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Contract:")]
        public string contract { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstVRSOutMessaging { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        public List<ArrayList> lstOutMsgType { get; set; }
        [Display(Name = "Customer Order No.:")]
        public string orderNo { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Message Type:")]
        public string OutMsgType { get; set; }
        public bool ischecked { get; set; }
        #endregion

        public DataTable Contract() // Contract
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_OUTBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_OUTBOUND");
            }
            //oDAL = new cDAL("Z004_INBOUND");
            string query = string.Empty;
            //query = "SELECT ID, ContractNo as contract FROM ContractList ORDER BY ContractNo ";
            if (conType == "TRAN")
            {
                query = @"SELECT DISTINCT Contract , Contract as ProgramName FROM Outmessage_hdr WITH (NOLOCK) 
WHERE
    TRY_CAST(LTRIM(RTRIM(Contract)) AS SMALLINT) IS NOT NULL-- Ensure valid numeric contracts

    AND LTRIM(RTRIM(Contract)) != ''

    ORDER BY contract ";
            }

            else if (conType == "PROD")
            {
                query = @"SELECT DISTINCT 
    LTRIM(RTRIM(Omh.Contract)) AS Contract,  --Trim spaces from Contract
    COALESCE((P.Name + ' - ' + P.Site), LTRIM(RTRIM(Omh.Contract))) AS ProgramName  --If ProgramName is NULL, use Contract
FROM
    Outmessage_hdr Omh WITH(NOLOCK)
LEFT JOIN
    PLUS2.pls.Program P
    ON P.ID = TRY_CAST(LTRIM(RTRIM(Omh.Contract)) AS SMALLINT)-- Safely attempt conversion
WHERE
    TRY_CAST(LTRIM(RTRIM(Omh.Contract)) AS SMALLINT) IS NOT NULL-- Ensure valid numeric contracts

    AND LTRIM(RTRIM(Omh.Contract)) != ''
ORDER BY
    ProgramName
                 ";
            }
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public List<ArrayList> GetMessageType(int contract)
        {
            string query = string.Empty;
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_OUTBOUND");
            }
            else
            {
                oDAL = new cDAL("Z001_OUTBOUND");
            }


            query = @"select distinct  
                    Message_Type
                    from Outmessage_hdr WITH (NOLOCK) where  Message_Type != ' ' AND Contract = '<contract>' ORDER BY Message_Type";
            query = query.Replace("<contract>", contract.ToString());
            DataTable dt = oDAL.GetData(query);
            lstOutMsgType = cCommon.ConvertDtToArrayList(dt);

            return lstOutMsgType;
        }
        public bool GetList(string contract, string fDate, string tDate, string orderNo, string serialNo, string OutMsgType, string isFail, string isSuccessed, string isUnprocessed)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_OUTBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_OUTBOUND");
            }
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"
select h.Outmessage_hdr_Id MESSAGE_ID,
h.Insert_Date       MESSAGE_INSERTED,
h.Processed_Date   MESSAGE_SENT,
h.message_type MESSAGE_TYPE,
h.c01              CERTIFICATE_TYPE,
h.Message MESSAGE_STATUS,
h.processed PROCESSED,
h.c06              OUTCOME,
h.c04              VENDOR_NAME,
l.c01              SERIAL_NUMBER,
h.contract CONTRACT,
h.sender_id SENDER_ID,
h.Customer_order_No    SHIPMENT_REFERENCE,
h.c05              CERT_TYPE,
h.c02              ISSUE_TO,
h.d01              ISSUE_DATE,
h.c03              LEVEL_DETAILS,
h.c02              VENDOR_NAME,
l.description COMMODITY,
l.vendor_part_no   MODEL,
l.c02              as ASSET_TAG,
l.c03              as DRIVE_SERIAL_NUMBER,
l.d01              as DATE_OF_WIPE,
l.c04              as SERVICE_REQUEST,
h.n01 N01,
h.notes1 NOTES1,
-- Categorize response into different error categories
CASE
WHEN h.Message LIKE
'%The request received has is empty or missing fields%' THEN
'EMPTY_OR_MISSING_FIELDS'
WHEN h.Message LIKE
'%The request was aborted: The request was canceled%' THEN
'REQUEST_ABORTED_CANCELED'
WHEN h.Message LIKE '%Serial cannot be found%' THEN
'SERIAL_NOT_FOUND'
WHEN h.Message LIKE '%MESSAGE SENT%' THEN
'NO_ACKNOWLEDGEMENT'
ELSE
'UNDEFINED'
END AS ERROR_CATEGORY
from [dbo].[Outmessage_hdr] h
join [dbo].[OutMessage_line] l
on  l.Outmessage_hdr_Id = h.Outmessage_hdr_Id
WHERE h.contract = '<contract>'
 
@Processed ";

            if (string.IsNullOrEmpty(isUnprocessed))
            {
                query += "AND CONVERT(Date, h.Processed_Date) >= '<frmDt>' AND CONVERT(Date,h.Processed_Date) <= '<toDt>'";
            }
            if (!string.IsNullOrEmpty(orderNo))
                query += "AND h.Customer_order_No LIKE '%" + orderNo + "%' ";

            if (!string.IsNullOrEmpty(serialNo))
                query += "AND l.c01 = ('" + serialNo + "')";

            if (!OutMsgType.Equals("All"))
            {
                query += "AND h.message_type IN (" + OutMsgType + ")";
            }

            if (!string.IsNullOrEmpty(isFail))
            {
                query = query.Replace("@Processed", "AND h.processed = 'F' ");
            }

            if (!string.IsNullOrEmpty(isSuccessed))
            {
                query = query.Replace("@Processed", "AND h.processed IN ('A', 'T') ");
            }

            if (!string.IsNullOrEmpty(isUnprocessed))
            {
                query = query.Replace("@Processed", "AND h.processed = 'N' ");
            }
            else
            {
                query = query.Replace("@Processed", "");
            }
            query += "order by h.Outmessage_hdr_Id desc ";

            query = query.Replace("<contract>", contract);
            query = query.Replace("<frmDt>", fDate);
            query = query.Replace("<toDt>", tDate);

            DataTable dt = oDAL.GetData(query);

            filterString += "> Contract = '" + contract + "' ";
            if (string.IsNullOrEmpty(isUnprocessed))
            {
                filterString += " | From = '" + fDate + "' To = '" + tDate + "' ";
            }

            if (!string.IsNullOrEmpty(orderNo))
                filterString += " | Customer Order No. Like '" + orderNo + "' ";
            if (!string.IsNullOrEmpty(serialNo))
                filterString += " | Serial No. = '" + serialNo + "' ";

            if (!string.IsNullOrEmpty(isFail))
            {
                filterString += " | Show Failures Only ";
            }
            if (!string.IsNullOrEmpty(isSuccessed))
            {
                filterString += " | Show Successes Only ";
            }
            if (!string.IsNullOrEmpty(isUnprocessed))
            {
                filterString += " | Show Unprocessed Only ";
            }

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("222", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstVRSOutMessaging = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}