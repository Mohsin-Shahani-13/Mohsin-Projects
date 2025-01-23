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
    public class DellScrapPreAlert
    {

        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Scrap Order No.:")]
        public string custRef { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstDellScrapPreAlert { get; set; }
        #endregion
        #region Methods 
        public bool GetList(string custRef)
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"
SELECT 
  rou1.Value 'RTV Order Number', ps.PartNo 'VSP', sum(sl.QtyToShip) '91096'
  FROM pls.SOLine SL
  join pls.SOUnit su on su.solineid = sl.id
  JOIN pls.PartSerial ps on su.serialno = ps.serialno and ps.statusid = 18 
  join pls.rounit ru on  ru.SerialNo = ps.SerialNo
  join pls.soheader sh on sh.id = ps.soheaderid
  join pls.CodeConfiguration cc on cc.id = ps.ConfigurationID
  LEFT JOIN pls.CodeAttribute CA on CA.AttributeName ='RTV_NO'
  LEFT JOIN pls.ROUnitAttribute ROU1 on ROU1.ROUnitID= ru.ID AND ROU1.AttributeID = CA.ID
  where cc.Description = 'Bad'

";
            if (sites == "BYDGOSZCZ")
            {
                query += "AND ps.ProgramID = 10064 ";
            }
            else if (sites == "JUAREZ")
            {
                query += "AND ps.ProgramID = 10061 ";

            }
            else if (sites == "MEXICALI")
            {
                query += "AND ps.ProgramID  =10055 ";

            }
            else if (sites == "MEMPHIS")
            {
                query += "AND ps.ProgramID = 10053 ";

            }

            if (!string.IsNullOrEmpty(custRef))
                query += "AND sh.CustomerReference LIKE '%" + custRef + "%' ";

            query += "GROUP BY rou1.value, ps.partno ";
                
            DataTable dt = oDAL.GetData(query);

            filterString = "> Program = DELL ";

            if (!string.IsNullOrEmpty(custRef))
                filterString += "| Scrap Order No. Like '" + custRef + "' ";


            //For SQL Documentation

            cLog oLog = new cLog();
            oLog.AddSqlQuery("216", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDellScrapPreAlert = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}