using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class DELLWIPSnapshot
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstDELLWIPSnapshot { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList()
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"
select p.name ProgramName,
p.id ProgramID, 'EMEA' REGION,'Reconext Poland' PARTNER_NAME,ps.PartNo partno, COUNT(*) WIP  
from pls.PartSerial ps  
inner join pls.PartLocation pl on pl.ID=ps.LocationID and pl.LocationNo='WIP.10064.0.0.0' 
inner join pls.Program p on p.ID = ps.ProgramID  ";
            if (sites == "BYDGOSZCZ")
            {
                query += "\nwhere ps.ProgramID=10064 ";
            }
            else if (sites == "JUAREZ")
            {
                query += "\nwhere ps.ProgramID=10061 ";

            }
            else if (sites == "MEXICALI")
            {
                query += "\nwhere ps.ProgramID=10055 ";

            }
            else if (sites == "MEMPHIS")
            {
                query += "\nwhere ps.ProgramID = 10053 ";

            }
            query += @"and ps.StatusID=19 
group by  p.name, p.id, ps.PartNo
";

            DataTable dt = oDAL.GetData(query);

            filterString += "> Program = DELL";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("210", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDELLWIPSnapshot = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}