using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaClaimManagement
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }

        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        [Display(Name = "Claim Serial No.:")]
        public string claimSerialNo { get; set; }
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstMetaClaimManagement { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion

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
        public bool GetList(string programId, string ProgramName, string frmDt, string toDate, string claimclaimSerialNo)
        {
            string query = string.Empty;
            string _claimclaimSerialNo = GetInValue(claimclaimSerialNo);

            query = @"

-- Meta Claims Management field definition section - text fields
SELECT 
    ID, 
    ForDate, 
    QueryNumber, 
    InsertDatetime,
    ProgramId,
    ClaimBusinessCategory, 
    ClaimType, 
    ClaimCurrencyCode, 
    ClaimServiceCenter, 
    FORMAT(CONVERT(DATE, ClaimRmaReceiptDate, 101), 'yyyy.MM.dd') AS [ClaimRmaReceiptDate],
	FORMAT(CONVERT(DATE, ClaimServiceDate, 101), 'yyyy.MM.dd') AS [ClaimServiceDate],
    ClaimSku, 
    ClaimSerialNumber, 
    ClaimRmaOrder, 
    ClaimRmaOrderType, 
    ClaimWorkOrder, 
    ClaimDockLogReceiptDate, 
    ClaimAwbNumber, 
    ClaimBolNumber, 
    LaborWorkOrder, 
    LaborActivityType, 
    LaborPalletId, 
    LaborPalletQuantity, 
    LaborPalletWeight, 
    LaborPalletWeightUom, 
    LaborServiceStartDatetime, 
    LaborServiceEndDatetime, 
    LaborComponentSku, 
    LaborComponentSn, 
    LaborSealedBox, 
    LaborRtv, 
    LaborContainsMemory, 
    LaborDangerousGoods, 
    LaborGps, 
    LaborEffaProduct, 
    LaborDatawipeSuccess, 
    LaborEndOfProductLifecycle, 
    LaborDiagnosticCode, 
    LaborComments, 
    LaborGrade, 
    LaborRepairLevel, 
    PartActivityType, 
    PartCategory, 
    PartComponentSku, 
    PartComponentSn, 
    PartConsumptionQuantity, 
    PartCost, 
    ClaimTransformedSku, 
    ClaimTransformedSn
FROM PlusRS.meta.rptClaimMgmt CM
    WHERE CONVERT(Date, CM.ClaimServiceDate) >= '<frmDt>' AND CONVERT(Date, CM.ClaimServiceDate) <= '<toDt>'
";

            if (programId != "0" && programId != null)
            {
                query += " AND CM.ProgramId =  '" + programId + "'";
                //query += " AND CM.ClaimBusinessCategory =  '" + ProgramName + "'";
            }
            else
            {
                query += " AND CM.ProgramId = IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + " ) ";
                //query += " AND CM.ClaimBusinessCategory = IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + " ) ";
            }
            if (!string.IsNullOrEmpty(claimclaimSerialNo))
                query += " AND CM.ClaimSerialNumber  IN ( " + _claimclaimSerialNo + " ) ";
            //query += ") cm";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataTable dt = oDAL.GetData(query);
            //int col = dt.Columns.Count;

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";

            if (!string.IsNullOrEmpty(claimclaimSerialNo))
                filterString += "| Claim Serial No. =  '" + claimclaimSerialNo + "'";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("161", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaClaimManagement = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}