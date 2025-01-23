using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;


namespace IP.Areas.Meta.Models
{
    public class MetaShippingReport
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

        public List<Hashtable> lstMetaShipping { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList(string programId, string ProgramName, string PartNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"SELECT
p.ID as ProgramID,
p.Name as ProgramName ,
pt.ID AS [TRANSACTION_ID], 
	pn.PartNo AS [PART_NO], 
	pn.[Description] AS [DESCRIPTION],
(SELECT  CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
        FROM pls.PartSerial
        WHERE SerialNo = pt.SerialNo) AS HAS_SN,
	pt.SerialNo AS [SERIAL_NO], 
	pt.CreateDate AS [DATE_APPLIED], 
	pt.CreateDate AS [DATED] 
	,pt.PartTransactionID,
	p.ID,P.[Name]
FROM pls.PartTransaction pt 
	INNER JOIN pls.Program p ON p.ID = pt.ProgramID 
	INNER JOIN pls.PartNo pn ON pn.PartNo = pt.PartNo 
WHERE pt.PartTransactionID = 18 ";
            if (!string.IsNullOrEmpty(PartNo))
                query += " AND  pn.PartNo like '%" + PartNo + "%' ";



            //query += " WHERE CRT.ID  IN ('<RepairTypeID>') ";

            if (programId != "0" && programId != null)
            {
                query += " AND P.ID = '" + programId + "' ";
            }
            else
            {
                query += " AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            //if (!string.IsNullOrEmpty(PartNo))
            //{
            //    query += "AND pt.ID = '<PartNo>'";
            //}
            //query = query.Replace("<TransactionId>", PartNo);
         //   query = query.Replace("<toDt>", toDt);



            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(PartNo))
                filterString += "|  Part No.='" + PartNo + "' ";
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("118", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaShipping = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}