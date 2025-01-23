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
    public class Rptyield
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Iteration:")]
        public string iteration { get; set; }
        [Display(Name = "Group By WO No.:")]
        public bool ischecked { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstyield { get; set; }
        public List<Hashtable> lstDetail { get; set; }
        #endregion

        #region Methods 
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName, string partNo, string iteration, bool ischecked)
        {
            string query = string.Empty;
            if (ischecked == true)
            {
                query = @"
SELECT  ProgramId
      , PartNo
      , WorkStationId
      , WorkStationCode
	  , WONo
      , Iteration
      , SUM(TotalInsp) AS Inspected
      , SUM(TotalPass) AS Passed
      , SUM(TotalFail) AS Failed
      , SUM(TotalDefects) AS Defected  
     , ((SUM(CAST(TotalPass AS decimal)) / SUM(CAST(TotalInsp AS decimal))) * 100) AS YieldCost
FROM rpt.YieldMst
WHERE ForDate Between '<frmDt>' AND '<toDt>' ";

                if (programId != "0")
                {
                    query += "AND ProgramId = '" + programId + "' ";
                }
                else
                {
                    query += "AND ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }

                if (!string.IsNullOrEmpty(partNo))
                    query += "AND PartNo LIKE '%" + partNo + "%' ";

                if (iteration == "All")
                {
                   
                }

                if (iteration == "1")
                {
                    query += "AND Iteration = '" + iteration + "' ";
                }

                if (iteration == "2")
                {
                    query += "AND Iteration = '" + iteration + "' ";
                }

                if (iteration == "Above 2")
                {
                    query += "AND Iteration > 2 ";
                }
                query += @"GROUP BY ProgramId, PartNo, WorkStationId, WorkStationCode, WONo, Iteration ";
            }

            else
            {
                query = @"
SELECT  ProgramId
      , PartNo
      , WorkStationId
      , WorkStationCode
      , Iteration
      , SUM(TotalInsp) AS Inspected
      , SUM(TotalPass) AS Passed
      , SUM(TotalFail) AS Failed
      , SUM(TotalDefects) AS Defected  
     , ((SUM(CAST(TotalPass AS decimal)) / SUM(CAST(TotalInsp AS decimal))) * 100) AS YieldCost
FROM rpt.YieldMst
WHERE ForDate Between '<frmDt>' AND '<toDt>' ";

                if (programId != "0")
                {
                    query += "AND ProgramId = '" + programId + "' ";
                }
                else
                {
                    query += "AND ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }

                if (!string.IsNullOrEmpty(partNo))
                    query += "AND PartNo LIKE '%" + partNo + "%' ";

                if (iteration == "All")
                {

                }

                if (iteration == "1")
                {
                    query += "AND Iteration = '" + iteration + "' ";
                }

                if (iteration == "2")
                {
                    query += "AND Iteration = '" + iteration + "' ";
                }

                if (iteration == "Above 2")
                {
                    query += "AND Iteration > 2 ";
                }

                query += @"GROUP BY ProgramId, PartNo, WorkStationId, WorkStationCode, Iteration ";
            }

query = query.Replace("<frmDt>", frmDt);
query = query.Replace("<toDt>", toDt);
query = query.Replace("<iteration>", iteration);


            if (!string.IsNullOrEmpty(ProgramName))
                filterString += " Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            if (!string.IsNullOrEmpty(partNo))
                filterString += "| Part No. Like '" + partNo + "' ";

            if (!string.IsNullOrEmpty(iteration))
                filterString += "| Iteration = '" + iteration + "' ";

            if (ischecked == true)
            {
                filterString += "| Group By WO No. = 'Yes' ";
            }

            else
            {
                filterString += "| Group By WO No. = 'No' ";
            }


            oDAL = new cDAL("INIT");


            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("012", query, "", false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstyield = cCommon.ConvertDtToHashTable(dt);
                return true;

            }

        }
        public bool GetDetail(string programId, string frmDate, string toDate, string workStationId, string workStation, string woNo, string partNo, string type, string iteration)
        {
            oDAL = new cDAL("INIT");

            string query = string.Empty;
            query = @"  

SELECT Recnum
     , ProgramId
     , ProgramName
     , WorkStationId
     , WorkStationCode
     , WorkStationDesc
     , WoNo
     , PartNo
     , SerialNo
     , Iteration
     , CASE WHEN IsPass = 1 THEN 'Y' ELSE 'N' END IsPass
     , FaultId
     , FaultCode
     , FaultDesc
     , DefectedBy
     , DefectedOn
FROM   rpt.YieldDtl
WHERE  ForDate Between '<frmDate>' AND '<toDate>' 
";

            if (programId == "0")
            {
                query += "AND ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            else
            {
                query += "AND ProgramId = '" + programId + "' ";
            }

            if (!string.IsNullOrEmpty(workStationId))
                query += "AND WorkStationId = '<workStationId>' ";

            if (!string.IsNullOrEmpty(woNo))
                query += "AND WoNo = '<woNo>' ";

            if (!string.IsNullOrEmpty(partNo))
                query += "AND PartNo = '<partNo>' ";

            if(type == "Pass")
            {
                query += "AND IsPass = 1 ";
            }
            else if (type == "Fail")
            {
                query += "AND IsPass = 0 ";
            }
            else if (type == "Defects")
            {
                query += "AND IsPass = 0 AND FaultId IS NOT NULL ";
            }
            if (iteration == "All")
            {

            }

            if (iteration == "1")
            {
                query += "AND Iteration = '" + iteration + "' ";
            }

            if (iteration == "2")
            {
                query += "AND Iteration = '" + iteration + "' ";
            }

            if (iteration == "Above 2")
            {
                query += "AND Iteration > 2 ";
            }


            query += "ORDER BY  ForDate DESC";

            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDate>", frmDate);
            query = query.Replace("<toDate>", toDate);
            query = query.Replace("<workStationId>", workStationId);
            query = query.Replace("<woNo>", woNo);
            query = query.Replace("<partNo>", partNo);
            query = query.Replace("<iteration>", iteration);


            DataTable dt = oDAL.GetData(query);

           

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstDetail = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }

        #endregion
    }
}