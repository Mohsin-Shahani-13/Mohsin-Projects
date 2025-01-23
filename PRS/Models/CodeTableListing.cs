using IP.Classess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace IP.Models
{
    public class CodeTableListing
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
        [Display(Name = "Code Generic Table Name:")]
        public string GenericTableName { get; set; }
        [Display(Name = "Warehouse:")]
        public string Warehouse { get; set; }
        public string Parameter { get; set; }
        public string CtrlType { get; set; }
        public string CustomCtrlType { get; set; }
        public int viewId { get; set; }
        public bool HaveProgramId { get; set; }
        public string TableName { get; set; }
        public List<Hashtable> lstTable { get; set; }
        public List<Hashtable> lstCustomtable { get; set; }
        public List<ArrayList> lstData { get; set; }
        public List<ArrayList> lstProgram { get; set; }
        public string _lstProgram { get; set; }
        public List<ArrayList> lstDataColumn { get; set; }
        public List<ArrayList> lstColDate { get; set; }
        public List<ArrayList> lstParameter { get; set; }

        public List<ArrayList> lstValues { get; set; }
        public List<Dictionary<string, object>> lst_paramList { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public string ImageBase64String { get; set; }
        #endregion

        public bool GetList()
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            // query = @"SELECT TABLE_NAME AS CodeTables FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'View' AND TABLE_NAME LIKE 'vCode%' OR TABLE_NAME LIKE 'vPart%' ORDER BY TABLE_NAME ";
            query = @"SELECT ViewName AS CodeTables,ViewId , ViewHasProgramId ,ViewInputField,ViewHasOptionPage,ViewLabel, ViewOperator, ViewCtrlType FROM dbo.PlusViews where ViewActive =1 AND DBtype = '@DBTYPE' ORDER BY ViewName";

            string Site = HttpContext.Current.Session["CONN_TYPE"].ToString();
            query = query.Replace("@DBTYPE", Site);
            DataTable dt = oDAL.GetData(query);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstTable = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        public DataTable GetCustomList(string tblName)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            // query = @"SELECT TABLE_NAME AS CodeTables FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'View' AND TABLE_NAME LIKE 'vCode%' OR TABLE_NAME LIKE 'vPart%' ORDER BY TABLE_NAME ";
            query = @"SELECT 
	 CV.ViewName AS CustomCodeTables
	 ,CASE WHEN CV.ViewCustomQuery <> '' THEN 'Y'
	 ELSE 'N' END AS CustomQuery
	,PV.ViewName AS CodeTables
	,PV.ViewId
	,CASE 
    WHEN CV.ViewCustomQuery <> 'N' THEN 1
    ELSE 0 
	END AS ViewHasProgramId

	--,PV.ViewHasProgramId 
	,PV.ViewInputField
	,PV.ViewHasOptionPage
	,PV.ViewLabel
	,PV.ViewOperator
	,PV.ViewCtrlType 
FROM dbo.PlusViews PV

LEFT OUTER JOIN [PlusRS].[dbo].[PlusCustomViews] CV ON CV.ViewName = PV.ViewName

WHERE PV.ViewActive = 1 AND PV.DBtype = '@DBTYPE' AND CV.ViewName = '@ViewName'
ORDER BY PV.ViewName;";

            string Site = HttpContext.Current.Session["CONN_TYPE"].ToString();
            query = query.Replace("@DBTYPE", Site);
            query = query.Replace("@ViewName", tblName);
            DataTable dt = oDAL.GetData(query);

            if (dt.Rows.Count > 0)
            {
                CtrlType = "DDL";
            }
            return dt;

        }
        public bool GetDetail(string tblName, string ViewId, bool ViewHasProgramId)
        {
            //oDAL = new cDAL("ACTIVE");
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            DataTable listCount = GetCustomList(tblName);
            if (listCount.Rows.Count > 0)
            {
                query = @"SELECT [ViewCustomQuery] FROM [dbo].[PlusCustomViews] Where ViewName = '" + tblName + "' ";
            }
            else
            {
                query = @"SELECT ViewQuery FROM dbo.PlusViews WHERE ViewId = " + ViewId;
            }
            string vQuery = oDAL.GetObject(query).ToString();
            if (ViewHasProgramId == true)
            {
                program = HttpContext.Current.Session["ProgramForSite"].ToString();
                vQuery = vQuery.Replace("@PROGRAM_ID", program);
            }
            oDAL = new cDAL("ACTIVE");

            dt = oDAL.GetData(vQuery);

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
        public bool GetDetailParam(string[] param, string contract, string programName, bool isAllDate, string frmDt, string toDate, string tblName, string ViewId, bool ViewHasProgramId, string statusId, string status, string dockType, string CGStatusId, string CGStatus, string CGTableId, string CGTableName, string PSWarehouse)
        {
            bool _isValid = false;
            string _getclause = this.getCaluse(ViewId);
            string[] _clause = _getclause.Split(',');
            string _getLabel = this.getLabel(ViewId);
            string[] _Label = _getLabel.Split(',');
            string _getOperator = this.getOperator(ViewId);
            string[] _operator = _getOperator.Split(',');
            string[] _param = param[0].Split(',');
            string[] _addedQuery = new string[_param.Length];
            string getCtrlType = this.getCntrlType(ViewId);
            string[] _getCtrlType = getCtrlType.Split(',');

            for (int i = 0; i < _clause.Length; i++)
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

            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            DataTable listCount = GetCustomList(tblName);
            if (listCount.Rows.Count > 0)
            {
                ViewHasProgramId = true;
                query = @"SELECT [ViewCustomQuery] FROM [dbo].[PlusCustomViews] Where ViewName = '" + tblName + "' ";
                //program = HttpContext.Current.Session["ProgramForSite"].ToString();
                //query = query.Replace("@PROGRAM_ID", program);
            }
            else
            {
                query = @"SELECT ViewQuery FROM dbo.PlusViews WHERE ViewId = " + ViewId;
            }
            string vQuery = oDAL.GetObject(query).ToString();

            if (ViewHasProgramId == true)
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
                foreach (var item in _addedQuery)
                {
                    if (item != null)
                    {
                        vQuery += " Where ";
                        break;
                    }
                }
                int count = 0;
                for (int i = 0; i < _addedQuery.Length; i++)
                {
                    if (_addedQuery[i] != null)
                    {
                        if (count == 0)
                        {
                            vQuery += _addedQuery[i];
                        }
                        else
                        {
                            vQuery += " AND " + _addedQuery[i];
                        }
                        count++;
                    }
                }
            }
            if (_getCtrlType.Contains("STATUS"))
            {
                if (status != "All")
                {
                    if (!vQuery.Contains("WHERE"))
                    {
                        vQuery += @" WHERE StatusDescription = '" + status + "'";
                    }
                    else if (vQuery.Contains("WHERE"))
                    {
                        vQuery += @"AND StatusDescription = '" + status + "'";
                    }
                }
                if (dockType == "Dock Log")
                {
                    vQuery += @"AND RODockLogID is not null";
                }
                else if (dockType == "Ship Order")
                {
                    vQuery += @"AND SOHeaderID is not null";
                }
                else if (dockType == "Work Order")
                {
                    vQuery += @"AND WOHeaderID is not null";
                }

                //vQuery = vQuery.Replace("<toDt>", toDate);
            }
            if (_getCtrlType.Contains("CGStatus"))
            {
                if (CGStatus != "All")
                {
                    if (!vQuery.Contains("WHERE"))
                    {
                        vQuery += @" WHERE StatusDescription = '" + CGStatus + "'";
                    }
                    else if (vQuery.Contains("WHERE"))
                    {
                        vQuery += @"AND StatusDescription = '" + CGStatus + "'";
                    }
                }
                if (CGTableName != "All")
                {
                    if (!vQuery.Contains("WHERE"))
                    {
                        vQuery += @"WHERE GenericTableName = '" + CGTableName + "'";
                    }
                    else
                    {
                        vQuery += @"AND GenericTableName = '" + CGTableName + "'";
                    }
                }

                //vQuery = vQuery.Replace("<toDt>", toDate);
            }

            if (_getCtrlType.Contains("PSWarehouse"))
            {
                if (PSWarehouse != "All")
                {

                    if (!vQuery.Contains("WHERE"))
                    {
                        vQuery += @"WHERE Warehouse = '" + PSWarehouse + "'";
                    }
                    else
                    {
                        vQuery += @"AND Warehouse = '" + PSWarehouse + "'";
                    }

                }
                
            }
            if (isAllDate != true)
            {
                if (frmDt != "undefined" && toDate != "undefined")
                {
                    if (ViewHasProgramId != true)
                    {
                        for (int i = 0; i < _addedQuery.Length; i++)
                        {
                            if (_addedQuery[i] == null)
                            {
                                if (!vQuery.Contains("Where"))
                                {
                                    vQuery += @" WHERE CONVERT(Date, A.CreateDate) >=  '<frmDt>' AND CONVERT(Date, A.CreateDate) <= '<toDt>'";
                                    break;
                                }
                            }
                            else if (vQuery.Contains("Where"))
                            {
                                vQuery += @" AND CONVERT(Date, A.CreateDate) >=  '<frmDt>' AND CONVERT(Date, A.CreateDate) <= '<toDt>'";
                                break;
                            }
                        }
                    }
                    else
                    {
                        vQuery += @" AND CONVERT(Date, A.CreateDate) >=  '<frmDt>' AND CONVERT(Date, A.CreateDate) <= '<toDt>'";
                    }
                }
                vQuery = vQuery.Replace("<frmDt>", frmDt);
                vQuery = vQuery.Replace("<toDt>", toDate);
                if (frmDt != "undefined" && toDate != "undefined")
                {
                    vQuery += @" Order By A.CreateDate DESC";
                }
            }


            oDAL = new cDAL("ACTIVE");
            dt = oDAL.GetData(vQuery);

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
                if (_param[i] != "" && _operator[i] == "%")
                {
                    filterString += " | " + _Label[i] + " Like '" + _param[i] + "' ";
                }
                else if (_param[i] != "" && _operator[i] == "=")
                {
                    filterString += " | " + _Label[i] + " = '" + _param[i] + "' ";
                }
            }
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
        public string getCaluse(string viewId)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            query = @"SELECT [ViewInputField] FROM dbo.PlusViews WHERE ViewId = " + viewId;
            string Clause = oDAL.GetObject(query).ToString();
            return Clause;
        }
        public string getLabel(string viewId)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            query = @"SELECT [ViewLabel] FROM dbo.PlusViews WHERE ViewId = " + viewId;
            string Label = oDAL.GetObject(query).ToString();
            return Label;
        }
        public string GetReportName(string ViewId)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            DataTable dt = new DataTable();
            query = @"SELECT ViewName FROM dbo.PlusViews WHERE ViewId = " + ViewId;
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
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);

            return dt;
        }
        public DataTable GetStatus()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select distinct CS.ID, CS.Description
                    from pls.CodeStatus cs
                    INNER JOIN pls.CrossDock cd ON cd.StatusID = CS.ID
                    ORDER BY CS.Description ";
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
        public DataTable CodeGenericTableName()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @" SELECT distinct A.Name from pls.CodeGenericTableDefinition A
INNER JOIN pls.CodeGenericTable cg on cg.GenericTableDefinitionID = A.ID
ORDER BY A.Name
";
            //query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);

            return dt;
        }
        public string getCntrlType(string viewId)

        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            query = @"SELECT [ViewCtrlType] FROM dbo.PlusViews WHERE ViewId = " + viewId;
            string cntrlType = oDAL.GetObject(query).ToString();
            return cntrlType;
        }
        public string getOperator(string viewId)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            string program = String.Empty;
            DataTable dt = new DataTable();
            query = @"SELECT [ViewOperator] FROM dbo.PlusViews WHERE ViewId = " + viewId;
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

    }
}