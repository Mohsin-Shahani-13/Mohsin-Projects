using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaMTDGlobalShipping
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

        [Display(Name = "Status:")]
        public string statusId { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstMetaMTDGlobal { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }
        public DataTable Status()
        {
            string query = string.Empty;
            query = @"SELECT DISTINCT 
                            CS.ID,
		                    CS.Description		
                FROM pls.CodeStatus CS
                Inner Join pls.SOHeader On CS.ID = StatusID 
                ORDER BY CS.Description	ASC  ";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        #endregion
        public bool GetList(string programId, string ProgramName, string RMARef, string frmDt, string toDt, string status, string statusId)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"SELECT P.ID AS ProgramID,
       'SFW' AS [SITE], 
        SOH.ID  AS ORDER_ID,
        SOH.CustomerReference AS CUST_REF,
       U.Username AS USER_ID,
       SOH.CreateDate AS DATED,
       B2B.OutMessage_Hdr_Id AS MESSAGE_ID,
       B2B.Processed_Date AS DATE_PROCESSED,
       SOL.ID AS LINE_NO,
       SOL.PartNo AS PART_NO,
       SOU.SerialNo AS SERIAL_NO,
       SOU.QtyReserved AS QUANTITY,
       SOL.QtyReserved AS QTY_PER_LINE
FROM [pls].[SOHeader] SOH
INNER JOIN Pls.Program P ON P.ID = SOH.ProgramID
LEFT JOIN pls.SOLine SOL ON SOL.SOHeaderID = SOH.ID
LEFT JOIN pls.SOUnit SOU ON SOU.SOLineID = SOL.ID
INNER JOIN pls.PartNo PN ON PN.PartNo = SOL.PartNo
INNER JOIN pls.CodeStatus CS ON SOU.StatusID = CS.ID
INNER JOIN pls.[User] U ON SOH.UserID = U.ID
LEFT JOIN Pls.CodeCommodity CCY ON CCY.ID = pn.SecondaryCommodityID
LEFT JOIN PLS.CodeConfiguration CC ON CC.ID = SOL.ConfigurationID
LEFT JOIN pls.[SOShipmentInfo] SOSI ON SOH.ID = SOSI.SOHeaderID
LEFT JOIN pls.CodeAttribute CA ON CA.AttributeName = 'StorageLocation'
LEFT JOIN pls.SOHeaderAttribute SOHA ON SOHA.SOHeaderId = SOH.Id AND SOHA.AttributeId = CA.ID
LEFT JOIN [TPDC01Z004].Biztalk_OutMessages.dbo.Outmessage_hdr B2B ON B2B.CO_Number = CAST(SOH.ID AS VARCHAR)
AND B2B.CONTRACT = '10009'
 ";

          
            query += "WHERE CONVERT(Date, SOH.CreateDate) >= '<frmDt>' AND CONVERT(Date, SOH.CreateDate) <= '<toDt>'";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            //query = query.Replace("<programId>", programId);


            if (!status.Equals("All"))
                query += "AND  SOH.StatusID ='" + statusId + "' ";


            if (programId != "0" && programId != null)
            {
                query += "AND P.ID = '" + programId + "' ";
            }
            else
            {
                query += "AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

         
            

            query += "ORDER BY SOH.CreateDate DESC";

            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            //if (!string.IsNullOrEmpty(RMARef))
            //    filterString += "| Customer Ref. '" + RMARef + "' ";
            

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' | Status = '" + status + "' ";
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("123", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaMTDGlobal = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}