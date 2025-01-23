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
    public class ComponentPickList
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Component Part No.:")]
        public string compPartNo { get; set; }
        public List<Hashtable> lstCompPickList { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string programId, string ProgramName, string compPartNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT 
       P.ID,
       P.Name,
	   WL.componentpartno,
       SUM(CAST(WL.qtyrequested - wl.qtyconsumed AS bigint)) AS qtyrequired,
       pl.locationno,
       clg.Description AS LocationGroup,
       cc.Description AS Configuration,
       SUM(CAST(pq.availableqty AS bigint)) AS qtyavailable
FROM   pls.WOLine wl
JOIN pls.WOHeader wh ON wl.woheaderid = wh.id 
JOIN pls.Program p ON p.ID = wh.ProgramID
LEFT JOIN pls.PartQty pq ON wh.programid = pq.programid
AND wl.componentpartno = pq.partno
JOIN pls.PartLocation pl ON pq.programid = pl.programid
AND pq.LocationID = pl.ID 
JOIN pls.CodeLocationGroup clg ON clg.ID = pl.LocationGroupID
JOIN pls.CodeConfiguration cc ON cc.ID = pq.ConfigurationID
WHERE  wl.qtyconsumed < wl.qtyrequested
       AND wl.StatusID = 7
       AND clg.Description NOT IN ( 'NONINVENTORY', 'SCRAP' )
       AND pq.availableqty > 0

";

            if (!string.IsNullOrEmpty(compPartNo))
                query += "AND WL.componentpartno LIKE '%" + compPartNo + "%'";

            if (programId != "0")
            {
                query += "AND wh.ProgramId = '" + programId + "' ";
            }
            else
            {
                query += "AND wh.ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += @"GROUP  BY 
          P.ID,
          P.Name,
		  wl.componentpartno,
          pl.locationno,
          clg.Description,
          cc.Description";
            //query += "ORDER BY sol.ID";
            //query = query.Replace("<programId>",programId);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(compPartNo))
                filterString += "| Component Part No. Like '" + compPartNo + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("022", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstCompPickList = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}