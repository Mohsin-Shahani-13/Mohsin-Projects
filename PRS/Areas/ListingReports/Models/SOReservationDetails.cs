using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class SOReservationDetails
    {
        #region fields
        cDAL oDAL = new cDAL("ACTIVE");

        [Display(Name = "Program:")]
        public string ProgramName { get; set; }
        [Display(Name = "Program:")]
        public string ProgramID { get; set; }
        [Display(Name = "Customer Ref.:")]
        public string custRef { get; set; }
        public List<Hashtable> lstSOReserveDetails { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        #endregion
        #region methods
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
        public bool GetList(string ProgramId, string ProgramName, string custRef)
        {
            string query = string.Empty;
            query = @"
            select 
	sh.ProgramID,
    sh.ID as soHeaderId,
	P.Name as programName,
	sh.CustomerReference, 
	sl.PartNo, 
	su.SerialNo, 
    su.qtyreserved,
	psa.Value as Cartonid, 
	su.FromPalletBoxNo, 
	pl.LocationNo, 
	cs.Description as Status 
from pls.soheader sh
join pls.Program p on p.ID = sh.ProgramID
join pls.soline sl on sh.id = sl.soheaderid
join pls.sounit su on sl.id = su.solineid
join pls.PartLocation pl on su.FromLocationID = pl.id
join pls.partserial ps on su.SerialNo = ps.SerialNo
join pls.PartSerialAttribute psa on ps.ID = psa.PartSerialID and psa.AttributeID = '49'
join pls.CodeStatus cs on cs.id = su.StatusID
";

            if (ProgramId != "0")
            {
                query += "WHERE sh.ProgramID = '" + ProgramId + "' ";
            }
            else
            {
                query += "WHERE sh.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            if (!string.IsNullOrEmpty(custRef))
                query += "AND sh.CustomerReference LIKE '%" + custRef + "%' ";
            query += @"
order by 
sl.partno, 
psa.Value, 
su.FromPalletBoxNo
";
            DataTable dt = oDAL.GetData(query);

            //if (!string.IsNullOrEmpty(ProgramName))
            //    filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(custRef))
                filterString += "> Customer Ref. Like  '" + custRef + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("179", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSOReserveDetails = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        #endregion
    }
}