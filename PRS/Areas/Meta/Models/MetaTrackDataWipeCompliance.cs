using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaTrackDataWipeCompliance
    {

        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Model:")]
        public string model { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "Idle:")]
        public string idle { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstMetaDatawipeCompliance { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        cDAL oDAL = new cDAL("INIT");
        //new cDAL("ACTIVE");


        #endregion

        public DataTable Model() 
        {
            string query = string.Empty;
            query = @"select distinct model from meta.rptTrackDataWipe Order by model";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetList(string frmDt, string toDt, bool isAllDate, string faFilter, string bounceFilter, string TestFilter, string LogFilter, string ListFilter, string DataWipeTestPerformed24HrsFilter, string IdleFilter, string DataWipeResult, string Model)
        {

            string query = string.Empty;
            string programId = HttpContext.Current.Session["ProgramIdBySiteForMeta"].ToString();

            string[] idleFilter = IdleFilter.Split('-');

            query = @" 
               SELECT [ProgramID]
      ,[ReceiptSKU]
      ,[ReceiptSerialNo]
      ,[CustRef]
      ,[ReceiptTransactionID]
      ,[DekitTransactionID]
      ,[ReceiptDate]
      ,[DekitDate]
      ,[WOHeaderID]
      ,[WIPStartDate]
      ,[Model]
      ,[CanBeDataWiped]
      ,[ReceiptType]
      ,[WIPE_PASS_DATE]
      ,[WIPE_FAIL_DATE]
      ,[WrkstnMoveToMRB]
      ,[FAFIlter]
      ,[ProblemLogFilter]
      ,[BounceFilter]
      ,[BlacklistFilter]
      ,[LastRunOn]
      ,[DataWipeTestPerformed]
      ,[DataWipeTestPerformed24Hrs]
      ,[DockLogDate]
      ,[RepairType]
      ,[OrderType]
      ,[DockLogToReceiptDelta] AS DecimalDockLogToReceiptDelta
      ,[ReceiveToDataWipeDelta] AS DecimalReceiveToDataWipeDelta
      ,[DecimalWorkingDays]
      ,[CalculatedHours] AS TAT
      ,DataWipeFailOccurrences
      ,UnitFailureCode
      ,CurrentLocation
      ,CurrentWorkstation
      ,DataWipeResult
      ,DataWipeComplete
      ,CurrentMachineName
      ,CurrentResultMessage
  FROM [PlusRS].[meta].[rptTrackDataWipe]
        WHERE 
        ProgramID = <ProgramId>
        
        ";

            filterString += "> Program = 'META'";

            if (isAllDate != true)
            {
                query += " AND CONVERT(Date, DockLogDate) >= '<frmDt>' AND  CONVERT(Date, DockLogDate) <= '<toDt>' ";
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            }


            if (faFilter != "All")
            {
                query += "AND FAFIlter = '" + faFilter + "' ";
                //filterString += " | FAFIlter = '" + faFilter + "' ";
            }
            if (bounceFilter != "All")
            {
                query += "AND BounceFilter = '" + bounceFilter + "' ";
                //filterString += " | BounceFilter = '" + bounceFilter + "' ";
            }
            if (TestFilter != "All")
            {
                query += "AND DataWipeComplete = '" + TestFilter + "' ";
                filterString += " | Data Wipe Complete = '" + TestFilter + "' ";
            }
            if (LogFilter != "All")
            {
                query += "AND ProblemLogFilter = '" + LogFilter + "' ";
                //filterString += " | Problem Log Filter = '" + LogFilter + "' ";
            }
            if (ListFilter != "All")
            {
                query += "AND BlacklistFilter = '" + ListFilter + "' ";
                // filterString += " | Black list Filter = '" + ListFilter + "' ";
            }
            if (DataWipeTestPerformed24HrsFilter != "All")
            {
                query += "AND DataWipeTestPerformed24Hrs = '" + DataWipeTestPerformed24HrsFilter + "' ";
                filterString += " | Data Wipe Test Performed 24Hrs Filter = '" + DataWipeTestPerformed24HrsFilter + "' ";
            }


            if (DataWipeResult != "All")
            {
                query += "AND DataWipeResult = '" + DataWipeResult + "' ";
            }

            if (Model != "All")
            {
                query += " AND Model = '" + Model + "' ";
                filterString += " | Model = '" + Model + "' ";
            }

            if (IdleFilter != "All")
            {
                if (IdleFilter.Contains('-'))
                {
                    query += "AND CalculatedHours between '" + idleFilter[0] + "' AND '" + idleFilter[1] + "' ";
                    filterString += " | Docklog to Current Date TAT = '" + IdleFilter + "' ";
                }
                else
                {
                    query += "AND CalculatedHours > '" + idleFilter[0].Replace(">", "") + "'";
                    filterString += " | Docklog to Current Date TAT = '" + IdleFilter + "'";
                }
            }


            query += " ORDER BY DockLogDate DESC ";

            query = query.Replace("<ProgramId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("181", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaDatawipeCompliance = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}