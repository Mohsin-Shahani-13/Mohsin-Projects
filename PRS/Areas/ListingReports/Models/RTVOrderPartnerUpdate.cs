using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class RTVOrderPartnerUpdate
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstRTVOrderPartnerUpdate { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList()
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"
SELECT
pt.ProgramID,
rua.Value as 'RTV_ORDER_NUMBER_ORDER',
Pt.PartNo as 'PART Number',
PS.LastActivityDate as 'Updated Date',
'' as 'REASON_CATEGORY',
'' as 'ETA Wk',
'' as 'Shipping Tracking No',
'' as 'Remarks Free Text'
FROM pls.PartSerial PS
INNER JOIN pls.PartTransaction pt ON pt.ProgramID = ps.ProgramID
AND pt.PartNo = ps.PartNo
AND pt.SerialNo = ps.SerialNo
AND pt.PartTransactionId = 1
inner join pls.ROUnit ru on ru.ROLineID = pt.OrderLineID and ru.SerialNo = pt.SerialNo
inner join pls.ROUnitAttribute rua on rua.ROUnitID = ru.ID
inner join pls.CodeAttribute ca on ca.ID = rua.AttributeID and ca.AttributeName = 'RTV_NO'
WHERE ps.StatusID NOT IN (18, 8, 32)
and pt.CreateDate < GETDATE() -30 ";

            if (sites == "BYDGOSZCZ")
            {
                query += "AND Pt.ProgramID = '10064' ";
            }
            else if (sites == "JUAREZ")
            {
                query += "AND Pt.ProgramID = 10061 ";

            }
            else if (sites == "MEXICALI")
            {
                query += "AND Pt.ProgramID = 10055 ";

            }
            else if (sites == "MEMPHIS")
            {
                query += "AND Pt.ProgramID = 10053 ";

            }
            query += "Order BY PS.LastActivityDate DESC ";


            DataTable dt = oDAL.GetData(query);

            filterString += "> Program = DELL";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("219", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRTVOrderPartnerUpdate = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}