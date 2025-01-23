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
    public class OutboundTransactionDetail
    {
        cDAL oDAL;
        #region Fields
        [Display(Name = "Contract:")]
        public string ProgramId { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Message Type:")]
        public string MsgType { get; set; }
        [Display(Name = "Processed:")]
        public string Processed { get; set; }
        [Display(Name = "Process Date:")]
        public string ProcessDateTime { get; set; }
        [Display(Name = "Sequence No.:")]
        public string SequenceNo { get; set; }      
        [Display(Name = "Transaction Id:")]
        public string TransactionId { get; set; }
        [Display(Name = "Message:")]
        public string Message { get; set; }
        [Display(Name = "Insert Date:")]
        public string InsertDate { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstOutboundTransactionDetail { get; set; }
        public List<ArrayList> lstMessageType { get; set; }
        public List<Hashtable> lstDetail { get; set; }
        public string lstMessage { get; set; }


        #endregion
        #region Methods 
        public DataTable Contract() // Contract
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
           
                oDAL = new cDAL("B2B");
            
            string query = string.Empty;
            query = @"SELECT DISTINCT ProgramId AS Contract 
                      FROM OutboundMessagesXML
                      WHERE ExecutionOrder > 0
                      ORDER BY Contract";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }

        public bool MessageType(string Contract)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            oDAL = new cDAL("B2B");

            string query = string.Empty;
            query = @"SELECT   
                         Description AS MessageType
                         FROM OutboundMessagesXML 
                         WHERE ProgramId = '<Contract>' 
                         AND ExecutionOrder > 0
                         GROUP BY ProgramId, Description";
            query = query.Replace("<Contract>", Contract);
            DataTable dt = oDAL.GetData(query);
            lstMessageType = cCommon.ConvertDtToArrayList(dt);
            return true;
        }

        public bool GetList(string contract, string MsgType, string frmDt, string toDt)
        {

            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();  
            
                oDAL = new cDAL("B2B");           
            string query = string.Empty;
            query = @" 
SELECT   ProgramId as Contract,
         MessageType,
		 Processed, 
		 ProcessDateTime,
		 SequenceNo,
		 InsertDate
FROM     OutboundTransactions 
WHERE CONVERT(Date, InsertDate) >= '<frmDt>' AND CONVERT(Date,InsertDate) <= '<toDt>'
 ";

            if (contract != "All")
            {
                query += "AND ProgramId = '" + contract + "' ";
            }
            if (MsgType != "All")
            {
                query += "AND MessageType = '" + MsgType + "' ";
            }
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
        
            DataTable dt = oDAL.GetData(query);

            //Filterstring
            filterString += "> Contract = '" + contract + "' ";
            filterString += "| Message Type = '" + MsgType + "' ";
            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("156", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstOutboundTransactionDetail = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        public bool GetDetail(string contract, string sequenceNo)
        {

            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            oDAL = new cDAL("B2B");
            string query = string.Empty;

            #region Header

            query = @"SELECT   ProgramId as Contract,
                               MessageType,
		                       Processed, 
		                       ProcessDateTime,
		                       SequenceNo,
		                       InsertDate
                      FROM     OutboundTransactions
                              where ProgramId = '<contract>' and SequenceNo = '<SequenceNo>'";


            query = query.Replace("<contract>", contract);
            query = query.Replace("<SequenceNo>", sequenceNo);


            DataTable dtHeader = oDAL.GetData(query);
           
            if (dtHeader.Rows.Count > 0)
            {
                DataRow dr = dtHeader.Rows[0];
                ProgramId = contract;
                MsgType = dr["MessageType"].ToString();
                Processed = dr["Processed"].ToString();
                ProcessDateTime = dr["ProcessDateTime"].ToString();
                SequenceNo = dr["SequenceNo"].ToString();
                InsertDate = dr["InsertDate"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["InsertDate"]).ToString("yyyy.MM.dd HH:mm:ss");


            }
            #endregion
            query = @"
SELECT   TransactionId,
         TransactionDate,
		 MessageType, 
		 CustomerReference,
		 PartNo,
		 SerialNo, 
		 Qty, 
         ProcessDateTime,
		 SequenceNo,
		 Message,
		 Processed,
		 InsertDate
FROM     Transactions 
where ProgramId = '<contract>' and SequenceNo = '<SequenceNo>'
Order By TransactionId Desc";

            query = query.Replace("<contract>", contract);
            query = query.Replace("<SequenceNo>", sequenceNo);


            oDAL = new cDAL("B2B");
            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("156-1", query, "Detail", false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDetail = cCommon.ConvertDtToHashTable(dt);
                return true;

            }

        }
        public string GetMessage(string TransactionId)
        {

            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            oDAL = new cDAL("B2B");
            string query = string.Empty;

            
            query = @"
            ---Message---
                    select 
                       Message 
                      from Transactions 
                      where TransactionId = '<TransactionId>' ";

            query = query.Replace("<TransactionId>", TransactionId);
          


            oDAL = new cDAL("B2B");
            string _result = oDAL.GetObject(query).ToString() ;

           

            return _result;

        }


    }
#endregion
}