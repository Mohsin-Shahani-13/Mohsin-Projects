using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.Meta.Models
{
    public class MetaTestRunFaultCodes
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
        public List<Hashtable> lstMetaTestRunFaultCode { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @" 
SELECT    
               woh.ProgramID
              ,prg.Name AS ProgramName 
              ,(CASE WHEN TimeZone = 'Central Europe Standard Time' THEN 'EMEA'
		             WHEN TimeZone IN ('Singapore Standard Time','Eastern Standard Time') THEN 'APAC'
		             ELSE 'AMER' END) Region
			  , (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
							FROM pls.partserial PS
							WHERE PS.SerialNo = woh.SerialNo AND PS.ProgramID = woh.ProgramID ) HAS_SN
			  , woh.SerialNo
              , woh.ID
			  , woh.CustomerReference 
			  , pn.ModelNo AS [TYPE_DESIGNATION] 
			  , CRT.Description AS Order_Type
			  , woh.PartNo
			  , pn.Description as PartDesc
              , CASE WHEN wsd.Code IS NULL 
					 THEN cws.Description
					 ELSE wsd.Description END AS WorkStationDesc
			  , CASE WHEN wsh.IsPass > 0 
					 THEN 'Pass' 
					 ELSE 'Fail' END AS Result
			  , cft.Description as FaultDesc
              , cft.Code As FaultCode
			  , Null AS Code
              , Null Result_message
              , usr.Username AS [By]
			  , whf.CreateDate AS SO_Dated          
FROM pls.WOStationHistory wsh
        INNER JOIN pls.WOHeader woh ON
                   woh.ID = wsh.WOHeaderID
        INNER JOIN pls.Program prg ON
                   prg.ID = woh.ProgramId
		INNER JOIN pls.PartNo pn ON
				   pn.PartNo = woh.PartNo 
        INNER JOIN pls.CodeWorkStation cws ON
                   cws.ID = wsh.WorkStationID
        LEFT JOIN pls.CodeWorkStationCustomDescription wsd ON
                   wsd.ProgramID = woh.ProgramID
                   AND wsd.RepairTypeID = woh.RepairTypeID
                   AND wsd.CodeWorkStationID = wsh.WorkStationID
		INNER JOIN pls.CodeRepairType CRT ON
					CRT.ID = WOH.RepairTypeID
        INNER JOIN pls.WOStationHistoryFailReasons whf ON
                   whf.WOStationHistoryId = wsh.ID
        LEFT JOIN pls.CodeFault cft ON
                   cft.ID = whf.FaultID
        LEFT JOIN pls.[User] usr ON usr.ID = whf.UserId
        WHERE wsh.IsPass = 0
AND CONVERT(Date, whf.CreateDate) >= '<frmDt>' AND CONVERT(Date, whf.CreateDate) <= '<toDt>'
 ";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            //if (!string.IsNullOrEmpty(ProgramName))
            //    filterString += "> Program = '" + ProgramName + "' ";

            //filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            if (programId != "0")
            {
                query += "AND prg.ID = '" + programId + "' ";
            }
            else
            {
                query += "AND prg.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            query += "ORDER BY whf.CreateDate DESC";



            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("125", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaTestRunFaultCode = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}