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
    public class PartStructureBOM
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstPartStructBOM { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string programId, string ProgramName, string partNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT  PB.ID,
        PB.ProgramID,
		p.Name, 
		PB.PartNo, 
		ComponentPartNo,
        PN.Description,
		RevisionNo,
		Qty, 
		CS.Description AS STATUS,
		U.Username, 
		PB.CreateDate, 
		PB.LastActivityDate
FROM pls.PartBom PB
INNER JOIN PLS.PartNo PN ON PN.PartNo = PB.ComponentPartNo
INNER JOIN PLS.Program P ON P.ID = PB.ProgramID
INNER JOIN PLS.CodeStatus CS ON CS.ID = PB.StatusID
INNER JOIN PLS.[User] U ON U.ID = PB.UserID
";
            if (!string.IsNullOrEmpty(partNo))
                query += "WHERE PB.PartNo LIKE '%" + partNo + "%'";
            
            if (programId != "0")
            {
                query += " AND P.ID = '" + programId + "' ";
            }
            else
            {
                query += " AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName) && !string.IsNullOrEmpty(partNo))
                filterString += " Program = '" + ProgramName + "' | Part No. Like '" + partNo + "' ";

            else if (!string.IsNullOrEmpty(ProgramName))

                filterString += " Program = '" + ProgramName + "'";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("018", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPartStructBOM = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}