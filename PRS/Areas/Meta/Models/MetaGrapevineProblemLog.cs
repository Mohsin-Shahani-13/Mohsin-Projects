using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaGrapevineProblemLog
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "Part No.:")]
        public string PartNo { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstMetaGrapevineProblemLog { get; set; }

        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList(string programId, string ProgramName) 
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"SELECT  CM.Id,
       cm.type,
       cm.priority,
       cm.subject,
       cm.description,
       cm.customerreference,
       ca1.value AS RETURNTYPE,
       cm.ordertype,
       cm.parentserialno,
       cm.partno,
       cm.serialno,
       ca5.value AS PARTNO2,
       ca6.value AS SERIALNO2,
       ca7.value AS PARTNO3,
       ca8.value AS SERIALNO3,
       CR.Description ReasonDESC,
       CS.Description AS StatusDescription,
       ca4.value AS CUSTOMERCASEID,
       ca3.Value AS TrackingNumber,
       u.Username AS assignedToUser,
       ca2.value AS RESOLUTION,
       cm.createdate,
       cm.lastactivitydate
FROM   pls.casemgt cm

      INNER JOIN pls.CodeReason CR ON CR.ID = cm.ReasonID
       INNER JOIN pls.CodeStatus CS ON CS.ID = cm.StatusID     

       LEFT JOIN [pls].CodeAttribute CAT1 ON CAT1.AttributeName = 'RETURNTYPE'
       LEFT JOIN pls.casemgtattribute ca1 ON cm.id = ca1.casemgtid AND ca1.AttributeID = CAT1.ID

       LEFT JOIN [pls].CodeAttribute CAT2 ON CAT2.AttributeName = 'RESOLUTION'
       LEFT JOIN pls.casemgtattribute ca2 ON cm.id = ca2.casemgtid AND ca2.AttributeID = CAT2.ID

       LEFT JOIN [pls].CodeAttribute CAT3 ON CAT3.AttributeName = 'TRACKING NUMBER'
       LEFT JOIN pls.casemgtattribute ca3 ON cm.id = ca3.casemgtid AND ca3.AttributeID = CAT3.ID

       LEFT JOIN [pls].CodeAttribute CAT4 ON CAT4.AttributeName = 'CUSTOMER CASE ID'
       LEFT JOIN pls.casemgtattribute ca4 ON cm.id = ca4.casemgtid AND ca4.AttributeID = CAT4.ID

       LEFT JOIN [pls].CodeAttribute CAT5 ON CAT5.AttributeName = 'PART NO 2'
       LEFT JOIN pls.casemgtattribute ca5 ON cm.id = ca5.casemgtid AND ca5.AttributeID = CAT5.ID

       LEFT JOIN [pls].CodeAttribute CAT6 ON CAT6.AttributeName = 'SERIAL NO 2'
       LEFT JOIN pls.casemgtattribute ca6 ON cm.id = ca6.casemgtid AND ca6.AttributeID = CAT6.ID


       LEFT JOIN [pls].CodeAttribute CAT7 ON CAT7.AttributeName = 'PART NO 3'
       LEFT JOIN pls.casemgtattribute ca7 ON cm.id = ca7.casemgtid AND ca7.AttributeID = CAT7.ID

       LEFT JOIN [pls].CodeAttribute CAT8 ON CAT8.AttributeName = 'SERIAL NO 3'
       LEFT JOIN pls.casemgtattribute ca8 ON cm.id = ca8.casemgtid AND ca8.AttributeID = CAT8.ID
       INNER JOIN  [pls].[user] u  on u.ID = cm.AssignedToUserID 
       
";


            if (programId != "0")
            {
                query += "WHERE cm.programid = '" + programId + "' ";
  
                query = query.Replace("<programid>", programId);

            }
            else
            {
                query += "AND cm.programid IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("152", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaGrapevineProblemLog = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}