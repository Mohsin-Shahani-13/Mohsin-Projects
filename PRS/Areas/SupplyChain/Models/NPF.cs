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
    public class NPF
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Order No.:")]
        public string orderno { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
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
        public List<Hashtable> lstNPF { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string partNo, string serialNo, string programId, string ProgramName, string orderno)
        {
            // oDAL = new cDAL("ACTIVE", "ST");

            string query = string.Empty;
            //DateTime toDate = Convert.ToDateTime(toDt);
            //toDate.ToString(Format.DateOnly);
            //DateTime aaj = toDate.AddDays(1);

            //string to_date = toDate.ToString(Format.DateOnly);

            query = @"
SELECT    WOH.ProgramID
        , P.Name ProgramName
        , WOH.Id
        , WOH.PartNo
        , PN.Description AS PartDesc
        , (SELECT CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
           FROM pls.PartSerial
           WHERE SerialNo = WOH.SerialNo AND ProgramID = WOH.ProgramID) AS HAS_SN
        , WOH.SerialNo
        , CF.Code
        , CF.Description
        , WOH.LastActivityDate
        , CASE WHEN wsd.Code IS NULL THEN cws.Description ELSE wsd.Description END AS FromWorkStation
        , CASE WHEN CWSCD.Code IS NULL THEN CWOS.Description ELSE CWSCD.Description END AS ToWorkStation
FROM pls.WOHeader WOH
INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
INNER JOIN pls.PartNo PN ON PN.PartNo = WOH.PartNo
INNER JOIN pls.WOLine WOL ON WOL.WOHeaderID = WOH.ID
AND WOL.ComponentPartNo = WOH.PartNo
INNER JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID
LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = WOU.ID
LEFT OUTER JOIN pls.CodeFault CF ON CF.ID = WUC.FaultID
AND (WUC.FaultId IS NULL OR WUC.FaultId IN (1019,1023,1053,1054))
INNER Join pls.WOStationHistory wsh on wsh.WOHeaderID = woh.ID
INNER JOIN pls.CodeWorkStation CWS ON
CWS.ID = WOH.WorkStationID
INNER JOIN pls.CodeWorkStationCustomDescription WSD ON wsd.ProgramID = woh.ProgramID
AND WSD.RepairTypeID = WOH.RepairTypeID
AND WSD.CodeWorkStationID = Wsh.WorkStationID
INNER JOIN pls.CodeWorkStation CWOS ON
CWOS.ID = WOH.WorkStationID
INNER JOIN pls.CodeWorkStationCustomDescription CWSCD ON CWSCD.ProgramID = WOH.ProgramID
AND CWSCD.RepairTypeID = WOH.RepairTypeID
AND CWSCD.CodeWorkStationID = wsh.ToWorkStationID
WHERE CONVERT(Date, WOH.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, WOH.LastActivityDate) <= '<toDt>' AND WOH.StatusID = 15
AND wsh.WorkStationID = 14 AND Wsh.ToWorkStationID = 13
 ";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            if (!string.IsNullOrEmpty(partNo))
                query += "AND WOH.PartNo LIKE '%" + partNo + "%' ";
            if (!string.IsNullOrEmpty(serialNo))
                query += "AND WOH.SerialNo LIKE '%" + serialNo + "%' ";
            if (!string.IsNullOrEmpty(orderno))
                query += "AND WOH.Id LIKE '%" + orderno + "%' ";

            if (programId != "0")
            {
                query += "AND WOH.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += "AND WOH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            query += "ORDER BY WOH.LastActivityDate";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. Like '" + partNo + "' ";
            if (!string.IsNullOrEmpty(serialNo))
                filterString += " | Serial No. Like '" + serialNo + "' ";
            if (!string.IsNullOrEmpty(orderno))
                filterString += " | Order No. Like '" + orderno + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("112", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstNPF = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}