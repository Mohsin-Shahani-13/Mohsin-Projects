using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;


namespace IP.Areas.Biztalk.Models
{
    public class Outbound
    {
        cDAL oDAL;
        #region Fields
        [Display(Name = "Contract:")]
        public string contract { get; set; }
        [Display(Name = "Customer Order Type:")]
        public string custordertype { get; set; }
        [Display(Name = "Company:")]
        public string company { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
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
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
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
        [Display(Name = "Message Type:")]
        public string OutMsgType { get; set; }
        public string isFail { get; set; }
        public bool ischecked { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public string DocPath { get; set; }
        public List<Hashtable> lstOutbound { get; set; }     
        public List<Hashtable> lstSummary { get; set; }
        public List<Hashtable> lstGetOutbound { get; set; }
        public List<Hashtable> lstDetail { get; set; }
        public List<Hashtable> lstSerial { get; set; }
        public List<Hashtable> lstattribute { get; set; }
        public List<Hashtable> lstLineAttr { get; set; }
        public List<Hashtable> lstSerialAttr { get; set; }
        public List<ArrayList> lstOutMsgType { get; set; }
        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
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
        public bool GetMessageType()
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
                    from Outmessage_hdr WITH (NOLOCK) where  Message_Type != ' ' ORDER BY Message_Type";

            DataTable dt = oDAL.GetData(query);
            lstOutMsgType = cCommon.ConvertDtToArrayList(dt);
            if (!oDAL.HasErrors)
                return true;
            else
                return false;
        }
        public bool GetList(string contract, string frmDt, string toDt, string orderNo, string serialNo, string custordertype , string OutMsgType, string isFail, string isSuccessed, string isUnprocessed)
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

            //string _OutMsgType = GetInValue(OutMsgType);
            string query = string.Empty;

            if (!string.IsNullOrEmpty(isUnprocessed))
            {
                
                query = @" 
                SELECT Outmessage_Hdr_Id
                        , Message_Type
                        , Message_Sub_Type
                        , Processed
                        , Message
                        , Message AS CommentTextForExport
                         , Insert_Date
                         , Processed_Date
                         , Source
                         , RetryCount
                         , AllowDuplicate
                         , Sender_Id
                         , Customer_Message_Id
                         , Contract
                         , Customer_order_No
                         , Customer_Prev_order_No
                         , Customer_Order_type
                         , Customer_Id
                         , Order_Date
                         , Guid
                         , CO_number
                         , C01
                         , C02
                         , C03
                         , C04
                         , C05
                         , C06
                         , C07
                         , C08
                         , C10
                         , C20
                         , Notes1
                         , Notes2
                         , Notes3
                         , Notes4
                         , Reference_No1
                         , N01
                         , N02
                         , N03
                         , N05
                         , N06
                         , D01
                         , D02
                         , D03
                         , From_Org_Code
                         , Carrier_Name
                         , Waybill
                         , Carrier_Code
                         , Supplier_Name
                         , Supplier_Code
                         , IFS_order_no
                         , Ship_To_Code
                         , Contact
                         , BillOfLading
                         , Currency
                         , Carrier
                         , NeedIfsUpdate
                         , IFSUpdateMessage
                         , IFSUpdateMessage As CommentTextForExport1
                          , Delivered_to_Biztalk
                          , IFS_Message_id
FROM Outmessage_hdr
WHERE Processed = 'N'
 ";

            }

            else
            {
                query = @" 
SELECT  Outmessage_Hdr_Id	
		,Message_Type	
		,Message_Sub_Type	
		,Processed   
		,Message
        ,Message AS CommentTextForExport
		,Insert_Date	
		,Processed_Date	
		,Source	
		,RetryCount	
		,AllowDuplicate		
		,Sender_Id		
		,Customer_Message_Id	
		,Contract		
		,Customer_order_No	
		,Customer_Prev_order_No	
		,Customer_Order_type
        ,Customer_Id
		,Order_Date		
		,Guid			
		,CO_number		
		,C01			
		,C02			
		,C03			
		,C04			
		,C05			
		,C06			
		,C07			
		,C08			
		,C10			
		,C20			
		,Notes1
		,Notes2	
		,Notes3	
		,Notes4	
		,Reference_No1	
		,N01	
		,N02	
		,N03	
		,N05	
		,N06	
		,D01	
		,D02	
		,D03	
		,From_Org_Code	
		,Carrier_Name	
		,Waybill	
		,Carrier_Code	
		,Supplier_Name	
		,Supplier_Code	
		,IFS_order_no	
		,Ship_To_Code		
		,Contact	
		,BillOfLading	
		,Currency	
		,Carrier	
		,NeedIfsUpdate	
		,IFSUpdateMessage
        ,IFSUpdateMessage As CommentTextForExport1
		,Delivered_to_Biztalk	
		,IFS_Message_id		
FROM Outmessage_hdr
WHERE CONVERT(Date, Processed_Date) >= '<frmDt>' AND CONVERT(Date,Processed_Date) <= '<toDt>'
 ";
            }
            

            if (contract != "All")
            {
                query += "AND Contract = '" + contract + "' ";
            }
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            if (!string.IsNullOrEmpty(custordertype))
                query += "AND Customer_Order_type LIKE '%" + custordertype + "%' ";
            //query = query.Replace("<Customer_order_No>", orderNo);
            if (!string.IsNullOrEmpty(orderNo))
                query += "AND Customer_order_No LIKE '%" + orderNo + "%' ";
            if (!string.IsNullOrEmpty(serialNo))
                query += "AND Outmessage_Hdr_Id IN (select distinct Outmessage_Hdr_Id from dbo.Outmessage_Serial where Serial_no = '"+ serialNo +"')";
            if (!OutMsgType.Equals("All"))
            {
                query += "AND Message_Type IN (" + OutMsgType + ")";
            }

            if (!string.IsNullOrEmpty(isFail))
            {
                query += "AND Processed = 'F' ";
            }
            if (!string.IsNullOrEmpty(isSuccessed))
            {
                query += "AND Processed IN ('A', 'T') ";
            }


            query += "ORDER BY Outmessage_Hdr_Id DESC";

            DataTable dt = oDAL.GetData(query);

            //Filterstring

            filterString += "> Contract = '" + contract + "' ";
            if (string.IsNullOrEmpty(isUnprocessed))
            {
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            }
        
            if (!string.IsNullOrEmpty(custordertype))
                filterString += " | Customer Order Type Like '" + custordertype + "' ";
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
            oLog.AddSqlQuery("025", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstOutbound = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public string GetDocPath(string contract)
        {
            cDAL oDAL1 = new cDAL("INIT");
            string sqlPath = @"select DocPath from [dbo].[OutboundDocs] WHERE Contract = '" + contract + "' ";
            DocPath = oDAL1.GetObject(sqlPath).ToString();

            return DocPath;
        }

        public bool Summary(string contract, string type, string isFail, string isSuccessed, string isUnprocessed)
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
FROM  Outmessage_hdr hdr WITH(NOLOCK)
WHERE Contract = '<contract>' ";
            if (string.IsNullOrEmpty(isUnprocessed))
            {
                query += "AND CONVERT(Date, Processed_Date) >= GETDATE() - 30";
            }
            query += @"
                
@Processed
) temp
PIVOT
(
	COUNT(Count_Days) 
	FOR Count_Days In ([Today],[WithInAWeek], [WithInAMonth])
)
AS Pivottbl

 ";

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
            oLog.AddSqlQuery("025", query, string.Empty, false);

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

        public bool GetOutbound(string contract, string msgType, string Today, string WithInAWeek, string WithInAMonth, string Total, string isFail)
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
            query = @"
  
SELECT 
      Outmessage_Hdr_Id
      ,Message_Type
      ,Message_Sub_Type
      ,Processed
      ,C02
      ,C06
      ,C16
      ,Message
      ,Message AS CommentTextForExport
      ,Insert_Date
      ,Processed_Date
      ,Response_Date
      ,Source
      ,RetryCount
      ,DestinationMsgName
      ,DestinationMsgType
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
FROM Outmessage_hdr
WHERE Contract = '<contract>'
 ";
            if (!string.IsNullOrEmpty(msgType))
                query += "AND Message_Type = '" + msgType + "' ";

            if (!string.IsNullOrEmpty(Today))
                query += "AND CAST(Processed_Date AS date) = CAST(Getdate() AS date) ";

            if (!string.IsNullOrEmpty(WithInAWeek))
                query += "AND CONVERT(DATE, Processed_Date) >= DATEADD(DAY,-7 , GETDATE()) AND CONVERT(DATE, Processed_Date) <= GETDATE() - 1";

            if (!string.IsNullOrEmpty(WithInAMonth))
                query += "AND CONVERT(DATE, Processed_Date) >= DATEADD(DAY,-30 , GETDATE()) AND CONVERT(DATE, Processed_Date) <= GETDATE() - 7";

            if (!string.IsNullOrEmpty(Total))
                query += "AND CONVERT(DATE, Processed_Date) >=  GETDATE() - 30";
            // query += "ORDER BY Insert_Date DESC";
            query = query.Replace("<contract>", contract);

            if (isFail == "Y")
            {
                query += "AND Processed = 'F'";
            }

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("025", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstGetOutbound = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }


        public bool GetDetail(string hdrId)
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
            #region Header

            query = @"

SELECT  Outmessage_Hdr_Id
       ,Contract
       ,Company
       ,Message_Type
       ,Message_Sub_Type
	   ,Processed
       ,Message
	   ,Sender_Id 
       ,Customer_Message_Id 
	   ,Customer_order_No
       ,Customer_Order_type
       ,Order_Code
       ,Buyer_Code
       ,Authorize_Code
       ,TrackingRef
       ,BillOfLading
       ,ComercialInvoice
       ,Processed_Date 
       ,Response_Date
	   ,Order_Date   
       ,Insert_Date
FROM   Outmessage_hdr
WHERE  Outmessage_Hdr_Id = '<hdrId>' ";

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
                Processed = dr["Processed"].ToString();
                msg = dr["Message"].ToString();
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
                processedDate = dr["Processed_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Processed_Date"]).ToString("yyyy.MM.dd HH:mm");
                responseDate = dr["Response_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Response_Date"]).ToString("yyyy.MM.dd HH:mm");
                orderDate = dr["Order_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Order_Date"]).ToString("yyyy.MM.dd HH:mm");
                insertDate = dr["Insert_Date"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["Insert_Date"]).ToString("yyyy.MM.dd HH:mm");
            }
            #endregion

            query = @"
                          --Line

SELECT   ML.Outmessage_Hdr_Id
		,ML.Line_No
        ,(SELECT CASE WHEN COUNT(Serial_no) > 0 THEN 'Y' ELSE 'N' END
         FROM Outmessage_Serial
         WHERE Line_No = ML.Line_No AND Outmessage_Hdr_Id = ML.Outmessage_Hdr_Id ) HAS_LN
		,ML.Qty	
		,ML.Qty_UOM	
		,ML.Part_no	
		,ML.Description	
		,ML.Price	
		,ML.Cust_Line_Id	
		,ML.Model_Catalog_No	
		,ML.Configuration_Code	
		,ML.Airbill_No1	
		,ML.Notes1		
		,ML.C02	
		,ML.C03	
		,ML.C04	
		,ML.C05	
		,ML.C06	
		,ML.C07	
		,ML.C08	
		,ML.C09	
		,ML.C10	
		,ML.N01	
		,ML.N02	
		,ML.N03	
		,ML.D01	
		,ML.Price_UOM	
		,ML.Weight_UOM	              
FROM  OutMessage_line ML
WHERE Outmessage_Hdr_Id = '<hdrId>' ;

 ";

            query = query.Replace("<hdrId>", hdrId);

            filterString = "Header Id = '" + hdrId + "' ";


            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_OUTBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_OUTBOUND");
            }
            //oDAL = new cDAL("Z004_INBOUND");
            DataSet DS = oDAL.GetDataSet(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("025-1", query, "Outbound Detail", false);

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
                oDAL = new cDAL("Z004_OUTBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_OUTBOUND");
            }

            string query = string.Empty;
            query = @"
                    --Serial

SELECT  Outmessage_Hdr_Id	
		,Line_No	
		,Serial_no	
		,Serial_no2	
		,RMA_No	
		,RMA_Line_No	
		,C01	
		,C02	
		,C03	
		,QTY_TO_RETURN	
		,REPAIR_CODE_ID	
		,PALLET_ID	
FROM Outmessage_Serial 
WHERE Outmessage_Hdr_Id = '<hdrId>' AND Line_No = '<lineNo>'

 ";

            query = query.Replace("<lineNo>", lineNo);
            query = query.Replace("<hdrId>", hdrId);

            filterString = "Line No. = '" + lineNo + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("025-2", query, "Outbound Detail", false);


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
                oDAL = new cDAL("Z004_OUTBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_OUTBOUND");
            }
            string query = string.Empty;
            query = @"
                    --Attribute

SELECT  HdrInfo.Outmessage_Hdr_Id AS ID, 
		Hdr.Message_Type,
		Field_Name, 
		Value
FROM    Outmessage_Extra_Hdr_Info HdrInfo
INNER JOIN [dbo].[Outmessage_hdr] Hdr ON Hdr.Outmessage_Hdr_Id = HdrInfo.Outmessage_Hdr_Id
WHERE HdrInfo.Outmessage_Hdr_Id = '<hdrId>'
 ";

            //query = query.Replace("<lineNo>", lineNo);
            query = query.Replace("<hdrId>", hdrId);

            //filterString = "Line No. = '" + lineNo + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("025-2", query, "Outbound Detail", false);


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
                oDAL = new cDAL("Z004_OUTBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_OUTBOUND");
            }
            string query = string.Empty;
            query = @"
                    --Line Attribute

SELECT  Outmessage_Hdr_Id As ID,
		Field_Name, 
		Value
FROM	Outmessage_Extra_Line_Info
WHERE Outmessage_Hdr_Id = '<hdrId>'
 ";

            //query = query.Replace("<lineNo>", lineNo);
            query = query.Replace("<hdrId>", hdrId);

            //filterString = "Line No. = '" + lineNo + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("025-2", query, "Outbound Detail", false);


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
                oDAL = new cDAL("Z004_OUTBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_OUTBOUND");
            }
            string query = string.Empty;
            query = @"
                    --Serial Attribute

SELECT  Outmessage_Hdr_Id As ID,
		Field_Name, 
		Value
FROM    Outmessage_Extra_Serial_Info
WHERE Outmessage_Hdr_Id = '<hdrId>'
 ";

            //query = query.Replace("<lineNo>", lineNo);
            query = query.Replace("<hdrId>", hdrId);

            //filterString = "Line No. = '" + lineNo + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("025-2", query, "Outbound Detail", false);


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
                oDAL = new cDAL("Z004_OUTBOUND");
            else if (conType == "PROD")
                oDAL = new cDAL("Z001_OUTBOUND");

            string query = string.Empty;
            query = @"
                SELECT 
                       Outmessage_Hdr_Id
                      ,Message
                      ,Process
                FROM Outmessage_hdr
                WHERE Inmessage_Hdr_Id = '" + hdrID + "'";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        private string GetInValue(string Value)
        {
            string[] arr = Value.Split(',');
            string _arr = null;
            foreach (var item in arr)
            {
                if (_arr == null)
                {
                    _arr = "\'" + item + "\'";
                }
                else
                {
                    _arr += "," + "\'" + item + "\'";
                }

            }
            return _arr;
        }
    }
}