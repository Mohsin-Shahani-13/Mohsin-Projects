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
    public class RepairConsumptionReport
    {
        cDAL oDAL;
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstRepairConsumptionReport { get; set; }
        public bool GetList()
        {
            oDAL = new cDAL("ACTIVE");
            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            string query = string.Empty;

            query = @"
SELECT [ID]
      ,[PartNo]
      ,[SerialNo]
      ,[StatusDescription]
      ,[RepairTypeDescription]
      ,[WSID]
      ,[WorkStationDesc]
      ,[ECOName]
      ,[Component]
      ,[ComponentDescription]
      ,[ConsumedQty]
      ,[ConsumedCost]
      ,[RepairScreening]
  FROM [PlusRS].[meta].[rptMetaRepairConsReport]
";

            DataTable dt = oDAL.GetData(query);

            //Filterstring
            filterString += "> Program = META";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("251", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRepairConsumptionReport = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}