using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaWip
    {
        cDAL oDAL = new cDAL("ACTIVE", "INIT");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }

        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "Part No.:")]
        public string PartNo { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstMetaWIP { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE", "INIT");
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

        public bool GetList(string programId, string ProgramName, string PartNo, string frmDt, string toDt)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            if (conType == "PROD")
            {
                query = @" EXEC [usp].[MetaGenerateWIP_PROD]";
            }
            else if (conType == "TRAN")
            {
                query = @" EXEC [usp].[MetaGenerateWIP_TRAN]";
            }
            else if (conType == "TEST")
            {
                query = @" EXEC [usp].[MetaGenerateWIP_TEST]";
            }
            query += @"
                
                     @PROGRAM_ID = '<programId>',
                     @FROM_DATE = '<frmDt>',
                     @TO_DATE = '<toDt>' ";


            query += @"
SELECT [ProgramId]
      ,[ProgramName]
      ,[Region]
      ,[RMANo]
      ,[KITPart]
      ,[KITSerialNo]
      ,[CustomerRef]
      ,[PartNo]
      ,[PartDesc]
      ,[SerialNo]
      ,(SELECT CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
        FROM Pls.partserial PS
        WHERE SerialNo = PS.SerialNo AND PS.ProgramID = ProgramID ) HAS_SN
      ,[WorkStationId]
      ,[WorkStation]
      ,[Warehouse]
      ,[WoId]
      ,[HoldReason]
      ,[IsPass]
      ,[WorkStationHisId]
      ,[FaultCode]
      ,[FaultDesc]
      ,[WOStartDate]
      ,[WOEndDate]
      ,[PrimCommodity]
      ,[SecondCommodity]
      ,[Family]
      ,[QtyReq]
      ,[QtyCons]
      ,[Capacity]
      ,[PairedSerialNo]
      ,[TriageGrade]
      ,[FinalGrade]
      ,[RcvdTimes]
      ,[WOStatus]
      ,[LastRunOn]
  FROM [rpt].[MetaWIP]
WHERE CONVERT(Date, LastRunOn) >= '<frmDt>' 
AND CONVERT(Date, LastRunOn) <= '<toDt>'
 ";
            if (conType == "PROD")
            {
                query += @"AND DbType = 'Prod'";
            }
            else if (conType == "TRAN")
            {
                query += @"AND DbType = 'Tran'";
            }
            else if (conType == "TEST")
            {
                query += @"AND DbType = 'Test'";
            }

            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            //if (!string.IsNullOrEmpty(PartNo))
            //    query += " AND  pn.PartNo like '%" + PartNo + "%' ";



            //query += " WHERE CRT.ID  IN ('<RepairTypeID>') ";

            if (programId != "0" && programId != null)
            {
                query += " AND  ProgramId = '" + programId + "' ";
            }
            else
            {
                query += " AND ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }


            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";
            filterString += " | From = '" + frmDt + "' To = '" + toDt + "'";
            //if (!string.IsNullOrEmpty(PartNo))
            //    filterString += "|  Part No.='" + PartNo + "' ";
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("124", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaWIP = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}