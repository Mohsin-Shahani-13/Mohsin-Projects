using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.SupplyChain.Models
{
    public class MinMaxLevel
    {
        cDAL oDAL = new cDAL("ACTIVE");

        #region Fields
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Program:")]
        public string programId { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstMinMaxLevel { get; set; }
        public List<Hashtable> lstGetSerials { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string partNo, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
select    PQ.ProgramID
        , P.Name
        , cgt.C01 AS 'PartNo'
        , cgt.C02 AS 'COW'
        , cgt.C03 AS 'Min Level'
        , cgt.C04 AS 'Max Level'
        , SUM(PQ.AvailableQty) AS InventoryStock
        , CASE WHEN SUM(PQ.AvailableQty) >= cgt.C03 THEN 'Good' ELSE 'Low' END AS Status
from pls.CodeGenericTable cgt
left outer join pls.PartQty pq on pq.PartNo = cgt.C01
INNER JOIN Pls.Program P ON P.ID = PQ.ProgramID
INNER JOIN pls.CodeGenericTableDefinition cgtd on cgtd.ID = cgt.GenericTableDefinitionID 
Where cgtd.Name = 'VFMINLEVEL'
";
            if (programId != "0")
            {
                query += "AND PQ.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "AND PQ.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (!string.IsNullOrEmpty(partNo))
                query += "AND C01 LIKE '%" + partNo + "%' ";

            query += @" GROUP BY PQ.ProgramID
                        , P.Name
                        , cgt.C01
                        , cgt.C02
                        , cgt.C03
                        , cgt.C04  ";
            query += "ORDER BY PartNo ";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";



            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. Like '" + partNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("115", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMinMaxLevel = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}