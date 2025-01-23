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
    public class FINSReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public bool isAllDate { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Serial No.:")]
        public string SerialNo { get; set; }
        [Display(Name = "Part No.:")]
        public string PartNo { get; set; }
        [Display(Name = "Cust. Reference:")]
        public string CustRef { get; set; }


        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public List<Hashtable> lstFinsReport { get; set; }

        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("Active");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);


            return dt;
        }
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName, string SerialNo, string PartNo, string CustRef)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
				SELECT 
    t.ProgramID,
	t.OrderHeaderID,
    t.parttransaction AS PartTransaction,
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
";

            if (!string.IsNullOrEmpty(frmDt) && !string.IsNullOrEmpty(toDt))
                query += "AND t.CreateDate >= '" + frmDt + "' " + "AND t.CreateDate <= '" + toDt + "' ";

            if (!string.IsNullOrEmpty(SerialNo))
                query += "AND t.[SerialNo] LIKE '%" + SerialNo + "%'";

            if (!string.IsNullOrEmpty(PartNo))
                query += "AND t.[PartNo] LIKE '%" + PartNo + "%'";

            if (!string.IsNullOrEmpty(CustRef))
                query += "AND t.CustomerReference LIKE '%" + CustRef + "%'";

            query += "\nORDER BY t.CreateDate;";

            query = query.Replace("<programId>", programId);

            DataTable dt = oDAL.GetData(query);




            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            if (!string.IsNullOrEmpty(SerialNo))
                filterString += " | Serial No. LIKE '" + SerialNo + "' ";

            if (!string.IsNullOrEmpty(PartNo))
                filterString += " | Part No. Like '" + PartNo + "' ";

            if (!string.IsNullOrEmpty(CustRef))
                filterString += " | Customer Ref. Like '" + CustRef + "' ";



            

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("197", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstFinsReport = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}