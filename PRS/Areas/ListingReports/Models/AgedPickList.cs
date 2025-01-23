using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class AgedPickList
    {

        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstAgedPickList { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList()
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"
SELECT  
        PS.ProgramID,
        PS.SerialNo,   
        PS.PalletBoxNo,
        PN.PartNo,	
		pl.Warehouse,
		PL.LocationNo,
		CS.Description AS Status, 		
		ro.CustomerReference as RMA,
		pt.CreateDate as ReceiptDate
FROM pls.PartSerial PS
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
INNER JOIN pls.PartNo PN ON PN.PartNo = PS.PartNo
INNER JOIN pls.PartLocation PL ON PL.id = PS.LocationID
INNER JOIN pls.CodeStatus CS ON CS.ID = PS.StatusID
INNER JOIN pls.[User] U ON U.ID = PS.UserID 
INNER JOIN pls.roheader ro on ro.id = ps.roheaderID
INNER JOIN pls.PartTransaction pt ON pt.ProgramID = ps.ProgramID
                                                                AND pt.PartNo = ps.PartNo 
                                                                AND pt.SerialNo = ps.SerialNo 
                                                                AND pt.PartTransactionId = 1 
                                                           
WHERE ps.StatusID NOT IN (18, 8, 32) AND pl.Warehouse = 'FGI'
 ";

            if (sites == "BYDGOSZCZ")
            {
                query += "\nAND P.ID = 10064 ";
            }
            else if (sites == "JUAREZ")
            {
                query += "\nAND  P.ID = 10061 ";

            }
            else if (sites == "MEXICALI")
            {
                query += "\nAND P.ID = 10055 ";

            }
            else if (sites == "MEMPHIS")
            {
                query += "\nAND P.ID = 10053 ";

            }
            query += @"Order BY pt.CreateDate ";

            DataTable dt = oDAL.GetData(query);

            filterString += "> Program = DELL";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("224", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstAgedPickList = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}