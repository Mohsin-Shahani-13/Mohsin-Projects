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
    public class MaterialInventory
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
        public List<Hashtable> lstMaterialInventory { get; set; }
        public List<Hashtable> lstGetSerials { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string partNo, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT   ProgramId
		,PrimCommDesc
		,CompPartNo
		,CompPartDesc
		,AvgCons AS AvgConsQty
		,CrntStock AS CrntStockQty
		,WeekOfInv
		,WhereUsed
FROM    PlusRS.rpt.MaterialInv
";
            if (programId != "0")
            {
                query += "WHERE ProgramId = '" + programId + "' ";
            }
            else
            {
                query += "WHERE ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (!string.IsNullOrEmpty(partNo))
                query += "AND CompPartNo LIKE '%" + partNo + "%' ";

            
            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";



            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. Like '" + partNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("103", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMaterialInventory = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}