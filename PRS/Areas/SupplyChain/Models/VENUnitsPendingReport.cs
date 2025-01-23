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
    public class VENUnitsPendingReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "VEN Type:")]
        public string venType { get; set; }
        [Display(Name = "VEN No.:")]
        public string venNo { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }

        public List<Hashtable> lstVENUnitsPendingReport { get; set; }

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
        public bool GetList(string programId, string programName, string fDate, string tDate, string venType, string venNo, string pendingVen)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            //string programName = HttpContext.Current.Session["Program"].ToString();

            string query = string.Empty;

            query = @"SELECT P.Name,
       P.ID,
       PSA4.LastActivityDate,
       PSA2.value                            AS Customer_PO,
       ps.serialno                           AS Tracker,
       (SELECT CASE
                 WHEN Count(serialno) > 0 THEN 'Y'
                 ELSE 'N'
               END
        FROM   pls.partserial
        WHERE  serialno = ps.serialno
               AND programid = ps.programid) AS HAS_SN,
       --for validating serialize hyperlink
       PSA3.value                            AS RMA,
       ps.partno                             AS FRU,
       PSA4.value                            AS VEN_TYPE,
       PSA5.value                            AS VEN_COMMENT,
       PSA6.value                            AS VEN_NUMBER,
       PSA7.value                            AS VEN_IBM_COMMENT,
       ROUA.value                            AS serial,
       PSA9.value                            AS CREDIT_NOTE,
       ( CASE Isnull(PSA8.value, 'X')
           WHEN 'X' THEN 'NO'
           ELSE 'YES'
         END )                               AS OEM_RETURNED,
       ( CASE Isnull(PSA7.value, 'X')
           WHEN 'X' THEN 'YES'
           ELSE 'NO'
         END )                               AS PENDING_VEN
FROM   pls.partserial PS
       INNER JOIN pls.partserialattribute PSA
               ON PS.id = PSA.partserialid
                  AND PSA.attributeid = (SELECT id
                                         FROM   pls.codeattribute
                                         WHERE  attributename = 'PROCESS_CODE')
       INNER JOIN pls.partserialattribute PSA2
               ON ps.id = PSA2.partserialid
                  AND PSA2.attributeid = (SELECT id
                                          FROM   pls.codeattribute
                                          WHERE  attributename = 'CARTONNO')
       INNER JOIN pls.partserialattribute PSA3
               ON ps.id = PSA3.partserialid
                  AND PSA3.attributeid = (SELECT id
                                          FROM   pls.codeattribute
                                          WHERE  attributename = 'RMA')
       INNER JOIN pls.partserialattribute PSA4
               ON ps.id = PSA4.partserialid
                  AND PSA4.attributeid = (SELECT id
                                          FROM   pls.codeattribute
                                          WHERE  attributename = 'VEN_TYPE')
       INNER JOIN pls.partserialattribute PSA5
               ON PSA5.partserialid = ps.id
                  AND PSA5.attributeid = (SELECT id
                                          FROM   pls.codeattribute
                                          WHERE  attributename = 'VEN_COMMENT')
       LEFT JOIN pls.partserialattribute PSA6
              ON PSA6.partserialid = ps.id
                 AND PSA6.attributeid = (SELECT id
                                         FROM   pls.codeattribute
                                         WHERE  attributename = 'VEN_NUMBER')
       LEFT JOIN pls.partserialattribute PSA7
              ON ps.id = PSA7.partserialid
                 AND PSA7.attributeid = (SELECT id
                                         FROM   pls.codeattribute
                                         WHERE  attributename = 'IBM_COMMENT')
       LEFT JOIN pls.partserialattribute PSA8
              ON ps.id = PSA8.partserialid
                 AND PSA8.attributeid = (SELECT id
                                         FROM   pls.codeattribute
                                         WHERE  attributename = 'CONDITION')
       LEFT JOIN pls.partserialattribute PSA9
              ON ps.id = PSA9.partserialid
                 AND PSA9.attributeid = (SELECT id
                                         FROM   pls.codeattribute
                                         WHERE  attributename = 'CREDIT')
       INNER JOIN pls.roline ROL
               ON ROL.roheaderid = ps.roheaderid
                  and (ROL.PartNo = PS.PartNo
						OR ROL.PartNo  = (select PT.SourcePartNo from pls.PartTransaction PT 
										where PT.ProgramID = ps.ProgramID 
										and PT.PartNo = PS.PartNo 
										and PT.SerialNo = ps.SerialNo 
										and PT.SourcePartNo is not null))
       INNER JOIN pls.rounit ROU
               ON ROU.rolineid = ROL.id
                  AND ROU.serialno = ps.serialno
       INNER JOIN pls.rounitattribute ROUA
               ON ROUA.rounitid = ROU.id
                  AND ROUA.attributeid = (SELECT id
                                          FROM   pls.codeattribute
                                          WHERE  attributename = 'SerialNumber')
       INNER JOIN pls.program P
               ON P.id = ps.programid
WHERE  PS.programid = 10030
       AND PS.statusid NOT IN(8, 32 )";

            query += " \nAND CONVERT(Date, PSA4.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, PSA4.LastActivityDate) <= '<toDt>'";
            if (!string.IsNullOrEmpty(venType))
                query += "\nAND PSA4.Value = '" + venType + "' ";

            if (!string.IsNullOrEmpty(venNo))
                query += "\nAND PSA6.Value LIKE '%" + venNo + "%' ";

            if (!string.IsNullOrEmpty(pendingVen))
                query += "\nAND (CASE ISNULL(PSA7.Value, 'X') WHEN 'X' THEN 'YES' ELSE 'NO' END) = '" + pendingVen + "'";

            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", fDate);
            query = query.Replace("<toDt>", tDate);

            query += "\nORDER BY PSA4.LastActivityDate";
            DataTable dt = oDAL.GetData(query);

            //if (!string.IsNullOrEmpty(programName))
            filterString += "> Program = '" +programName+ "' ";

            filterString += " | From = '" + fDate + "' To = '" + tDate + "' ";
            if (!string.IsNullOrEmpty(venType))
                filterString += " | VEN Type = '" + venType + "' ";
            if (!string.IsNullOrEmpty(venNo))
                filterString += " | VEN No. Like '" + venNo + "' ";

            if (!string.IsNullOrEmpty(pendingVen))
                filterString += " | Pending VEN = '" + pendingVen + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("188", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstVENUnitsPendingReport = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        #endregion
    }
}