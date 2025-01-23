using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.Finance.Models
{
    public class GLTransactions
    {
        cDAL oDAL ;
        #region Fields

        [Display(Name = "Fiscal Year:")]
        public string FiscalYear { get; set; }

        [Display(Name = "Fiscal Period:")]
        public string FiscalPeriod { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstGLTransactions { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string FiscalYear, string FiscalPeriod, string FiscalPeriodText)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            oDAL = new cDAL("Finance");
            string query = string.Empty;
            query = @"
SELECT
       glj.fiscalyear,
       glj.fiscalperiod,
       comp.NAME,
       glj.glaccount,
       gla.accountdesc,
       glj.posteddate,
       glj.description,
       glj.bookdebitamount,
       glj.bookcreditamount,
       glj.postedby 
FROM   erp.gljrndtl glj
       INNER JOIN erp.company comp
               ON glj.company = comp.company
       LEFT OUTER JOIN erp.glaccount gla
               ON glj.company = gla.company
               AND glj.glaccount = gla.glaccount  
WHERE FiscalYear = '<FiscalYear>' AND Fiscalperiod = '<FiscalPeriod>'
 ";

            query = query.Replace("<FiscalYear>", FiscalYear);
            query = query.Replace("<FiscalPeriod>", FiscalPeriod);



            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(FiscalYear) && !string.IsNullOrEmpty(FiscalPeriod))
                filterString = " > Fiscal Year = '" + FiscalYear + "' | Fiscal Period = '" + FiscalPeriod + "' ";



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("105", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstGLTransactions = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}