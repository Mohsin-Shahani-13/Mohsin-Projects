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
    public class SummaryRR60
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
      
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Year:")]
        public string year { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);


            return dt;
        }
        public List<Hashtable> lstSummaryRR60 { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        //public DataTable GetYear()
        //{
        //    oDAL = new cDAL("INIT");

        //    string query = string.Empty;
        //    query = @"select DISTINCT RRYear from [rpt].[RR60] ORDER BY RRYear ";
        //    DataTable dt = oDAL.GetData(query);
        //    return dt;
        //}

        public bool GetList(string year, string programId, string programName)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
IF OBJECT_ID('tempdb.dbo.#RRSmry') IS NULL
BEGIN
CREATE TABLe #RRSmry
(
 RRYear int,
 RRMonth int,
 RR60Units Decimal(18,0),
 TotalShipped Decimal(18,0),
 RR60Percent Decimal(18,2)
 )
 END
 -- Inserting Count Year and Month
 INSERT INTO #RRSmry (RRYear, RRMonth, TotalShipped)
SELECT
Year(t.woclose)RRyear,
Month(t.woclose)RRMont,
Count(t.SerialNo)
FROM (SELECT
  pt.programid,
  pt.partno,
  pt.serialno,
  pt.OrderHeaderID,
  pt.CreateDate woclose,
  ISNULL(psh.roheaderid, ps.roheaderid) ro
FROM pls.parttransaction pt
INNER JOIN pls.WOHeader woh ON woh.id = pt.OrderHeaderID  
LEFT JOIN pls.codeworkstationcustomdescription wcd  ON wcd.RepairTypeID = woh.RepairTypeID 
                                  AND wcd.CodeWorkStationID = woh.WorkStationID AND wcd.ProgramID = pt.ProgramID
LEFT JOIN pls.PartSerial ps ON ps.WOHeaderID  = pt.OrderHeaderID AND ps.PartNo= pt.PartNo
  AND ps.ProgramID = pt.ProgramID and ps.SerialNo =pt.SerialNo
LEFT JOIN pls.PartSerialHistory psh
  ON psh.woheaderid = pt.orderheaderid
  AND psh.ProgramID = pt.ProgramID
  AND psh.id = (SELECT
    MIN(id)
  FROM pls.PartSerialHistory pshck
  WHERE psh.WOHeaderID = pshck.woheaderid
  AND pshck.ProgramID = psh.ProgramID)
WHERE pt.ProgramID = '<programId>'
AND pt.PartTransactionID = 7
--AND pt.ProgramID = '<programId>'
--AND CONVERT(Date, pt.ForDate) >= '2022.12.01' AND CONVERT(Date, pt.ForDate) <= '2022.12.31' 
--AND pt.formonth IN ()
--AND pt.foryear = 2022
AND pt.id = (SELECT
  MAX(ck.id)
FROM pls.PartTransaction ck
WHERE ck.OrderHeaderID = pt.OrderHeaderID
AND ck.PartTransactionID = 7
AND ck.ProgramID = pt.ProgramID
AND ck.ForMonth = pt.ForMonth)
AND NOT EXISTS (SELECT
  rop.id
FROM pls.parttransaction rop
WHERE rop.id > pt.id AND pt.orderheaderid = rop.orderheaderid
AND rop.PartTransactionID = 9 AND rop.ProgramID = pt.ProgramID)) t
INNER JOIN pls.ROHeader roh
  ON roh.id = t.ro
  AND roh.ProgramID = t.programid
INNER JOIN pls.ROLine rol
  ON rol.ROHeaderID = roh.ID
  AND rol.PartNo = t.PartNo
INNER JOIN pls.ROUnit rou
  ON rou.ROLineID = rol.ID
  AND rou.SerialNo = t.SerialNo
GROUP BY   year(t.woclose), Month(t.woclose)

  -- Get Count Return unit Count 
 UPDATE rr SET rr.RR60Units =
     (
     SELECT  Count(*) from plusrs.rpt.RepeatReturn 
     where Under60 =1 AND ProgramId = '<programId>'  AND year(ShippedOn) = RR.RRYear and
         Month(shippedon) = RR.RRMonth  
     )FROM #RRSmry RR
       
       UPDATE #RRSmry SET RR60Percent = (RR60Units*100/TotalShipped)
    SELECT CONCAT(LEFT(DateName( month , DateAdd( month ,RRMonth , 0 ) - 3 ),3),' ',RRYear) AS 'Month', RR60Units, TotalShipped, RR60Percent AS 'RR60_cost'
       FROM #RRSmry 
       WHERE RRYear = '<year>'
       ORDER BY RRMonth
    DROP TABLE #RRSmry
  


 ";


            query = query.Replace("<programId>", programId);
            query = query.Replace("<year>", year);
           

          
            //if (programId != "0")
            //{
            //    query += "AND ProgramId = '" + programId + "' ";
            //}
            //else
            //{
            //    query += "AND ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            //}

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            filterString += "| Year = '" + year + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("150", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSummaryRR60 = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}