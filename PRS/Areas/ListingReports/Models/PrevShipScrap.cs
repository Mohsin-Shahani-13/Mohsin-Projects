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
    public class PrevShipScrap
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

      
        [Display(Name = "Invoice Date:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
      

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
       
        public List<Hashtable> lstPrevShipScrap { get; set; }
        #endregion
        #region Methods 
     
        public DataTable GetList(string fDate)
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"
	DECLARE @InputDate DATE = '<fDate>'; -- Replace with the user-entered date

-- Calculate the start of the week (Monday)
DECLARE @StartOfWeek DATE = DATEADD(DAY, 2 - DATEPART(WEEKDAY, @InputDate), @InputDate);

-- Calculate the end of the week (Friday)
DECLARE @EndOfWeek DATE = DATEADD(DAY, 6 - DATEPART(WEEKDAY, @InputDate), @InputDate);

SELECT 
    @InputDate AS ReportDate,
	pt.ProgramID,
    pt.PartNo, 
    'Reconext Poland' AS Partner, 
    SUM(CASE WHEN pt.PartTransaction = 'SO-SHIP' THEN pt.qty ELSE 0 END) AS 'Repair QTY',
    SUM(CASE WHEN pt.PartTransaction = 'WO-SCRAP' THEN pt.qty ELSE 0 END) AS 'Scrap QTY'
FROM
    [pls].[vPartTransaction] pt ";

                  if (sites == "BYDGOSZCZ")
            {
                query += "where pt.programid = 10064 ";
            }
            else if (sites == "JUAREZ")
            {
                query += "where pt.programid = 10061 ";

            }
            else if (sites == "MEXICALI")
            {
                query += "where pt.programid = 10055 ";

            }
            else if (sites == "MEMPHIS")
            {
                query += "where pt.programid = 10053 ";

            }
            query += @"AND pt.PartTransaction IN('SO-SHIP', 'WO-SCRAP')
    AND pt.ForDate BETWEEN @StartOfWeek AND @EndOfWeek

GROUP BY
    pt.ProgramID,
    pt.PartNo

 ";


            query = query.Replace("<fDate>", fDate);


                filterString = "> Program = Dell ";
            filterString += " | Date = '" + fDate + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("211", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return new DataTable();
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPrevShipScrap = cCommon.ConvertDtToHashTable(dt);
                return dt;
            }
        }
        #endregion
    }
}