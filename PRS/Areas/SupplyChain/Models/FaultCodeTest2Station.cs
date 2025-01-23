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
    public class FaultCodeTest2Station
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Serial No.:")]
        public string SerialNo { get; set; }
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
        public List<Hashtable> lstFaultCodeTest2Station { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string SerialNo, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"

SELECT WOH.ProgramID,
       P.Name,
       WOSH.woheaderid,
       WOH.serialno,
       (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
       FROM pls.partserial PS
       WHERE PS.SerialNo = WOH.SerialNo AND PS.ProgramID = WOH.ProgramID ) HAS_SN,
       Upper(woh.partno)                            AS PartNo,
       --WOSH.ispass,
       CASE WHEN WOSH.IsPass = 1 THEN 'Y' ELSE 'N' END AS ispass,
       WOSH.iteration,
       WOSH.createdate,
       WOSH.lastactivitydate,
       CASE
         WHEN wsd.code IS NULL THEN cws.description
         ELSE wsd.description
       END                                          AS Workstation,
       (SELECT usr.username
        FROM   pls.[user] usr
        WHERE  usr.id = WOSH.userid)                AS stationUser,
       (SELECT usr.username
        FROM   pls.[user] usr
        WHERE  usr.id = WOSA.userid)                AS FaultCodeUser,
       WOSA.value                                   AS FaultCode,
       wosa.lastactivitydate                        AS FaultCodeDate,
       CONVERT(DATE, wosa.lastactivitydate)         AS [Day],
       Format(wosa.lastactivitydate, 'hh:mm:ss tt') AS [Time]
FROM   pls.woheader WOH
       INNER JOIN pls.Program P ON WOH.ProgramID = P.ID
       INNER JOIN pls.wostationhistory WOSH
               ON WOH.id = WOSH.woheaderid
       LEFT JOIN pls.codeworkstation CWS
              ON CWS.id = WOSH.workstationid
       LEFT JOIN pls.codeworkstationcustomdescription wsd
              ON wsd.programid = WOH.programid
                 AND wsd.repairtypeid = WOH.repairtypeid
                 AND wsd.codeworkstationid = WOSH.workstationid
       LEFT JOIN pls.wostationattribute WOSA
              ON WOSA.wostationhistoryid = WOSH.id
              AND WOSA.AttributeID IN (27,29)
WHERE  WOSH.lastactivitydate >= '<frmDt>' AND WOSH.lastactivitydate <= '<toDt>'
       AND WOSH.ispass IS NOT NULL
       AND wosh.workstationid IN (2,12,15)
       AND WOSA.value IS NOT NULL
       
";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            if (!string.IsNullOrEmpty(SerialNo))
                query += "AND WOH.serialno LIKE '%" + SerialNo + "%' ";

            if (programId != "0")
            {
                query += "AND WOH.programid = '" + programId + "' ";
            }
            else
            {
                query += "AND WOH.programid IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            query += "ORDER BY WOSH.lastactivitydate DESC";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            if (!string.IsNullOrEmpty(SerialNo))
                filterString += " | Serial No. Like '" + SerialNo + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("111", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstFaultCodeTest2Station = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}