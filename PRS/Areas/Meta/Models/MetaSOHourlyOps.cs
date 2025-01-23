using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaSOHourlyOps
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }


        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
       
        public string program_Id { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }


        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstMetaSO { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        public string ProgramBySite { get; set; }
        public string ProgramNameforSite { get; set; }
        #endregion

        public string GetProgramBysite(string site)
        {

            oDAL = new cDAL("ACTIVE");
            string query = "SELECT Id  FROM pls.Program WHERE  NAME ='META' AND Site = '<Site>'";
            query = query.Replace("<Site>", site);
            DataTable dt = oDAL.GetData(query);

            var ProgramNameList = (from p in dt.AsEnumerable()
                                   select p.Field<object>("Id")).ToList().Distinct();
            ProgramNameforSite = String.Join("','", ProgramNameList).Insert(0, "'").Insert(String.Join("','", ProgramNameList).Insert(0, "'").Length, "'");
            return ProgramNameforSite;


        }
     

        public bool GetList(string programId, string ProgramName, string serialNo, string frmDt, string toDt, bool ischecked )
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string progid = "";
            progid = GetProgramBysite(sites);
          
            cDAL oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"

SELECT  [WshId]
      ,[ProgramId]
      ,[ProgramName]
      ,[Region]
      ,[Warehouse]
      ,[ReferenceNo]
      ,[CustomerOrderType]
      ,[WorkTypeID]
      ,[ServiceRequestNo]
      ,[OrderNo]
      ,[PartNo]
      ,[Description]
      ,[TypeDesignation]
      ,[WOUnitId]
      ,[SerialNo]
      ,[DateEntered]
      ,[State]
      ,FORMAT(CONVERT(datetime, CloseDate), 'yyyy.MM.dd hh:mm:ss') AS CloseDate
      ,[QuotationResult]
      ,[OperationNo]
      ,[OperationStatusCode]
      ,[WorkCenterNo]
      ,[RepairTypeId]
      ,[WCID]
      ,[WCCode]
      ,[WCDescription]
      ,[Iteration]
      ,[NoteText]
      ,[EmpNo]
      ,[StationName]
      ,[UserId]
      ,[TestResult]
      ,[RoutResult]
      ,[OverallResult]
      ,[ErrItem]
      ,[ErrItemDescription]
      ,[SOFaultCode]
      ,[SOFaultDescription]
      ,[FCM1]
      ,[FCM2]
      ,[FCM3]
      ,[FCM4]
      ,[FCM5]
      ,[FCT1]
      ,[FCT2]
      ,[FCT3]
      ,[FCT4]
      ,[FCT5]
      ,[TriageGrading]
      ,[CosmeticGrading]
      ,[FinalGrading]
      ,[SODated]
      ,[RowVersion]
      ,[NextWC]
      ,[DefWC]
      ,[StatusID]
      ,[LastRunOn]
      ,[StartTime]
      ,[EndTime]
      ,[ROHeaderId]
      ,[Username]
      ,[plsStartTime]
      ,[plsEndTime]
      ,[plsDaysDiff]
      ,[Source]
  FROM [PlusRS].[meta].[rptYield]
 Where   ProgramId = " + progid + " ";
         
            if (!string.IsNullOrEmpty(serialNo) && ischecked ==true )
            {
                query += "AND SerialNo LIKE '%" + serialNo + "%' ";
                //filterString += "> Program = '" + ProgramName + "' ";
               
                //filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";
                filterString += " > Serial No. Like  '" + serialNo + "' ";
            }
            else if (string.IsNullOrEmpty(serialNo) && ischecked == false)
            {
                //filterString += "> Program = '" + ProgramName + "' ";
                query += "AND CONVERT(Date, RowVersion) >= '<frmDt>' AND CONVERT(Date, RowVersion) <= '<toDt>' ";
                filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";
            }

            else if (!string.IsNullOrEmpty(serialNo) && ischecked == false)
            {
                query += "AND SerialNo LIKE '%" + serialNo + "%' ";

                filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";
                filterString += " | Serial No. Like  '" + serialNo + "' ";
            }

            query += "ORDER BY RowVersion DESC ";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);
           
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("126", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaSO = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}