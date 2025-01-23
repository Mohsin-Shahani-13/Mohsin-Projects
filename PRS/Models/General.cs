using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Models
{
    public class General
    {
        cDAL oDAL = new cDAL("INIT");
        public List<ArrayList> lstRpt { get; set; }
        public string Name { get; set; }
        public string RptCode { get; set; }       
        public string Target { get; set; }
        public string Url { get; set; }
        public string DesignedBy { get; set; }
        public string MailTo { get; set; }

        public void GetList()
        {
            string query = string.Empty;
            string linkedSrvr = HttpContext.Current.Session["LinkedSrvr"].ToString();
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            query = @"
SELECT RptId, 
       Company, 
       ProgramId, 
       Program, 
       Region,
       Environment,
       Site, 
       Title AS Name,
       Status,
       IsProd,
	   IsRept,
	   IsTran,
	   IsTest
FROM RPT.IMPORTED_LIST WHERE IsActive = 1  AND RptType = 'RC' ";


            // un comment before publish
           // query += "AND Region = '" + linkedSrvr + "' ";
            //query += "AND Environment ='" + conType + "' ";


            string WinITTeam = "idrak,kashif,hasham,imdad,tahir,yousuf,mohsin,junaid,arsalan,shahzaib,bilal,internee,asif,kamran,salman,huzaifa,kamlesh,deewan";
            string logonUser = HttpContext.Current.Session["LogonUser"].ToString();
            bool developer = false;

            foreach (string s in WinITTeam.Split(','))
            {
                if (logonUser.Contains(s))
                    developer = true;
            }

            if (!developer)
                query += @"
AND IsReady = 1 ";

            query += @"
ORDER BY Name";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("0024", query, string.Empty, false);

            DataTable dt = oDAL.GetData(query);
            lstRpt = cCommon.ConvertDtToArrayList(dt);


        }

        public void GetReport(string rptId)
        {
            string query = string.Empty;

            query = @"SELECT RptId, RptCode, Title AS Name, Target, Url, ShortTitle, Title, DesignedBy, MailTo
            FROM RPT.IMPORTED_LIST WHERE IsActive = 1 AND RptId = " + rptId + " ORDER BY Name";

            DataTable dt = oDAL.GetData(query);
            if (dt.Rows.Count > 0)
            {
                Name = dt.Rows[0]["Name"].ToString();
                RptCode = dt.Rows[0]["RptCode"].ToString();            
                Target = dt.Rows[0]["Target"].ToString();
                Url = dt.Rows[0]["Url"].ToString();
                DesignedBy = dt.Rows[0]["DesignedBy"].ToString();
                MailTo = dt.Rows[0]["MailTo"].ToString();
              
            }
        }
    }
}