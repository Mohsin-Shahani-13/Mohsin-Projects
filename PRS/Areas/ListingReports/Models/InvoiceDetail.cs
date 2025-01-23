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
    public class InvoiceDetail
    {

       
        [Display(Name = "From:")]
        public string _RecfromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string RecfromDt { get { return _RecfromDt; } set { _RecfromDt = value; } }
        [Display(Name = "To:")]
        public string _RectoDt = DateTime.Now.ToString(Format.DateOnly);
        public string RectoDt { get { return _RectoDt; } set { _RectoDt = value; } }
        public bool isAllDate { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public List<Hashtable> lstInvoiceDetail { get; set; }
        public string ErrorMessage { get; set; }


        cDAL oDAL = new cDAL("ACTIVE");
      

        public bool GetList(string fromDt, string toDt, bool isAllDate)
        {

            string query = string.Empty;
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            query = @"
      WITH PTHISTORY AS (
SELECT 
    concat(left(pt.SerialNo,20),left(psa1.value,3)) as 'REG PPID',
    (case when psa2.value is null then pt.serialno when psa2.value ='' then pt.serialno else concat(left(pt.SerialNo,20),left(psa2.value,3)) end) as 'SHIP PPID',
    (case when psa2.value > psa1.value then 'YES' else 'NO' END) as 'UPGRADE YES/NO',
    pt.PartNo as 'MODEL', 
    wsa.value as 'SCREENING TEST RESULT',
    cf.Description as 'QUICK TEST RESULT',
(CASE 
            WHEN pt.PartTransactionID = 18 AND pt.Configuration = 'GOOD' 
            THEN CONVERT(VARCHAR, pt.CreateDate, 102) 
            ELSE '' 
        END) AS 'SHIP DATE',
  (CASE WHEN pt.PartTransactionID =8 THEN convert(varchar, pt.CreateDate,23) ELSE '' END) AS 'SCRAP DATE',
    wl.ComponentPartNo as 'COMPONENTS'
FROM
    [pls].[PartTransaction] pt 
JOIN pls.PartSerial ps ON pt.SerialNo = ps.SerialNo
JOIN pls.WOHeader wo ON wo.CustomerReference = pt.SerialNo
JOIN pls.WOStationHistory wsh ON wsh.WOHeaderID = wo.ID and wsh.WorkStationID = (SELECT ws.id FROM pls.CodeWorkStation ws WHERE ws.Description = 'gTest0')
LEFT JOIN pls.WOStationAttribute wsa ON wsa.WOStationHistoryID = wsh.ID AND wsa.AttributeID = (SELECT ca4.id FROM pls.CodeAttribute ca4 WHERE ca4.AttributeName = 'SCREENING_RESULT')
LEFT JOIN pls.WOLine wl ON wl.WOHeaderID = wo.ID 
LEFT JOIN pls.WOUnit wou ON wou.WOLineID = wl.id
LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = WOU.ID
LEFT JOIN pls.CodeFault CF ON CF.ID = WUC.FaultID 
LEFT JOIN pls.PartSerialAttribute psa1 ON psa1.PartSerialID = ps.ID AND psa1.AttributeID = (SELECT ca1.id FROM pls.CodeAttribute ca1 WHERE ca1.AttributeName = 'INCOMING_REVISION')
LEFT JOIN pls.PartSerialAttribute psa2 ON psa2.PartSerialID = ps.ID AND psa2.AttributeID = (SELECT ca2.id FROM pls.CodeAttribute ca2 WHERE ca2.AttributeName = 'NEW_REVISION')
";

            if (sites == "BYDGOSZCZ")
            {
                query += "\nWHERE pt.programid=10064 ";
            }
            else if (sites == "JUAREZ")
            {
                query += "\nWHERE pt.programid=10061 ";

            }
            else if (sites == "MEXICALI")
            {
                query += "\nWHERE pt.programid=10055 ";

            }
            else if (sites == "MEMPHIS")
            {
                query += "\nWHERE pt.programid = 10053 ";

            }

            if (isAllDate != true)
            {
                query += @"
AND (
            (pt.PartTransactionID = 18 AND pt.Configuration = 'GOOD' 
            AND CONVERT(Date, pt.CreateDate) >= '<fromDt>' AND CONVERT(Date, pt.CreateDate) <= '<toDt>')
            OR pt.PartTransactionID = 8
        )  ";
            }
            query += @"AND pt.PartTransactionID IN(18,8))
select [REG PPID],[SHIP PPID],[UPGRADE YES/NO],MODEL,[SCREENING TEST RESULT],STRING_AGG([QUICK TEST RESULT],',') as 'QUICK TEST RESULTS',[SHIP DATE], [SCRAP DATE] , REPLACE(STRING_AGG(COMPONENTS,','),MODEL,'') as 'PARTS USED'
FROM PTHISTORY
GROUP BY [REG PPID],[SHIP PPID],[UPGRADE YES/NO],MODEL,[SCREENING TEST RESULT],[SHIP DATE], [SCRAP DATE] 
";
            query = query.Replace("<fromDt>", fromDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);




            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("237", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstInvoiceDetail = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
      
    }
}