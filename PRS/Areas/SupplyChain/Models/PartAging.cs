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
    public class PartAging
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Idle Over:")]
        public string IdleOver { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstPartAging { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        public bool GetList(string IdleOver, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT PQ.ProgramID
     ,P.Name
     ,PQ.PartNo
     ,PL.LocationNo
	 ,CLG.Description AS LocationGroup
     ,CC.[Description] AS Configuration 
	 ,PalletBoxNo
	 ,LotNo
	 ,AvailableQty
	 ,U.Username
	 ,PQ.LastActivityDate
FROM pls.PartQty PQ
     INNER JOIN pls.Program P ON P.ID = PQ.ProgramID
	 INNER JOIN pls.PartLocation PL ON PL.ID = PQ.LocationID
     INNER JOIN pls.[user] U ON U.ID = PQ.UserID
     INNER JOIN pls.CodeConfiguration CC ON CC.ID = PQ.ConfigurationID
	 LEFT OUTER JOIN pls.CodeLocationGroup CLG ON CLG.ID = PL.LocationGroupID
WHERE AvailableQty > 0 AND 
DATEDIFF(DAY, PQ.LastActivityDate, GetDate()) +1 > <IdleOver>
";

            //if (!string.IsNullOrEmpty(programId))
            //    query += "AND PQ.ProgramID = '" + programId + "' ";

            if (programId != "0")
            {
                query += "AND PQ.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "AND PQ.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }


            query += "ORDER BY LastActivityDate DESC";

            query = query.Replace("<IdleOver>", IdleOver);
            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | Idle Over = '" + IdleOver + "' ";

            

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("014", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPartAging = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}