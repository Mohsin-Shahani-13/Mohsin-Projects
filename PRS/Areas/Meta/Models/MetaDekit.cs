using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaDekit
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "kit Part No.:")]
        public string PartNo { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstMetaDekit { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList(string programId, string ProgramName, string PartNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT 
      
        FORMAT(PT.ForDate, 'MM/dd/yyyy hh:mm:ss tt') AS KIT_REC_DATE
      , WOH.ID AS ORDER_NO
      , PN.ModelNo AS FAMILY
      , PT.PartNo AS KIT_PART_NO
      , PN.Description AS KIT_DESCRIPTION
      , SUM(PTD.Qty) AS DEKIT_QTY
      , FORMAT(PTD.ForDate, 'MM/dd/yyyy hh:mm:ss tt') AS DEKIT_DATE
      , WOL.ComponentPartNo AS COMP_PART_NO
      , PNC.Description AS COMP_DESCRIPTION
      , SUM(convert(bigint ,WOL.QtyRequested)) AS COMP_QTY
FROM pls.PartTransaction PTD
LEFT JOIN pls.PartTransaction PT ON PT.SerialNo = PTD.SerialNo AND PT.PartTransactionId = 1
INNER JOIN pls.WOHeader WOH ON WOH.ProgramId = PTD.ProgramId AND WOH.PartNo = PTD.PartNo AND WOH.SerialNo = PTD.SerialNo 
INNER JOIN pls.WOLine WOL ON WOL.WoHeaderId = WOH.ID
INNER JOIN pls.Program P ON P.ID = PTD.ProgramID
LEFT JOIN pls.PartNo PN ON PN.PartNo = PT.PartNo
LEFT JOIN pls.PartNo PNC ON PNC.PartNo = WOL.ComponentPartNo 


                WHERE PTD.PartTransactionId = 27 AND (PT.PartNo LIKE '301%' OR PT.PartNo LIKE '899%') ";

            if (programId != "0" && programId != null)
            {
                query += "AND PTD.ProgramId= '" + programId + "' ";
            }
            else
            {
                query += "AND PTD.ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            if (!string.IsNullOrEmpty(PartNo))
                query += " AND PT.PartNo LIKE '<PartNo>'";
                query += @"  
              GROUP BY   
                  PT.ForDate,
                  WOH.ID,
                  PN.ModelNo,
                  PT.PartNo,
                  PN.Description,
                  PTD.ForDate,
                  WOL.ComponentPartNo,
                  PNC.Description
        ORDER BY PTD.ForDate DESC";

            query = query.Replace("<PartNo>", PartNo);
            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";
            if (!string.IsNullOrEmpty(PartNo))
                filterString += "| Part No. = '" + PartNo + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("135", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaDekit = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}