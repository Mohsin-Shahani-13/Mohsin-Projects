using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.Meta.Models
{
    public class MetaRepairReport
    {
        cDAL oDAL;
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstMetaRepairReport { get; set; }
        public bool GetList()
        {
            oDAL = new cDAL("ACTIVE");
            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            string query = string.Empty;

            query = @"
SELECT [PartNo]
      ,[Description]
      ,[SerialNo]
      ,[StatusDescription]
      ,[CurrentFATPStation]
      ,[RepairType]
      ,[ConsumptionCost]
      ,[PreScreenDate]
      ,[Username]
      ,[FirstFADate]
      ,[TimesFA]
      ,[FACloseDate]
      ,[QuickTestCloseDate]
      ,[PostScreenLastStartDate]
      ,[PassRepairedCloseDate]
  FROM [PlusRS].[meta].[rptMetaRepairReport]
";

            DataTable dt = oDAL.GetData(query);

            //Filterstring
            filterString += "> Program = META";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("250", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaRepairReport = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}