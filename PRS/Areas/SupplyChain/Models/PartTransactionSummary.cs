using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using Newtonsoft.Json;
using System.Web.Helpers;

namespace IP.Areas.SupplyChain.Models
{
    public class PartTransactionSummary
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region fields

        [Display(Name = "Program:")]
        public string programId { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<ArrayList> lstTran { get; set; }
        public List<ArrayList> lstPartInventory { get; set; }
        public List<ArrayList> lstPartTransaction { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
       
        public string ErrorMessage { get; set; }
        public string DESC = string.Empty;
        #endregion
        #region Methods
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

        public bool GetList(string programId, string partNo, string frmDate, string toDate)
        {
            cDAL oDAL = new cDAL("ACTIVE");
            string DESC = string.Empty;
            DESC = @"
                    SELECT DISTINCT Description
                    FROM pls.partno
                    WHERE PartNo = '<partNo>'";
            DESC = DESC.Replace("<partNo>", partNo);
        
            string query = string.Empty;

            query = @"
---------------1---------------
SELECT  PQ.ProgramID,
        PN.PartNo,
        PN.Description
FROM   pls.partqty PQ 
INNER JOIN pls.partno PN ON PQ.partno = PN.partno 
WHERE  PQ.ProgramID = '<programId>'
       AND PN.PartNo LIKE '%<partNo>%'
       AND CONVERT(Date,PQ.CreateDate) >= '<frmDt>'
       AND CONVERT(Date,PQ.CreateDate) <= '<toDt>'
       group by PQ.ProgramID,PN.PartNo ,
        PN.Description
";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDate);
            query = query.Replace("<toDt>", toDate);
            query = query.Replace("<partNo>", partNo);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(programId))
                filterString += " Program = '" + programId + "' | From = '" + frmDate + "' To = '" + toDate + "' | Part No. Like '" + partNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("142", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstTran = cCommon.ConvertDtToArrayList(dt);

                return true;

            }
        }

        public bool GetDetail(string programId, string partNo, string frmDate, string toDate)
        {
            cDAL oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
-----Party Inventory By Location-----

SELECT  PL.locationno,
        SUM(PQ.availableqty) AS QTY
FROM   pls.partqty PQ 
INNER JOIN pls.partno PN ON PQ.partno = PN.partno 
INNER JOIN pls.partlocation PL ON PL.id = PQ.locationid 
INNER JOIN pls.program P ON P.id = PQ.programid 
INNER JOIN [pls].[CodeConfiguration] CC ON CC.ID = PQ.ConfigurationID
INNER JOIN [pls].[User] U ON U.ID = PQ.UserID  
WHERE  PQ.availableqty > 0
       AND P.ID = '<program>'
       AND PN.partno = '<partNo>'
       AND CONVERT(Date,PQ.CreateDate) >= '<frmDt>'
       AND CONVERT(Date,PQ.CreateDate) <= '<toDt>'
Group by PL.locationno
ORDER BY PL.locationno DESC

 ";
            query = query.Replace("<frmDt>", frmDate.Trim());
            query = query.Replace("<toDt>", toDate);
            query = query.Replace("<program>", programId);
            query = query.Replace("<partNo>", partNo);

            string sql = string.Empty;
            sql = @"
-----Part Transaction-----
SELECT  CPT.Description AS PartTransactionType,
        PT.Location AS FROM_LOCATION,
        PT.ToLocation AS TO_LOCATION,
        PT.Qty,
        U.Username,
         FORMAT( PT.createdate, 'yyyy.MM.dd') AS createdate
FROM   pls.PartTransaction PT
INNER JOIN pls.Program P ON P.ID = PT.ProgramID
INNER JOIN pls.[User] U ON U.ID = PT.UserID
INNER JOIN [pls].[CodePartTransaction] CPT ON CPT.ID = PT.PartTransactionID
LEFT OUTER JOIN pls.partno PN ON PN.partno = PT.partno
LEFT OUTER JOIN pls.CodePartType CT ON CT.Id = PN.PartTypeId 
      WHERE CONVERT(Date, PT.createdate) >= '<frmDt>' AND CONVERT(Date, PT.createdate) <= '<toDt>'
      AND PN.partno = '<partNo>'
      AND P.ID = '<program>'
ORDER BY PT.createdate DESC";

            sql = sql.Replace("<frmDt>", frmDate.Trim());
            sql = sql.Replace("<toDt>", toDate);
            sql = sql.Replace("<partNo>", partNo);
            sql = sql.Replace("<program>", programId);


            DataTable dt = oDAL.GetData(query);
            DataTable dt1 = oDAL.GetData(sql);

            if (!string.IsNullOrEmpty(programId))
                filterString += " Program = '" + programId + "' | From = '" + frmDate + "' To = '" + toDate + "' | Part No. = '" + partNo + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("142-1", query + sql, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                
                if (dt.Rows.Count > 0 && dt1.Rows.Count > 0)
                    lstPartInventory = cCommon.ConvertDtToArrayList(dt);
                lstPartTransaction = cCommon.ConvertDtToArrayList(dt1);
                return true;

            }
        }

        public string GetPartDesc(string programId, string partNo)
        {
            cDAL oDAL = new cDAL("ACTIVE");
            string DESC = string.Empty;
            try
            {
                string part_Desc_query = @"
SELECT Distinct   PN.Description
FROM   pls.partqty PQ 
INNER JOIN pls.partno PN ON PQ.partno = PN.partno 
INNER JOIN pls.program P ON P.id = PQ.programid 
WHERE  PQ.availableqty > 0  AND P.ID = '<program>' AND PN.PartNo = '<part_no>'
             ";
                part_Desc_query = part_Desc_query.Replace("<program>", programId);
                part_Desc_query = part_Desc_query.Replace("<part_no>", partNo);
                DataTable part_Desc = oDAL.GetData(part_Desc_query);
                DESC = part_Desc.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {


            }

            return DESC;
        }
        #endregion
    }
}