using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class BoseIFSInventory
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstBoseIFSInventory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList()
        {

            string query = string.Empty;

            query = @"
SELECT *
FROM OPENQUERY(AMER, 'select part_no, configuration_id, location_no, sum(qty_onhand) qty
from IFSAPP.INVENTORY_PART_IN_STOCK_LOC
where contract = ''56001'' and location_no not in (''INTRANSITTOPLUS'',''PLUSWIP'')
group by part_no, configuration_id, location_no
');";



            DataTable dt = oDAL.GetData(query);
            
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("252", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstBoseIFSInventory = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}