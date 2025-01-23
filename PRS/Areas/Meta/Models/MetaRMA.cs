using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaRMA
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

        public List<Hashtable> lstMetaRMA { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList(string programId, string ProgramName, string RMARef, string frmDt, string toDt)
        {

            string query = string.Empty;

            query = @"
SELECT ROH.ID,
       ROH.ProgramID,
       ROH.biztalkid AS MessageId,
       ROH.customerreference AS CustomerReferenceNo,
       RDL.trackingno AS AWB,
       RDL.carriername AS CarrierRouting,
       ROL.partno AS Model_Catalog,
       ROU.serialno,
       ( CASE
           WHEN ROU.prealert = 1 THEN ROU.serialno
         END )
       PreRegistered_SN,
       ROH.id AS IFS_RMA_Order_No,
       CONVERT(DATE, ROH.createdate, 121) AS RMA_Created,
       REPLACE(Try_convert(Varchar(30), Try_convert(date, DLA.value), 121), '-', '.')  AS DeliveryDate,
       RDL.id AS Shipment_ID,
       RDL.createdate AS Dock_Log,
       Datediff(day, CONVERT(DATE, ROH.createdate, 121),
       Try_convert(Varchar(30), Try_convert(date, DLA.value), 121)) AS  Receiving_TAT,
       ROU.createdate AS Received_Date,
       ROLA.value AS Cust_PO_NO,
       ROL.id AS RMA_LINE_NO,
       ROL.partno AS PartNO,
       PN.description AS
       PartDescription,
       CS.description AS Status,
       ROH.ordertypeid AS
       WorkTypeId
FROM   pls.roheader ROH
       INNER JOIN pls.rodocklog RDL
               ON ROH.id = RDl.roheaderid
       INNER JOIN pls.roline ROL
               ON ROL.roheaderid = ROH.id
       INNER JOIN pls.rounit ROU
               ON ROU.rolineid = ROL.id
       INNER JOIN pls.rodocklogattribute DLA
               ON DLA.rodocklogid = RDL.id
       LEFT JOIN pls.codeattribute CAA
              ON CAA.id = DLA.attributeid
                 AND CAA.attributename = 'DELIVERED_DATE'
       LEFT JOIN pls.rolineattribute ROLA
              ON ROLA.rolineid = ROL.id
       LEFT JOIN pls.codeattribute CA
              ON CA.id = ROLA.attributeid
                 AND CA.attributename = 'CUSTOMERPONUMBER'
       LEFT JOIN pls.codestatus CS
              ON CS.id = ROL.statusid
       INNER JOIN pls.partno PN
               ON PN.partno = ROL.partno
    WHERE CONVERT(Date, ROH.createdate) >= '<frmDt>' AND CONVERT(Date, ROH.createdate) <= '<toDt>'
";



            if (programId != "0" && programId != null)
            {
                query += " AND ROH.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += " AND ROH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }


            if (!string.IsNullOrEmpty(RMARef))
            {
                query += "AND roh.CustomerReference = '<RMARef>'";
            }

            query = query.Replace("<RMARef>", RMARef);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";


            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            if (!string.IsNullOrEmpty(RMARef))
                filterString += "| Customer Ref. = '" + RMARef + "' ";
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("133", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaRMA = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}