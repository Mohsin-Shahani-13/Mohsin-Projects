using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Xml.Linq;
using IP.Classess;
using System.IO;

namespace IP.Areas.ListingReports.Models
{
    public class DataWipeResult
    {
        cDAL oDAL;
        #region Fields
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }

        [Display(Name = "Contract:")]
        public string contract { get; set; }

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        //for Detail Header
        [Display(Name = "Machine Name:")]
        public string machineName { get; set; }
        [Display(Name = "Result:")]
        public string result { get; set; }
        [Display(Name = "Test Area:")]
        public string testArea { get; set; }
        [Display(Name = "Cell No.:")]
        public string cellNo { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public bool isAllDate { get; set; }
        [Display(Name = "Misc. Info:")]
        public string miscInfo { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstDataWipeResult { get; set; }
        public List<string> listFileUrls { get; set; }
        
        public List<Hashtable> lstDetail { get; set; }

        #endregion
        #region Methods 
        string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
        public DataTable Program() // Contract
        {
            string query = string.Empty;
            if (conType == "PROD")
            {
                oDAL = new cDAL("Redw");
                query += @"select ProgramId as Program FROM tia.DataWipeValidations";
            }
            else if (conType == "TEST" || conType == "TRAN")
            {
                oDAL = new cDAL("TESTREDW");
                query += @"select ProgramId as Program FROM tia.DataWipeValidations_Test ";
            }

            DataTable dt = oDAL.GetData(query);
            return dt;
        }

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
        private string GetInValue(string Value)
        {
            string[] arr = Value.Split(',');
            string _arr = null;
            foreach (var item in arr)
            {
                if (_arr == null)
                {
                    _arr = "\'" + item.Trim() + "\'";
                }
                else
                {
                    _arr += "," + "\'" + item.Trim() + "\'";
                }

            }
            return _arr;
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
                case "10066":
                    return " WHERE Contract IN (10066)";
                case "10067":
                    return " WHERE Contract IN (10067)";
                default:
                    return "";
            }
        }

        public bool GetList(string Program, string frmDt, string toDt, bool isAllDate, string serialNo, string partNo, string Contract)
        {
            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string query = string.Empty;
            string _SerialNo = GetInValue(serialNo);
                    query = @"
        SELECT ID
        , Contract
        , OrderNumber
        , SerialNumber
        , PartNumber
        ,CASE  
        WHEN Contract IN (10009, 21455) THEN StartTime --Grapevine
        WHEN Contract IN (10041, 12535) THEN FORMAT(redw.udf.ConvertTimeZone(StartTime, Contract), 'yyyy.MM.dd HH:mm')--SYDNEY
        WHEN Contract IN (10034, 12527) THEN FORMAT(redw.udf.ConvertTimeZone(StartTime, Contract), 'yyyy.MM.dd HH:mm')--Prague
        WHEN Contract IN (10042, 12534) THEN FORMAT(redw.udf.ConvertTimeZone(StartTime, Contract), 'yyyy.MM.dd HH:mm')--Tokyo
        WHEN Contract IN (10066) THEN FORMAT(redw.udf.ConvertTimeZone(StartTime, Contract), 'yyyy.MM.dd HH:mm')       --HAVANT
        WHEN Contract IN (10067) THEN FORMAT(redw.udf.ConvertTimeZone(StartTime, Contract), 'yyyy.MM.dd HH:mm')       --Reynosa
ELSE StartTime
        END AS StartTime
         ,CASE  
        WHEN Contract IN (10009, 21455) THEN EndTime --Grapevine
        WHEN Contract IN (10041, 12535) THEN FORMAT(redw.udf.ConvertTimeZone(EndTime, Contract), 'yyyy.MM.dd HH:mm')--SYDNEY
        WHEN Contract IN (10034, 12527) THEN FORMAT(redw.udf.ConvertTimeZone(EndTime, Contract), 'yyyy.MM.dd HH:mm')--Prague
        WHEN Contract IN (10042, 12534) THEN FORMAT(redw.udf.ConvertTimeZone(EndTime, Contract), 'yyyy.MM.dd HH:mm')--Tokyo
        WHEN Contract IN (10066) THEN FORMAT(redw.udf.ConvertTimeZone(EndTime, Contract), 'yyyy.MM.dd HH:mm')   --HAVANT
        WHEN Contract IN (10067) THEN FORMAT(redw.udf.ConvertTimeZone(EndTime, Contract), 'yyyy.MM.dd HH:mm')   --Reynosa
ELSE EndTime
        END AS EndTime
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
        ,CASE  
        WHEN Contract IN (10009, 21455) THEN AsOf --Grapevine
        WHEN Contract IN (10041, 12535) THEN FORMAT(redw.udf.ConvertTimeZone(AsOf, Contract), 'yyyy.MM.dd HH:mm')--SYDNEY
        WHEN Contract IN (10034, 12527) THEN FORMAT(redw.udf.ConvertTimeZone(AsOf, Contract), 'yyyy.MM.dd HH:mm')--Prague
        WHEN Contract IN (10042, 12534) THEN FORMAT(redw.udf.ConvertTimeZone(AsOf, Contract), 'yyyy.MM.dd HH:mm')--Tokyo
        WHEN Contract IN (10066) THEN FORMAT(redw.udf.ConvertTimeZone(AsOf, Contract), 'yyyy.MM.dd HH:mm')  --HAVANT
        WHEN Contract IN (10067) THEN FORMAT(redw.udf.ConvertTimeZone(AsOf, Contract), 'yyyy.MM.dd HH:mm')  --Reynosa
ELSE AsOf
        END AS AsOf
        ";
            if (conType == "PROD")
            {
                        oDAL = new cDAL("Redw");
                
                        query += @"FROM tia.DataWipeResult";
            }
            else if (conType == "TEST" || conType == "TRAN")
            {
                        oDAL = new cDAL("TESTREDW");
                        query += @" FROM tia.DataWipeResult_Test ";
            }

            if (Contract != "" && Program == "Meta")
            {
                query += GetContractValues(Contract);
                
            }
            else if(Program != "Meta")
            {
                query += " WHERE Program = '<Program>' "; 
            }

     //       query += @"WHERE Contract IN (" + Contract + ") ";
            if (isAllDate != true)
            {
                query += " AND CONVERT(Date, EndTime) >= '<frmDt>' AND CONVERT(Date,EndTime) <= '<toDt>' ";
            }
           //if (!string.IsNullOrEmpty(contract))
            //    query += "AND Contract = '" + contract + "' ";


            if (!string.IsNullOrEmpty(serialNo))
                //query += "AND SerialNumber LIKE '%" + serialNo + "%' ";
            query += " AND SerialNumber IN (" + _SerialNo + ") ";

            if (!string.IsNullOrEmpty(partNo))
                query += " AND PartNumber LIKE '%" + partNo + "%' ";

            query = query.Replace("<Program>", Program);
            // query = query.Replace("<Contract>", Contract);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            query += "ORDER BY EndTime DESC, AsOf DESC";


            DataTable dt = oDAL.GetData(query);

            //Filterstring
            filterString += " Program = '" + Program + "' ";
            if (isAllDate != true)
            {
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            }
            if (!string.IsNullOrEmpty(serialNo))
                filterString += " | Serial No. = '" + serialNo + "' ";

            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. Like '" + partNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("117", query, string.Empty, false);


            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDataWipeResult = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
            
        }
        #endregion
        public List<string> GetXmlFileUrls(string frmDt, string toDt, bool isAllDate, string serialNo, string partNo, string Program, string Contract)
        {
            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string query = string.Empty;
            string _SerialNo = GetInValue(serialNo);
            query = @"
                    SELECT LogFile
                        ";
            if (conType == "PROD")
            {
                oDAL = new cDAL("Redw");
                query += @" FROM tia.DataWipeResult ";
            }
            else if (conType == "TEST" || conType == "TRAN")
            {
                oDAL = new cDAL("TESTREDW");
                query += @" FROM tia.DataWipeResult_Test ";
            }

            //query += @" WHERE Contract IN (" + Contract + ") ";
            if (Program == "Meta")
            {
                query += GetContractValues(Contract);

            }
            else
            {
                query += " WHERE Program = '<Program>' ";
            }
            if (isAllDate != true)
            {
                query += " AND CONVERT(Date, EndTime) >= '<frmDt>' AND CONVERT(Date,EndTime) <= '<toDt>' ";
            }

            if (!string.IsNullOrEmpty(serialNo))
                query += " AND SerialNumber IN (" + _SerialNo + ") ";

            if (!string.IsNullOrEmpty(partNo))
                query += " AND PartNumber LIKE '%" + partNo + "%' ";

            query += " AND LogFile LIKE '\\%.xml' ";
            query += " AND Exported = 1 ";

            query = query.Replace("<Program>", Program);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            query += " ORDER BY EndTime DESC, AsOf DESC ";

            DataTable dt = oDAL.GetData(query);
            listFileUrls =  dt.AsEnumerable().Select(row => row.Field<string>("LogFile")).ToList();
            //listFileUrls = oDAL.GetData(query).ToString.to;
            


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

        public (string fileContents, string extension) GetContent(string Id)
        {
            
           
            
            using (new cImpersonate())
            {
                try
                {
                    string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

                    //oDAL = new cDAL("Redw");
                    string query = string.Empty;
                    string fileContents = string.Empty;
                    //string result = string.Empty;
                    string filePath = string.Empty;
                    string extension = string.Empty;

                    query = @"
            ---XML---
                    SELECT
                        LogFile ";
                    if (conType == "PROD")
                    {
                        oDAL = new cDAL("Redw");

                        query += @"FROM tia.DataWipeResult";
                    }
                    else if (conType == "TEST" || conType == "TRAN")
                    {
                        oDAL = new cDAL("TESTREDW");
                        query += @" FROM tia.DataWipeResult_Test ";
                    }

                    query += @" WHERE 
                        Program = 'Meta' AND 
                        Id = '<Id>'  
                        ORDER BY EndTime DESC, AsOf DESC
                      ";

                    query = query.Replace("<Id>", Id);

                    //oDAL = new cDAL("Redw");
                    //object result = oDAL.GetObject(query);
                    //if (result != null)
                    //{
                    //    filePath = result.ToString();

                    //}
                    filePath = oDAL.GetObject(query).ToString();
                    extension = Path.GetExtension(filePath);
                    fileContents = File.ReadAllText(filePath);
                    return (fileContents, extension); 
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error reading the file: {ex.Message}");
                    //return "";
                }
                return ("", "");
            }
            

            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            //oDAL = new cDAL("Redw");
            //string query = string.Empty;


            //query = @"
            //---XML---
            //        select 
            //           LogFile 
            //          from tia.DataWipeResult
            //          where Id = '<Id>' ";

            //query = query.Replace("<Id>", Id);



            //oDAL = new cDAL("Redw");
            //string _result = oDAL.GetObject(query).ToString() ;



            //return _result;

        }
        //public string GetTxt(string Id)
        //{
        //    using (new cImpersonate())
        //    {
        //        try
        //        {
        //            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

        //            oDAL = new cDAL("Redw");
        //            string query = string.Empty;
        //            string fileContents = string.Empty;
        //            string result = string.Empty;

        //            query = @"
        //    ---XML---
        //            SELECT
        //                LogFile
        //                FROM tia.DataWipeResult
        //                WHERE 
        //                Program = 'Meta' AND 
        //                Id = '<Id>' AND 
        //                CONVERT(Date, EndTime) >= '2015.08.01' AND CONVERT(Date,EndTime) <= '2023.08.17' 
        //                ORDER BY EndTime DESC, AsOf DESC
        //              ";

        //            query = query.Replace("<Id>", Id);

        //            oDAL = new cDAL("Redw");
        //            string filePath = oDAL.GetObject(query).ToString();
        //            fileContents = File.ReadAllText(filePath);


        //            if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
        //            {
        //                return fileContents;
        //            }
        //            else if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        //            {
        //                _xmlResult = XElement.Parse(fileContents);
        //            }

        //            Console.WriteLine("File contents:");
        //            Console.WriteLine(fileContents);
        //            return fileContents;
        //        }
        //        catch (IOException ex)
        //        {
        //            Console.WriteLine($"Error reading the file: {ex.Message}");
        //            return "";
        //        }
        //    }


            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();

            //oDAL = new cDAL("Redw");
            //string query = string.Empty;


            //query = @"
            //---XML---
            //        select 
            //           LogFile 
            //          from tia.DataWipeResult
            //          where Id = '<Id>' ";

            //query = query.Replace("<Id>", Id);



            //oDAL = new cDAL("Redw");
            //string _result = oDAL.GetObject(query).ToString() ;



            //return _result;

        //}
        public bool GetDetail(string Id)
        {
            //oDAL = new cDAL("Redw");
            string query = string.Empty;
            #region Header

             query = @"
                        SELECT ID
                        , SerialNumber
                        , PartNumber
                        , StartTime
                        , EndTime
                        , MachineName
                        , Result
                        , TestArea
                        , CellNumber
                        , Program
                        , MiscInfo 
                        ";


            if (conType == "PROD")
            {
                oDAL = new cDAL("Redw");
                query += @"FROM tia.DataWipeResult";
            }
            else if (conType == "TEST" || conType == "TRAN")
            {
                oDAL = new cDAL("TESTREDW");
                query += @" FROM tia.DataWipeResult_Test ";
            }

            query += @" WHERE  ID = '<Id>' ";

            query = query.Replace("<Id>", Id);

            filterString = "Id = '" + Id + "' ";

            //oDAL = new cDAL(HttpContext.Current.Request["DB"]);
            DataTable dtHeader = oDAL.GetData(query);



            if (dtHeader.Rows.Count > 0)
            {
                DataRow dr = dtHeader.Rows[0];
                serialNo = dr["SerialNumber"].ToString();
                partNo = dr["PartNumber"].ToString();
                fromDt = dr["StartTime"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["StartTime"]).ToString("yyyy.MM.dd HH:mm");
                toDt = dr["EndTime"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["EndTime"]).ToString("yyyy.MM.dd HH:mm");
                machineName = dr["MachineName"].ToString();
                result = dr["Result"].ToString();
                testArea = dr["TestArea"].ToString();
                cellNo = dr["CellNumber"].ToString();
                program = dr["Program"].ToString();
                miscInfo = dr["MiscInfo"].ToString();
            }
            #endregion

            query = @"
                     SELECT ID
                    , MainTestID
                    , TestIDNumber
                    , TestName
                    , TestDesc
                    , StartTime
                    , EndTime
                    , Result
                    , ErrorMessage
                    , ResultMessage
, resultunit ";
            if (conType == "PROD")
            {
                oDAL = new cDAL("Redw");
                query += @" FROM tia.SubTestLogs";
            }
            else if (conType == "TEST" || conType == "TRAN")
            {
                oDAL = new cDAL("TESTREDW");
                query += @" FROM tia.SubTestLogs_Test ";
            }

            //if (conType == "PROD")
            //{
            //    query += @" FROM tia.SubTestLogs";
            //}
            //else if (conType == "TEST")
            //{
            //    query += @" FROM tia.SubTestLogs_Test ";
            //}


            query += @" WHERE  MainTestID = '<MainTestID>' ";
          

            query = query.Replace("<MainTestID>", Id);

            filterString = "MainTestID = '" + Id + "' ";



            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("117-1", query, "Test Results Detail", false);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstDetail = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            else
            {
                return false;
            }


        }
    }
}
