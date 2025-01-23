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
    public class GoodInventoryReadytoShip
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstGoodInventoryReadytoShip { get; set; }
        #endregion
        #region Methods 

        public bool GetList(string partNo, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT P.ID,
       PQ.partno, 
       PN.description, 
       PQ.LocationId,
	   PL.locationno,
       PL.Bin,
       PL.Warehouse,
       CC.ID AS ConfigId,
	   CC.Description AS Configuration ,
       PQ.PalletBoxNo,
	   PQ.LotNo,
       CASE WHEN PN.SerialFlag = 0 THEN 'N' ELSE 'Y' END AS SerialFlag,
	   --PQ.LotNo,
         PQ.availableqty,
	   --PQ.ReservedQty,
       P.NAME,
	   U.Username,
	   PQ.CreateDate,
       PQ.LastActivityDate
FROM   pls.partqty PQ 
INNER JOIN pls.partno PN ON PQ.partno = PN.partno 
INNER JOIN pls.partlocation PL ON PL.id = PQ.locationid 
INNER JOIN pls.program P ON P.id = PQ.programid 
INNER JOIN [pls].[CodeConfiguration] CC ON CC.ID = PQ.ConfigurationID
INNER JOIN [pls].[User] U ON U.ID = PQ.UserID  
       WHERE  PQ.availableqty > 0 AND PL.warehouse ='Storage' AND CC.Description = 'Good' AND LEFT(PQ.PalletBoxNo,7) = 'C205474' ";

            if (!string.IsNullOrEmpty(partNo))
                query += "AND PQ.partno LIKE '%" + partNo + "%' ";


            query += "ORDER BY PQ.LastActivityDate DESC";
            //  query += "ORDER BY PQ.partno";
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. Like '" + partNo + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("203", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstGoodInventoryReadytoShip = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}