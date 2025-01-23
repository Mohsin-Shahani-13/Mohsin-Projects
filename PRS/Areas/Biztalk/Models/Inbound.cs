using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Biztalk.Models
{
    public class Inbound
    {
        cDAL oDAL;
        #region Fields
        [Display(Name = "Contract:")]
        public string contract { get; set; }
        [Display(Name = "Company:")]
        public string company { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Customer Order Type:")]
        public string custordertype { get; set; }
        [Display(Name = "Customer Order No.:")]
        public string orderNo { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Header Id:")]
        public string HdrId { get; set; }
        [Display(Name = "Msg Type:")]
        public string messageType { get; set; }
        [Display(Name = "Sub Type:")]
        public string messageSubType { get; set; }
        [Display(Name = "Processed:")]
        public string Processed { get; set; }
        [Display(Name = "Sender Id:")]
        public string senderId { get; set; }
        [Display(Name = "CO Msg Id:")]
        public string customerMessageId { get; set; }
        [Display(Name = "CO Type:")]
        public string coType { get; set; }
        [Display(Name = "Order Code:")]
        public string orderCode { get; set; }
        [Display(Name = "Buyer Code:")]
        public string buyerCode { get; set; }
        [Display(Name = "Authorize Code:")]
        public string authorizeCode { get; set; }
        [Display(Name = "Tracking Ref.:")]
        public string trackingRef { get; set; }
        [Display(Name = "Bill Of Lading:")]
        public string billOfLading { get; set; }
        [Display(Name = "Commercial Invoice:")]
        public string comInv { get; set; }
        [Display(Name = "Response Date:")]
        public string responseDate { get; set; }
        [Display(Name = "Processed Date:")]
        public string processedDate { get; set; }
        [Display(Name = "Order Date:")]
        public string orderDate { get; set; }
        [Display(Name = "Added On:")]
        public string insertDate { get; set; }
        [Display(Name = "Message:")]
        public string msg { get; set; }
        [Display(Name = "Header Attr.:")]
        public string HeaderAttr { get; set; }
        [Display(Name = "Line Attr.:")]
        public string LineHeader { get; set; }
        [Display(Name = "Serial Attr.:")]
        public string SerialHeader { get; set; }
        public bool ischecked { get; set; }
        [Display(Name = "Message Type:")]
        public string inMsgType { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstInbound { get; set; }
        public List<Hashtable> lstDetail { get; set; }
        public List<Hashtable> lstSerial { get; set; }
        public List<Hashtable> lstattribute { get; set; }
        public List<Hashtable> lstLineAttr { get; set; }
        public List<Hashtable> lstSerialAttr { get; set; }
        public List<Hashtable> lstSummary { get; set; }
        public List<Hashtable> lstGetInbound { get; set; }
        public List<ArrayList> lstInMsgType { get; set; }
        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 

        public DataTable Contract() // Contract
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            //oDAL = new cDAL("Z004_INBOUND");
            string query = string.Empty;
            // query = "SELECT ID, ContractNo as contract FROM ContractList ORDER BY ContractNo ";
            if (conType == "TRAN")
            {
                query = @"SELECT DISTINCT Contract , Contract as ProgramName FROM Inmessage_hdr WITH (NOLOCK) 
WHERE
    TRY_CAST(LTRIM(RTRIM(Contract)) AS SMALLINT) IS NOT NULL-- Ensure valid numeric contracts

    AND LTRIM(RTRIM(Contract)) != ''

    ORDER BY contract";
            }
            else if (conType == "PROD")
            {
                query = @"SELECT DISTINCT 
    LTRIM(RTRIM(Imh.Contract)) AS Contract,  --Trim spaces from Contract
    COALESCE((P.Name + ' - ' + P.Site), LTRIM(RTRIM(Imh.Contract))) AS ProgramName  --If ProgramName is NULL, use Contract
FROM
    Inmessage_hdr Imh WITH(NOLOCK)
LEFT JOIN
    PLUS2.pls.Program P
    ON P.ID = TRY_CAST(LTRIM(RTRIM(Imh.Contract)) AS SMALLINT)-- Safely attempt conversion
WHERE
    TRY_CAST(LTRIM(RTRIM(Imh.Contract)) AS SMALLINT) IS NOT NULL-- Ensure valid numeric contracts

    AND LTRIM(RTRIM(Imh.Contract)) != ''
ORDER BY
    ProgramName ;
                 ";
            }
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetMessageType()
        {
            string query = string.Empty;
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }


            query = @"select distinct  
                    Message_Type
                    from Inmessage_hdr WITH (NOLOCK) where  Message_Type != ' ' ORDER BY Message_Type";

            DataTable dt = oDAL.GetData(query);
            lstInMsgType = cCommon.ConvertDtToArrayList(dt);
            if (!oDAL.HasErrors)
                return true;
            else
                return false;
        }
        public bool GetList(string contract, string frmDt, string toDt, string orderNo, string serialNo, string inMsgType, string isFail, string isSuccessed, string isUnprocessed)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            //oDAL = new cDAL("Z004_INBOUND");
            string query = string.Empty;

            if (!string.IsNullOrEmpty(isUnprocessed))
            {
                query = @"
SELECT 
       Inmessage_Hdr_Id
      ,Message_Type
      ,Message_Sub_Type
      ,Processed
      ,Message
      ,Message AS CommentTextForExport
      ,Insert_Date
      ,Processed_Date
      ,Response_Date
      ,Destination
      ,RetryCount
      ,SourceMsgName
      ,SourceMsgType
      ,AllowDuplicate
      ,Sender_Id
      ,Customer_Message_Id
      ,Contract
      ,Customer_order_No
      ,Customer_Prev_order_No
      ,Customer_Order_type
      ,Order_Date
      ,Company
      ,Customer_Id
      ,Guid
      ,CO_Number
      ,C01
      ,C02
      ,C03
      ,C04
      ,C05
      ,C06
      ,C07
      ,C08
      ,C09
      ,C10
      ,C11
      ,Notes1
      ,Notes2
      ,Notes3
      ,Notes4
      ,Reference_No1
      ,N01
      ,N02
      ,D01
      ,D02
      ,To_Org_Code
      ,From_Org_Code
      ,Carrier_Name
      ,Waybill
      ,Carrier_Code
      ,Shipment_Date
      ,Supplier_Code
      ,ship_addr_no
      ,Incoterms
      ,Agreement
FROM Inmessage_hdr WITH (NOLOCK)
WHERE Processed = 'N'
 ";
            }

            else
            {
                query = @"
SELECT 
       Inmessage_Hdr_Id
      ,Message_Type
      ,Message_Sub_Type
      ,Processed
      ,Message
      ,Message AS CommentTextForExport
      ,Insert_Date
      ,Processed_Date
      ,Response_Date
      ,Destination
      ,RetryCount
      ,SourceMsgName
      ,SourceMsgType
      ,AllowDuplicate
      ,Sender_Id
      ,Customer_Message_Id
      ,Contract
      ,Customer_order_No
      ,Customer_Prev_order_No
      ,Customer_Order_type
      ,Order_Date
      ,Company
      ,Customer_Id
      ,Guid
      ,CO_Number
      ,C01
      ,C02
      ,C03
      ,C04
      ,C05
      ,C06
      ,C07
      ,C08
      ,C09
      ,C10
      ,C11
      ,Notes1
      ,Notes2
      ,Notes3
      ,Notes4
      ,Reference_No1
      ,N01
      ,N02
      ,D01
      ,D02
      ,To_Org_Code
      ,From_Org_Code
      ,Carrier_Name
      ,Waybill
      ,Carrier_Code
      ,Shipment_Date
      ,Supplier_Code
      ,ship_addr_no
      ,Incoterms
      ,Agreement
FROM Inmessage_hdr WITH (NOLOCK)
WHERE CONVERT(Date, Processed_Date) >= '<frmDt>' AND CONVERT(Date,Processed_Date) <= '<toDt>'
";
            }

            if (contract != "All")
            {
                query += "AND Contract = '" + contract + "' ";
            }

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            //if (!string.IsNullOrEmpty(custordertype))
            //    query += "AND Customer_Order_type LIKE '%" + custordertype + "%' ";


            if (!string.IsNullOrEmpty(orderNo))
                query += "AND Customer_order_No LIKE '%" + orderNo + "%' ";
            if (!string.IsNullOrEmpty(serialNo))
                query += "AND Inmessage_Hdr_Id IN (select distinct Inmessage_Hdr_Id from dbo.Inmessage_Serial where Serial_no = '" + serialNo + "')";


            if (!inMsgType.Equals("All"))
            {
                query += "AND Message_Type IN (" + inMsgType + ")";
            }

            if (!string.IsNullOrEmpty(isFail))
            {
                query += "AND Processed = 'F' ";
            }
            if (!string.IsNullOrEmpty(isSuccessed))
            {
                query += "AND Processed IN ('A', 'T') ";
            }

            query += "ORDER BY Inmessage_Hdr_Id DESC";

            DataTable dt = oDAL.GetData(query);

            //Filterstring
            filterString += "> Contract = '" + contract + "' ";
            if (string.IsNullOrEmpty(isUnprocessed))
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            //if (!string.IsNullOrEmpty(custordertype))
            //    filterString += " | Customer Order Type Like '" + custordertype + "' ";
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
            oLog.AddSqlQuery("024", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstInbound = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool Summary(string contract, string type, string isFail, string isSuccessed, string isUnprocessed)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            //oDAL = new cDAL("Z004_INBOUND");

            string query = string.Empty;

            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            if (sites == "HONGKONG" || sites == "SUZHOU")
            {
                query = @"
  
select *
from 
(
select Contract, Message_Type
, case 
        when convert(date, DateAdd(Hour, 13, Processed_Date)) = convert(date, DateAdd(Hour, 13, getdate())) then 'Today'
        when convert(date, DateAdd(Hour, 13, Processed_Date)) >= convert(date, DateAdd(Hour, 13, getdate() - 7)) 
             and convert(date, DateAdd(Hour, 13, Processed_Date)) <= convert(date, DateAdd(Hour, 13, getdate() - 1)) then 'WithInAWeek'
        when convert(date, DateAdd(Hour, 13, Processed_Date)) >= convert(date, DateAdd(Hour, 13, getdate()-30))
             and convert(date, DateAdd(Hour, 13, Processed_Date)) <= convert(date, DateAdd(Hour, 13, getdate() - 8)) then 'WithInAMonth'
    end count_days  
from Inmessage_hdr hdr WITH (NOLOCK)
WHERE Contract = '<contract>'
 ";
                if (string.IsNullOrEmpty(isUnprocessed))
                {
                    query += @"and DateAdd(Hour, 13, Processed_Date) >=  DateAdd(Hour, 13, GETDATE()-30) ";
                }
                query += @"@Processed
) tmp
pivot
(
count(count_days)
for count_days in (Today, WithInAWeek, WithInAMonth)
) as PivotTable

 ";
            }

            else
            {
                query = @"
  
SELECT * FROM
(
SELECT   Contract   
        ,Message_Type 	
		,CASE  
		 WHEN CONVERT(DATE, Processed_date) = CONVERT(DATE, GETDATE()) THEN 'Today'
		 WHEN CONVERT(DATE, Processed_Date) >= DATEADD(DAY,-7 , GETDATE()) 
			  AND CONVERT(DATE, Processed_Date) <= GETDATE() -1 THEN 'WithInAWeek'
         ELSE 'WithInAMonth'
		 END Count_Days		
FROM  Inmessage_hdr hdr WITH (NOLOCK)
WHERE Contract = '<contract>'
 ";
                if (string.IsNullOrEmpty(isUnprocessed))
                {
                    query += "AND CONVERT(Date, Processed_Date) >= GETDATE() - 30";
                }
                query += @"@Processed
) temp
PIVOT
(
	COUNT(Count_Days) 
	FOR Count_Days In ([Today],[WithInAWeek], [WithInAMonth])
)
AS Pivottbl

 ";
            }

            query = query.Replace("<contract>", contract);

            if (!string.IsNullOrEmpty(isFail))
            {
                query = query.Replace("@Processed", "AND Processed = 'F' ");
            }

            if (!string.IsNullOrEmpty(isSuccessed))
            {
                query = query.Replace("@Processed", "AND Processed IN ('A', 'T') ");
            }

            if (!string.IsNullOrEmpty(isUnprocessed))
            {
                query = query.Replace("@Processed", "AND Processed = 'N' ");
            }


            else
            {
                query = query.Replace("@Processed", "");
            }


            //query += @" GROUP BY Message_Type, Contract, Processed ";

            // query += "ORDER BY Insert_Date DESC";

            filterString = "Report Type = '" + type + "' | Contract = '" + contract + "' ";

            if (!string.IsNullOrEmpty(isFail))
            {
                filterString += " | Failures ";
            }

            if (!string.IsNullOrEmpty(isSuccessed))
            {
                filterString += " | Successes ";
            }

            if (!string.IsNullOrEmpty(isUnprocessed))
            {
                filterString += " | Unprocessed ";
            }
            DataTable dt = oDAL.GetData(query);
            dt.Columns.Add("Total", typeof(int));

            foreach (DataRow row in dt.Rows)
            {
                int total = 0;
                for (int i = 2; i < dt.Columns.Count - 1; i++)
                {
                    int value1 = 0;
                    if (row[i] != DBNull.Value)
                        value1 = Convert.ToInt32(row[i]);

                    total += value1;
                }


                row["TOTAL"] = total;
            }

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("024", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSummary = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool GetInbound(string contract, string msgType, string Today, string WithInAWeek, string WithInAMonth, string Total, string isFail)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            //oDAL = new cDAL("Z004_INBOUND");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            if (sites == "HONGKONG" || sites == "SUZHOU")
            {
                query = @"
 
SELECT 
      Inmessage_Hdr_Id
      ,Message_Type
      ,Message_Sub_Type
      ,Processed
      ,C02
      ,Message
      ,Message AS CommentTextForExport
      ,Insert_Date
      ,Processed_Date
      ,Response_Date
      ,Destination
      ,RetryCount
      ,SourceMsgName
      ,SourceMsgType
      ,AllowDuplicate
      ,Sender_Id
      ,Customer_Message_Id
      ,Contract
      ,Customer_order_No
      ,Customer_Prev_order_No
      ,Customer_Order_type
      ,Order_Date
      ,Company
      ,Customer_Id
      ,Guid
FROM Inmessage_hdr WITH (NOLOCK)
WHERE Contract = '<contract>'
 ";

                if (!string.IsNullOrEmpty(Today))
                    query += "AND CAST(DATEADD(HOUR, 13, Processed_Date) AS DATE) = CAST(DATEADD(HOUR, 13, GETDATE()) AS date) ";

                if (!string.IsNullOrEmpty(WithInAWeek))
                    query += @"AND convert(date, DateAdd(Hour, 13, Processed_Date)) >= convert(date, DateAdd(Hour, 13, getdate() - 7))
                               AND convert(date, DateAdd(Hour, 13, Processed_Date)) <= convert(date, DateAdd(Hour, 13, getdate() - 1))  ";

                if (!string.IsNullOrEmpty(WithInAMonth))
                    query += @"AND convert(date, DateAdd(Hour, 13, Processed_Date)) >= convert(date, DateAdd(Hour, 13, getdate()-30)) 
             and convert(date, DateAdd(Hour, 13, Processed_Date)) <= convert(date, DateAdd(Hour, 13, getdate() - 8)) ";

                if (!string.IsNullOrEmpty(Total))
                    query += "AND CONVERT(DATE, DateAdd(Hour, 13,Processed_Date)) >=  convert(date, DateAdd(Hour, 13, getdate() - 30)) ";



            }

            else
            {
                query = @"
  
SELECT 
      Inmessage_Hdr_Id
      ,Message_Type
      ,Message_Sub_Type
      ,Processed
      ,C02
      ,Message
      ,Message AS CommentTextForExport
      ,Insert_Date
      ,Processed_Date
      ,Response_Date
      ,Destination
      ,RetryCount
      ,SourceMsgName
      ,SourceMsgType
      ,AllowDuplicate
      ,Sender_Id
      ,Customer_Message_Id
      ,Contract
      ,Customer_order_No
      ,Customer_Prev_order_No
      ,Customer_Order_type
      ,Order_Date
      ,Company
      ,Customer_Id
      ,Guid
FROM Inmessage_hdr WITH (NOLOCK)
WHERE Contract = '<contract>'
 ";
                if (!string.IsNullOrEmpty(Today))
                    query += "AND CAST(Processed_Date AS date) = CAST(Getdate() AS date) ";

                if (!string.IsNullOrEmpty(WithInAWeek))
                    query += "AND CONVERT(DATE, Processed_Date) >= DATEADD(DAY,-7 , GETDATE()) AND CONVERT(DATE, Processed_Date) <= GETDATE() - 1 ";

                if (!string.IsNullOrEmpty(WithInAMonth))
                    query += "AND CONVERT(DATE, Processed_Date) >= DATEADD(DAY,-30 , GETDATE()) AND CONVERT(DATE, Processed_Date) <= GETDATE() - 7 ";

                if (!string.IsNullOrEmpty(Total))
                    query += "AND CONVERT(DATE, Processed_Date) >=  GETDATE() - 30 ";

            }

            if (!string.IsNullOrEmpty(msgType))
                query += "AND Message_Type = '" + msgType + "' ";


            // query += "ORDER BY Insert_Date DESC";
            query = query.Replace("<contract>", contract);

            if (isFail == "Y")
            {
                query += "AND Processed = 'F'";
            }

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("024", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstGetInbound = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool GetDetail(string hdrId)
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            //oDAL = new cDAL("Z004_INBOUND");
            string query = string.Empty;
            #region Header

            query = @"

SELECT Inmessage_Hdr_Id, 
       Contract, 
       Company,
       Message_Type,
       Message_Sub_Type,
       Message,
	   Processed, 
	   Sender_Id, 
       Customer_Message_Id,   
	   Customer_order_No,
       Customer_Order_type,
       Order_Code,
       Buyer_Code,
       Authorize_Code,
       TrackingRef,
       BillOfLading,
       ComercialInvoice,
       FORMAT(Processed_Date, 'yyyy.MM.dd HH:mm') AS Processed_Date,
FORMAT(Processed_Date AT TIME ZONE 'Pacific Standard Time' AT TIME ZONE 'Singapore Standard Time', 'yyyy.MM.dd HH:mm') AS Processed_Date_Converted,
       FORMAT(Response_Date, 'yyyy.MM.dd HH:mm') AS Response_Date,
FORMAT(Response_Date AT TIME ZONE 'Pacific Standard Time' AT TIME ZONE 'Singapore Standard Time', 'yyyy.MM.dd HH:mm') AS Response_Date_Converted,
	   Order_Date,     
       FORMAT(Insert_Date, 'yyyy.MM.dd HH:mm') AS Insert_Date,
FORMAT(Insert_Date AT TIME ZONE 'Pacific Standard Time' AT TIME ZONE 'Singapore Standard Time', 'yyyy.MM.dd HH:mm') AS Insert_Date_Converted
FROM   Inmessage_hdr WITH (NOLOCK)
WHERE  Inmessage_Hdr_Id = '<hdrId>' ";

            query = query.Replace("<hdrId>", hdrId);

            filterString = "Header Id = '" + hdrId + "' ";

            //oDAL = new cDAL(HttpContext.Current.Request["DB"]);
            DataTable dtHeader = oDAL.GetData(query);



            if (dtHeader.Rows.Count > 0)
            {
                DataRow dr = dtHeader.Rows[0];
                contract = dr["Contract"].ToString();
                company = dr["Company"].ToString();
                messageType = dr["Message_Type"].ToString();
                messageSubType = dr["Message_Sub_Type"].ToString();
                msg = dr["Message"].ToString();
                Processed = dr["Processed"].ToString();
                senderId = dr["Sender_Id"].ToString();
                customerMessageId = dr["Customer_Message_Id"].ToString();
                orderNo = dr["Customer_order_No"].ToString();
                coType = dr["Customer_Order_type"].ToString();
                orderCode = dr["Order_Code"].ToString();
                buyerCode = dr["Buyer_Code"].ToString();
                authorizeCode = dr["Authorize_Code"].ToString();
                trackingRef = dr["TrackingRef"].ToString();
                billOfLading = dr["BillOfLading"].ToString();
                comInv = dr["ComercialInvoice"].ToString();

                if ((sites == "HONGKONG" || sites == "SUZHOU") )
                {
                    processedDate = dr["Processed_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Processed_Date"]).AddHours(14).ToString("yyyy.MM.dd HH:mm");
                    responseDate = dr["Response_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Response_Date"]).AddHours(14).ToString("yyyy.MM.dd HH:mm");
                    insertDate = dr["Insert_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Insert_Date"]).AddHours(14).ToString("yyyy.MM.dd HH:mm");
                }
                else
                {
                    processedDate = dr["Processed_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Processed_Date"]).ToString("yyyy.MM.dd HH:mm");
                    responseDate = dr["Response_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Response_Date"]).ToString("yyyy.MM.dd HH:mm");
                    insertDate = dr["Insert_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Insert_Date"]).ToString("yyyy.MM.dd HH:mm");
                }


                orderDate = dr["Order_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Order_Date"]).ToString("yyyy.MM.dd HH:mm");
            }
            #endregion

            query = @"
                             --Line

SELECT  ML.Inmessage_Hdr_Id
        ,ML.Line_No
        ,(SELECT CASE WHEN COUNT(Serial_no) > 0 THEN 'Y' ELSE 'N' END
        FROM Inmessage_Serial
        WHERE Line_No = ML.Line_No AND Inmessage_Hdr_Id = ML.Inmessage_Hdr_Id) HAS_LN
        ,ML.Message_Line_No
        ,ML.Qty
        ,ML.Qty_UOM
        ,ML.Part_no
        ,ML.Vendor_Part_No
        ,ML.Description
        ,ML.Price
        ,ML.Tax
        ,ML.Cust_Line_Id
        ,ML.Configuration_Id
        ,ML.Hazardous
        ,ML.Class_Code
        ,ML.Model_Catalog_No
        ,ML.Configuration_Code
        ,ML.Box_Length
        ,ML.Box_Width
        ,ML.Box_Height
        ,ML.Notes1
        ,ML.Notes2
        ,ML.Notes4
        ,ML.C01
        ,ML.C02
        ,ML.C03
        ,ML.C04
        ,ML.C05
        ,ML.C06
        ,ML.C07
        ,ML.C08
        ,ML.C09
        ,ML.C10
        ,ML.N02
        ,ML.N03
        ,ML.N04
        ,ML.N05
        ,ML.D01
        ,ML.D02
        ,ML.DO4
        ,ML.D05
        ,ML.Currency
        ,ML.e_c_c_n
        ,ML.hts_code2
        ,ML.country1
        ,ML.country2
        ,ML.Box_Weight
        ,ML.Delivery_Date
        ,ML.RR_Code
        ,ML.RE_Code
        ,ML.WT_ID
        ,ML.Pre_Reg
FROM InMessage_line ML 
WHERE Inmessage_Hdr_Id = '<hdrId>' ;

";

            query = query.Replace("<hdrId>", hdrId);

            filterString = "Header Id = '" + hdrId + "' ";


            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "TRAN")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            //oDAL = new cDAL("Z004_INBOUND");
            DataSet DS = oDAL.GetDataSet(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("024-1", query, "Inbound Detail", false);

            if (!oDAL.HasErrors)
            {
                List<ArrayList> lstDtl = new List<ArrayList>();

                lstDtl = cCommon.ConvertDtToArrayList(DS.Tables[0]);
                lstDetail = cCommon.ConvertDtToHashTable(DS.Tables[0]);


                return true;

            }
            else
            {
                return false;
            }

            #endregion
        }
        public bool GetSerial(string hdrId, string lineNo)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }

            string query = string.Empty;
            query = @"
                    --Serial

SELECT  Inmessage_Hdr_Id	
		,Line_No			
		,Serial_no		
		,Serial_no2		
		,FaultCode1		
		,Faultcode3	
		,Fault_Code_ID		
		,Carton_Id				
		,C01			
		,C02		
		,C09			
FROM Inmessage_Serial 
where Line_No = <lineNo> AND Inmessage_Hdr_Id = <hdrId>   
 ";

            query = query.Replace("<lineNo>", lineNo);
            query = query.Replace("<hdrId>", hdrId);

            filterString = "Line No. = '" + lineNo + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("024-2", query, "Inbound Detail", false);


            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstSerial = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }
        public bool GetAttribute(string hdrId)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            string query = string.Empty;
            query = @"
                    --Header Attribute

SELECT  HdrInfo.Inmessage_Hdr_Id As ID,
		Hdr.Message_Type,
		Field_Name, 
		Value
FROM    Inmessage_Extra_Hdr_Info HdrInfo
INNER JOIN [dbo].[Inmessage_hdr] Hdr ON HdrInfo.Inmessage_Hdr_Id = Hdr.Inmessage_Hdr_Id
WHERE HdrInfo.Inmessage_Hdr_Id = '<hdrId>'
 ";

            //query = query.Replace("<lineNo>", lineNo);
            query = query.Replace("<hdrId>", hdrId);

            //filterString = "Line No. = '" + lineNo + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("024-3", query, "Header Detail", false);


            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstattribute = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }

        public bool GetLineAttr(string hdrId)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            string query = string.Empty;
            query = @"
                    --Line Attribute

SELECT      Inmessage_Hdr_Id As ID,
			Field_Name, 
			Value
FROM        Inmessage_Extra_Line_Info 
WHERE Inmessage_Hdr_Id = '<hdrId>'
 ";

            //query = query.Replace("<lineNo>", lineNo);
            query = query.Replace("<hdrId>", hdrId);

            //filterString = "Line No. = '" + lineNo + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("024-3", query, "Line Detail", false);


            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstLineAttr = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }

        public bool GetSerialAttr(string hdrId)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            string query = string.Empty;
            query = @"
                    --Serial Attribute

SELECT      Inmessage_Hdr_Id As ID, 
			Field_Name, 
			Value
FROM        Inmessage_Extra_Serial_Info
WHERE Inmessage_Hdr_Id = '<hdrId>'
 ";

            //query = query.Replace("<lineNo>", lineNo);
            query = query.Replace("<hdrId>", hdrId);

            //filterString = "Line No. = '" + lineNo + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("024-3", query, "Serial Detail", false);


            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstSerialAttr = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }

        public DataTable GetMessageInfo(string hdrID)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            if (conType == "TRAN")
                oDAL = new cDAL("Z004_INBOUND");
            else if (conType == "PROD")
                oDAL = new cDAL("Z001_INBOUND");

            string query = string.Empty;
            query = @"
                SELECT 
                       Inmessage_Hdr_Id
                      ,Message
                      ,Processed
                FROM Inmessage_hdr
                WHERE Inmessage_Hdr_Id = '" + hdrID + "'";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }

    }
}
