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
    public class DiscrepancyReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        [Display(Name = "Customer Reference:")]
        public string custRef { get; set; }

        [Display(Name = "Status:")]
        public string status { get; set; }

        [Display(Name = "Type:")]
        public string type { get; set; }

        [Display(Name = "Assigned To:")]
        public string assignedTo { get; set; }
        public bool OpenWO { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<ArrayList> lstStatus { get; set; }
        public List<ArrayList> lstType { get; set; }
        public List<Hashtable> lstDiscrepancyReport { get; set; }
        #endregion
        #region Methods 

        //        public DataTable Getstatus(string programId, string ProgramName) // onHand warehouse method
        //        {
        //            string query = string.Empty;
        //            query = @"select Distinct CS.Description AS Status from pls.CaseMgt CM
        //INNER JOIN pls.CodeStatus CS ON CS.ID = CM.StatusID ";
        //            if (programId != "0" && programId != null)
        //            {
        //                query += " WHERE CM.ProgramID = '" + programId + "' ";
        //            }
        //            else
        //            {
        //                query += " WHERE CM.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
        //            }
        //            query += "ORDER BY CS.Description ";

        //            DataTable dt = oDAL.GetData(query);
        //            return dt;
        //        }

        public bool Status(string programId, string ProgramName)
        {
            string query = string.Empty;
            query = @"select DISTINCT CS.Description AS Status from pls.CaseMgt CM
INNER JOIN pls.CodeStatus CS ON CS.ID = CM.StatusID ";
            if (programId != "0" && programId != null)
            {
                query += " WHERE CM.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += " WHERE CM.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += "ORDER BY CS.Description ";

            DataTable dt = oDAL.GetData(query);
            lstStatus = cCommon.ConvertDtToArrayList(dt);
            if (!oDAL.HasErrors)
                return true;
            else
                return false;

            //return dt;
        }


        //public DataTable GetType(string programId, string ProgramName) // onHand warehouse method
        //{
        //    string query = string.Empty;
        //    query = @"select Distinct Type from pls.CaseMgt  ";
        //    if (programId != "0" && programId != null)
        //    {
        //        query += " WHERE ProgramID = '" + programId + "' ";
        //    }
        //    else
        //    {
        //        query += " WHERE ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
        //    }
        //    query += "ORDER BY Type ";

        //    DataTable dt = oDAL.GetData(query);
        //    return dt;
        //}

        public bool Type(string programId, string ProgramName)
        {
            string query = string.Empty;
            query = @"select Distinct Type from pls.CaseMgt ";

            if (programId != "0" && programId != null)
            {
                query += " WHERE ProgramID = '" + programId + "' ";
            }
            else
            {
                query += " WHERE ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += "ORDER BY Type ";

            DataTable dt = oDAL.GetData(query);
            lstType = cCommon.ConvertDtToArrayList(dt);
            if (!oDAL.HasErrors)
                return true;
            else
                return false;

            //return dt;
        }
        public DataTable GetAssignedTo(string programId, string ProgramName) // onHand warehouse method
        {
            string query = string.Empty;
            query = @"select Distinct U.Username as AssignedTo from pls.CaseMgt CM
INNER JOIN pls.[User] U on U.ID = CM.AssignedToUserID ";
            if (programId != "0" && programId != null)
            {
                query += " WHERE CM.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += " WHERE CM.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += "ORDER BY U.Username ";

            DataTable dt = oDAL.GetData(query);
            return dt;
        }


        public bool GetList(string programId, string ProgramName, string frmDt, string toDate, string custRef, string status, string type, string assignedTo, bool OpenWO)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT
c.ProgramID,
c.id Case_ID, 
c.Type, 
c.Subject, 
c.Description, 
c.CustomerReference, 
c.OrderType, 
c.OrderHeaderID,
c.PartNo, 
c.SerialNo, 
c.ReasonDesc Reason, 
c.StatusDescription AS Status,
c.Priority, 
c.AssignedToUser Assigned_to,
(select a.value from[pls].[vCaseMgtAttribute] a where a.CaseMgtID = c.id and a.AttributeName = 'OWNER') AS OWNER,
c.Username Created_By,
(select a.value from[pls].[vCaseMgtAttribute] a where a.CaseMgtID = c.id and a.AttributeName = 'TRACKING NUMBER') TRACKING_NUMBER,
(select a.value from[pls].[vCaseMgtAttribute] a where a.CaseMgtID = c.id and a.AttributeName = 'COUNTRY') COUNTRY,
(select a.value from[pls].[vCaseMgtAttribute] a where a.CaseMgtID = c.id and a.AttributeName = 'CUSTOMER NAME') CUSTOMER_NAME,
(select a.value from[pls].[vCaseMgtAttribute] a where a.CaseMgtID = c.id and a.AttributeName = 'LABEL REFERENCE #2') LABEL_REFERENCE_2,
(select a.value from[pls].[vCaseMgtAttribute] a where a.CaseMgtID = c.id and a.AttributeName = 'NEW ORDER NO.') NEW_ORDER_NO,
(select a.value from[pls].[vCaseMgtAttribute] a where a.CaseMgtID = c.id and a.AttributeName = 'DISPOSITION') DISPOSITION,
c.CreateDate,
c.LastActivityDate
FROM [pls].[vCaseMgt] c
";

            if (programId != "0")
            {
                query += "WHERE c.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "WHERE c.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            if (!OpenWO)
                query += "AND convert(Date,c.CreateDate) >= '<frmDt>' AND convert(Date,c.CreateDate) <= '<toDate>'";

            if (!string.IsNullOrEmpty(custRef))
                query += "AND c.CustomerReference LIKE '%" + custRef + "%' ";


            if (!status.Equals("All"))
            {
                query += "AND c.StatusDescription IN (" + status + ") ";
            }

            if (!type.Equals("All"))
            {
                query += "AND c.Type IN (" + type + ") ";
            }



            if (!assignedTo.Equals("All") && !assignedTo.Equals("No Data Available"))
                query += "AND c.AssignedToUser ='" + assignedTo + "' ";

            if (OpenWO)
                query += "AND c.StatusDescription NOT IN ('CLOSED', 'CANCELED') ";

            query += " ORDER BY StatusDescription desc, CreateDate desc";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDate>", toDate);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString = "> Program = '" + ProgramName + "' ";

            if (!OpenWO)
                filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";

            if (!string.IsNullOrEmpty(custRef))
                filterString += " | Customer Reference No. Like '" + custRef + "' ";

            //if (!status.Equals("All"))
            //    filterString += " | Status = '" + status + "' ";

            //if (!type.Equals("All"))
            //    filterString += " | Type = '" + type + "' ";

            if (!assignedTo.Equals("All") && !assignedTo.Equals("No Data Available"))
                filterString += " | Assigned To User = '" + assignedTo + "' ";
            if (OpenWO)
                filterString += " | Open Work Orders ";
            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("202", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDiscrepancyReport = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}