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
    public class ECOCheck
    {
        cDAL oDAL;

        #region Fields

        public bool isAllDate { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Release From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        //[Display(Name = "To:")]
        //public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        //public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstECOCheck{ get; set; }
        //public List<Hashtable> lstGetSerials { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string partNo, string frmDt, bool isAllDate)
        {
            if (isAllDate == true)
                frmDt = "";

            // oDAL = new cDAL("ACTIVE", "ST");
            oDAL = new cDAL("ALCATEL");
            string query = string.Empty;
            query = @"
SELECT  ID
      ,Col001
      ,Col002
      ,Col003
      ,Col004
      ,Col005
      ,Col006
      ,Col007
      ,Col008
      ,Col009
      ,Col010
  FROM Alcatel.dbo.Disposition

";


            //query = query.Replace("<frmDt>", frmDt);
            // query = query.Replace("<toDt>", toDt);
            if ((!string.IsNullOrEmpty(frmDt) && !string.IsNullOrEmpty(partNo)))
            {
                query += " WHERE CONVERT(Date, Col010) >= '" + frmDt + "' " + "AND COL001 LIKE '%" + partNo + "%' ";
            }

            else
            {

                if (!string.IsNullOrEmpty(frmDt) || !string.IsNullOrEmpty(partNo))
                {
                    query += " WHERE ";
                }


                if (isAllDate != true)
                {
                    if (!string.IsNullOrEmpty(frmDt))
                        query += " CONVERT(Date, Col010) >= '" + frmDt + "' ";
                }

                if (!string.IsNullOrEmpty(partNo))
                    query += " COL001 LIKE '%" + partNo + "%' ";

            }

            query += "ORDER BY COL010 DESC";
            
            DataTable dt = oDAL.GetData(query);

            //Filterstring
            if (!string.IsNullOrEmpty(partNo))
                filterString += "  Part No. Like '" + partNo + "' | ";

            if (!string.IsNullOrEmpty(frmDt))
                filterString += "  Release From = '" + frmDt + "' ";

            if (isAllDate == true)
                filterString += "  Release From = All ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("027", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstECOCheck = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}