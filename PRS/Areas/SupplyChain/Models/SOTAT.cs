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
    public class SOTAT
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
      
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
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
        public List<Hashtable> lstSOTAT { get; set; }
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt,string programId, string programName)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
		select d.ID, b.ProgramID as ProgramID, d.CustomerReference, b.TrackingNo, b.Carrier,e.City, e.Zip,e.State,e.Country,d.lastactivitydate as shipdate, 
	 concat(FORMAT(b.EventDate, 'yyyy.MM.dd'), ' ' ,CONVERT(VARCHAR(5), b.EventTime, 108)) as deliverdate,
((DATEDIFF(dd, d.lastactivitydate, b.EventDate) + 1)
  -(DATEDIFF(wk, d.lastactivitydate, b.EventDate) * 2)
  -(CASE WHEN DATENAME(dw, d.lastactivitydate) = 'Sunday' THEN 1 ELSE 0 END)
  -(CASE WHEN DATENAME(dw, b.EventDate) = 'Saturday' THEN 1 ELSE 0 END)
  +(CASE WHEN DATENAME(dw, b.EventDate) = 'Saturday' THEN 1 ELSE 0 END)
  +(CASE WHEN DATENAME(dw, b.EventDate) = 'Sunday' THEN 1 ELSE 0 END)
  ) - 1 as TAT
  from
[pls].TrackingHeader b
JOIN [pls].vSOShipmentInfo c on b.trackingno=c.trackingno
JOIN pls.SOHeader d on c.SOHeaderid= d.id
LEFT JOIN pls.CodeAddressDetails e on d.Addressid=e.AddressID and e.AddressType = 'SHIPTO'
LEFT JOIN pls.Carrierresult f on b.trackingno=f.trackingno
WHERE b.EventDescription in( 'DELIVERED' , 'Delivered')
AND (f.labeltype <> 'Customer Return' or f.labeltype is NULL)

";
            //query = query.Replace("<frmDt>", frmDt);
            //query = query.Replace("<toDt>", toDt);

            if (programId != "0")
            {
                query += "AND b.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "AND b.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            
                if (!string.IsNullOrEmpty(frmDt) && !string.IsNullOrEmpty(toDt))
                    // query += "AND Format(Cast(CD.MMDDYYYY as date), 'yyyy.MM.dd') = '" + OrdCreatOnFrm + "'";
                    query += "AND b.CreateDate > '" + frmDt + "' " + "AND b.CreateDate < '" + toDt + "' ";
            

            query += "order by b.CreateDate asc";

            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";


           
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
          



            DataTable dt = oDAL.GetData(query);



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("198", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSOTAT = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}