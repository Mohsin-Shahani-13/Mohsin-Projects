using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.ListingReports.Models
{
    public class PartsTransaction
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Location:")]
        public string Location { get; set; }      
        [Display(Name = "To Location:")]
        public string ToLocation { get; set; }       
        [Display(Name = "Trans Type:")]
        public string TransType { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
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
        public List<ArrayList> lstTranstype { get; set; }
        public List<Hashtable> lstPartsTransaction { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetTransType()
        {
            string query = string.Empty;
            query = @"select distinct
		            PartTransactionID,
		            Description AS [Trans Type] 
from pls.CodePartTransaction CPT

INNER JOIN pls.PartTransaction PT ON PT.PartTransactionID = CPT.Id 
ORDER BY Description";

            DataTable dt = oDAL.GetData(query);
            lstTranstype = cCommon.ConvertDtToArrayList(dt);
            if (!oDAL.HasErrors)
                return true;
            else
                return false;
        }
        public bool GetList(string frmDt, string toDt, bool isAllDate, string serialNo, string partNo, string Location, string ToLocation, string programId, string ProgramName, string transTypeID, string transType)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            string _SerialNo = GetInValue(serialNo);
            string _transtype = GetInValue(transType);
            query = @"

SELECT  PT.ProgramID,
        PT.OrderHeaderID,
        PN.partno,
        PN.Description,
       P.Name AS Program, 
       PT.ParentSerialNo,
       (SELECT  CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
        FROM pls.PartSerial
        WHERE SerialNo = PT.SerialNo AND PT.ProgramID = ProgramID) AS HAS_SN,
       PT.SerialNo, 
       PT.Qty, 
       PT.Source, 
       PT.Condition,
       PT.Configuration , 
       PT.Location, 
       PT.ToLocation,
       PT.Reason, 
       CT.Description AS PartType, 
       PT.CustomerReference, 
       CPT.Description AS PartTransactionType,
	   PT.OrderType,
       <LotNo>
	   U.Username, 
       PT.CreateDate,
       PT.PartTransactionID
FROM   pls.PartTransaction PT
INNER JOIN pls.Program P ON P.ID = PT.ProgramID
INNER JOIN pls.[User] U ON U.ID = PT.UserID
INNER JOIN [pls].[CodePartTransaction] CPT ON CPT.ID = PT.PartTransactionID
LEFT OUTER JOIN pls.partno PN ON PN.partno = PT.partno
LEFT OUTER JOIN pls.CodePartType CT ON CT.Id = PN.PartTypeId";
            if (programId != "0")
            {
                query += " \nWHERE P.ID = '" + programId + "' ";
            }
            else
            {
                query += " \nWHERE P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }


            if (isAllDate != true)
            {
                query += " \nAND CONVERT(Date, PT.createdate) >= '<frmDt>' AND CONVERT(Date, PT.createdate) <= '<toDt>'";
            }
            
            //query = query.Replace("<Location>", Location);
            //query = query.Replace("<ToLocation>", ToLocation);
            if (!string.IsNullOrEmpty(partNo))
                query += " \nAND PN.partno = '" + partNo + "' ";
            if (!string.IsNullOrEmpty(Location))
                query += " \nAND PT.Location LIKE '%" + Location + "%' ";
            if (!string.IsNullOrEmpty(ToLocation))
                query += " \nAND PT.ToLocation LIKE '%" + ToLocation + "%' ";
            if (!string.IsNullOrEmpty(serialNo))
                query += " \nAND SerialNo IN (" + _SerialNo + ") ";

            

            if (!transType.Equals("All"))
                query += " \nAND PT.PartTransactionID IN (" + transTypeID + ") ";

            if (ProgramName == "META")
            {
                query = query.Replace("<LotNo>", "PT.LotNo,");
            }
            else
            {
                query = query.Replace("<LotNo>", "");
            }
            query += " \nORDER BY PT.createdate DESC";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);

            // Filters

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (isAllDate != true)
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            //else
            //    filterString += " | Date = 'All' ";
            //if (!string.IsNullOrEmpty(serialNo))
            //    filterString += " | Serial No. = " + _SerialNo + " ";
            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. = '" + partNo + "' ";
            if (!string.IsNullOrEmpty(Location))
                filterString += " | Location Like '" + Location + "' ";
            if (!string.IsNullOrEmpty(ToLocation))
                filterString += " | To Location Like '" + ToLocation + "' ";
            //filterString += " | Location Like '" + Location + "' | ToLocation Like '" + ToLocation + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("005", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPartsTransaction = cCommon.ConvertDtToHashTable(dt);
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
                    _arr = "\'" + item.Trim() + "\'";
                }
                else
                {
                    _arr += "," + "\'" + item.Trim() + "\'";
                }

            }
            return _arr;
        }
        #endregion
    }
}