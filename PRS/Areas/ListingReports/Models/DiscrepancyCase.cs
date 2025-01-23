using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class DiscrepancyCase
    {
        [Display(Name = "Program:")]
        public string program { get; set; }
       
        [Display(Name = "Queue:")]
        public string queue { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public List<Hashtable> lstDiscrepancyCase { get; set; }
       
        public string ErrorMessage { get; set; }


        cDAL oDAL;
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


        public bool GetList(string programID, string programName, string queue)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            

            query = @"SELECT DISTINCT 
    cm.id,
    cm.ProgramID,
    cm.SerialNo,
    cm.PartNo,
    cm.Subject,
    cs.Description AS 'Plus Case Status',
    ISNULL(cms.value, '') AS CustomerStatus,
    ISNULL(ccid.value,'') AS CustomerCaseId,
    q.value AS Queue,
    cm.createdate,
    cm.Description
FROM 
    pls.casemgt cm
JOIN 
    pls.CaseMgtAttribute cma ON cma.CaseMgtID = cm.id
JOIN 
    pls.codestatus cs ON cs.id = cm.StatusID
LEFT JOIN 
    pls.CaseMgtAttribute cms ON cms.AttributeID = '351' AND cms.CaseMgtID = cm.id
LEFT JOIN 
    pls.CaseMgtAttribute ccid ON ccid.AttributeID = '319' AND ccid.CaseMgtID = cm.id
LEFT JOIN 
    pls.CaseMgtAttribute q ON q.AttributeID = '272' AND q.CaseMgtID = cm.id
WHERE 
    cm.programid = " + programID + " ";
            

            if (!queue.Equals("All"))
                query += "AND q.value = '" + queue + "' ";

               

            query += "GROUP BY cm.id, cm.ProgramID, cm.SerialNo, cm.partno, cm.Subject, cs.Description, cm.CreateDate, cm.Description, cms.value, ccid.value, q.value ";

            query += "ORDER BY cm.createdate desc ";

            DataTable dt = oDAL.GetData(query);

            query = query.Replace("<programID>", programID);

            ///////////FILTER SRINGS/////////

            if (!string.IsNullOrEmpty(programName))
                filterString = "> Program = '" + programName + "' ";

            if (!queue.Equals("All"))
                filterString += " | Queue = '" + queue + "'";

          

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("205", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDiscrepancyCase = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}