using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class ERS
    {
        cDAL oDAL = new cDAL("INIT");
        #region Fields

        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstERS { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList()
        {

            string query = string.Empty;

            query = @"
SELECT [PR Number]
      ,[Header Number]
      ,[Originator]
      ,[Contract ID]
      ,[PR Date]
      ,[PR Required Date]
      ,[Bill To Site Name]
      ,[Site Name]
      ,[Request Type]
      ,[MRA Code]
      ,[Condition]
      ,[PR Detail Status]
      ,[Current Queue]
      ,[Repair Status]
      ,[Supply COW]
      ,[Product Code Description]
      ,[Outgoing Unit Part Number]
      ,[Quantity]
      ,[Outgoing Part Num Description]
      ,[Software Config APP ID]
      ,[Software Config Name]
      ,[Software Config No]
      ,[Activation Required]
      ,[Clear Cert]
      ,[Customer Type]
      ,[Debit Key Slot 0]
      ,[Debit Key Slot 1]
      ,[Debit Key Slot 2]
      ,[Deployment System]
      ,[DID]
      ,[DNS1]
      ,[DNS2]
      ,[Encryption Key]
      ,[Encryption Key2]
      ,[Gateway]
      ,[IP Address]
      ,[Lane ID]
      ,[MDK Label]
      ,[MID]
      ,[OS Version]
      ,[Processor Name]
      ,[Test Config Flag]
      ,[VHQ Instance Name]
      ,[Ship to Address]
      ,[Ship to State]
      ,[Ship to City]
      ,[Ship to Zip]
      ,[Reported Problem]
      ,[ISSUE?]
      ,[CASE]
      ,[CASE STATUS]
      ,[CURRENT_QUEUE]
  FROM [PlusRS].[rpt].[ERS]
";



            //if (programId != "0" && programId != null)
            //{
            //    query += " AND ROH.ProgramID = '" + programId + "' ";
            //}
            //else
            //{
            //    query += " AND ROH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            //}


            //if (!string.IsNullOrEmpty(RMARef))
            //{
            //    query += "AND roh.CustomerReference = '<RMARef>'";
            //}

            //query = query.Replace("<RMARef>", RMARef);
            //query = query.Replace("<frmDt>", frmDt);
            //query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);
            //if (!string.IsNullOrEmpty(ProgramName))
            //    filterString += "> Program = '" + ProgramName + "' ";


            //filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            //if (!string.IsNullOrEmpty(RMARef))
            //    filterString += "| Customer Ref. = '" + RMARef + "' ";
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("169", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstERS = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}