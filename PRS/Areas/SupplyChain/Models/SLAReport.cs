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
    public class SLAReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

        [Display(Name = "From:")]
        public string _frmDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string frmDt { get { return _frmDt; } set { _frmDt = value; } }

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

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Customer Ref.:")]
        public string CustomerReference { get; set; }
        [Display(Name = "Part No.:")]
        public string PartNo { get; set; }
        [Display(Name = "Status:")]
        public string statusId { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Status:")]
        public string description { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Status:")]
        public string Status { get; set; }
        public List<ArrayList> lstStatus { get; set; }
        public List<Hashtable> lstSLAReport { get; set; }

        public List<object> lstMst = new List<object>();

        #endregion
        #region Methods 
        //public DataTable Status()
        //{
        //    string query = string.Empty;
        //    query = @"SELECT ID,Description FROM pls.CodeStatus
        //                WHERE ID IN ( 15, 17, 19, 28 )";
        //    DataTable dt = oDAL.GetData(query);
        //    return dt;
        //}

        public bool GetStatus()
        {
            string query = string.Empty;
            query = @"SELECT ID,Description FROM pls.CodeStatus
                        WHERE ID IN ( 15, 17, 19, 28 )";
            DataTable dt = oDAL.GetData(query);
            lstStatus = cCommon.ConvertDtToArrayList(dt);
            if (!oDAL.HasErrors)
                return true;
            else
                return false;
        }

        public bool GetList(string frmDt, string toDt, string CustomerReference, string PartNo, string programId, string ProgramName, string statusId, string Status)
        {
            // oDAL = new cDAL("ACTIVE", "ST");

          

                string query = string.Empty;
                query = @" 

      SELECT ProgramId,
       ProgramName,
       WOHeaderID,
       SerialNo,
       (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
		FROM pls.partserial PS
		WHERE PS.SerialNo = SerialNo AND PS.ProgramID = ProgramID ) HAS_SN,
       CustomerReference,
       ThirdPartyReference,
       PartNo,
       PartDesc,
       WorkStationId,
       WorkStation,
       HoldReason,
       StatusId,
       Status,
       UserId,
       UserName,
       FaultId,
       FaultCode,
       FaultDesc,
       CreateDate,
       LastActivityDate,
       DaysAtLoc,
       DaysInWIP,
       WorkingDaysInWip,
       DaysInHold,
       WorkingDaysInHold,
       CurrentAging,
       TargetTAT,
       LastRunOn
FROM   plusrs.rpt.sla
WHERE ProgramId = <ProgramId> AND CONVERT(Date, lastactivitydate) >= '<frmDt>' AND CONVERT(Date, lastactivitydate) <= '<toDt>'
 ";

                query = query.Replace("<ProgramId>", programId);
                query = query.Replace("<frmDt>", frmDt);
                query = query.Replace("<toDt>", toDt);


             

                if (!string.IsNullOrEmpty(CustomerReference))
                    query += "AND customerreference LIKE '%" + CustomerReference + "%' ";

                if (!string.IsNullOrEmpty(PartNo))
                    query += "AND partno LIKE '%" + PartNo + "%' ";

                //if (Status != "All")
                //{
                //    query += "AND status = '" + Status + "' ";
                //}

                if (!Status.Equals("All"))
                    query += "AND statusid  IN (" + statusId + ") ";

                query += "ORDER BY lastactivitydate DESC";

                DataTable dt = oDAL.GetData(query);

                //if (!string.IsNullOrEmpty(ProgramName))
                //    filterString += " > Program = '" + ProgramName + "' | From = '" + frmDt + "' To = '" + toDt + "' ";

                if (!string.IsNullOrEmpty(ProgramName))
                    filterString += "> Program = '" + ProgramName + "' ";

                filterString += "| From = '" + frmDt + "' To = '" + toDt + "' ";

                if (!string.IsNullOrEmpty(CustomerReference))
                    filterString += "| Customer Ref. = '" + CustomerReference + "' ";

                if (!string.IsNullOrEmpty(PartNo))
                    filterString += "| Part No. = '" + PartNo + "' ";

                if (!string.IsNullOrEmpty(Status))
                    filterString += "| Status = '" + Status + "'";




                //For SQL Documentation
                cLog oLog = new cLog();
                oLog.AddSqlQuery("109", query, string.Empty, false);

                if (oDAL.HasErrors)
                {
                    ErrorMessage = oDAL.ErrMessage;
                    return false;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                        lstSLAReport = cCommon.ConvertDtToHashTable(dt);
                    return true;

                }
            }
            #endregion
        }
}
