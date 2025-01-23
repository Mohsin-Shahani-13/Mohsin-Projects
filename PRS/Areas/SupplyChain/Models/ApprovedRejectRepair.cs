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
    public class ApprovedRejectRepair
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        //[Display(Name = "From:")]
        //public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        //public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        //[Display(Name = "To:")]
        //public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        //public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        //public DataTable GetProgramBySite()
        //{
        //    oDAL = new cDAL("ACTIVE");
        //    string sites = HttpContext.Current.Session["DefaultSite"].ToString();

        //    string query = string.Empty;
        //    query = @"select ID AS programId
        //                     ,NAME AS programName
        //                     FROM pls.PROGRAM  
        //              WHERE SITE = '<site>'
        //              ORDER BY NAME ";
        //    query = query.Replace("<site>", sites);
        //    DataTable dt = oDAL.GetData(query);


        //    return dt;
        //}
        public List<Hashtable> lstApprovedRejectRepair { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string partNo, string serialNo, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");

            string query = string.Empty;
            //DateTime toDate = Convert.ToDateTime(toDt);
            //toDate.ToString(Format.DateOnly);
            //DateTime aaj = toDate.AddDays(1);

            //string to_date = toDate.ToString(Format.DateOnly);

            query = @"
SELECT WOH.ID,
       WOH.ProgramID,
       P.NAME AS ProgramName,
       WOH.customerreference,
       WOH.PartNo,
       WOH.SerialNo,
       PT.reason,
       ROHA.value AS PRStatus,
       CS.description AS Status,
       U.username AS CreatedBy,
       Format(WOH.createdate, 'yyyy.MM.dd HH:mm') AS CreatedOn,
       Format(WOH.lastactivitydate, 'yyyy.MM.dd HH:mm') AS LastActivityOn
FROM   pls.woheader WOH
       INNER JOIN pls.program P
               ON P.id = WOH.programid
       INNER JOIN pls.parttransaction PT
               ON PT.programid = WOH.programid
                  AND PT.orderheaderid = WOH.id
                  AND PT.partno = WOH.partno
                  AND PT.serialno = WOH.serialno
       INNER JOIN pls.partserial PS
               ON PS.programid = WOH.programid
                  AND PS.woheaderid = PT.orderheaderid
                  AND PS.partno = WOH.partno
                  AND PS.serialno = WOH.serialno
       INNER JOIN pls.roheaderattribute ROHA
               ON ROHA.roheaderid = PS.roheaderid
                  AND attributeid = 165
       INNER JOIN pls.codestatus CS
               ON CS.id = WOH.statusid
       INNER JOIN pls.[user] U
               ON U.id = WOH.userid
WHERE  PT.reason = 'On hold - Awaiting Approval'
       AND ROHA.value IN ( 'Rejected - No Repair', 'Rejected - Scrap','Approved Functional','Approval Rejected','Approved', 'Approval Rejected','Service Charge - Approved','Auto-Approved' ) 
 ";

            //query = query.Replace("<frmDt>", frmDt);
            //query = query.Replace("<toDt>", toDt);
            if (!string.IsNullOrEmpty(partNo))
                query += "AND WOH.PartNo LIKE '%" + partNo + "%' ";
            if (!string.IsNullOrEmpty(serialNo))
                query += "AND WOH.SerialNo LIKE '%" + serialNo + "%' ";
            //if (!string.IsNullOrEmpty(orderno))
            //    query += "AND WOH.Id LIKE '%" + orderno + "%' ";

            if (programId != "0" && programId != null)
            {
                query += " AND WOH.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += " AND WOH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            query += "ORDER BY WOH.lastactivitydate";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            //filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. Like '" + partNo + "' ";
            if (!string.IsNullOrEmpty(serialNo))
                filterString += " | Serial No. Like '" + serialNo + "' ";
            //if (!string.IsNullOrEmpty(orderno))
            //    filterString += " | Order No. Like '" + orderno + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("121", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstApprovedRejectRepair = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}