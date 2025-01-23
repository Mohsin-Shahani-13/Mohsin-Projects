using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class TestArea
    {
        public string Category { get; set; }
        public string Tester { get; set; }
    }
    public class DownloadTestResult
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



        [Display(Name = "Test Area:")]
        public string testArea { get; set; }

        [Display(Name = "Program:")]
        public string program { get; set; }
        public bool isAllDate { get; set; }
        [Display(Name = "Misc. Info:")]
        public string miscInfo { get; set; }
        public string ReportTitle { get; set; }

        public int xmlRowCount { get; set; }
        //public string serialNumbers { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstDownloadTestResult { get; set; }
        public List<TestArea>  lstTestAreas { get; set; }
        public List<string> listFileUrls { get; set; }


        //public List<Hashtable> lstDetail { get; set; }

        string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
        string empName = HttpContext.Current.Session["EmpName"].ToString();

        //static string GetContractValues(string site)
        //{
        //    switch (site)
        //    {
        //        case "GRAPEVINE":
        //            return "10009,21455";
        //        case "PRAGUE":
        //            return "10034,12527";
        //        case "SYDNEY":
        //            return "10041,12535";
        //        case "TOKYO":
        //            return "10042,12534";
        //        default:
        //            return string.Empty;
        //    }
        //}
        //public DataTable GetSitewiseContract()
        //{// Contract
        //    oDAL = new cDAL("INIT");
        //    string sites = HttpContext.Current.Session["DefaultSite"].ToString();

        //    string query = string.Empty;
        //    query = @"select  Site, 
        //                      Name AS programName,
        //                      Contract
        //                     FROM dbo.SitewiseContract
        //              WHERE SITE = '<site>'
        //              ORDER BY NAME ";
        //    query = query.Replace("<site>", sites);
        //    DataTable dt = oDAL.GetData(query);
        //    return dt;
        //}
        //public DataTable Program() // Contract
        //{
        //    string query = string.Empty;
        //    if (conType == "PROD")
        //    {
        //        oDAL = new cDAL("Redw");
        //        query += @"select ProgramId as Program FROM tia.DataWipeValidations";
        //    }
        //    else if (conType == "TEST")
        //    {
        //        oDAL = new cDAL("TESTREDW");
        //        query += @"select ProgramId as Program FROM tia.DataWipeValidations_Test ";
        //    }
        //    DataTable dt = oDAL.GetData(query);
        //    return dt;
        //}
        public void GetTestAreas()
        {
            oDAL = new cDAL("INIT");
            lstTestAreas = new List<TestArea>();
            string site = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = "SELECT Category, Tester FROM rpt.MetaTestAreas";
            if (site != "GRAPEVINE")
                query += " Where Category = '" + "CTS'";
            
            DataTable dt = oDAL.GetData(query);

            foreach (DataRow row in dt.Rows)
            {
                lstTestAreas.Add(new TestArea
                {
                    Category = row["Category"].ToString(),
                    Tester = row["Tester"].ToString()
                });
            }
            
        }
        static string GetContractValues(string Contract)
        {
            switch (Contract)
            {
                case "10009":
                    return " WHERE Contract IN (10009,21455)";
                case "10034":
                    return " WHERE Contract IN (10034,12527)";
                case "10041":
                    return " WHERE Contract IN (10041,12535)";
                case "10042":
                    return " WHERE Contract IN (10042,12534)";
                default:
                    return " WHERE Contract = '" + Contract + "' ";
            }
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
        public bool GetList(string frmDt, string toDt, bool isAllDate, string testArea, string contract,string program, DataTable dataTable)
        {
            cDAL aDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            //string formattedserialNos = "'" + serialNos.Replace(",", "','") + "'";
            string formattedtestArea = "'" + testArea.Replace(",", "','") + "'";
            //serialNumbers = formattedserialNos;
            //serialNumbers = serialNos;
            //serialNumbers = dataTable;

            string dltQuery = @"DELETE FROM PlusRS.meta.rptUploadedSN where UploadedBy = '" + empName + "' AND UploadFrom = 'PRS'";
            aDAL.Execute(dltQuery);
            BulkInsertToDatabase(dataTable);

            query = @"
SELECT ID
, Contract
, OrderNumber
, SerialNumber
, PartNumber
, StartTime
, EndTime
, UploadTime
, MachineName
, Result
, FileReference
, substring(LogFile,1,9) AS LogFile
, RIGHT(LogFile, 3) AS Extension
, Exported
, TestArea
, CellNumber
, Program
, MiscInfo
, MACAddress
, Msg
, AsOf
";
            if (conType == "PROD")
            {
                oDAL = new cDAL("Redw");

                query += @"FROM tia.DataWipeResult";
            }
            else if (conType == "TEST")
            {
                oDAL = new cDAL("TESTREDW");
                query += @" FROM tia.DataWipeResult_Test ";
            }
            //if (program == "Meta")
            //{
            if (!string.IsNullOrEmpty(contract))
                query += GetContractValues(contract);

            //}
            //else
            //{
            //    query += " WHERE Program = '<Program>' ";
            //}
            query += @" AND TestArea IN (" + formattedtestArea + ") ";

            if (isAllDate != true)
            {
                query += "AND CONVERT(Date, UploadTime) >= '<frmDt>' AND CONVERT(Date,UploadTime) <= '<toDt>' ";
            }


            //if (!string.IsNullOrEmpty(serialNos))
                //query += " AND SerialNumber IN (" + serialNos + ") ";
            query += " AND SerialNumber IN ( SELECT SerialNo FROM PlusRS.meta.rptUploadedSN where UploadedBy = '" + empName + "' ) ";

            //if (!string.IsNullOrEmpty(contract))
            //    query += @" AND Contract IN (" + contract + ") ";

            //query = query.Replace("<Program>", program);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            query += "ORDER BY EndTime DESC, AsOf DESC";

            DataTable dt = oDAL.GetData(query);
            xmlRowCount = dt.AsEnumerable().Count(row => row.Field<string>("Extension") == "xml");
            filterString += "  >  Program = Meta";// + program ;
            if (isAllDate != true)
            {
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            }

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("167", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDownloadTestResult = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        public List<string> GetXmlFileUrls(string program, string frmDt, string toDt, bool isAllDate, string testArea, string contract)
        {
            
            cDAL aDAL = new cDAL("ACTIVE");
            //string dltQuery = @"DELETE FROM PlusRS.meta.rptUploadedSN where UploadedBy = '" + empName + "'";
            //aDAL.Execute(dltQuery);
            //BulkInsertToDatabase(serialNos);
            string query = string.Empty;
            string formattedtestArea = "'" + testArea.Replace(",", "','") + "'";
            query = @" SELECT LogFile ";

            if (conType == "PROD")
            {
                oDAL = new cDAL("Redw");

                query += @" FROM tia.DataWipeResult ";
            }
            else if (conType == "TEST")
            {
                oDAL = new cDAL("TESTREDW");
                query += @" FROM tia.DataWipeResult_Test ";
            }
            //if (program == "Meta")
            //{
            if (!string.IsNullOrEmpty(contract))
                query += GetContractValues(contract);

            //}
            //else
            //{
            //    query += " WHERE Program = '<Program>' ";
            //}
            query += @" AND TestArea IN (" + formattedtestArea + ") ";

            if (isAllDate != true)
            {
                query += " AND CONVERT(Date, UploadTime) >= '<frmDt>' AND CONVERT(Date,UploadTime) <= '<toDt>' ";
            }

            //if (!string.IsNullOrEmpty(serialNos))
            //    query += " AND SerialNumber IN (" + serialNos + ") ";

            query += " AND SerialNumber IN ( SELECT SerialNo FROM PlusRS.meta.rptUploadedSN where UploadedBy = '" + empName + "' ) ";

            //if (!string.IsNullOrEmpty(contract))
            //    query += @" AND Contract IN (" + contract + ") ";

            //query = query.Replace("<Program>", program);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            query += " AND LogFile LIKE '\\%.xml' ";
            query += " AND Exported = 1 ";
            query += " ORDER BY EndTime DESC, AsOf DESC ";

            DataTable dt = oDAL.GetData(query);
            listFileUrls = dt.AsEnumerable().Select(row => row.Field<string>("LogFile")).ToList();

            ////For SQL Documentation
            //cLog oLog = new cLog();
            //oLog.AddSqlQuery("117", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                //return false;
                return listFileUrls;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    return listFileUrls;
                //listFileUrls = cCommon.ConvertDtToHashTable(dt);
                //return true;
                return listFileUrls;
            }
        }
    }
}