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
    public class PartsUsage
    {
        cDAL oDAL = new cDAL("INIT");
        #region Fields

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        public string type { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }

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
        public List<Hashtable> lstPartsUsage { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string partNo, string fromDt, string toDt, string programId, string ProgramName, string type)
        {
            // oDAL = new cDAL("ACTIVE", "ST"); 
            string query = string.Empty;
            query = @"
                EXEC [usp].[PartUsageCalc]
                     @programId = '<programId>',
                     @startDate = '<frmDt>',
                     @endDate = '<toDt>'";

            if (type == "PartUsageByModel")
            {
                query += @"SELECT  ProgramId,
                                   ProgramName,
                                   PartNo, 
                                   QtyUsed,
                                   QtyProcessed,
                                   UsagePerc,
                                   CompPartNo,
                                   CompPartDesc,
                                   CompPartCost,
                                   (UsageCost / 100) as UsageCost ,
                                   ForDate
                          FROM rpt.PartUsage
                          @WhereClause_PartNo
                          ORDER BY ForDate";
            }
            else
            {
                query += @"SELECT  ProgramId,
                                   ProgramName,
                                   STRING_AGG(PartNo,',') PartNo,
                                   STRING_AGG(PartNo,',') AS CommentTextForExport,
                                   SUM(QtyUsed) as QtyUsed,
                                   SUM(QtyProcessed) as QtyProcessed,
                                   (SUM(QtyUsed) * 100 / SUM(QtyProcessed) ) as UsagePerc,
                                   CompPartNo,
                                   CompPartDesc,
                                   CompPartCost,
                                   (SUM(UsageCost) / 100) as UsageCost ,
                                   ForDate
                          FROM rpt.PartUsage
                          @WhereClause_PartNo
                          GROUP  BY ProgramId,
                                    ProgramName,
                                    CompPartNo,
                                    CompPartDesc,
                                    CompPartCost,
                                    ForDate 
                          ORDER BY ForDate";
            }

            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", fromDt);
            query = query.Replace("<toDt>", toDt);

            if (!string.IsNullOrEmpty(partNo))
                query = query.Replace("@WhereClause_PartNo", "WHERE PartNo LIKE '%" + partNo + "%'");
            else
                query = query.Replace("@WhereClause_PartNo", "");

            DataTable dt = oDAL.GetData(query);

            if (type == "PartUsageByModel")
                filterString += "> By Model ";
            else
                filterString += "> By Overall ";

            filterString += "| Program = '" + ProgramName + "' ";

            filterString += " | From = '" + fromDt + "' To = '" + toDt + "' ";
            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. Like '" + partNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("113", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPartsUsage = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}