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
    public class VFNWSHistory
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstVFNWSHistory { get; set; }

        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @" 
DECLARE @inicio datetime = CAST(convert(varchar,'<frmDt>', 23) + ' 06:59:00'   AS datetime)
DECLARE @fin datetime =  CAST(convert(varchar,'<toDt>', 23) +  ' 07:00:00'   AS datetime)
 
SELECT DISTINCT
    ps.SerialNo,
    who.CustomerReference,
    pn.[Description],
    --IIF(LEFT(vROh.[Value],6) = 'MEIJER', 'MEIJER', vROh.[Value]) AS [Client],
    --vROLA.[Value] AS [BID],
    --vROUn.[Value] AS CostumerComplaint,
    vha.[Value] AS [PROBLEM FOUND],
    vhb.[Value] AS [FAILURE MODE],
    vhc.[Value] AS [REPAIR NOTES],
    ps.PartNo,
    pl.LocationNo,
    crt.Description AS RepairTypeDescription,
    CONCAT(FromSt.Code, ' - ', FromSt.[Description]) AS FromWS,
    CONCAT(ToSt.Code, ' - ', ToSt.[Description]) AS ToWS,
    IIF(wh.IsPass IS NULL, '', IIF(wh.IsPass = 1, 'PASS', 'FAIL')) AS RESULT,
    wh.Iteration,
    wh.ID AS [WH ID],
    CS.Description AS [WS Status],
    CSw.Description AS [WO Status],
    U.Username,
    wh.LastActivityDate
FROM 
    pls.PartSerial ps WITH (NOLOCK)
LEFT JOIN 
    pls.PartLocation pl WITH (NOLOCK) ON pl.ID = ps.LocationID
LEFT JOIN 
    pls.PartNo pn WITH (NOLOCK) ON ps.PartNo = pn.PartNo
LEFT JOIN 
    pls.WOHeader who WITH (NOLOCK) ON ps.WOHeaderID = who.ID
LEFT JOIN 
    pls.CodeStatus CSw WITH (NOLOCK) ON CSw.ID = who.StatusID
LEFT JOIN 
    pls.CodeRepairType crt WITH (NOLOCK) ON crt.ID = who.RepairTypeID
LEFT JOIN 
    pls.WOStationHistory wh WITH (NOLOCK) ON who.ID = wh.WOHeaderID
LEFT JOIN 
    pls.CodeStatus CS WITH (NOLOCK) ON CS.ID = wh.StatusID
LEFT JOIN 
    pls.[USER] U WITH (NOLOCK) ON U.ID = wh.UserID
LEFT JOIN 
    pls.vWOStationAttribute vha WITH (NOLOCK) ON wh.ID = vha.WOStationHistoryID AND vha.Attribute = 'PROBLEM FOUND'
LEFT JOIN 
    pls.vWOStationAttribute vhb WITH (NOLOCK) ON wh.ID = vhb.WOStationHistoryID AND vhb.Attribute = 'FAILURE MODE'
LEFT JOIN 
    pls.vWOStationAttribute vhc WITH (NOLOCK) ON wh.ID = vhc.WOStationHistoryID AND vhc.Attribute = 'REPAIR NOTES'
LEFT JOIN 
    pls.CodeWorkStationCustomDescription FromSt WITH (NOLOCK) ON wh.WorkStationID = FromSt.CodeWorkStationID 
    AND FromSt.ProgramID = 10010 AND FromSt.RepairTypeID = who.RepairTypeID
LEFT JOIN 
    pls.CodeWorkStationCustomDescription ToSt WITH (NOLOCK) ON wh.WorkStationID = ToSt.CodeWorkStationID 
    AND ToSt.ProgramID = 10010 AND ToSt.RepairTypeID = who.RepairTypeID
LEFT JOIN 
    pls.ROLine rol WITH (NOLOCK) ON rol.ROHeaderID = ps.ROHeaderID
LEFT JOIN 
    pls.ROUnit vROU WITH (NOLOCK) ON vROU.ROLineID = rol.ID
LEFT JOIN 
    pls.CodeAttribute CA2 WITH (NOLOCK) ON CA2.AttributeName = 'SHIPTOSITENAME'
LEFT JOIN 
    pls.ROHeaderAttribute vROh WITH (NOLOCK) ON vROh.ROHeaderID = ps.ROHeaderID AND CA2.ID = vROh.AttributeID
LEFT JOIN 
    pls.CodeAttribute CA3 WITH (NOLOCK) ON CA3.AttributeName = 'BID'
LEFT JOIN 
    pls.ROLineAttribute vROLA WITH (NOLOCK) ON vROLA.ROLineID = rol.ID AND CA3.ID = vROLA.AttributeID
LEFT JOIN 
    pls.CodeAttribute CA WITH (NOLOCK) ON CA.AttributeName = 'REPORTEDPROBLEM'
LEFT JOIN 
    pls.ROUnitAttribute vROUn WITH (NOLOCK) ON vROUn.ROUnitID = vROU.ID AND CA.ID = vROUn.AttributeID
WHERE 
    ps.ProgramID = 10010
    AND wh.LastActivityDate BETWEEN @inicio AND @fin
ORDER BY 
    SerialNo ASC, [WH ID] ASC;
 ";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);

            filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("234", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstVFNWSHistory = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}