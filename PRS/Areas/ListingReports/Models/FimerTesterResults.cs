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
    public class FimerTesterResults
    {
        cDAL oDAL;
      
        #region Fields
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstFimerTesterResults { get; set; }
        public List<Hashtable> lstGetSerials { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string serialNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
             oDAL = new cDAL("Fimer");
            string query = string.Empty;
            query = @"
SELECT	id
		,SERIAL
		,USER_NAME
		,MICRO_VER
		,DCDC_VER
		,INV_VER
		,MICRO_S_N
		,MICRO_P_N
		,TYPE
		,STANDARD
		,STATO_MACCHINA
		,STATO_DCDC1
		,STATO_DCDC2
		,STATO_INVERTER
		,STATO_ALLARME
		,TEXTBOX_WA
        ,TEXTBOX_WA AS CommentTextForExport
		,TEXTBOX_AL
        ,TEXTBOX_AL AS CommentTextForExport1
		,TEXTBOX_VAR
        ,TEXTBOX_VAR AS CommentTextForExport2
		,LIFETIME_ENERGY
		,PARTIAL_ENERGY
		,LIFETIME
		,GRID_TIME
		,PARTIAL_TIME
		,TODAY_ENERGY
		,WEEK_ENERGY
		,MONTH_ENERGY
		,YEAR_ENERGY
		,LAST_CPD
		,CREATION_TIME
FROM    fat.data_capture
";

            if (!string.IsNullOrEmpty(serialNo))
                query += "Where SERIAL LIKE '%" + serialNo + "%' ";

           
            if (!string.IsNullOrEmpty(serialNo))
                filterString += " > Serial No. Like '" + serialNo + "' ";


            DataTable dt = oDAL.GetData(query);



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("026", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstFimerTesterResults = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}