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
    public class DailyProductionsCopy 
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Workstation:")]
        public string Workstation { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialno { get; set; }
        [Display(Name = "Parent Serial No.:")]
        public string parentserialno { get; set; }
        [Display(Name = "Status:")]
        public string status { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Location No.:")]
        public string locationNo { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "WOHeader ID:")]
        public string WOHeaderId { get; set; }
        [Display(Name = "RO No.:")]
        public string ROHcustomerreference { get; set; }
        [Display(Name = "RO Date:")]
        public string rodate { get; set; }
        [Display(Name = "WO No.:")]
        public string WOHNo { get; set; }
        [Display(Name = "WO Station:")]
        public string station { get; set; }
        [Display(Name = "WO Strat Date:")]
        public string wostartdate { get; set; }
        [Display(Name = "WO End Date:")]
        public string woenddate { get; set; }
        [Display(Name = "Discription:")]
        public string description { get; set; }
        [Display(Name = "SO No.:")]
        public string SONo { get; set; }
        [Display(Name = "SO Date:")]
        public string sodate { get; set; }
        public string SerialNo { get; private set; }
        public string ROId { get; set; }
        public string WOId { get; set; }
        public string SOId { get; set; }
        public bool isPass { get; set; }
        public bool isFail { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstpartsSerial { get; set; }
        public List<Hashtable> lstTransDetail { get; set; }
        public List<Hashtable> lstDetail { get; set; }
     

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
        public DataTable GetIDBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select distinct top 10 ID
                             FROM pls.WOHeader ";
            DataTable dtId = oDAL.GetData(query);
            return dtId;
        }

        public List<Hashtable> lstDailyProductionsCopy { get; set; }

        public List<object> lstMst = new List<object>();
        private object serialNo;
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string Workstation, string serialno, string WOHeaderId, string programId, string ProgramName, bool isPass, bool isFail)
        {
            // oDAL = new cDAL("ACTIVE", "ST");

            string query = string.Empty;
            //DateTime toDate = Convert.ToDateTime(toDt);
            //toDate.ToString(Format.DateOnly);
            //DateTime aaj = toDate.AddDays(1);

            //string to_date = toDate.ToString(Format.DateOnly);

            query = @"



SELECT WOH.ProgramID,
WOSH.woheaderid,
WOH.serialno,
(SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
FROM pls.partserial PS
WHERE PS.SerialNo = woh.SerialNo AND PS.ProgramID = woh.ProgramID ) HAS_SN,
CASE WHEN WOSH.IsPass = 1 THEN 'Y' ELSE 'N' END AS Ispass,
WOSH.iteration,
WOSH.createdate,
WOSH.lastactivitydate,
CASE
WHEN wsd.code IS NULL THEN cws.description
ELSE wsd.description
END AS Workstation,
(
select CASE WOH.RepairTypeID WHEN 42 THEN '1' WHEN 71 THEN '3' ELSE MAX(pna.Value) END
from pls.woline wl
LEFT JOIN pls.PartNoAttribute pna ON WOH.ProgramID = pna.ProgramID and wl.ComponentPartNo = pna.PartNo and pna.AttributeID = 149
where wl.WOHeaderID = WOH.ID and wl.StatusID = 14
) as RepairLevel,
(SELECT usr.username
FROM pls.[user] usr
WHERE usr.id = (SELECT wsh.userid
FROM pls.wostationhistory wsh
WHERE wsh.toworkstationid = wosh.workstationid
AND wsh.woheaderid = WOSH.woheaderid
AND wsh.lastactivitydate = WOSH.createdate)) AS UserName,
U.username AS Technician,
CONVERT(DATE, WOSH.lastactivitydate) AS [Day],
Format(WOSH.lastactivitydate, 'hh:mm:ss tt') AS [Time],
CASE
WHEN ( Cast(WOSH.lastactivitydate AS TIME) >= '06:00:00' )
AND ( Cast(WOSH.lastactivitydate AS TIME) <= '15:30:00' ) THEN 1
ELSE 2
END AS [Shift],
Upper(woh.partno) AS Model,
(SELECT value
FROM pls.partnoattribute
WHERE programid = WOH.programid
AND partno = woh.partno
AND attributeid = 278) AS Family,
(SELECT value
FROM pls.partnoattribute
WHERE programid = WOH.programid
AND partno = woh.partno
AND attributeid = 279) AS Technology,
Datepart(hour, WOSH.lastactivitydate) AS [Hour],
Concat(WOSH.woheaderid, WOH.serialno) AS [RMA/SN]
FROM pls.woheader WOH
INNER JOIN pls.wostationhistory WOSH
ON WOH.id = WOSH.woheaderid
LEFT JOIN pls.codeworkstation CWS
ON CWS.id = WOSH.workstationid
AND cws.passfail = 1
LEFT JOIN pls.codeworkstationcustomdescription wsd
ON wsd.programid = WOH.programid
AND wsd.repairtypeid = WOH.repairtypeid
AND wsd.codeworkstationid = WOSH.workstationid
INNER JOIN pls.[User] U ON U.ID = WOSH.UserID
WHERE WOSH.ispass IS NOT NULL
";
            //query = query.Replace("serialNo", serialNo);
            //query = query.Replace("programId", programId);
            //query = query.Replace("PartNo", PartNo);

            filterString = "serial No. = '" + serialNo + "' ";


            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            if (!string.IsNullOrEmpty(Workstation))
                query += "AND wsd.Description LIKE '%" + Workstation + "%' ";

            if (!string.IsNullOrEmpty(serialno))
                query += "AND WOH.serialno LIKE '%" + serialno + "%' ";

            if (!string.IsNullOrEmpty(WOHeaderId))
                query += "AND WOSH.WOHeaderId LIKE '%" + WOHeaderId + "%' ";

            if (programId != "0")
            {
                query += "AND WOH.programid = '" + programId + "' ";
            }
            else
            {
                query += "AND WOH.programid IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            if (isPass == true)
            {
                query += "AND WOSH.ispass = 1";
            }
            else
            if (isPass == false)
            {
                query += "AND WOSH.ispass = 0";
            }

            query += "ORDER BY WOSH.lastactivitydate";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            if (!string.IsNullOrEmpty(Workstation))
                filterString += " | Workstation Like '" + Workstation + "' ";

            if (!string.IsNullOrEmpty(serialno))
                filterString += " | serialno '" + serialno + "' ";

            if (!string.IsNullOrEmpty(WOHeaderId))
                filterString += "> WOHeaderId = '" + WOHeaderId + "' ";
            if (isPass == true)
                filterString += "checked";
            if (isFail == true)
                filterString += "checked";
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("131", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDailyProductionsCopy = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
        public bool GetDetail(string serialNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");

            string query = string.Empty;
           

            query = @"



SELECT WOH.ProgramID,
WOSH.woheaderid,
WOH.serialno,
(SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
FROM pls.partserial PS
WHERE PS.SerialNo = woh.SerialNo AND PS.ProgramID = woh.ProgramID ) HAS_SN,
CASE WHEN WOSH.IsPass = 1 THEN 'Y' ELSE 'N' END AS Ispass,
WOSH.iteration,
WOSH.createdate,
WOSH.lastactivitydate,
CASE
WHEN wsd.code IS NULL THEN cws.description
ELSE wsd.description
END AS Workstation,
(
select CASE WOH.RepairTypeID WHEN 42 THEN '1' WHEN 71 THEN '3' ELSE MAX(pna.Value) END
from pls.woline wl
LEFT JOIN pls.PartNoAttribute pna ON WOH.ProgramID = pna.ProgramID and wl.ComponentPartNo = pna.PartNo and pna.AttributeID = 149
where wl.WOHeaderID = WOH.ID and wl.StatusID = 14
) as RepairLevel,
(SELECT usr.username
FROM pls.[user] usr
WHERE usr.id = (SELECT wsh.userid
FROM pls.wostationhistory wsh
WHERE wsh.toworkstationid = wosh.workstationid
AND wsh.woheaderid = WOSH.woheaderid
AND wsh.lastactivitydate = WOSH.createdate)) AS UserName,
U.username AS Technician,
CONVERT(DATE, WOSH.lastactivitydate) AS [Day],
Format(WOSH.lastactivitydate, 'hh:mm:ss tt') AS [Time],
CASE
WHEN ( Cast(WOSH.lastactivitydate AS TIME) >= '06:00:00' )
AND ( Cast(WOSH.lastactivitydate AS TIME) <= '15:30:00' ) THEN 1
ELSE 2
END AS [Shift],
Upper(woh.partno) AS Model,
(SELECT value
FROM pls.partnoattribute
WHERE programid = WOH.programid
AND partno = woh.partno
AND attributeid = 278) AS Family,
(SELECT value
FROM pls.partnoattribute
WHERE programid = WOH.programid
AND partno = woh.partno
AND attributeid = 279) AS Technology,
Datepart(hour, WOSH.lastactivitydate) AS [Hour],
Concat(WOSH.woheaderid, WOH.serialno) AS [RMA/SN]
FROM pls.woheader WOH
INNER JOIN pls.wostationhistory WOSH
ON WOH.id = WOSH.woheaderid
LEFT JOIN pls.codeworkstation CWS
ON CWS.id = WOSH.workstationid
AND cws.passfail = 1
LEFT JOIN pls.codeworkstationcustomdescription wsd
ON wsd.programid = WOH.programid
AND wsd.repairtypeid = WOH.repairtypeid
AND wsd.codeworkstationid = WOSH.workstationid
INNER JOIN pls.[User] U ON U.ID = WOSH.UserID
WHERE WOSH.ispass IS NOT NULL
";

            filterString = "serial No. = '" + serialNo + "' ";


           
            if (!string.IsNullOrEmpty(serialNo))
                query += "AND WOH.serialNo LIKE '%" + serialNo + "%' ";

            query += "ORDER BY WOSH.lastactivitydate";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(serialno))
                filterString += " | serialno '" + serialno + "' ";

            if (!string.IsNullOrEmpty(Workstation))
                filterString += " | Workstation Like '" + Workstation + "' ";

            if (!string.IsNullOrEmpty(WOHeaderId))
                filterString += "> WOHeaderId = '" + WOHeaderId + "' ";
            if (isPass == true)
                filterString += "checked";
            if (isFail == true)
                filterString += "checked";
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("131", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDailyProductionsCopy = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        
    }
}