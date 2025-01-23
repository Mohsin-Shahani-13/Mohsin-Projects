using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using ClosedXML.Excel;
using System.IO;

namespace IP.Areas.ListingReports.Models
{
    public class ReceiptAndCreditREFURB
    {
        #region fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstRefurbRecvCreditingSummary { get; set; }
        public List<Hashtable> lstRefurbRecvCrediting { get; set; }
        public List<Hashtable> lstRefurbNewRepackSummary { get; set; }
        public List<Hashtable> lstRefurbNewRepack { get; set; }
        public List<Hashtable> lstRefurbProcessFGISummary { get; set; }
        public List<Hashtable> lstRefurbProcessFGI { get; set; }
        public List<Hashtable> lstRefurbRecvScrapSummary { get; set; }
        public List<Hashtable> lstRefurbRecvScrap { get; set; }
        public List<Hashtable> lstRepairRecvCreditSummary { get; set; }
        public List<Hashtable> lstRepairRecvCredit { get; set; }
        public List<Hashtable> lstRepairProcessFGISummary { get; set; }
        public List<Hashtable> lstRepairProcessFGI { get; set; }
        public List<Hashtable> lstRepairRecvTestedSummary { get; set; }
        public List<Hashtable> lstRepairRecvTested { get; set; }

        public List<Hashtable> lstRepairRecvPackSummary { get; set; }
        public List<Hashtable> lstRepairRecvPack { get; set; }
        #endregion
        public DataTable Program() // onHand warehouse method
        {
            cDAL oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }

        cDAL oDAL = new cDAL("ACTIVE");
        #region methods
        public bool GetRefurbRecvCrediting(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {

            string query = string.Empty;

            query = @"

-- SUMMARY: RECEIVED AND NOT SCRAPPED EXCHANGE/RETURN --date filter on receipts transaction --final SQL
select 'Return/Exchange' ProcessType , cc.Description  Commodity,  
count(pt.SerialNo) Value 
from pls.PartTransaction pt with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
left join pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
where 
    PartTransactionID = 1 and 
    pt.ProgramID = '<programId>' and 
    roha.AttributeName ='PROCESS_TYPE' and
	roha.value in ('RETURN','EXCHANGE') 
and 
	pt.SerialNo NOT IN
 	(
	select SerialNo  from pls.PartTransaction pt with (nolock)  where pt.ProgramID = '<programId>' and pt.ToLocation like 'SCRAP%' and pt.SerialNo NOT LIKE '%DUMMY%'
	)
	 AND CONVERT(DATE, pt.CreateDate) >= '<frmDt>'
     AND CONVERT(DATE, pt.CreateDate) <= '<toDt>'
group by cc.Description;

   -----DETAIL------           
				SELECT 
    pt.ProgramID, 
    roh.ID, 
    roh.CustomerReference, 
    roha.value AS ProcessType, 
    cc.Description AS Commodity, 
    pn.PartNo, 
    pn.Description, 
    pt.SerialNo, 
    pt.CreateDate AS ReceiptDate, 
    u.Username
FROM 
    pls.PartTransaction pt WITH (NOLOCK)
LEFT JOIN 
    pls.ROHeader roh with (nolock) ON pt.OrderHeaderID = roh.ID
LEFT JOIN 
    pls.vROHeaderAttribute roha with (nolock) ON roh.ID = roha.ROHeaderID AND roha.AttributeName = 'PROCESS_TYPE'
LEFT JOIN 
    pls.PartNo pn with (nolock) ON pn.PartNo = pt.PartNo
LEFT JOIN 
    pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
LEFT JOIN 
    pls.[User] u with (nolock) ON u.ID = pt.UserID
WHERE
    pt.PartTransactionID = 1 
    AND pt.ProgramID = '<programId>' 
    AND roha.value IN ('RETURN', 'EXCHANGE') 
    AND pt.SerialNo NOT IN (
        SELECT SerialNo  
        FROM pls.PartTransaction pt with (nolock) 
        WHERE pt.ProgramID = '<programId>' 
        AND pt.ToLocation LIKE 'SCRAP%' 
        AND pt.SerialNo NOT LIKE '%DUMMY%'

    )
    AND CONVERT(DATE, pt.CreateDate) >= '<frmDt>'
    AND CONVERT(DATE, pt.CreateDate) <= '<toDt>';

                ";

            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataSet DS = oDAL.GetDataSet(query);

            ///////////FILTER SRINGS/////////

            filterString += " > Program = 'BOSE' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("207", query, "REFURB Receipt And Credit", false);

            if (!oDAL.HasErrors)
            {
                lstRefurbRecvCreditingSummary = cCommon.ConvertDtToHashTable(DS.Tables[0]);
                lstRefurbRecvCrediting = cCommon.ConvertDtToHashTable(DS.Tables[1]);
                //lstRefurbRecvCrediting = cCommon.ConvertDtToHashTable(DS.Tables[0]);

                return true;

            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                oDAL.HasErrors = false;
                return false;
            }
        }

        public bool RefurbNewRepack(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {


            string query = string.Empty;
            query = @"--NEW and REPACK (SEALBOX and REPACK) --Final SQL
select 'Return/Exchange' ProcessType , cc.Description Commodity,  count(pt.SerialNo) Value 
from pls.PartTransaction pt with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
LEFT JOIN 
    pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
where 
    pt.PartTransactionID =  1--'RO-RECEIVE' 
	and pt.ProgramID = '<programId>'  and 
    roha.AttributeName ='PROCESS_TYPE' and 
	roha.value in ('RETURN','EXCHANGE') and
	(
    pt.SerialNo IN (select SerialNo  from pls.WOHeader woh with (nolock) where woh.ProgramID = '<programId>'  and woh.RepairTypeID = '204')
	OR
	pt.SerialNo IN (select SerialNo  from pls.PartTransaction pt with (nolock) where pt.ProgramID = '<programId>'  and pt.Location like 'SEALBOX%' and pt.SerialNo NOT LIKE '%DUMMY%')
	)
	 AND CONVERT(DATE, pt.CreateDate) >= '<frmDt>'
     AND CONVERT(DATE, pt.CreateDate) <= '<toDt>'
group by cc.Description
--NEW and REPACK (SEALBOX and REPACK) --Final SQL
select pt.ProgramID, roh.ID, roh.CustomerReference, roha.value ProcessType , cc.Description Commodity, pn.PartNo, pn.Description, pt.SerialNo, pt.CreateDate receiptdate, u.Username 
from pls.PartTransaction pt  with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
LEFT JOIN pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
LEFT JOIN pls.[User] u with (nolock) ON u.ID = pt.UserID
where 
      pt.PartTransactionID =  1 --'RO-RECEIVE' 
	  and pt.ProgramID = '<programId>'  and 
    roha.AttributeName ='PROCESS_TYPE' and 
	roha.value in ('RETURN','EXCHANGE') and
	(
    pt.SerialNo IN (select SerialNo  from pls.WOHeader woh with (nolock) where woh.ProgramID = '<programId>'  and woh.RepairTypeID = '204')
	OR
	pt.SerialNo IN (select SerialNo  from pls.PartTransaction pt with (nolock) where pt.ProgramID = '<programId>'  and pt.Location like 'SEALBOX%' and pt.SerialNo NOT LIKE '%DUMMY%')
	)
	 AND CONVERT(DATE, pt.CreateDate) >= '<frmDt>'
     AND CONVERT(DATE, pt.CreateDate) <= '<toDt>'
--group by pn.PrimaryCommodity
";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataSet DS = oDAL.GetDataSet(query);

            ///////////FILTER SRINGS/////////

            filterString += " > Program = 'BOSE' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("207-2", query, "REFURB New and REPACK", false);

            if (!oDAL.HasErrors)
            {

                lstRefurbNewRepackSummary = cCommon.ConvertDtToHashTable(DS.Tables[0]);
                lstRefurbNewRepack = cCommon.ConvertDtToHashTable(DS.Tables[1]);
                return true;

            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                oDAL.HasErrors = false;
                return false;
            }

        }
        public bool RefurbProcessFGI(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {


            string query = string.Empty;
            query = @"--RECEIVED TO FGI RETURN/EXCHANGE --date filter on move to FGI - wo completion?
select 'Return/Exchange' ProcessType , cc.Description Commodity,  count(pt.SerialNo) Value 
from pls.PartTransaction pt with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
LEFT JOIN  pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
where 
    pt.PartTransactionID =  1--'RO-RECEIVE' 
	and pt.ProgramID = '<programId>' and 
    roha.AttributeName ='PROCESS_TYPE' and 
	roha.value in ('RETURN','EXCHANGE') 
and
    pt.SerialNo IN (select SerialNo  from pls.PartTransaction pt with (nolock) where pt.ProgramID = '<programId>' and pt.Location like 'FGI%' and pt.SerialNo NOT LIKE '%DUMMY%'
	AND 
    CONVERT(DATE, pt.CreateDate) >= '<frmDt>'
	AND
	CONVERT(DATE, pt.CreateDate) <= '<toDt>'
	)
    
group by cc.Description



SELECT DISTINCT 
    pt.ProgramID,
roh.ID,
    roh.CustomerReference, 
    roha.value AS ProcessType, 
    cc.Description AS Commodity, 
    pn.PartNo, 
    pn.Description, 
    pt.SerialNo, 
    pt.CreateDate AS ReceiptDate, 
    sub.CreateDate AS Workorder_Close_Date 
FROM 
    pls.PartTransaction pt with (nolock)
LEFT JOIN 
    pls.ROHeader roh with (nolock) ON pt.OrderHeaderID = roh.ID 
LEFT JOIN 
    pls.vROHeaderAttribute roha with (nolock) ON roh.ID = roha.ROHeaderID 
LEFT JOIN 
    pls.PartNo pn ON pn.PartNo = pt.PartNo 
LEFT JOIN pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
LEFT JOIN 
    (SELECT 
         SerialNo, 
         MIN(CreateDate) AS CreateDate  -- Use MIN to ensure one row per serial number
     FROM 
         pls.PartTransaction with (nolock)
     WHERE 
         ProgramID = '<programId>'
         AND Location LIKE 'FGI%' 
         AND SerialNo NOT LIKE '%DUMMY%'
         AND CONVERT(DATE, CreateDate)>= '<frmDt>'  AND CONVERT(DATE, CreateDate) <= '<toDt>'
     GROUP BY 
         SerialNo) sub 
ON sub.SerialNo = pt.SerialNo
WHERE 
      pt.PartTransactionID =  1 --'RO-RECEIVE' 
    AND pt.ProgramID = '<programId>'
    AND roha.AttributeName = 'PROCESS_TYPE' 
    AND roha.value IN ('RETURN', 'EXCHANGE') 
    AND pt.SerialNo IN (SELECT 
                            DISTINCT SerialNo  -- Use DISTINCT to prevent duplication
                        FROM 
                            pls.PartTransaction with (nolock)
                        WHERE 
                            ProgramID = '<programId>'
                            AND Location LIKE 'FGI%' 
                            AND SerialNo NOT LIKE '%DUMMY%'
                            AND CONVERT(DATE, CreateDate)>= '<frmDt>'  AND CONVERT(DATE, CreateDate) <= '<toDt>');
";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataSet DS = oDAL.GetDataSet(query);

            ///////////FILTER SRINGS/////////

            filterString += " > Program = 'BOSE' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("207-3", query, "REFURB Processed to FGI", false);

            if (!oDAL.HasErrors)
            {

                lstRefurbProcessFGISummary = cCommon.ConvertDtToHashTable(DS.Tables[0]);
                lstRefurbProcessFGI = cCommon.ConvertDtToHashTable(DS.Tables[1]);
                return true;

            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                oDAL.HasErrors = false;
                return false;
            }

        }

        public bool RefurbRecvScrap(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {


            string query = string.Empty;
            query = @" -- REFURB Receive and Scrap -- 
-- RECEIVED AND SCRAPPED --date filter on receipt --Final SQL
select 'Return/Exchange' ProcessType , cc.Description Commodity,  count(pt.SerialNo) Value
from pls.vPartTransaction pt with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
LEFT JOIN pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
where 
    pt.PartTransaction = 'RO-RECEIVE' 
	and  pt.ProgramID = '<programId>' and 
    roha.AttributeName ='PROCESS_TYPE' and 
	roha.value in ('RETURN','EXCHANGE') and 
    pt.SerialNo IN 
	(
	select SerialNo  from pls.vPartTransaction pt with (nolock) where pt.ProgramID =  '<programId>' and pt.ToLocation like 'SCRAP%' and pt.SerialNo NOT LIKE '%DUMMY%'
	)and
    CONVERT(DATE, pt.CreateDate) >= '<frmDt>' 
	AND
	CONVERT(DATE, pt.CreateDate) <= '<toDt>'
   group by cc.Description
-- Detail RECEIVED AND SCRAPPED --date filter on receipt --Final SQL
select pt.ProgramID, roh.ID, roh.CustomerReference, roha.value ProcessType , cc.Description Commodity, pn.PartNo, pn.Description, pt.SerialNo, pt.CreateDate receiptdate 
from pls.vPartTransaction pt  with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
LEFT JOIN pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
where 
    PartTransaction = 'RO-RECEIVE' 
	and pt.ProgramID =  '<programId>' and 
    roha.AttributeName = 'PROCESS_TYPE' and 
	roha.value in ('RETURN','EXCHANGE') 
and 
    pt.SerialNo IN 
	(
	select SerialNo  from pls.vPartTransaction pt with (nolock) where pt.ProgramID =  '<programId>' and pt.ToLocation like 'SCRAP%' and pt.SerialNo NOT LIKE '%DUMMY%'
	)AND
    CONVERT(DATE, pt.CreateDate) >= '<frmDt>' 
	AND
	CONVERT(DATE, pt.CreateDate) <= '<toDt>'


";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataSet DS = oDAL.GetDataSet(query);

            ///////////FILTER SRINGS/////////

            filterString += " > Program = 'BOSE' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("207-4", query, "REFURB Receive and Scrap", false);

            if (!oDAL.HasErrors)
            {

                lstRefurbRecvScrapSummary = cCommon.ConvertDtToHashTable(DS.Tables[0]);
                lstRefurbRecvScrap = cCommon.ConvertDtToHashTable(DS.Tables[1]);
                return true;

            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                oDAL.HasErrors = false;
                return false;
            }

        }

        public bool RepairRecvCredit(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {


            string query = string.Empty;
            query = @"-- RECEIVED AND NOT SCRAPPED REPAIR --date filter on receipt transaction --final SQL
select 'REPAIR' ProcessType , cc.Description Commodity, count(pt.SerialNo) Value 
from pls.PartTransaction pt with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
LEFT JOIN  pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
where 
    pt.PartTransactionID =  1--'RO-RECEIVE'
	and pt.ProgramID = '<programId>' and 
    roha.AttributeName ='PROCESS_TYPE' and
	roha.value = 'REPAIR' and 
	pt.SerialNo NOT IN
 	(
	select SerialNo  from pls.PartTransaction pt with (nolock) where pt.ProgramID = '<programId>' and pt.ToLocation like 'SCRAP%' and pt.SerialNo NOT LIKE '%DUMMY%'
	)AND
    CONVERT(DATE, pt.CreateDate) >= '<frmDt>' 
	AND
	CONVERT(DATE, pt.CreateDate) <= '<toDt>'
group by cc.Description;
-- RECEIVED AND NOT SCRAPPED REPAIR --date filter on receipt transaction --final SQL
select pt.ProgramID, roh.ID, roh.CustomerReference, roha.value ProcessType , cc.Description Commodity, pn.PartNo, pn.Description, pt.SerialNo, pt.CreateDate receiptdate 
from pls.PartTransaction pt with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
LEFT JOIN  pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
where 
      pt.PartTransactionID =  1 --'RO-RECEIVE'
	  and pt.ProgramID = '<programId>' and 
    roha.AttributeName ='PROCESS_TYPE' and
	roha.value = 'REPAIR' and 
	pt.SerialNo NOT IN
 	(
	select SerialNo  from pls.PartTransaction pt with (nolock) where pt.ProgramID = '<programId>' and pt.ToLocation like 'SCRAP%' and pt.SerialNo NOT LIKE '%DUMMY%'
	)AND
    CONVERT(DATE, pt.CreateDate) >= '<frmDt>'
	AND
	CONVERT(DATE, pt.CreateDate) <= '<toDt>'
--group by pn.PrimaryCommodity;
";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataSet DS = oDAL.GetDataSet(query);

            ///////////FILTER SRINGS/////////

            filterString += " > Program = 'BOSE' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("207-5", query, "REPAIR Receiving and Crediting", false);

            if (!oDAL.HasErrors)
            {

                lstRepairRecvCreditSummary = cCommon.ConvertDtToHashTable(DS.Tables[0]);
                lstRepairRecvCredit = cCommon.ConvertDtToHashTable(DS.Tables[1]);
                return true;

            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                oDAL.HasErrors = false;
                return false;
            }

        }

        public bool RepairProcessFGI(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {


            string query = string.Empty;
            query = @"--RECEIVED TO FGI REPAIR --date filter on move to FGI - wo completion?
select 'Return/Exchange' ProcessType , cc.Description Commodity,  count(pt.SerialNo) Value
from pls.PartTransaction pt with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
LEFT JOIN  pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
where 
     pt.PartTransactionID =  1--'RO-RECEIVE'
	 and pt.ProgramID = '<programId>' and 
    roha.AttributeName ='PROCESS_TYPE' and 
	roha.value in ('REPAIR') and
    pt.SerialNo IN (select SerialNo  from pls.PartTransaction pt with (nolock) where pt.ProgramID = '<programId>' and pt.Location like 'FGI%' and pt.SerialNo NOT LIKE '%DUMMY%' 
	AND
    CONVERT(DATE, pt.CreateDate) >= '<frmDt>' 
	AND
	CONVERT(DATE, pt.CreateDate) <= '<toDt>')
    
group by cc.Description
SELECT
    pt.ProgramID,
    roh.ID,
    roh.CustomerReference,
    roha.value AS ProcessType,
    cc.Description AS Commodity,
    pn.PartNo,
    pn.Description,
    pt.SerialNo,
    MIN(pt.CreateDate) AS receiptdate,  -- Use MIN to handle multiple CreateDates
    MIN(sub.CreateDate) AS Workorder_close_date
FROM pls.PartTransaction pt with (nolock)
LEFT JOIN pls.ROHeader roh with (nolock) ON pt.OrderHeaderID = roh.ID
LEFT JOIN pls.vROHeaderAttribute roha with (nolock) ON roh.ID = roha.ROHeaderID
LEFT JOIN pls.PartNo pn with (nolock) ON pn.PartNo = pt.PartNo
LEFT JOIN pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
LEFT JOIN (
    SELECT SerialNo, CreateDate
    FROM pls.PartTransaction pt with (nolock)
    WHERE pt.ProgramID = '<programId>'
      AND pt.Location LIKE 'FGI%'
      AND pt.SerialNo NOT LIKE '%DUMMY%'
      AND CONVERT(DATE, pt.CreateDate) >= '<frmDt>' 
      AND CONVERT(DATE, pt.CreateDate) <= '<toDt>'
) sub ON sub.serialno = pt.serialNo
WHERE pt.PartTransactionID = 1 --'RO-RECEIVE'
  AND pt.ProgramID = '<programId>'
  AND roha.AttributeName = 'PROCESS_TYPE'
  AND roha.value IN ('REPAIR')
  AND pt.SerialNo IN (
    SELECT SerialNo
    FROM pls.PartTransaction pt with (nolock)
    WHERE pt.ProgramID = '<programId>'
      AND pt.Location LIKE 'FGI%'
      AND pt.SerialNo NOT LIKE '%DUMMY%'
      AND CONVERT(DATE, pt.CreateDate) >= '<frmDt>' 
      AND CONVERT(DATE, pt.CreateDate) <= '<toDt>'
)
GROUP BY
    pt.ProgramID,
    roh.ID,
    roh.CustomerReference,
    roha.value,
    cc.Description,
    pn.PartNo,
    pn.Description,
    pt.SerialNo;

";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataSet DS = oDAL.GetDataSet(query);

            ///////////FILTER SRINGS/////////

            filterString += " > Program = 'BOSE' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("207-6", query, "REPAIR Processed to FGI", false);

            if (!oDAL.HasErrors)
            {

                lstRepairProcessFGISummary = cCommon.ConvertDtToHashTable(DS.Tables[0]);
                lstRepairProcessFGI = cCommon.ConvertDtToHashTable(DS.Tables[1]);
                return true;

            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                oDAL.HasErrors = false;
                return false;
            }

        }

        public bool RepairRecvTested(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {


            string query = string.Empty;
            query = @"--REPAIR RECEIVED and tested (moved to Gtask4) --date filter on move to Gtask4 - final SQL
select 'Repair' ProcessType , cc.Description Commodity,  count(pt.SerialNo) Value 
from pls.PartTransaction pt with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
LEFT JOIN  pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
where 
     pt.PartTransactionID =  1--'RO-RECEIVE'
	 and pt.ProgramID = '<programId>' and 
    roha.AttributeName ='PROCESS_TYPE' and 
	roha.value in ('REPAIR') and
    pt.SerialNo IN (select woh.SerialNo FROM PLS.WOHeader woh with (nolock) JOIN PLS.WOStationHistory wosh ON woh.ID = wosh.WOHeaderID WHERE woh.ProgramID = '<programId>' 
	AND wosh.WorkStationID = 15 
	AND CONVERT(DATE, wosh.CreateDate) >= '<frmDt>' AND CONVERT(DATE, wosh.CreateDate) <= '<toDt>')
    
group by cc.Description

SELECT DISTINCT 
pt.ProgramID,
roh.ID,
    roh.CustomerReference, 
    roha.value AS ProcessType, 
    cc.Description AS Commodity, 
    pn.PartNo, 
    pn.Description, 
    pt.SerialNo, 
    pt.CreateDate AS ReceiptDate, 
    sub.CreateDate AS TestDate
FROM 
    pls.PartTransaction pt with (nolock)
LEFT JOIN 
    pls.ROHeader roh with (nolock) ON pt.OrderHeaderID = roh.ID 
LEFT JOIN 
    pls.vROHeaderAttribute roha with (nolock) ON roh.ID = roha.ROHeaderID 
LEFT JOIN 
    pls.PartNo pn with (nolock) ON pn.PartNo = pt.PartNo 
	LEFT JOIN  pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
LEFT JOIN 
    (SELECT 
         woh.SerialNo, 
         MIN(wosh.CreateDate) AS CreateDate  -- Use MIN to ensure one row per serial number
     FROM 
         PLS.WOHeader woh with (nolock)
     JOIN 
         PLS.WOStationHistory wosh with (nolock) ON woh.ID = wosh.WOHeaderID 
     WHERE 
         woh.ProgramID = '<programId>'
         AND wosh.WorkStationID = 15
         AND CONVERT(DATE, wosh.CreateDate) >= '<frmDt>' AND CONVERT(DATE, wosh.CreateDate) <= '<toDt>'
     GROUP BY 
         woh.SerialNo) sub 
ON pt.SerialNo = sub.SerialNo
WHERE 
   
     pt.PartTransactionID =  1--'RO-RECEIVE'
    AND pt.ProgramID = '<programId>'
    AND roha.AttributeName = 'PROCESS_TYPE' 
    AND roha.value = 'REPAIR' 
    AND pt.SerialNo IN (SELECT 
                            DISTINCT woh.SerialNo  -- Use DISTINCT to prevent duplication
                        FROM 
                            PLS.WOHeader woh with (nolock)
                        JOIN 
                            PLS.WOStationHistory wosh with (nolock) ON woh.ID = wosh.WOHeaderID 
                        WHERE 
                            woh.ProgramID = '<programId>'
                            AND wosh.WorkStationID = 15
                            AND CONVERT(DATE, wosh.CreateDate) >= '<frmDt>' AND CONVERT(DATE, wosh.CreateDate) <= '<toDt>')
";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataSet DS = oDAL.GetDataSet(query);

            ///////////FILTER SRINGS/////////

            filterString += " > Program = 'BOSE' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("207-7", query, "REPAIR Received and Tested", false);

            if (!oDAL.HasErrors)
            {

                lstRepairRecvTestedSummary = cCommon.ConvertDtToHashTable(DS.Tables[0]);
                lstRepairRecvTested = cCommon.ConvertDtToHashTable(DS.Tables[1]);
                return true;

            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                oDAL.HasErrors = false;
                return false;
            }

        }

        public bool RepairRecvPack(string programId, string ProgramName, string rptType, string frmDt, string toDate)
        {


            string query = string.Empty;
            query = @"--REPAIR RECEIVED and tested (moved to Gtask4) --date filter on move to Gtask4 - final SQL
select 'Repair' ProcessType , cc.Description Commodity,  count(pt.SerialNo) Value 
from pls.PartTransaction pt with (nolock)
left join pls.ROHeader roh with (nolock) on pt.OrderHeaderID = roh.ID 
left join pls.vROHeaderAttribute roha with (nolock) on roh.ID = roha.ROHeaderID 
left join pls.PartNo pn with (nolock) on pn.PartNo=pt.PartNo 
LEFT JOIN  pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
where 
    pt.PartTransactionID =  1--'RO-RECEIVE'
	and pt.ProgramID = '<programId>' and 
    roha.AttributeName ='PROCESS_TYPE' and 
	roha.value in ('REPAIR') and
    pt.SerialNo IN (select woh.SerialNo FROM PLS.WOHeader woh with (nolock) JOIN PLS.WOStationHistory wosh ON woh.ID = wosh.WOHeaderID WHERE woh.ProgramID = '<programId>' 
	AND wosh.WorkStationID = 16
		AND CONVERT(DATE, wosh.CreateDate) >= '<frmDt>' AND CONVERT(DATE, wosh.CreateDate) <= '<toDt>')
    
group by cc.Description
SELECT pt.ProgramID,
roh.ID,
    roh.CustomerReference, 
    roha.value AS ProcessType, 
    cc.Description AS Commodity, 
    pn.PartNo, 
    pn.Description, 
    pt.SerialNo, 
    pt.CreateDate AS ReceiptDate, 
    sub.CreateDate AS TestDate
FROM 
    pls.PartTransaction pt with (nolock)
LEFT JOIN 
    pls.ROHeader roh with (nolock) ON pt.OrderHeaderID = roh.ID 
LEFT JOIN 
    pls.vROHeaderAttribute roha with (nolock) ON roh.ID = roha.ROHeaderID 
LEFT JOIN 
    pls.PartNo pn with (nolock) ON pn.PartNo = pt.PartNo 
	LEFT JOIN  pls.CodeCommodity cc with (nolock) ON cc.ID = pn.PrimaryCommodityID
LEFT JOIN 
    (SELECT 
         woh.SerialNo, 
         MIN(wosh.CreateDate) AS CreateDate  -- Use MIN to ensure one row per serial number
     FROM 
         PLS.WOHeader woh with (nolock)
     JOIN 
         PLS.WOStationHistory wosh with (nolock) ON woh.ID = wosh.WOHeaderID 
     WHERE 
         woh.ProgramID = '<programId>'
         AND wosh.WorkStationID = 16
         	AND CONVERT(DATE, wosh.CreateDate) >= '<frmDt>' AND CONVERT(DATE, wosh.CreateDate) <= '<toDt>'
     GROUP BY 
         woh.SerialNo) sub 
ON 
    pt.SerialNo = sub.SerialNo
WHERE 
     pt.PartTransactionID =  1 --'RO-RECEIVE'
    AND pt.ProgramID = '<programId>' 
    AND roha.AttributeName = 'PROCESS_TYPE' 
    AND roha.value = 'REPAIR' 
    AND pt.SerialNo IN (
        SELECT DISTINCT woh.SerialNo  -- Use DISTINCT to prevent duplication
        FROM PLS.WOHeader woh with (nolock)
        JOIN PLS.WOStationHistory wosh with (nolock) ON woh.ID = wosh.WOHeaderID 
        WHERE woh.ProgramID = '<programId>'
        AND wosh.WorkStationID = 16
         	AND CONVERT(DATE, wosh.CreateDate) >= '<frmDt>' AND CONVERT(DATE, wosh.CreateDate) <= '<toDt>'
    );
";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataSet DS = oDAL.GetDataSet(query);

            ///////////FILTER SRINGS/////////

            filterString += " > Program = 'BOSE' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("207-8", query, "REPAIR Received and Packed", false);

            if (!oDAL.HasErrors)
            {

                lstRepairRecvPackSummary = cCommon.ConvertDtToHashTable(DS.Tables[0]);
                lstRepairRecvPack = cCommon.ConvertDtToHashTable(DS.Tables[1]);
                return true;

            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                oDAL.HasErrors = false;
                return false;
            }

        }
        #endregion
        #region excel methods

        #endregion
    }
    public class ExcelReportGenerator
    {
        // This method generates an Excel file from the detail data in the model
        public void GenerateExcelFromModel(ReceiptAndCreditREFURB oReceiptAndCreditREFURB, Stream stream)
        {
            using (var workbook = new XLWorkbook())
            {
                // Define preferred order of columns
                List<string> preferredOrder = new List<string>
        {
            "CustomerReference",
            "ProcessType",
            "Commodity",
            "PartNo",
            "Description",
            "SerialNo",
            "ReceiptDate"
        };

                // Define report titles for each set of detail data
                List<(string detailTitle, List<Hashtable> detailData)> reports = new List<(string, List<Hashtable>)>
        {
            ("RefurbRecvCreditingDetail", oReceiptAndCreditREFURB.lstRefurbRecvCrediting),
            ("RefurbNewRepackDetail", oReceiptAndCreditREFURB.lstRefurbNewRepack),
            ("RefurbProcessFGIDetail", oReceiptAndCreditREFURB.lstRefurbProcessFGI),
            ("RefurbRecvScrapDetail", oReceiptAndCreditREFURB.lstRefurbRecvScrap),
            ("RepairRecvCreditDetail", oReceiptAndCreditREFURB.lstRepairRecvCredit),
            ("RepairProcessFGIDetail", oReceiptAndCreditREFURB.lstRepairProcessFGI),
            ("RepairRecvTestedDetail", oReceiptAndCreditREFURB.lstRepairRecvTested),
            ("RepairRecvPackDetail", oReceiptAndCreditREFURB.lstRepairRecvPack)
        };

                foreach (var report in reports)
                {
                    // Convert list of Hashtable to DataTable using the preferred order
                    DataTable dataTable = ConvertToDataTable(report.detailData, preferredOrder);

                    // Create a worksheet for each report's detail data
                    string sheetName = report.detailTitle;
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    // Write DataTable to the sheet
                    WriteDataToSheet(worksheet, dataTable, report.detailTitle);
                }

                // Save the workbook to the specified stream
                workbook.SaveAs(stream);
            }
        }

        private void WriteDataToSheet(IXLWorksheet worksheet, DataTable dataTable, string title)
        {
            // Write column headers from DataTable
            for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
            {
                worksheet.Cell(1, colIndex + 1).Value = dataTable.Columns[colIndex].ColumnName;
                worksheet.Cell(1, colIndex + 1).Style.Font.Bold = true;
            }

            // Write data rows if there are any
            for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
            {
                DataRow row = dataTable.Rows[rowIndex];
                for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
                {
                    worksheet.Cell(rowIndex + 2, colIndex + 1).Value = row[colIndex] ?? string.Empty;
                }
            }

            // Auto-fit column widths
            worksheet.Columns().AdjustToContents();
        }
        private DataTable ConvertToDataTable(List<Hashtable> data, List<string> preferredOrder)
        {
            DataTable dataTable = new DataTable();

            if (data.Count == 0)
            {
                // If there is no data, create columns from preferredOrder and return
                foreach (var column in preferredOrder)
                {
                    dataTable.Columns.Add(column);
                }
                return dataTable;
            }

            // Add preferred columns to the DataTable if they exist in the data
            foreach (var column in preferredOrder)
            {
                if (data[0].ContainsKey(column))
                {
                    dataTable.Columns.Add(column);
                }
            }

            // Add any additional columns not in the preferred order, excluding columns containing "ID"
            var additionalColumns = data[0].Keys.Cast<string>()
                .Where(key => !preferredOrder.Contains(key) && !key.ToLower().Contains("id"));

            foreach (var column in additionalColumns)
            {
                dataTable.Columns.Add(column);
            }

            // Populate the DataTable with data rows
            foreach (var row in data)
            {
                DataRow dataRow = dataTable.NewRow();
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (row.ContainsKey(column.ColumnName))
                    {
                        dataRow[column.ColumnName] = row[column.ColumnName];
                    }
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

    }
}