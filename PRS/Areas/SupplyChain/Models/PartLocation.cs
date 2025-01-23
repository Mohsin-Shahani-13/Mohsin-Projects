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
    public class PartLocation
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        //public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        //public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        //[Display(Name = "To:")]
        //public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        //public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstPartLoc { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
Select 
    P.Name AS Program
    ,LocationNo
    ,Warehouse
    ,Bin
    ,Building
    ,CS.Description AS Status
    ,CLG.Description AS LocGroup
    ,U.Username AS CreatedBy
    ,PL.CreateDate AS CreatedOn
    ,PL.LastActivityDate AS LastActivityOn
FROM pls.PartLocation PL
INNER JOIN pls.Program P ON P.ID = PL.ProgramID 
INNER JOIN pls.[User] U ON U.ID = PL.UserID
LEFT OUTER JOIN PLS.CodeStatus CS ON CS.ID = PL.StatusID
LEFT OUTER JOIN pls.CodeLocationGroup CLG ON CLG.ID = PL.LocationGroupID
WHERE 1 = 1 
 ";
            if (programId != "0")
            {
                query += "AND P.ID = '" + programId + "' ";
            }
            else
            {
                query += "AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            //query += "AND P.ID = '" + programId + "' ";
            query += "ORDER BY PL.LastActivityDate DESC";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
            filterString += "> Program = '" + ProgramName + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("015", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPartLoc = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}