using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class WIPOutline
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string frmDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Customer Ref.:")]
        public string custRef { get; set; }
        [Display(Name = "Part No.:")]
        public string PartNo { get; set; }
        [Display(Name = "Program:")]
        public string ProgramName { get; set; }
        [Display(Name = "Program:")]
        public string ProgramID { get; set; }
        [Display(Name = "Third Party Ref.:")]
        public string thirdPartyReference { get; set; }
        [Display(Name = "Order Type:")]
        public string orderType { get; set; }
        [Display(Name = "Return Reason:")]
        public string returnReason { get; set; }
        [Display(Name = "Created On:")]
        public string createDate { get; set; }
        [Display(Name = "Last Activity On:")]
        public string lastActivityDate { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Status:")]
        public string description { get; set; }
        [Display(Name = "Way Bill:")]
        public string wayBill { get; set; }
        [Display(Name = "Address:")]
        public string address { get; set; }
        [Display(Name = "Created By:")]
        public string username { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstWIPOutline { get; set; }

        public List<object> lstMst = new List<object>();
        public string ErrorMessage { get; set; }

        #endregion

        #region Methods 
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

        public bool GetList(/*string frmDt, string toDt,*/ string custRef, string ProgramId, string ProgramName, string partNo)
        {
            cDAL oDAL = new cDAL("ACTIVE");

            string query = string.Empty;
            if (ProgramName == "VALVE")
            {
                query = @"
SELECT * FROM (
SELECT  
       PS.ProgramID
       , p.Name
      , WOH.Id
      , ps.WOHeaderID
      , ps.SerialNo
      , roh.CustomerReference
      , roh.ThirdPartyReference
      , ps.PartNo
      , pn.Description
 , cws.Description as WorkCenter
      , CASE WHEN wsd.Code IS NULL THEN cws.Description ELSE wsd.Description END AS WorkStationDesc
      , usr.Username as [User]
      , pt.Reason as HoldReason
      , cf.code AS faultcode
      , cf.description as Fault
      , woh.CreateDate
      , woh.LastActivityDate
      , DATEDIFF(Day, Wsh.CreateDate, GETDATE()) AS DaysAtLoc
      , DATEDIFF(Day, woh.CreateDate, GETDATE()) AS DaysInWIP
      , (
            SELECT COUNT(CCD.CalendarDate)
            FROM pls.WOHeader wo with (nolock)
            INNER JOIN pls.CodeCalendarDetails ccd ON CountDay = 1
                       AND ccd.CalendarDate >= wo.CreateDate
                       AND ccd.CalendarDate <= GETDATE()
            WHERE wo.Id = ps.WOHeaderID
        ) As WorkingDaysInWip
FROM [pls].[PartSerial] ps with (nolock)
INNER JOIN pls.Program p  with (nolock)ON p.ID = ps.ProgramID
INNER JOIN pls.ROHeader roh with (nolock) ON roh.ID = ps.ROHeaderID
INNER JOIN pls.WOHeader woh with (nolock) ON woh.ID = ps.WOHeaderID
INNER JOIN pls.PartNo pn with (nolock) ON pn.PartNo = ps.PartNo
INNER JOIN pls.CodeWorkStation cws with (nolock) ON cws.ID = woh.WorkStationID
LEFT JOIN pls.CodeWorkStationCustomDescription wsd with (nolock) ON
          wsd.ProgramID = woh.ProgramID
          AND wsd.RepairTypeID = woh.RepairTypeID
          AND wsd.CodeWorkStationID = woh.WorkStationID
LEFT JOIN pls.PartTransaction pt with (nolock) ON pt.ProgramId = ps.ProgramId
          AND pt.CustomerReference = woh.CustomerReference
          AND pt.PartNo = ps.PartNo
          AND pt.SerialNo = ps.SerialNo
INNER JOIN pls.woline wl with (nolock)ON  woh.id = wl.woheaderid
INNER JOIN pls.wounit wu with (nolock) ON  wl.id = wu.wolineid
LEFT JOIN pls.WOUnitCodes wc with (nolock) ON wu.id = wc.wounitid
LEFT JOIN pls.CodeRepair cr with (nolock) ON  wc.repairid = cr.id
LEFT JOIN pls.CodeFault cf with (nolock) ON  wc.faultid = cf.id
INNER JOIN pls.WOStationHistory wsh with (nolock) ON woh.ID = wsh.WOHeaderID 
INNER JOIN pls.[User] usr with (nolock) ON usr.Id = wsh.UserId

 ";
            }

            else
            {
              query += @" SELECT * FROM (
SELECT  
       PS.ProgramID
       , p.Name
      , WOH.Id
      , ps.WOHeaderID
      , ps.SerialNo
      , roh.CustomerReference
      , roh.ThirdPartyReference
      , ps.PartNo
      , pn.Description
      , cws.Description as WorkCenter
      , CASE WHEN wsd.Code IS NULL THEN cws.Description ELSE wsd.Description END AS WorkStationDesc
      , usr.Username as [User]
      , cf.code AS faultcode
      , cf.description as Fault
      , woh.CreateDate
      , woh.LastActivityDate
      , DATEDIFF(Day, Wsh.CreateDate, GETDATE()) AS DaysAtLoc
      , DATEDIFF(Day, woh.CreateDate, GETDATE()) AS DaysInWIP
      , (
            SELECT COUNT(CCD.CalendarDate)
            FROM pls.WOHeader wo
            INNER JOIN pls.CodeCalendarDetails ccd ON CountDay = 1
                       AND ccd.CalendarDate >= wo.CreateDate
                       AND ccd.CalendarDate <= GETDATE()
            WHERE wo.Id = ps.WOHeaderID
        ) As WorkingDaysInWip
FROM [pls].[PartSerial] ps with (nolock)
INNER JOIN pls.Program p with (nolock) ON p.ID = ps.ProgramID
INNER JOIN pls.ROHeader roh with (nolock) ON roh.ID = ps.ROHeaderID
INNER JOIN pls.WOHeader woh with (nolock) ON woh.ID = ps.WOHeaderID
INNER JOIN pls.PartNo pn with (nolock) ON pn.PartNo = ps.PartNo
INNER JOIN pls.CodeWorkStation cws with (nolock) ON cws.ID = woh.WorkStationID
LEFT JOIN pls.CodeWorkStationCustomDescription wsd with (nolock) ON
          wsd.ProgramID = woh.ProgramID
          AND wsd.RepairTypeID = woh.RepairTypeID
          AND wsd.CodeWorkStationID = woh.WorkStationID
LEFT JOIN pls.PartTransaction pt with (nolock) ON pt.ProgramId = ps.ProgramId
          AND pt.CustomerReference = woh.CustomerReference
          AND pt.PartNo = ps.PartNo
          AND pt.SerialNo = ps.SerialNo
INNER JOIN pls.woline wl with (nolock) ON  woh.id = wl.woheaderid and wl.ComponentPartNo = woh.PartNo
INNER JOIN pls.wounit wu with (nolock) ON  wl.id = wu.wolineid
LEFT JOIN pls.WOUnitCodes wc with (nolock) ON wu.id = wc.wounitid
LEFT JOIN pls.CodeRepair cr with (nolock) ON  wc.repairid = cr.id
LEFT JOIN pls.CodeFault cf  with (nolock) ON  wc.faultid = cf.id
INNER JOIN pls.WOStationHistory WSH with (nolock) ON WSH.ID = (
    SELECT MAX(WSH1.ID) 
    FROM pls.WOStationHistory WSH1 
    WHERE WSH1.WOHeaderID = woh.ID
)
INNER JOIN pls.[User] usr with (nolock) ON usr.Id = wsh.UserId

 ";
            }
            
            if (ProgramId != "0")
            {
                query += "WHERE ps.ProgramID = '" + ProgramId + "' ";
            }
            else
            {
                query += "WHERE ps.ProgramID = '" + ProgramId + "' ";
            }

            query += "AND ps.StatusID in (19, 28) ";

            if (!string.IsNullOrEmpty(partNo))
                query += "AND ps.PartNo = '" + partNo + "' ";
            if (!string.IsNullOrEmpty(custRef))
                query += "AND roh.CustomerReference = '" + custRef + "' ";
            if (ProgramName == "VALVE")
            {
                query += ") TEMP GROUP BY ProgramID,Name, Id, WOHeaderID, SerialNo, CustomerReference, ThirdPartyReference, PartNo, Description, WorkCenter, WorkStationDesc, [User], HoldReason, faultcode, Fault, CreateDate, LastActivityDate, DaysAtLoc, DaysInWIP, WorkingDaysInWip ";

            }
            else
            {
                query += ") TEMP GROUP BY ProgramID,Name, Id, WOHeaderID, SerialNo, CustomerReference, ThirdPartyReference, PartNo, Description, WorkCenter, WorkStationDesc, [User], faultcode, Fault, CreateDate, LastActivityDate, DaysAtLoc, DaysInWIP, WorkingDaysInWip ";

            }
            query += " Order BY LastActivityDate DESC";

            if (!string.IsNullOrEmpty(ProgramName))
                filterString = " > Program = '" + ProgramName /*+ "' | From = '" + frmDt + "' To = '" + toDt + "' "*/;

            if (!string.IsNullOrEmpty(custRef))
                filterString += " | Customer Ref. = '" + custRef + "' ";

            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. = '" + partNo + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("100", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWIPOutline = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}