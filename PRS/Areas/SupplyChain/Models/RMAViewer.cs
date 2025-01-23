using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class RMAViewer
    {
    #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public bool isAllDate { get; set; }
        [Display(Name = "RMA No.:")]
        public string RMANo { get; set; }
        [Display(Name = "Waybill:")]
        public string waybill { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public List<Hashtable> lstRMAViewerSummary { get; set; }
        public List<Hashtable> lstRMAViewerDetail { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        public string Report_Name { get; set; }

        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        #endregion
    #region methods
        cDAL oDAL = new cDAL("ACTIVE");
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>' AND NAME ='TOSHIBA'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);

            return dt;
        }

        public bool GetSummary(string frmDt, string toDt, string rptType, string RMANo)
        {
            string query = string.Empty;
            query = @"
declare @RMA varchar(30) = '<rmaNo>';
declare @From varchar(10) = '<fromDt>';
declare @To varchar(10) = '<toDt>';

SELECT 
	p.Name,
    p.ID AS ProgramID,
    ROH.ID,
    ROH.CustomerReference AS RMA, 
    ROH.ThirdPartyReference AS MessageID,
    roh.ID as RcvOrder, 
    CS.Description AS Status,
    cad.Name AS Customer,
    CONCAT(CAD.Address1, ' ', CAD.Address2) AS Address,
    CAD.City,
    SUM(ROL.QtyToReceive) AS To_Rcv,
    SUM(ROL.QtyReceived) AS Rcvd,
    SUM(ROL.QtyToReceive) - SUM(ROL.QtyReceived) AS Pending,
    FORMAT(ROH.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn, 
    FORMAT(ROH.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
FROM   
    pls.ROHeader ROH
    INNER JOIN PLS.ROLine ROL ON ROL.ROHeaderID = ROH.ID
    INNER JOIN pls.Program P ON P.ID = ROH.ProgramID
    INNER JOIN pls.[User] U ON U.ID = ROH.UserID 
    LEFT OUTER JOIN pls.CodeStatus CS ON CS.ID = ROH.StatusID
    LEFT OUTER JOIN pls.CodeOrderType COT ON COT.ID = ROH.OrderTypeID
    LEFT OUTER JOIN pls.CodeAddressDetails CAD ON CAD.AddressId = ROH.AddressID AND CAD.AddressType = 'ShipFrom'
WHERE 
    1 = 1
    AND (P.ID = '10013' )
    AND (ROH.CustomerReference LIKE @RMA OR @RMA = '')
    AND (CONVERT(Date, ROH.LastActivityDate) >= @From OR @From = '')
    AND (CONVERT(Date, ROH.LastActivityDate) <= @To OR @To = '')
GROUP BY 
	p.Name,
    p.ID,
    ROH.CustomerReference, 
    ROH.ThirdPartyReference,
    roh.ID, 
    cad.Name,
    CS.Description, 
    CONCAT(CAD.Address1, ' ', CAD.Address2),
    CAD.City,
    FORMAT(ROH.CreateDate, 'yyyy.MM.dd HH:mm'), 
    FORMAT(ROH.LastActivityDate, 'yyyy.MM.dd HH:mm')
ORDER BY 
    CreatedOn DESC;

";
            query = query.Replace("<rmaNo>", RMANo);
            query = query.Replace("<fromDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);

            ///////////FILTER SRINGS/////////

            if (!string.IsNullOrEmpty(rptType))
                filterString += " > Report Type = '" + rptType + "'";
            if (!string.IsNullOrEmpty(RMANo))
                filterString += " | RMA No. = '" + RMANo + "'";

            //if (!string.IsNullOrEmpty(ProgramName))
            //    filterString += " | Program = '" + ProgramName + "' ";

            //if (!string.IsNullOrEmpty(currentInventoryLocation))
            //    filterString += "> | PartSerial.PartLocationNo = '" + currentInventoryLocation + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("178", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRMAViewerSummary = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        public bool GetDetail(string frmDt, string toDt, string rptType, string RMANo, string waybill, string serialNo, string partNo)
        {
            string query = string.Empty;
            query = @"
        

declare @RMA varchar(30) = '<rmaNo>';
declare @From varchar(10) = '<fromDt>';
declare @To varchar(10) = '<toDt>';
SELECT 
	P.name AS Program,
    p.ID AS ProgramID,
    ROH.ID,
    roh.CustomerReference AS RMA,
    roh.ThirdPartyReference AS MessageID, 
    roh.ID as ReceiveOrder,
    CS.Description AS RMA_Status,
    doc.TrackingNo AS Waybill,
    --convert(varchar(16),doc.CreateDate,120)  AS DockLogged,
FORMAT(doc.CreateDate, 'yyyy.MM.dd HH:mm') AS DockLogged, 
    ROL.PartNo,
    ROU.SerialNo,
    SUM(CASE 
            WHEN pt.PartTransactionID = 1
            THEN pt.Qty
            ELSE pt.Qty * -1
        END) AS QtyReceived,
    CASE
        WHEN CC.Description = 'Good' THEN 'YES'
        ELSE 'NO'
    END AS SealedBox,
    cad.Name AS Customer,
    CONCAT(CAD.Address1, ' ', CAD.Address2) AS Address,
    CAD.City,
    FORMAT(ROH.CreateDate, 'yyyy.MM.dd HH:mm') AS RO_Created, 
    FORMAT(ROH.LastActivityDate, 'yyyy.MM.dd HH:mm') AS RO_LastActivity
FROM
    pls.ROHeader ROH
INNER JOIN PLS.ROLine ROL ON ROL.ROHeaderID = ROH.ID
INNER JOIN PLS.Program P ON P.ID = ROH.ProgramID
INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID
INNER JOIN PLS.CodeStatus CS ON CS.ID = ROH.StatusID
INNER JOIN pls.RODockLog doc ON doc.ROHeaderID = roh.ID
INNER join pls.PartTransaction pt on pt.SerialNo = ROU.SerialNo
                                 and pt.OrderHeaderID = roh.ID and pt.OrderLineID = ROL.ID and pt.OrderType = 'RO'
                                 and ((pt.PartTransactionID = 1 and pt.RODockLogID = doc.ID) or
                                      (pt.PartTransactionID = 2 and pt.RODockLogID is null)    )
LEFT OUTER JOIN PLS.CodeAddressDetails CAD ON CAD.AddressID = ROH.AddressID AND CAD.AddressType = 'ShipFrom'
LEFT OUTER JOIN pls.PartSerial PS ON PS.ROHeaderID = ROH.ID AND PS.PartNo = ROL.PartNo AND PS.SerialNo = ROU.SerialNo
LEFT OUTER JOIN PLS.CodeConfiguration CC ON CC.ID = ROL.ConfigurationID
WHERE
    1 = 1
    AND (P.ID = '10013' )
    AND (ROH.CustomerReference LIKE @RMA OR @RMA = '')
    AND (CONVERT(Date, ROH.LastActivityDate) >= @From OR @From = '')
    AND (CONVERT(Date, ROH.LastActivityDate) <= @To OR @To = '')

";
            if (!string.IsNullOrEmpty(waybill))
            {
                query += " AND doc.TrackingNo = '"+ waybill +"'";
            }
            if (!string.IsNullOrEmpty(partNo))
            {
                query += " AND ROL.PartNo LIKE '%" + partNo + "%'";
            }
            if (!string.IsNullOrEmpty(serialNo))
            {
                query += " AND ROU.SerialNo LIKE '%" + serialNo + "%'";
            }
            query = query.Replace("<rmaNo>",RMANo);
            query = query.Replace("<fromDt>",frmDt);
            query = query.Replace("<toDt>",toDt);
            query += @"GROUP BY

    P.name,
    p.ID,
    roh.CustomerReference,
    roh.ThirdPartyReference,
    roh.ID,
    CC.Description,
    doc.CreateDate,
    doc.TrackingNo,
    rol.id,
    ROL.PartNo,
    ROU.SerialNo,
    cad.Name,
    CS.Description,
    CAD.City,
    ROH.CreateDate,
    ROH.LastActivityDate,
    CONCAT(CAD.Address1, ' ', CAD.Address2)
ORDER BY

  RO_Created DESC,
  rol.PartNo,
    ROU.SerialNo";
            DataTable dt = oDAL.GetData(query);

            ///////////FILTER SRINGS/////////

            if (!string.IsNullOrEmpty(rptType))
                filterString += " > Report Type = '" + rptType + "'";
            if (!string.IsNullOrEmpty(RMANo))
                filterString += " | RMA No. = '" + RMANo + "'";
            if (!string.IsNullOrEmpty(waybill))
                filterString += " | Waybill = '" + waybill + "'";
            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. Like '" + partNo + "'";
            if (!string.IsNullOrEmpty(serialNo))
                filterString += " | Serial No. Like '" + serialNo + "'";

            //if (!string.IsNullOrEmpty(ProgramName))
            //    filterString += " | Program = '" + ProgramName + "' ";

            //if (!string.IsNullOrEmpty(currentInventoryLocation))
            //    filterString += "> | PartSerial.PartLocationNo = '" + currentInventoryLocation + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("178", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRMAViewerDetail = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
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
    #endregion
}