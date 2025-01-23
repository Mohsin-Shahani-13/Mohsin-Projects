using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class Reverse
    {
        cDAL oDAL = new cDAL("INIT");
        #region Fields

        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstReverse { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList()
        {

            string query = string.Empty;

            query = @"
SELECT  [Receiving Location]
      ,[Encryption Key]
      ,[Encryption Key2]
      ,[MDK Label]
      ,[Customer Site ID]
      ,[Ship to Site ID]
      ,[VHQ Instance Name]
      ,[Processor Name]
      ,[Customer Type]
      ,[MID]
      ,[Lane ID]
      ,[IP Address]
      ,[Gateway]
      ,[Subnet]
      ,[DNS1]
      ,[DNS2]
      ,[Reported Problem]
      ,[Repair Comment]
      ,[PR Number]
      ,[Software Config No]
      ,[Software Config Name]
      ,[Warranty Type]
      ,[Software Config APP ID]
      ,[Debit Key Slot 0]
      ,[Debit Key Slot 1]
      ,[Debit Key Slot 2]
      ,[OS Version]
      ,[Work Instructions]
      ,[Clear Cert]
      ,[Activation Required]
      ,[Request Type]
      ,[MRA Code]
      ,[Incoming Part Num Description]
      ,[Product Code Description]
      ,[Incoming Unit Part Number]
      ,[Incoming Unit Serial Number]
      ,[Condition]
      ,[Current Queue]
      ,[App ID]
      ,[Notes]
      ,[Date into Repair]
      ,[Ship to COW]
      ,[Originator]
      ,[Bill To Site Name]
      ,[Bill To Site ID]
      ,[Site Name]
      ,[Ship To Site]
      ,[Header Number]
      ,[Note]
      ,[Repair Status]
      ,[Problem Found]
      ,[Problem Summary]
      ,[TID]
      ,[Oracle Customer No.]  AS Oracle_Customer
      ,[InsertDate]
  FROM [PlusRS].[rpt].[Reverse]
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
            oLog.AddSqlQuery("170", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstReverse = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}