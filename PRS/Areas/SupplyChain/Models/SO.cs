using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.SupplyChain.Models
{
    public class SO
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Customer Ref.:")]
        public string custRef { get; set; }
        [Display(Name = "Status:")]
        public string statusId { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "Third Party Ref.:")]
        public string thirdPartyReference { get; set; }
        [Display(Name = "Order Type:")]
        public string orderType { get; set; }
        [Display(Name = "Created On:")]
        public string createDate { get; set; }
        [Display(Name = "Last Activity On:")]
        public string lastActivityDate { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Status:")]
        public string description { get; set; }
        [Display(Name = "Created By:")]
        public string username { get; set; }

        [Display(Name = "Ship To Address:")]
        public string shipTo { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstSO { get; set; }
        public List<Hashtable> lstSODetail { get; set; }
        public List<Hashtable> lstDetail { get; set; }
        public List<Hashtable> lstSOUnit { get; set; }
        public List<Hashtable> lstSOUnitAccessory { get; set; }
        public List<ArrayList> lstStatus { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion

        #region Methods 
        public DataTable Program() // onHand warehouse method
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }

        public bool Status()
        {
            string query = string.Empty;
            query = @"SELECT DISTINCT 
                            CS.ID,
		                    CS.Description		
                FROM pls.CodeStatus CS
                Inner Join pls.SOHeader On CS.ID = StatusID 
                ORDER BY CS.Description	ASC ";
            DataTable dt = oDAL.GetData(query);
            lstStatus = cCommon.ConvertDtToArrayList(dt);
            if (!oDAL.HasErrors)
                return true;
            else
                return false;

            //return dt;
        }

        public bool GetSO(string Id, string frmDt, string toDt, string custRef, string status, string statusId, string type, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;

            if (!string.IsNullOrEmpty(statusId) && type.Equals("Reserved Units SO"))
            {

                query = @"
       SELECT SOH.ProgramID,
       SOH.ID,
       SOH.CustomerReference,
       SOH.ThirdPartyReference,
       SOSI.TrackingNo,
       P.Name AS Program,
	   CONCAT(CAD.Address1, ' ' ,CAD.Address2) AS Address
        ,CAD.City
	    ,CAD.State
	    ,CAD.Country
	    ,CAD.Zip
       ,SOHA.Value
	   , (
		  SELECT SUM(QtyToShip)
		  FROM [pls].[SOLine]
		  WHERE  [SOHeaderID]= SOH.ID
		) AS CO5_RightAlign
	  , (
		  SELECT SUM(QtyReserved)
		  FROM [pls].[SOLine]
		  WHERE [SOHeaderID]= SOH.ID
		) AS CO6_RightAlign,
       CS.Description AS Status,
       U.Username,
       SOH.CreateDate,
       SOH.LastActivityDate
FROM   [pls].[SOUnit] SOU  
INNER JOIN [pls].[SOLine] SOL ON SOL.ID = SOU.SOLineID
INNER JOIN [pls].[SOHeader] SOH ON SOH.id = SOL.SOHeaderID 
INNER JOIN pls.[User] U ON SOH.UserID = U.ID 
LEFT OUTER JOIN pls.Program P ON SOH.ProgramID = P.ID
LEFT OUTER JOIN pls.CodeAddressDetails CAD ON CAD.AddressID = SOH.AddressID AND CAD.AddressType = 'ShipTo'
LEFT OUTER JOIN pls.CodeStatus CS ON SOH.StatusID = CS.ID
LEFT OUTER JOIN pls.[SOShipmentInfo] SOSI ON SOH.ID = SOSI.SOHeaderID
LEFT JOIN pls.CodeAttribute CA on CA.AttributeName ='CUSTORDERTYPE'
LEFT JOIN pls.SOHeaderAttribute SOHA on SOHA.SOHeaderID = SOH.ID AND SOHA.AttributeID = CA.ID


                     ";
                query += "WHERE SOU.StatusID   = '<ID>'";
                //query += " AND P.ID ='" + programId + "' ";
                if (programId != "0" && programId != null)
                {
                    query += "AND P.ID = '" + programId + "' ";
                }
                else
                {
                    query += "AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }

                query = query.Replace("<ID>", statusId);
                query += "ORDER BY CreateDate DESC";
                filterString = "Status = '" + status + "'";
            }
            else if (!string.IsNullOrEmpty(statusId) && type.Equals("NEW SO") || type.Equals("RESERVED SO") || type.Equals("Partially Reserved SO"))
            {

                query = @"
     SELECT SOH.ProgramID,
       SOH.ID,
       SOH.CustomerReference,
       SOH.ThirdPartyReference,
        SOSI.TrackingNo,
       P.Name AS Program,
	   CONCAT(CAD.Address1, ' ' ,CAD.Address2) AS Address,
       CAD.City,
	   CAD.State,
	   CAD.Country,
	   CAD.Zip,
       SOHA.Value,
	   SUM(SOL.QtyToShip) AS QtyToShip,
	   SUM(SOL.QtyReserved)AS QtyShipped,
       CS.Description AS Status,
       U.Username,
       SOH.CreateDate,
       SOH.LastActivityDate
FROM   [pls].[SOHeader] SOH
INNER JOIN pls.SOLine SOL ON SOL.SOHeaderID = SOH.ID
INNER JOIN pls.Program P ON SOH.ProgramID = P.ID
INNER JOIN pls.CodeAddressDetails CAD ON CAD.AddressID = SOH.AddressID AND CAD.AddressType = 'ShipTo'
INNER JOIN pls.CodeStatus CS ON SOH.StatusID = CS.ID
INNER JOIN pls.[User] U ON SOH.UserID = U.ID
LEFT OUTER JOIN pls.[SOShipmentInfo] SOSI ON SOH.ID = SOSI.SOHeaderID
LEFT JOIN pls.CodeAttribute CA on CA.AttributeName ='CUSTORDERTYPE'
LEFT JOIN pls.SOHeaderAttribute SOHA on SOHA.SOHeaderID = SOH.ID AND SOHA.AttributeID = CA.ID


                     ";
                query += "WHERE SOH.StatusID   = '<ID>'";
                //query += " AND P.ID ='" + programId + "' ";

                if (programId != "0" && programId != null)
                {
                    query += "AND P.ID = '" + programId + "' ";
                }
                else
                {
                    query += "AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }

                query = query.Replace("<ID>", statusId);

                query += @"
       GROUP BY
       SOH.ProgramID,       
       SOH.ID,
       SOH.CustomerReference,
       SOH.ThirdPartyReference,
	   SOSI.TrackingNo,
       P.Name,
	   CONCAT(CAD.Address1, ' ' ,CAD.Address2), 
       CAD.City,
	   CAD.State,
	   CAD.Country,
	   CAD.Zip,
       SOHA.Value,
	   CS.Description ,
       U.Username,
       SOH.CreateDate,
       SOH.LastActivityDate ";

                query += "ORDER BY CreateDate DESC";

                filterString = "Status = '" + status + "'";
            }
            else
            {


                query = @"

  SELECT  SOH.ProgramID,
       SOH.ID,
       SOH.CustomerReference,
       SOH.ThirdPartyReference,
	   SOSI.TrackingNo,
       P.Name AS Program,
	   CONCAT(CAD.Address1, ' ' ,CAD.Address2) AS Address,
       CAD.City,
	   CAD.State,
	   CAD.Country,
	   CAD.Zip,
       SOHA.Value,
	   SUM(SOL.QtyToShip) AS QtyToShip,
	   SUM(SOL.QtyReserved)AS QtyShipped,
       CS.Description AS Status,
       U.Username,
       SOH.CreateDate,
       SOH.LastActivityDate
FROM   [pls].[SOHeader] SOH
INNER JOIN pls.SOLine SOL ON SOL.SOHeaderID = SOH.ID
INNER JOIN pls.Program P ON SOH.ProgramID = P.ID
INNER JOIN pls.CodeAddressDetails CAD ON CAD.AddressID = SOH.AddressID AND CAD.AddressType = 'ShipTo'
INNER JOIN pls.CodeStatus CS ON SOH.StatusID = CS.ID
INNER JOIN pls.[User] U ON SOH.UserID = U.ID
LEFT OUTER JOIN pls.[SOShipmentInfo] SOSI ON SOH.ID = SOSI.SOHeaderID
LEFT JOIN pls.CodeAttribute CA on CA.AttributeName ='CUSTORDERTYPE'
LEFT JOIN pls.SOHeaderAttribute SOHA on SOHA.SOHeaderID = SOH.ID AND SOHA.AttributeID = CA.ID

 ";
                
                    query += "WHERE CONVERT(Date, SOH.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, SOH.LastActivityDate) <= '<toDt>'";
                    query = query.Replace("<frmDt>", frmDt);
                    query = query.Replace("<toDt>", toDt);
                
               


               

                if (!string.IsNullOrEmpty(custRef))
                    query += "AND SOH.CustomerReference LIKE '%" + custRef + "%'";

                if (!status.Equals("All"))
                    query += "AND cs.ID  IN (" + statusId + ") ";

                //if (!program.Equals("All"))
                //query += "AND P.ID ='" + programId + "' ";

                if (programId != "0" && programId != null)
                {
                    query += "AND P.ID = '" + programId + "' ";
                }
                else
                {
                    query += "AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }

                query += @"
       GROUP BY
       SOH.ProgramID,
       SOH.ID,
       SOH.CustomerReference,
       SOH.ThirdPartyReference,
	   SOSI.TrackingNo,
       P.Name,
	   CONCAT(CAD.Address1, ' ' ,CAD.Address2), 
       CAD.City,
	   CAD.State,
	   CAD.Country,
	   CAD.Zip,
       SOHA.Value,
	   CS.Description ,
       U.Username,
       SOH.CreateDate,
       SOH.LastActivityDate ";

                query += "ORDER BY CreateDate DESC";

                if (!string.IsNullOrEmpty(ProgramName))
                    filterString += " Program = '" + ProgramName + "' ";

                if (string.IsNullOrEmpty(custRef))
                    filterString += "| From = '" + frmDt + "' To = '" + toDt + "' | Status = '" + status + "' ";
                else
                    filterString += "| From = '" + frmDt + "' To = '" + toDt + "' | Customer Ref. Like '" + custRef + "' | Status = '" + status + "' ";

            }



            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("006", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSO = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
        public bool GetSODetail(string Id, string frmDt, string toDt, string custRef, string status, string statusId, string type, string programId, string ProgramName)
        {
            string query = string.Empty;
            query = @"
     SELECT
     SOH.ProgramID
    ,SOH.ID
	,P.Name AS Program
	,CustomerReference
	,SOL.PartNo
	,CC.Description AS Configuration
	,ThirdPartyReference
    ,SOSI.TrackingNo
    ,SUM(SOL.QtyToShip) AS QtyToShip
	,SUM(SOL.QtyReserved)AS QtyShipped
	,CONCAT(CAD.Address1, ' ' ,CAD.Address2) AS Address
    ,CAD.City
	,CAD.State
	,CAD.Country
	,CAD.Zip
    ,SOHA.Value
	,CS.Description AS Status
	,U.Username
	,SOH.CreateDate
	,SOH.LastActivityDate
FROM pls.SOHeader SOH
INNER JOIN PLS.SOLine SOL ON SOL.SOHeaderID = SOH.ID
INNER JOIN PLS.Program P ON P.ID = SOH.ProgramID
INNER JOIN PLS.CodeAddressDetails CAD ON CAD.AddressID = SOH.AddressID AND CAD.AddressType = 'ShipTo'
INNER JOIN PLS.CodeStatus CS ON CS.ID = SOH.StatusID
INNER JOIN PLS.[User] U ON U.ID = SOH.UserID
LEFT JOIN PLS.CodeConfiguration CC ON CC.ID = SOL.ConfigurationID
LEFT OUTER JOIN pls.[SOShipmentInfo] SOSI ON SOH.ID = SOSI.SOHeaderID
LEFT JOIN pls.CodeAttribute CA on CA.AttributeName ='CUSTORDERTYPE'
LEFT JOIN pls.SOHeaderAttribute SOHA on SOHA.SOHeaderID = SOH.ID AND SOHA.AttributeID = CA.ID
                ";
            
                query += "WHERE CONVERT(Date, SOH.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, SOH.LastActivityDate) <= '<toDt>'";
                query = query.Replace("<frmDt>", frmDt);
                query = query.Replace("<toDt>", toDt);
            
            

            

            if (!string.IsNullOrEmpty(custRef))
                query += "AND SOH.CustomerReference LIKE '%" + custRef + "%'";

            if (!status.Equals("All"))
                query += "AND cs.ID  IN (" + statusId + ") ";

            if (programId != "0" && programId != null)
            {
                query += "AND P.ID = '" + programId + "' ";
            }
            else
            {
                query += "AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            query = query.Replace("<ID>", statusId);

            query += @"
            GROUP BY
            SOH.ProgramID
           ,SOH.ID
           ,P.Name 
           ,CustomerReference
           ,SOL.PartNo
           ,CC.Description 
           ,ThirdPartyReference
           ,SOSI.TrackingNo
           ,CONCAT(CAD.Address1, ' ' ,CAD.Address2) 
           ,CAD.City
           ,CAD.State
           ,CAD.Country
           ,CAD.Zip
           ,SOHA.Value
           ,CS.Description 
           ,U.Username
           ,SOH.CreateDate
           ,SOH.LastActivityDate ";

            query += "ORDER BY CreateDate DESC";

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += " Program = '" + ProgramName + "' ";

            if (string.IsNullOrEmpty(custRef))
                filterString += "| From = '" + frmDt + "' To = '" + toDt + "' | Status = '" + status + "' ";
            else
                filterString += "| From = '" + frmDt + "' To = '" + toDt + "' | Customer Ref. Like '" + custRef + "' | Status = '" + status + "' ";



            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("006", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSODetail = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }


        public bool GetDetail(string SOHeaderId, string custRef)
        {
            string query = string.Empty;

            #region Header

            query = @"
SELECT 
       SOH.ID,
       SOH.CustomerReference,
       SOH.ThirdPartyReference,
       P.Name AS Program,
       CS.Description AS Status,
       U.Username,
       SOH.CreateDate,
       SOH.LastActivityDate,
       CONCAT(CAD.Address1, ' ' ,CAD.Address2) AS Address
FROM   pls.SOHeader SOH
INNER JOIN pls.Program P ON SOH.ProgramID = P.ID
INNER JOIN pls.CodeStatus CS ON SOH.StatusID = CS.ID
INNER JOIN pls.[User] U ON SOH.UserID = U.ID 
INNER JOIN pls.CodeAddressDetails CAD ON CAD.AddressID = SOH.AddressID
WHERE SOH.ID = '<SOHeaderId>'
ORDER BY CreateDate DESC ";
            query = query.Replace("<SOHeaderId>", SOHeaderId);

            filterString = "Customer Ref. = '" + custRef + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("006-1", query, "Header", false);

            // oDAL = new cDAL(HttpContext.Current.Request["DB"]);
            DataTable dtHeader = oDAL.GetData(query);

            if (dtHeader.Rows.Count > 0)
            {
                DataRow dr = dtHeader.Rows[0];
                thirdPartyReference = dr["ThirdPartyReference"].ToString();
                program = dr["Program"].ToString();
                description = dr["Status"].ToString();
                username = dr["Username"].ToString();
                createDate = dr["CreateDate"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["CreateDate"]).ToString("yyyy.MM.dd HH:mm");
                lastActivityDate = dr["LastActivityDate"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["LastActivityDate"]).ToString("yyyy.MM.dd HH:mm");
                shipTo = dr["Address"].ToString();
            }
            #endregion

            #region
            query = @"
SELECT  SOH.ProgramID, 
        SOL.ID, 
		SOL.SOHeaderID,  
        SOL.PartNo,
		CC.Description AS Configuration, 
        SOL.QtyToShip,
		SOL.QtyReserved, 
		CS.Description AS Status,
		U.Username, 
		SOL.CreateDate, 
		SOL.LastActivityDate
FROM pls.SOLine SOL
INNER JOIN pls.CodeConfiguration CC ON CC.ID = SOL.ConfigurationID
INNER JOIN pls.CodeStatus CS ON CS.ID = SOL.StatusID
INNER JOIN pls.[User] U ON U.ID = SOL.UserID
INNER JOIN pls.SOHeader SOH ON SOH.ID = SOL.SOHeaderID
WHERE SOHeaderID = '<SOHeaderId>'
ORDER BY CreateDate DESC";

            query = query.Replace("<SOHeaderId>", SOHeaderId);
            //For SQL Documentation
            oLog.AddSqlQuery("006-2", query, "---Detail---", false);
            // oDAL = new cDAL(HttpContext.Current.Request["DB"]);
            DataTable dt = oDAL.GetData(query);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstDetail = cCommon.ConvertDtToHashTable(dt);
                    return true;
                }
            }

            return false;

        }
        #endregion

        #region
        public bool GetSOUnit(string SOLineId, string statusId)
        {
            string query = string.Empty;
            query = @"
SELECT 
        PL.ProgramID,
        SOU.ID,
        SOL.PartNo,
		SOU.SOLineID,
        (SELECT  CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
        FROM pls.PartSerial
        WHERE SerialNo = SOU.SerialNo) AS HAS_SN,
		SOU.SerialNo, 
		SOU.QtyReserved, 
		PL.LocationNo, 
		CS.Description AS Status, 
		U.Username, 
		SOU.CreateDate, 
		SOU.LastActivityDate
FROM pls.SOUnit SOU
INNER JOIN pls.PartLocation PL ON PL.ID = SOU.LocationID
INNER JOIN pls.SOLine SOL ON SOU.SOLineID = SOL.ID
INNER JOIN pls.CodeStatus CS ON CS.ID = SOU.StatusID
INNER JOIN pls.[User] U ON U.ID = SOU.UserID
WHERE SOLineID = '<SOLineId>' AND CS.ID IN (12, 18)
ORDER BY CreateDate DESC
";

            //if (!string.IsNullOrEmpty(statusId))
            //{
            //    query += "AND CS.ID = '<StatusId>' ";

            //}
            //else
            //{
            //   // query += "ORDER BY LastActivityDate DESC";
            //}

            query = query.Replace("<SOLineId>", SOLineId);
            //query = query.Replace("<StatusId>", statusId);


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("006-3", query, "---SO Unit---", false);



            DataTable dt = oDAL.GetData(query);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstSOUnit = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }


        #endregion
    }
}