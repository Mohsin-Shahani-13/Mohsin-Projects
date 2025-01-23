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
    public class SODetailedData
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

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstSODetailedData { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName, string custRef)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @" 
			 select 
    soh.programId,
    case soh.ProgramID when 10058 then 'BOSE BYD' when 10059 then 'BOSE MEX' when 10064 then 'DELL BYD' end program,
    sosi.ShipmentDate,
    soh.id SOHeaderID,
    soh.CustomerReference SOCustomerReference,
    cs1.[Description]Status,
    soh.ThirdPartyReference,
    sosi.PackingSlipNo,
    sosi.TrackingNo,
    sosi.Carrier,
    sosi.ServiceTypeDescription,
    sosi.Amount,
    sosi.Currency,
    sosi.Weight,
    sol.PartNo,
    sou.SerialNo,
    cdd.addressid,
    cdd.AddressType,
    cdd.Country,
    pl.LocationNo,
    cc.[Description]config
from  pls.SOHeader soh
    inner join pls.CodeStatus cs1
    on cs1.id = soh.StatusID
    LEFT OUTER JOIN pls.SOShipmentInfo SOSI
    ON SOH.ID = SOSI.SOHeaderID
    left join pls.codeaddressdetails cdd
    on cdd.addressid = soh.addressid
        and cdd.addresstype = 'ShipTo'
    left join pls.SOLine sol
    on sol.SOHeaderID = soh.id
        and sol.StatusID = 18 /*SHIPPED*/
    left join pls.sounit sou
    on sou.SOLineID = sol.id
    left join pls.PartLocation pl
    on pl.id = sou.FromLocationID
    left join pls.CodeConfiguration cc
    on cc.id = sol.ConfigurationID
where soh.StatusID = 18 /*SHIPPED*/
    AND CONVERT(Date, soh.LastActivityDate) >= '<frmDt>'
    AND CONVERT(Date, soh.LastActivityDate) <= '<toDt>'
    
 ";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            if (!string.IsNullOrEmpty(programId))
            {
                query += "and soh.ProgramID = '" + programId + "'";
            }

            if (!string.IsNullOrEmpty(custRef))
                query += "and soh.CustomerReference = '" + custRef + "' ";

            //query += "ORDER BY whf.CreateDate DESC";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            if (!string.IsNullOrEmpty(custRef))
                filterString += " | Customer Ref. = '" + custRef + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("235", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSODetailedData = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}