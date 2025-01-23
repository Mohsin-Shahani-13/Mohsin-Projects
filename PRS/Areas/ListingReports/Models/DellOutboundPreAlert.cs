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
    public class DellOutboundPreAlert
    {

        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Ship Order No.:")]
        public string custRef { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstDellOutboundPreAlert { get; set; }
        #endregion
        #region Methods 
        public bool GetList(string custRef)
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"
SELECT
sh.CustomerReference 'ORDER_REF', rou1.Value 'RTV Order Number', '91096' 'DEPOT', ps.PartNo 'EXPECTED PARTNR', ps.PartNo 'ACTUAL PARTNR' , count(ps.serialno) as QtyToShip, dateadd(weekday,2,SHIP.ShipmentDate) 'ETA'
FROM pls.SOLine SL
join pls.SOUnit su on su.solineid = sl.id
JOIN pls.PartSerial ps on su.serialno = ps.serialno and ps.statusid = 18
join pls.roheader rh on rh.id = ps.roheaderid
join pls.roline rl on rl.ROHeaderID = rh.ID
join pls.rounit ru on ru.ROLineID = rl.id and ru.SerialNo = ps.SerialNo
join pls.soheader sh on sh.id = ps.soheaderid
join pls.SOShipmentInfo SHIP on SHIP.SOHeaderID = sh.ID
join pls.CodeConfiguration cc on cc.id = ps.ConfigurationID
LEFT JOIN pls.CodeAttribute CA on CA.AttributeName ='RTV_NO'
LEFT JOIN pls.ROUnitAttribute ROU1 on ROU1.ROUnitID= ru.ID AND ROU1.AttributeID = CA.ID
WHERE cc.Description = 'Good'
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
                query += "AND ps.ProgramID = 10055 ";

            }
            else if (sites == "MEMPHIS")
            {
                query += "AND ps.ProgramID = 10053 ";

            }

            if (!string.IsNullOrEmpty(custRef))
                query += "AND sh.CustomerReference LIKE '%" + custRef + "%' ";

            query += "GROUP BY sh.CustomerReference,rou1.Value,ps.PartNo,dateadd(weekday,2,SHIP.ShipmentDate) ";

                DataTable dt = oDAL.GetData(query);

                filterString = "> Program = DELL ";

            if (!string.IsNullOrEmpty(custRef))
                filterString += "| Ship Order No. Like '" + custRef + "' ";


            //For SQL Documentation

            cLog oLog = new cLog();
            oLog.AddSqlQuery("217", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDellOutboundPreAlert = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}