using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
namespace IP.Areas.ListingReports.Models
{
    public class BoseAWP
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstBoseAWP { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList()
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"
SELECT 
         WOH.ProgramID,
       WOH.ID, 
	   (SELECT CustomerReference FROM PLS.ROHeader WHERE ProgramID = WOH.ProgramID AND ID = ( 
	   SELECT ROHeaderID FROM PLS.PartSerial WHERE ProgramID = WOH.ProgramID AND SerialNo = WOH.SerialNo AND PartNo = WOH.PartNo)) AS RMA,
       WOH.PartNo,
	   PN.Description,
       WOH.SerialNo,
	   WOL.ComponentPartNo AS ComponentPartNo,
	   PNComp.Description AS ComponentDesc,
	   --CRT.Description as RepairType,
	   wol.QtyRequested,
	   max(pt.CreateDate) as HoldDate,
	   (SELECT max(ForDate)
		FROM PLS.PartTransaction ptran
		WHERE ptran.ProgramID = WOH.ProgramID AND ptran.PartNo = WOH.PartNo AND ptran.SerialNo = WOH.SerialNo AND ptran.PartTransactionID = 1) as ReceiveDate
    
FROM   pls.WOHeader WOH
 INNER JOIN pls.PartTransaction pt ON WOH.SerialNo = pt.SerialNo
             AND pt.OrderType = 'WO'
             AND WOH.ProgramID = pt.ProgramID
             AND WOH.UserID = pt.UserID
             AND CONVERT(DATETIME2(0), pt.CreateDate) = CONVERT(DATETIME2(0), WOH.LastActivityDate)
INNER JOIN PLS.PartNo PN ON PN.PartNo = WOH.PartNo 
INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
LEFT JOIN pls.WOLine WOL ON WOL.WOHeaderID = WOH.ID
LEFT JOIN PLS.PartNo PNComp ON PNComp.PartNo = WOL.ComponentPartNo
LEFT JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID
WHERE
  WOH.StatusID = 28
  AND (pt.Reason = 'Awaiting Parts')
AND WOL.StatusID = 7  ";

            if (sites == "BYDGOSZCZ")
            {
                query += " AND WOH.ProgramID = 10058 ";
                query = query.Replace("<ProgramId>", "10058");
            }
            else
            {
                query += " AND WOH.ProgramID = 10059 ";
                query = query.Replace("<ProgramId>", "10059");
            }

            query +=@"GROUP BY WOH.ID, WOH.ProgramID, WOH.CustomerReference, pt.reason,  WOH.PartNo, PN.Description, WOH.SerialNo,WOL.ComponentPartNo, PNComp.Description, wol.QtyRequested, pt.CreateDate
ORDER BY pt.CreateDate desc; 
";



            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("257", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstBoseAWP = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}