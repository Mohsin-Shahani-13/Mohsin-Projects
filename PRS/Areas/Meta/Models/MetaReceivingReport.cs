using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

// Name change to Month to Date Receiving Report*@
namespace IP.Areas.Meta.Models
{
    public class MetaReceivingReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }

        [Display(Name = "Program:")]
        public string program_Id { get; set; }


        [Display(Name = "Customer Ref.:")]
        public string RMARef { get; set; }

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }


        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstMetaReceiving { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList(string programId, string ProgramName, string RMARef, string frmDt, string toDt)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"SELECT
        roh.ID,
        P.ID As ProgramID,
		P.Name As ProgramName, 
        (CASE WHEN TimeZone = 'Central Europe Standard Time' THEN 'EMEA'
		 WHEN TimeZone IN ('Singapore Standard Time','Eastern Standard Time') THEN 'APAC'
		 ELSE 'AMER' END) Region,
		 pt.ID AS [TRANSACTION_ID],
        cpt.[Description] AS [TRANSACTION],
        roh.BizTalkID AS [PAYLOAD_ID],
        roh.CustomerReference AS [RMA_REF],
        rol.BizTalkID AS [LINE_ID],
        pn.PartNo AS [PART_NO],
        pn.[Description] AS [DESCRIPTION],
        pt.SerialNo AS [SERIAL_NO],
        SUM(pt.Qty) AS [QUANTITY],
        CO.Description AS [WORK_TYPE_ID],
        pn.ModelNo AS [TYPE_DESIGNATION],
        u.Username AS [RECEIVED_BY],
                FORMAT(pt.CreateDate, 'MM/dd/yyyy hh:mm:ss tt') AS [DATE_TIME],
                pt.CreateDate AS [APPLIED],
        rha.Value AS ORDER_TYPE
        --PNAF.Value AS FAMILY,
        --RDL.CreateDate AS DOCK_ON
FROM pls.PartTransaction pt

INNER JOIN pls.Program p ON p.ID = pt.ProgramID
INNER JOIN pls.CodePartTransaction cpt ON cpt.ID = pt.PartTransactionID
INNER JOIN pls.ROHeader roh ON pt.OrderHeaderID = roh.ID
INNER JOIN pls.ROLine rol ON pt.OrderLineID = rol.ID
           AND roh.ID = rol.ROHeaderID
INNER JOIN pls.PartNo pn ON pn.PartNo = pt.PartNo
INNER JOIN pls.[User] u ON u.ID = pt.UserID
LEFT JOIN pls.CodeAttribute CA ON CA.AttributeName = 'OrderCode'
LEFT JOIN  pls.ROHeaderAttribute rha ON rha.ROHeaderID = roh.ID
           AND rha.AttributeID = CA.Id
LEFT JOIN pls.CodeAttribute CAF ON CAF.AttributeName = 'FAMILY'
LEFT JOIN pls.PartNoAttribute PNAF ON PNAF.AttributeID = CAF.ID AND
                                      PNAF.ProgramID = pt.ProgramID AND
                                      PNAF.PartNo = pt.PartNo
LEFT JOIN pls.RODockLog RDL ON RDL.ROHeaderID = pt.RODockLogID
LEFT OUTER JOIN pls.CodeOrderType CO ON CO.ID = roh.OrderTypeID
WHERE pt.PartTransactionId = 1
AND CONVERT(Date, pt.CreateDate) >= '<frmDt>' AND CONVERT(Date, pt.CreateDate) <= '<toDt>' ";



            //query += " WHERE CRT.ID  IN ('<RepairTypeID>') ";

            if (programId != "0" && programId != null)
            {
                query += " AND P.ID = '" + programId + "' ";
            }
            else
            {
                query += " AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }


            if (!string.IsNullOrEmpty(RMARef))
            {
                query += "AND roh.CustomerReference = '<RMARef>'";
            }

            query += @" GROUP BY
     p.Name
    ,roh.ID
    ,P.ID
    ,P.Name
    ,P.TimeZone
    ,pt.ID  
    ,cpt.[Description]
    ,roh.BizTalkID  
    ,roh.ID  
    ,roh.CustomerReference
    ,rol.BizTalkID               
    ,pn.PartNo
    ,pn.[Description]
    ,pt.SerialNo
    ,CO.Description
    ,pn.ModelNo
    ,u.Username
    ,pt.CreateDate
    ,pt.CreateDate
    ,pt.OrderType
, rha.[Value]
, PNAF.Value
, RDL.CreateDate";

            // query = query.Replace("<OrderType>", OrderType);
            query = query.Replace("<RMARef>", RMARef);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);


            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(RMARef))
                filterString += "| Customer Ref. = '" + RMARef + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("119", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaReceiving = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}