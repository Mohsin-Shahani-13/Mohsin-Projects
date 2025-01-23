using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.IO;
using System.IO.Compression;
using IP.Classess;

namespace IP.Areas.Meta.Models
{
    public class FacebookInhouseTPPL
    {

        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        [Display(Name = "Warehouse:")]
        public string warehouse { get; set; }
        [Display(Name = "Part No.:")]
        public string PartNo { get; set; }

        public string outputCsvPath { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstInhouseTPPL { get; set; }
        public List<ArrayList> lstWarehouse { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetWareHouse(string ProgramId)
        {
            //string programName = HttpContext.Current.Session["ProgramForSite"].ToString();
            string _programId = GetInValue(ProgramId);
            string query = string.Empty;
            query = @"
--Declare @ProgramId int = <programId>

SELECT pl.Warehouse
FROM pls.PartSerial ps
INNER JOIN pls.PartLocation pl on pl.ProgramID = ps.ProgramID and ps.LocationID = pl.ID
INNER JOIN pls.CodeStatus cs ON cs.ID = ps.StatusID
where ps.ProgramID IN <programId>
and cs.Description not in ('SHIPPED', 'CONSUMED')

union

select pl.Warehouse 
from pls.PartQty pq
INNER JOIN pls.PartLocation pl on pl.ProgramId = pq.ProgramId and pl.ID = pq.LocationID
where pq.ProgramId IN <programId>
and pq.AvailableQty > 0 
order by pl.Warehouse ";

            query = query.Replace("<programId>", "(" + _programId + ")");
            DataTable dt = oDAL.GetData(query);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWarehouse = cCommon.ConvertDtToArrayList(dt);
                return true;

            }
        }
        public bool GetList(string programId, string ProgramName, string partNo, string warehouse)
        {
            string _warehouse = GetInValue(warehouse);
            string _partNo = GetInValue(partNo);


            string query = string.Empty;
            query = @"
            SELECT * 
            FROM 
            [PlusRS].[meta].[rptInhousingTPPL] 
            where ProgramID = <programId>
            <partNo>
            <warehouse>
            ";
            if (!string.IsNullOrEmpty(warehouse))
            {
                query = query.Replace("<warehouse>", "AND Warehouse IN (" + _warehouse + ")");
            }
            else
            {
                query = query.Replace("<warehouse>", " ");
            }
            if (!string.IsNullOrEmpty(partNo))
            {
                query = query.Replace("<partNo>", "AND partNo IN (" + _partNo + ")");
            }
            else
            {
                query = query.Replace("<partNo>", " ");
            }
            query = query.Replace("<programId>", programId);



            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(warehouse))
            {
                int numberOfElements = _warehouse.Split(',').Length;
                if (numberOfElements > 4) filterString += " | Warehouse = Multiple";
                else filterString += " | Warehouse = '" + _warehouse + "' ";

            }
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("166", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstInhouseTPPL = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        public bool Download(string programId, string ProgramName, string partNo, string warehouse)
        {
            string query = string.Empty;
            string _warehouse = GetInValue(warehouse);
            string _partNo = GetInValue(partNo);

            //outputCsvPath = @"\\TPDC01S213\Shared\Testing\MetaInhouseTPPL.csv.gz";
            outputCsvPath = @"C:\TPPLTemp1\MetaInhouseTPPL.csv.gz";
            query = @"
SELECT 
    ProgramId AS 'Program Id',
    SerialNo 'Serial No.',
    ServiceRequestNo  AS 'Service Request No.',
    FORMAT(RegDate, 'yyyy.MM.dd HH:mm:ss') AS 'Reg. Date',
    State,
    PartNo AS 'Part No.',
    Description,
    ModelNo AS 'Model No.',
    LocationNo AS 'Location No.',
    PalletLocation AS 'Pallet Location',
    Disposition,
Configuration,
    WorkCenterNo AS 'Work Center No.',
    WorkstationDescription AS 'Workstation Description',
    OrderNo AS 'Order No',
    TypeDesignation AS 'Type Designation',
    Qty,
    Warehouse,
    OrderState AS 'Order State',
    Wdr,
    LastTestResult AS 'Last Test Result',
    LastFaultTesterOrManual AS 'Last Fault Tester Or Manual',
    TriageGrade AS 'Triage Grade',
    FinalGrade AS 'Final Grade',
    WoMigratedFailed AS 'Wo Migrated Failed'
FROM PlusRS.meta.rptInhousingTPPL WITH (NOLOCK) WHERE ProgramId = <programId> <partNo> <warehouse> ";
            
            if (!string.IsNullOrEmpty(warehouse))
            {
                query = query.Replace("<warehouse>", "AND Warehouse IN (" + _warehouse + ")");
            }
            else
            {
                query = query.Replace("<warehouse>", " ");
            }
            if (!string.IsNullOrEmpty(partNo))
            {
                query = query.Replace("<partNo>", "AND partNo IN (" + _partNo + ")");
            }
            else
            {
                query = query.Replace("<partNo>", " ");
            }
            query = query.Replace("<programId>", programId);
            
            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(warehouse))
                filterString += " | Warehouse = '" + _warehouse + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("166", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                {
                    string writeSuccessFlag = WriteCsvWithCompression(dt, outputCsvPath);
                    if (!string.IsNullOrEmpty(writeSuccessFlag))
                    {
                        ErrorMessage = writeSuccessFlag;
                        return false;
                    }
                    return true;
                }
                ErrorMessage = "No Record Found.";
                return false;

            }
        }
        static string WriteCsvWithCompression(DataTable dataTable, string filePath)
        {
            // File writing Exception: Try and Catch
            try
            {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
                        {
                            using (StreamWriter writer = new StreamWriter(gzipStream))
                            {

                                // Writing column headers
                                for (int i = 0; i < dataTable.Columns.Count; i++)
                                {
                                    writer.Write($"{dataTable.Columns[i].ColumnName},");
                                }
                                writer.WriteLine();

                                // Writing data rows
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    for (int i = 0; i < dataTable.Columns.Count; i++)
                                    {
                                        //writer.Write($"{row[i]},");
                                        writer.Write($"\"{row[i]}\",");
                                    }
                                    writer.WriteLine();
                                }
                            }
                        }
                    }
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private string GetInValue(string Value)
        {
            string[] arr = Value.Split(',');
            string _arr = null;
            foreach (var item in arr)
            {
                if (_arr == null)
                {
                    _arr = "\'" + item + "\'";
                }
                else
                {
                    _arr += "," + "\'" + item + "\'";
                }

            }
            return _arr;
        }
        #endregion
    }
}