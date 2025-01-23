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
    public class RepairReportVerB
    {

        cDAL oDAL;
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstRepairReportVerB { get; set; }
        public bool GetList()
        {
            oDAL = new cDAL("INIT");
            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            string query = string.Empty;

            query = @"
SELECT  RepairSequence, RepairTimes, HeaderID, SerialNo, PartNo, Description, ModelNo, StatusDescription, ShopFloorArea, CurrentWS, 
        wsDesc, StartRepair, EndRepair, Pass, Username, ExtendedCost, MinConsumedDate, 
        FaultDescriptions, RepairDescriptions, OrderedExtendedCosts, MainFault
FROM     meta.rptRepairReport_Ver_B
";

            DataTable dt = oDAL.GetData(query);

            //Filterstring
            filterString += "> Program = META";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("259", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRepairReportVerB = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}