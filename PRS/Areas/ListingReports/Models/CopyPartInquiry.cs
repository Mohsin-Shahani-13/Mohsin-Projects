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
    public class CopyPartInquiry
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }        
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstCopyPartInquiry { get; set; }
        
        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string partNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT P.PartNo, 
      P.Description, 
      p.ManufacturePartNo, 
      p.ModelNo, 
      p.SerialFlag, 
      CC.Description AS PrimaryCommodity, 
      p.SecondaryCommodityID, 
      CP.Description AS PartType, 
      p.CycleCountFlag, 
      p.CycleCountPeriod, 
      CS.Description AS Status, 
      u.Username, 
      p.CreateDate, 
      p.LastActivityDate
FROM pls.PartNo P
INNER JOIN Pls.CodeCommodity CC ON CC.ID = P.PrimaryCommodityID
INNER JOIN pls.CodePartType CP ON CP.ID = P.PartTypeID
INNER JOIN pls.CodeStatus CS ON CS.ID = P.StatusID
INNER JOIN pls.[User] U ON U.ID = P.UserID ";


            if (!string.IsNullOrEmpty(partNo))
                query += "WHERE P.PartNo LIKE '%" + partNo + "%'";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(partNo))
                filterString = "> Part No. Like '" + partNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("003", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstCopyPartInquiry = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}