using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class OpenWipOrders
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstOpenWipOrders { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList()
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"
--Quality - Bose Bydgoszcz Open Wip Orders 
SELECT woh.ProgramID, woh.serialNo, cws.Description Workstation, woh.PartNo, DateDiff(DAY,woh.CreateDate,Getdate()) Aging
FROM PLS.WOHeader woh 
JOIN PLS.CodeStatus cs ON woh.StatusID = cs.ID
JOIN PLS.vCodeWorkStation cws ON woh.WorkStationID = cws.ID
WHERE woh.StatusID = 19 ";

            if (sites == "BYDGOSZCZ")
            {
                query += "AND woh.programId = 10058 ";
            }
            else if (sites == "MEXICALI")
            {
                query += "AND woh.programId = 10059 ";

            }
            else if (sites == "SUZHOU")
            {
                query += "AND woh.programId = 10060 ";

            }
           


            DataTable dt = oDAL.GetData(query);

            filterString += "> Program = Bose";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("213", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstOpenWipOrders = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}