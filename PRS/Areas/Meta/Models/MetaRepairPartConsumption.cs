using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaRepairPartConsumption
    {

        cDAL oDAL;

        //[Display(Name = "Serial No.:")]
        //public string serialNo { get; set; }
        //[Display(Name = "Part No.:")]
        //public string partNo { get; set; }

        //[Display(Name = "Contract:")]
        //public string contract { get; set; }

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        [Display(Name = "Program:")]
        public string program { get; set; }
        public bool isAllDate { get; set; }
        [Display(Name = "Misc. Info:")]
        public string miscInfo { get; set; }
        public string ReportTitle { get; set; }

        //public int xmlRowCount { get; set; }
        //public string serialNumbers { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstMetaRepairPartConsumption { get; set; }
        //public List<string> lstTestAreas { get; set; }
        //public List<string> listFileUrls { get; set; }


        //public List<Hashtable> lstDetail { get; set; }

        string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

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
        private void BulkInsertToDatabase(DataTable dataTable)
        {
            string conStr = @"Data Source=10.211.9.71;Initial Catalog=PlusRS;Persist Security Info=True;User ID=usrPRS;Password=mandy;"; 
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                connection.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    
                    bulkCopy.ColumnMappings.Add("SerialNo", "SerialNo");
                    bulkCopy.ColumnMappings.Add("UploadedBy", "UploadedBy");
                    bulkCopy.ColumnMappings.Add("UploadFrom", "UploadFrom");
                    bulkCopy.DestinationTableName = "PlusRS.meta.rptUploadedSN";
                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }

        public bool GetList(string programId, string programName, DataTable dataTable)
        {
            
            cDAL oDAL = new cDAL("ACTIVE");
            //string inputString = "value1,value2,value3,value4";
            //string[] values = serialNos.Split(',');
            string empName = HttpContext.Current.Session["EmpName"].ToString();
            string dltQuery = @"DELETE FROM PlusRS.meta.rptUploadedSN where UploadedBy = '" + empName + "' AND UploadFrom = 'PRS'";
            oDAL.Execute(dltQuery);
            BulkInsertToDatabase(dataTable);

            //foreach (string value in values)
            //{
            //    string insertQuery = @"INSERT INTO PlusRS.meta.rptUploadedSN(SerialNo, UploadedBy, UploadFrom) VALUES  (" + value + ", '" + empName + "', 'PRS')";
            //    oDAL.AddQuery(insertQuery);
            //}
            //oDAL.Execute();


            string query = string.Empty;
            //serialNumbers = serialNos;
            query = @"
SELECT 
    wh.ID, 
    ISNULL(wh.SourcePartNo, wh.PartNo) AS PartNo, 
    ISNULL(wh.SourceSerialNo, wh.serialNo) AS SerialNo, 
    cs.Description as StatusDescription, 
    wl.ComponentPartNo, 
    wl.QtyConsumed, 
    wu.QtyIssued, 
    wu.LastActivityDate, 
    cf.Description as FaultDescription, 
    cr.Description as RepairDescription,
    cws.Description as FaultWorkStation
FROM 
    pls.WOHeader wh
INNER JOIN pls.WOLine wl ON wl.WOHeaderID = wh.ID
INNER JOIN pls.WOUnit wu ON wu.WOLineID = wl.ID
LEFT JOIN pls.CodeStatus cs ON cs.ID = wh.StatusID
LEFT JOIN pls.CodeWorkStation cws ON cws.ID = wh.WorkStationID
LEFT JOIN pls.WOUnitCodes wc ON wc.WOUnitID = wu.ID
LEFT JOIN pls.CodeFault cf ON cf.ID = wc.FaultID
LEFT JOIN pls.CodeRepair cr ON cr.ID = wc.RepairID
WHERE 
    wh.ProgramID = '<ProgramID>'
AND 
    wh.ID IN (
        SELECT 
            MAX(wh.ID)
        FROM pls.WOHeader wh
        WHERE 
            wh.ProgramID = '<ProgramID>'
        AND 
            ISNULL(wh.SourceSerialNo, wh.serialNo) IN ( SELECT SerialNo FROM PlusRS.meta.rptUploadedSN where UploadedBy = '" + empName + "') ";


            //query += @" WHERE TestArea IN (" + formattedtestArea + ") ";

            //if (isAllDate != true)
            //{
            //    query += "AND CONVERT(Date, UploadTime) >= '<frmDt>' AND CONVERT(Date,UploadTime) <= '<toDt>' ";
            //}


            //if (!string.IsNullOrEmpty(serialNos))
            //    query += " AND SerialNumber IN (" + serialNos + ") ";

            //if (!string.IsNullOrEmpty(contract))
            //    query += @" AND Contract IN (" + contract + ") ";
            query = query.Replace("<ProgramID>", programId);
            //query = query.Replace("<frmDt>", frmDt);
            //query = query.Replace("<toDt>", toDt);
            query += " GROUP BY ISNULL(wh.SourceSerialNo, wh.serialNo)) ";

            DataTable dt = oDAL.GetData(query);

            filterString += "  >  Program = " + programName;
            //if (isAllDate != true)
            //{
            //    filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            //}

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("171", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaRepairPartConsumption = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}