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
    public class RepairReportVerA
    {
        cDAL oDAL;
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstRepairReportVerA { get; set; }
        public bool GetList()
        {
            oDAL = new cDAL("INIT");
            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            string query = string.Empty;

            query = @"
SELECT RepairSequence, RepairTimes, ID, SerialNo, PartNo, Description, ModelNo, StatusDescription, ShopFloorArea, CurrentWS, WorkstationDescription, StartRepair, EndRepair, Pass, Username, ExtendedCost, ConsumedDate, 
       LineID, FaultDescription, RepairDescription
FROM   meta.rptRepairReport_Ver_A
";

            DataTable dt = oDAL.GetData(query);

            //Filterstring
            filterString += "> Program = META";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("258", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRepairReportVerA = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}