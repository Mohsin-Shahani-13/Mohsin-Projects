using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaProductivityReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        [Display(Name = "Program:")]
        public string program { get; set; }

        [Display(Name = "Program:")]
        public string program_Id { get; set; }

        [Display(Name = "Part No.:")]
        public string PartNo { get; set; }

        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstMetaProductivity { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

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

        #endregion
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName,string PartNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"SELECT
    p.Name As ProgramName,
    WOH.ID As ID,
    (CASE WHEN TimeZone = 'Central Europe Standard Time' THEN 'EMEA'
		 WHEN TimeZone IN ('Singapore Standard Time','Eastern Standard Time') THEN 'APAC'
		 ELSE 'AMER' END) Region,
    woh.ProgramID [CONTRACT], 
	pn.PartNo AS [PART_NO],       
	woh.SerialNo [SERIAL_NO],
    CASE 
              WHEN cwscd.Code IS NULL 
              THEN cws.Description 
              ELSE cwscd.Code 
              END AS WORK_CENTER_NO,
    --(SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
	  -- FROM pls.partserial PS
	   --WHERE PS.SerialNo = woh.SerialNo AND PS.ProgramID = woh.ProgramID ) HAS_SN,
    --woh.ID,
    woh.CustomerReference AS [ORDER_NO],
	pn.[Description] AS [DESCRIPTION], 
	pn.ModelNo AS [TYPE_DESIGNATION], 
	cc.[Description] AS [SECOND_COMMODITY], 
    CASE 
			  WHEN cwscd.Code IS NULL 
			  THEN cws.Description 
			  ELSE cwscd.Description 
		 END AS OPERATION_DESCRIPTION,
	--ISNULL(cwscd.[Description],  cws.[Description]) AS [OPERATION_DESCRIPTION], 
	u.Username AS [EMPNO], 
    --wosh.CreateDate AS [DATED]
    FORMAT(wosh.CreateDate, 'MM/dd/yyyy hh:mm:ss tt') AS [DATED]
FROM pls.WOHeader woh

	INNER JOIN pls.Program p ON p.ID = woh.ProgramID 
	INNER JOIN pls.PartNo pn ON pn.PartNo = woh.PartNo 
	LEFT JOIN pls.CodeCommodity cc ON cc.ID = pn.SecondaryCommodityID 
	INNER JOIN pls.WOStationHistory wosh ON woh.ID = wosh.WOHeaderID 
	INNER JOIN pls.CodeWorkStation cws ON cws.ID = wosh.WorkStationID 
	LEFT JOIN pls.CodeWorkStationCustomDescription cwscd ON woh.ProgramID = cwscd.ProgramID 
		AND woh.RepairTypeID = cwscd.RepairTypeID 
		AND cws.ID = cwscd.CodeWorkStationID 
	INNER JOIN pls.[User] u ON u.ID = wosh.UserID 
WHERE CONVERT(Date, wosh.CreateDate) >= '<frmDt>' AND CONVERT(Date, wosh.CreateDate) <= '<toDt>'
 ";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            if (programId != "0" && programId != null)
            {
                query += " AND woh.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += " AND woh.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            if (!string.IsNullOrEmpty(PartNo))
                query +=" AND  pn.PartNo like '%" + PartNo + "%' ";

            query += "ORDER BY wosh.CreateDate DESC";

            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(PartNo))
                filterString += "| Part No. Like '" + PartNo + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("120", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaProductivity = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}