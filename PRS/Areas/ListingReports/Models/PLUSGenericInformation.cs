using IP.Classess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class PLUSGenericInformation
    {
        cDAL oDAL;
        #region Fields
        //public List<ArrayList> lstTable { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string Program { get; set; }
        [Display(Name = "Status:")]
        public string Status { get; set; }
        [Display(Name = "Process")]
        public string Process { get; set; }
        [Display(Name = "Warehouse:")]
        public string Warehouse { get; set; }
        public string Parameter { get; set; }
        public string CtrlType { get; set; }
        public string CustomCtrlType { get; set; }
        public int RptId { get; set; }
        public bool HaveProgramId { get; set; }
        public bool isDetail { get; set; }

        public string TableName { get; set; }
        public List<Hashtable> lstReports { get; set; }
        public List<Hashtable> lstCustomtable { get; set; }
        public List<ArrayList> lstData { get; set; }
        public List<ArrayList> lstProgram { get; set; }
        public string _lstProgram { get; set; }
        public List<ArrayList> lstDataColumn { get; set; }
        public List<ArrayList> lstColDate { get; set; }
        public List<ArrayList> lstParameter { get; set; }
        public List<List<ArrayList>> AllTableArrayLists { get; set; }
        public List<List<Hashtable>> AllTableHashTables { get; set; }

        public List<ArrayList> lstValues { get; set; }
        public List<Dictionary<string, object>> lst_paramList { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string SerializedData { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public string ImageBase64String { get; set; }
        public DataTable dt { get; set; }
        #endregion
        #region methods
        public bool GetReportsList()
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            query = @"SELECT RptName, RptId, RptHasProgramId, RptInputField, RptHasOptionPage, RptLabel, RptOperator, RptCtrlType, IsDetail from dbo.GenericReports where RptActive = 1 AND DBtype = '@DBTYPE' ORDER BY RptName";

            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            query = query.Replace("@DBTYPE", conType);
            DataTable dt = oDAL.GetData(query);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstReports = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        public bool GetDetail(string RptName, string RptId, bool RptHasProgramId)
        {
            //oDAL = new cDAL("ACTIVE");
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            //DataTable dt = new DataTable();

            query = @"SELECT RptQuery FROM dbo.GenericReports WHERE RptId = " + RptId;

            string vQuery = oDAL.GetObject(query).ToString();
            if (RptHasProgramId == true)
            {
                program = HttpContext.Current.Session["ProgramForSite"].ToString();
                vQuery = vQuery.Replace("@PROGRAM_ID", program);
            }
            oDAL = new cDAL("ACTIVE");

            dt = oDAL.GetDataForGeneric(vQuery);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery(RptId, vQuery, RptName, false);

            DataTable dtList = dt.Clone();
            DataTable dtColHeader = cCommon.GenerateTransposedTable(dtList);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {

                if (dt.Rows.Count > 0)
                {
                    lstData = cCommon.ConvertDtToArrayList(dt);
                    lstDataColumn = cCommon.ConvertDtToArrayList(dtColHeader);

                }
                else if (dt.Rows.Count == 0)
                {
                    lstDataColumn = cCommon.ConvertDtToArrayList(dtColHeader);
                }
                return true;
            }
        }
        public bool GetDetailParam(string[] param, string contract, string programName, bool isAllDate, string frmDt, string toDate, string RptName, string RptId, bool RptHasProgramId, string statusId, string status, string rptType, string CGStatusId, string CGStatus, string ddProcess, string CGTableName, string PSWarehouse, string DDLDataFeed)
        {
            bool _isValid = false;
            string _getclause = this.getCaluse(RptId);
            string[] _clause = _getclause.Split(',');
            string _getLabel = this.getLabel(RptId);
            string[] _Label = _getLabel.Split(',');
            string _getOperator = this.getOperator(RptId);
            string[] _operator = _getOperator.Split(',');
            string[] _param = param[0].Split(',');
            string[] _addedQuery = new string[_param.Length];
            string getCtrlType = this.getCntrlType(RptId);
            string[] _getCtrlType = getCtrlType.Split(',');

            for (int i = 0; i < _clause.Length; i++)
            {
                if (_operator.All(string.IsNullOrEmpty))
                {
                    
                }
                else
                {
                    if (_param[i] != "")
                    {
                        if (_operator[i] == "%")
                        {
                            _addedQuery[i] = " " + _clause[i] + " LIKE '%" + _param[i] + "%'";
                        }
                        else if (_operator[i] == "=")
                        {
                            _addedQuery[i] = " " + _clause[i] + " = '" + _param[i] + "'";
                        }
                        else if (_operator[i] == "IN")
                        {
                            string _serialNo = GetInValue(_param);
                            _addedQuery[i] = " " + _clause[i] + " IN (" + _serialNo + ")";
                        }
                    }
                }

            }

            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            //DataTable dt = new DataTable();
            //DataTable listCount = GetCustomList(RptName);
            query = @"SELECT RptQuery FROM dbo.GenericReports WHERE RptId = " + RptId;

            string vQuery = oDAL.GetObject(query).ToString();

            if (RptHasProgramId == true)
            {
                if (contract != "0")
                {
                    vQuery = vQuery.Replace("@PROGRAM_ID", contract);
                }
                else if (contract == "0")
                {
                    program = HttpContext.Current.Session["ProgramForSite"].ToString();
                    vQuery = vQuery.Replace("@PROGRAM_ID", program);
                }

                for (int i = 0; i < _addedQuery.Length; i++)
                {
                    if (_addedQuery[i] != null)
                    {
                        vQuery += " AND " + _addedQuery[i];
                    }
                }
            }
            else
            {
                // Check if the main WHERE keyword is already in the query
                if (!CheckWhereKeyword(vQuery))
                {
                    // Add WHERE only if it's not already in the main query
                    foreach (var item in _addedQuery)
                    {
                        if (item != null)
                        {
                            vQuery += " WHERE ";
                            break;
                        }
                    }
                }

                int count = 0;
                for (int i = 0; i < _addedQuery.Length; i++)
                {
                    if (_addedQuery[i] != null)
                    {
                        //if (count == 0)
                        //{
                        //    vQuery += _addedQuery[i];
                        //}
                        //else
                        //{
                            vQuery += " AND " + _addedQuery[i];
                        //}
                        count++;
                    }
                }
            }

            if (_getCtrlType.Contains("STATUS"))
            {
                if (status != "0")
                {
                    vQuery = vQuery.Replace("@status_Id", status);
                }
                else
                {
                    vQuery = vQuery.Replace("@status_Id", "");
                }
            }
            if (_getCtrlType.Contains("RPT"))
            {
                vQuery = vQuery.Replace("@RptType", rptType);
            }
            if (_getCtrlType.Contains("DDLDataFeed"))
            {
                vQuery = vQuery.Replace("@DDLDataFeed", DDLDataFeed);
            }
            if (_getCtrlType.Contains("DDProcess"))
            {
                vQuery = vQuery.Replace("@Process_value", ddProcess);
            }
            vQuery = vQuery.Replace("<frmDt>", frmDt);
            vQuery = vQuery.Replace("<toDt>", toDate);

            // Assuming _clause and _param are arrays of the same length
            for (int i = 0; i < _clause.Length; i++)
            {
                // Check if the current clause has a match in the param array
                //if (!string.IsNullOrWhiteSpace(_param[i])) // Ensure the parameter is not empty or null
                //{
                // Dynamically construct the placeholder
                string clause = _clause[i]; // E.g., "SerialNo" or "PartNo"
                if (!string.IsNullOrEmpty(clause))
                {
                    string placeholder = "@" + ConvertToPlaceholder(clause); // Generate the placeholder dynamically

                    // Replace the placeholder in your query
                    if (!string.IsNullOrEmpty(placeholder) && !string.IsNullOrEmpty(_param[i]) && vQuery.Contains(placeholder))
                    {
                        vQuery = vQuery.Replace(placeholder, _param[i]);
                    }
                    else
                    {
                        vQuery = vQuery.Replace(placeholder, "");
                    }
                }
                //}
            }

            oDAL = new cDAL("ACTIVE");
            dt = oDAL.GetDataForGeneric(vQuery);

            if (!string.IsNullOrEmpty(contract))
            {
                if (contract == "undefined")
                {
                }
                else if (contract == "0")
                {
                    filterString += "> Program = '" + "ALL" + "' ";
                }
                else
                {
                    filterString += "> Program = '" + programName + "' ";
                }
            }

            //for (int i = 0; i < _Label.Length; i++)
            //{
            //    if (_param[i] != "" && _operator[i] == "%")
            //    {
            //        filterString += " | " + _Label[i] + " Like '" + _param[i] + "' ";
            //    }
            //    else if (_param[i] != "" && _operator[i] == "=")
            //    {
            //        filterString += " | " + _Label[i] + " = '" + _param[i] + "' ";
            //    }
            //}
            if (isAllDate != true)
            {
                if (frmDt != "undefined" && toDate != "undefined" && contract == "undefined")
                {
                    filterString += "> From = '" + frmDt + "' To = '" + toDate + "' ";
                }
                else if (frmDt != "undefined" && toDate != "undefined")
                {
                    filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";
                }
            }
            if (!string.IsNullOrEmpty(CGStatus))
            {
                if (CGStatus != "All")
                {
                    filterString += " | Status Description = '" + CGStatus + "' ";
                }
            }
            if (!string.IsNullOrEmpty(CGTableName))
            {
                if (CGTableName != "All")
                {
                    filterString += " | Generic Table Name = '" + CGTableName + "' ";
                }
            }

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery(RptId, vQuery, RptName, false);

            DataTable dtList = dt.Clone();
            DataTable dtColHeader = cCommon.GenerateTransposedTable(dtList);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                {
                    lstData = cCommon.ConvertDtToArrayList(dt);
                    lstDataColumn = cCommon.ConvertDtToArrayList(dtColHeader);
                }
                else if (dt.Rows.Count == 0)
                {
                    lstDataColumn = cCommon.ConvertDtToArrayList(dtColHeader);
                }
                return true;
            }
        }
        public bool GetDetailReport(string[] param, string contract, string programName, bool isAllDate, string frmDt, string toDate, string RptName, string RptId, bool RptHasProgramId, string statusId, string status, string rptType, string CGStatusId, string CGStatus, string ddProcess, string CGTableName, string PSWarehouse, string DDLDataFeed)
        {
            bool _isValid = false;
            string _getclause = this.getCaluse(RptId);
            string[] _clause = _getclause.Split(',');
            string _getLabel = this.getLabel(RptId);
            string[] _Label = _getLabel.Split(',');
            string _getOperator = this.getOperator(RptId);
            string[] _operator = _getOperator.Split(',');
            string[] _param = param[0].Split(',');
            string[] _addedQuery = new string[_param.Length];
            string getCtrlType = this.getCntrlType(RptId);
            string[] _getCtrlType = getCtrlType.Split(',');

            for (int i = 0; i < _clause.Length; i++)
            {
                if (_operator.All(string.IsNullOrEmpty))
                {

                }
                else
                {
                    if (_param[i] != "")
                    {
                        if (_operator[i] == "%")
                        {
                            _addedQuery[i] = " " + _clause[i] + " LIKE '%" + _param[i] + "%'";
                        }
                        else if (_operator[i] == "=")
                        {
                            _addedQuery[i] = " " + _clause[i] + " = '" + _param[i] + "'";
                        }
                        else if (_operator[i] == "IN")
                        {
                            string _serialNo = GetInValue(_param);
                            _addedQuery[i] = " " + _clause[i] + " IN (" + _serialNo + ")";
                        }
                    }
                }

            }

            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            //DataTable listCount = GetCustomList(RptName);
            query = @"SELECT RptQuery FROM dbo.GenericReports WHERE RptId = " + RptId;

            string vQuery = oDAL.GetObject(query).ToString();

            if (RptHasProgramId == true)
            {
                if (contract != "0")
                {
                    vQuery = vQuery.Replace("@PROGRAM_ID", contract);
                }
                else if (contract == "0")
                {
                    program = HttpContext.Current.Session["ProgramForSite"].ToString();
                    vQuery = vQuery.Replace("@PROGRAM_ID", program);
                }

                for (int i = 0; i < _addedQuery.Length; i++)
                {
                    if (_addedQuery[i] != null)
                    {
                        vQuery += " AND " + _addedQuery[i];
                    }
                }
            }
            else
            {
                // Check if the main WHERE keyword is already in the query
                if (!CheckWhereKeyword(vQuery))
                {
                    // Add WHERE only if it's not already in the main query
                    foreach (var item in _addedQuery)
                    {
                        if (item != null)
                        {
                            vQuery += " WHERE ";
                            break;
                        }
                    }
                }

                int count = 0;
                for (int i = 0; i < _addedQuery.Length; i++)
                {
                    if (_addedQuery[i] != null)
                    {
                        //if (count == 0)
                        //{
                        //    vQuery += _addedQuery[i];
                        //}
                        //else
                        //{
                        vQuery += " AND " + _addedQuery[i];
                        //}
                        count++;
                    }
                }
            }

            if (_getCtrlType.Contains("STATUS"))
            {
                if (status != "0")
                {
                    vQuery = vQuery.Replace("@status_Id", status);
                }
                else
                {
                    vQuery = vQuery.Replace("@status_Id", "");
                }
            }
            if (_getCtrlType.Contains("RPT"))
            {
                vQuery = vQuery.Replace("@RptType", rptType);
            }

            if (_getCtrlType.Contains("DDLDataFeed"))
            {
                vQuery = vQuery.Replace("@DDLDataFeed", DDLDataFeed);
            }
            if (_getCtrlType.Contains("DDProcess"))
            {
                vQuery = vQuery.Replace("@Process_value", ddProcess);
            }
            vQuery = vQuery.Replace("<frmDt>", frmDt);
            vQuery = vQuery.Replace("<toDt>", toDate);

            // Assuming _clause and _param are arrays of the same length
            for (int i = 0; i < _clause.Length; i++)
            {
                // Check if the current clause has a match in the param array
                //if (!string.IsNullOrWhiteSpace(_param[i])) // Ensure the parameter is not empty or null
                //{
                // Dynamically construct the placeholder
                string clause = _clause[i]; // E.g., "SerialNo" or "PartNo"
                if (!string.IsNullOrEmpty(clause))
                {
                    string placeholder = "@" + ConvertToPlaceholder(clause); // Generate the placeholder dynamically

                    // Replace the placeholder in your query
                    if (!string.IsNullOrEmpty(placeholder) && !string.IsNullOrEmpty(_param[i]) && vQuery.Contains(placeholder))
                    {
                        vQuery = vQuery.Replace(placeholder, _param[i]);
                    }
                    else
                    {
                        vQuery = vQuery.Replace(placeholder, "");
                    }
                }
                //}
            }

            oDAL = new cDAL("ACTIVE");
            DataSet DS = oDAL.GetDataSet(vQuery);

            if (!string.IsNullOrEmpty(contract))
            {
                if (contract == "undefined")
                {
                }
                else if (contract == "0")
                {
                    filterString += "> Program = '" + "ALL" + "' ";
                }
                else
                {
                    filterString += "> Program = '" + programName + "' ";
                }
            }
            for (int i = 0; i < _Label.Length; i++)
            {
                if (_param[i] != "")
                {
                    filterString += " | " + _Label[i] + " Like '" + _param[i] + "' ";
                }
                //else if (_param[i] != "" && _operator[i] == "=")
                //{
                //    filterString += " | " + _Label[i] + " = '" + _param[i] + "' ";
                //}
            }
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery(RptId, vQuery, RptName, false);

            if (!oDAL.HasErrors)
            {
                // Initialize the properties
                AllTableArrayLists = new List<List<ArrayList>>();
                AllTableHashTables = new List<List<Hashtable>>();

                // Iterate through all tables in the DataSet
                foreach (DataTable table in DS.Tables)
                {
                    // Convert the current table to an ArrayList and a Hashtable
                    List<ArrayList> arrayListForTable = cCommon.ConvertDtToArrayList(table);
                    List<Hashtable> hashTableForTable = ConvertDtToHashTable(table);

                    // Add the results to the respective lists
                    AllTableArrayLists.Add(arrayListForTable);
                    AllTableHashTables.Add(hashTableForTable);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        public static List<Hashtable> ConvertDtToHashTable(DataTable dt)
        {
            List<Hashtable> lstRows = new List<Hashtable>();

            // Add a special row for column headers if no data rows exist
            if (dt.Rows.Count == 0)
            {
                Hashtable columnHeaders = new Hashtable();
                foreach (DataColumn col in dt.Columns)
                {
                    columnHeaders.Add(col.ColumnName, null); // Add column names as keys with null as values
                }
                lstRows.Add(columnHeaders);
                return lstRows;
            }

            // Process data rows
            foreach (DataRow dr in dt.Rows)
            {
                Hashtable ht = new Hashtable();
                foreach (DataColumn col in dt.Columns)
                {
                    object columnValue = dr[col];
                    if (columnValue.GetType().Equals(typeof(decimal)))
                    {
                        if (col.ColumnName.ToUpper().Contains("PRICE") || col.ColumnName.ToUpper().Contains("COST") || col.ColumnName.ToUpper().Contains("Amount") || col.ColumnName.ToUpper().Contains("DECIMAL"))
                        {
                            columnValue = cCommon.SetFormat(columnValue, cCommon.Format.ForPrice);
                            if (columnValue.ToString() == "0.00") { columnValue = ""; }
                        }
                        else
                        {
                            columnValue = cCommon.SetFormat(columnValue, cCommon.Format.ForQty);
                            if (columnValue.ToString() == "0") { columnValue = ""; }
                        }
                    }
                    else if (columnValue.GetType().Equals(typeof(DateTime)))
                    {
                        if (columnValue.ToString().Length == 10 || columnValue.ToString().Contains("12:00:00 AM"))
                            columnValue = Convert.ToDateTime(columnValue).ToString("yyyy.MM.dd");
                        else
                            columnValue = Convert.ToDateTime(columnValue).ToString("yyyy.MM.dd HH:mm");
                    }

                    if (columnValue.ToString() == "0") { columnValue = ""; }
                    if (DBNull.Value == columnValue)
                    {
                        columnValue = "";
                    }
                    ht.Add(col.ColumnName, columnValue);
                }
                lstRows.Add(ht);
            }

            return lstRows;
        }


        // Helper method to convert clause names to placeholders
        static string ConvertToPlaceholder(string clause)
        {
            // Insert an underscore before uppercase letters and capitalize the first letter
            var placeholder = string.Concat(clause.Select((ch, idx) => idx > 0 && char.IsUpper(ch) ? "_" + ch : ch.ToString()));

            return placeholder; // Convert the entire string to uppercase for uniformity
        }
        private bool CheckWhereKeyword(string query)
        {
            int mainSelectIndex = query.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
            if (mainSelectIndex == -1)
                return false; // No SELECT in the query, assume no main WHERE clause

            int whereIndex = query.IndexOf("WHERE", mainSelectIndex, StringComparison.OrdinalIgnoreCase);
            if (whereIndex == -1)
                return false; // No WHERE keyword after the main SELECT

            // Check for subqueries by looking for additional SELECTs after the main SELECT
            int nextSelectIndex = query.IndexOf("SELECT", mainSelectIndex + 6, StringComparison.OrdinalIgnoreCase);

            // If WHERE comes before the next SELECT, it's in the main query
            return nextSelectIndex == -1 || whereIndex < nextSelectIndex;
        }

        public string getCaluse(string RptId)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            query = @"SELECT [RptInputField] FROM dbo.GenericReports WHERE RptId = " + RptId;
            string Clause = oDAL.GetObject(query).ToString();
            return Clause;
        }
        public string getLabel(string RptId)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            query = @"SELECT [RptLabel] FROM dbo.GenericReports WHERE RptId = " + RptId;
            string Label = oDAL.GetObject(query).ToString();
            return Label;
        }
        public string GetReportName(string RptId)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            DataTable dt = new DataTable();
            query = @"SELECT RptName FROM dbo.GenericReports WHERE RptId = " + RptId;
            string vQuery = oDAL.GetObject(query).ToString();
            return vQuery;

        }
        public string GetContract()
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            DataTable dt = new DataTable();
            query = HttpContext.Current.Session["ProgramForSite"].ToString();
            return query;

        }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"SELECT 
    ID AS programId,
    CAST(ID AS VARCHAR) + ' - ' + Name AS programName
FROM pls.PROGRAM  
WHERE 
    Site = 'MEXICALI' 
    AND Name IN ('GOPRO', 'ARLO', 'DELL')
ORDER BY Name;
 ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);

            return dt;
        }
        public DataTable GetStatus()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"SELECT 0 Id, ' - All - ' Description UNION SELECT Id, Description FROM pls.CodeStatus 
                    WHERE Description IN ('PARTIALLYSHIPPED', 'PARTIALLYRESERVED', 'NEW','SHIPPED', 'CANCELED', 'RESERVED') 
                    ORDER BY Description";
            //query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);

            return dt;
        }
        public DataTable StatusForCodeGenericView()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select distinct CS.ID, CS.Description
                    from pls.CodeStatus cs
                    INNER JOIN pls.CodeGenericTable cg ON cg.StatusID = CS.ID
                    ORDER BY CS.Description";
            //query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);

            return dt;
        }

        public DataTable WarehouseForPartSerialView(int ProgramId)
        {
            oDAL = new cDAL("ACTIVE");
            string program = String.Empty;
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            program = HttpContext.Current.Session["ProgramForSite"].ToString();

            string query = string.Empty;
            query = @"SELECT distinct Warehouse
FROM pls.PartSerial ps with (nolock)
INNER JOIN pls.PartLocation pl with (nolock) on pl.ProgramID = ps.ProgramID and ps.LocationID = pl.ID 
inner join pls.program p with (nolock) on p.ID = ps.ProgramID

 ";
            //query += "WHERE ps.ProgramID IN (10058) ";

            query += @"where p.ID = <program>
ORDER BY Warehouse ";
            query = query.Replace("<sites>", sites);
            query = query.Replace("<program>", ProgramId.ToString());
            DataTable dt = oDAL.GetData(query);

            return dt;
        }
        public DataTable GetProcessList()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @" SELECT DISTINCT 
    SUBSTRING(REPLACE(ca.AttributeName, 'BOM_', ''), 0, CHARINDEX('_', REPLACE(ca.AttributeName, 'BOM_', ''))) AS Process
FROM pls.PartSerial ps
INNER JOIN pls.PartSerialAttribute pa ON pa.PartSerialID = ps.ID
INNER JOIN pls.CodeAttribute ca ON ca.ID = pa.AttributeID
WHERE ps.ProgramID = 10055 
  AND ca.AttributeName LIKE 'BOM_%'
  AND pa.[Value] IS NOT NULL -- Exclude empty values
ORDER BY Process;
";
            //query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);

            return dt;
        }
        public string getCntrlType(string RptId)

        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            query = @"SELECT [RptCtrlType] FROM dbo.GenericReports WHERE RptId = " + RptId;
            string cntrlType = oDAL.GetObject(query).ToString();
            return cntrlType;
        }
        public string getOperator(string RptId)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            query = @"SELECT [RptOperator] FROM dbo.GenericReports WHERE RptId = " + RptId;
            string cntrlType = oDAL.GetObject(query).ToString();
            return cntrlType;
        }
        public bool GetImage(string imageDataID)
        {
            cDAL oDAL = new cDAL("ACTIVE");
            cLog oLog = new cLog();
            string query = string.Empty;
            query = @"SELECT 
                Picture, 
                FilePath 
                    FROM 
                [plusimage].plusimage.pls.imagedata 
                    WHERE 
                id = '<imageDataID>'
";
            query = query.Replace("<imageDataID>", imageDataID);

            DataTable dt = oDAL.GetData(query);
            if (dt.Rows.Count == 1)
            {
                byte[] imageBytes = dt.Rows[0]["Picture"] as byte[];
                if (imageBytes != null)
                {
                    ImageBase64String = Convert.ToBase64String(imageBytes);
                }
                else
                {
                    try
                    {
                        string filePath = dt.Rows[0]["FilePath"] as string;
                        using (new cImpersonate())
                        {
                            byte[] imageDataFromFilePath = File.ReadAllBytes(filePath);
                            ImageBase64String = Convert.ToBase64String(imageDataFromFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        oLog.RecordError(ex.Message, ex.StackTrace, "Report: CodeTableListing - Method: GetImage(string imageDataID)");
                    }

                }

            }

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                //if (dt.Rows.Count > 0)
                //    lstImage = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        private string GetInValue(string[] arr)
        {
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
        #endregion
    }
}