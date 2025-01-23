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
    public class WIPAgingNSN
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
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable GetProgramBySite()
        {
            //oDAL = new cDAL("ACTIVE");
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
        public List<Hashtable> lstWIPAgingNSN { get; set; }

       
        #endregion
        #region Methods 
     
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName)
        {
            string query = string.Empty;
           
            query = @"
SELECT DISTINCT
    ps.ProgramID, 
    ps.SerialNo, 
	ps.PartNo, 
    pn.ModelNo, 
    cs.Description AS Status, 
    MAX(ps.RODate) AS RODate,
    ps.WOStartDate,
    -- Calculating Aging excluding weekends (Saturday and Sunday)
    (DATEDIFF(DAY, MAX(ps.RODate), GETDATE()) 
    - (DATEDIFF(WEEK, MAX(ps.RODate), GETDATE()) * 2) 
    -- Adjusting for partial weeks where the end date falls on a weekend
    + CASE 
        WHEN DATENAME(WEEKDAY, MAX(ps.RODate)) = 'Sunday' THEN 1 
        ELSE 0 
      END
    + CASE 
        WHEN DATENAME(WEEKDAY, GETDATE()) = 'Saturday' THEN 1 
        ELSE 0 
      END) AS Aging
FROM pls.PartSerial ps
INNER JOIN pls.PartNo pn ON pn.PartNo = ps.PartNo
INNER JOIN pls.CodeStatus cs ON cs.ID = ps.StatusID
WHERE ps.ProgramID = '<programId>' 
AND CONVERT(Date, ps.WOStartDate) >= '<frmDt>' AND CONVERT(Date, ps.WOStartDate) <= '<toDt>'
AND cs.Description <> ('SHIPPED')
GROUP BY ps.ProgramID, ps.SerialNo, ps.PartNo, pn.ModelNo, ps.WOStartDate, cs.Description
ORDER BY ps.WOStartDate DESC; ";
          



            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            query = query.Replace("<programId>", programId);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
          


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("221", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWIPAgingNSN = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}