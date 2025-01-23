using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class DriveInquiry
    { 

        [Display(Name = "Program:")]
        public string ProgramName { get; set; }
        [Display(Name = "Program:")]
        public string ProgramID { get; set; }

        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<KeyValue> PcbaValues { get; set; }
        public List<KeyValue> SerialValues { get; set; }
        public List<ArrayList> lstDriveInquiry { get; set; }

        //private readonly string connectionString = "Data Source=10.211.9.71;Initial Catalog=DmtDb;Persist Security Info=True;User ID=usrPRS;Password=mandy;";

        cDAL oDAL = new cDAL("ACTIVE");

        public bool GetList(string pcbaValues, string serialValues)
        {
            oDAL = new cDAL("ACTIVE");
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string Site = HttpContext.Current.Session["DefaultSite"].ToString();
            string sql = string.Empty;
            DataTable pcbaValuesdt = ConvertJsonToDataTable(pcbaValues);
            DataTable serialValuesdt = ConvertJsonToDataTable(serialValues);

            DataTable mergedDataTable = new DataTable("MergedTable");
            mergedDataTable.Columns.Add("Counter", typeof(string));
            mergedDataTable.Columns.Add("PCBA_Serials", typeof(string));
            mergedDataTable.Columns.Add("Serials", typeof(string));

            DataTable dt = new DataTable();
            dt.Columns.Add("Counter");
            dt.Columns.Add("Source");
            dt.Columns.Add("Serial No");
            dt.Columns.Add("PCBA Serial No");
            dt.Columns.Add("PCBA Box Id");
            dt.Columns.Add("Status");

            sql = @"SELECT [ID]
                    FROM [pls].[Program] 
                    WHERE Site = '<site>'";

            sql = sql.Replace("<site>", Site);

            string programId = Convert.ToString(oDAL.GetObject(sql));

            foreach (DataRow row1 in pcbaValuesdt.Rows)
            {
                string counter = row1["index"].ToString();
                string pcbaSerial = row1["Pcbavalue"].ToString();

                foreach (DataRow row2 in serialValuesdt.Rows)
                {
                    string serials = row2["Serialvalue"].ToString();
                    if (counter == row2["index"].ToString())
                    {
                        mergedDataTable.Rows.Add(counter, pcbaSerial, serials);
                    }
                }
            }
            foreach (DataRow objDataRow in mergedDataTable.Rows)
            {

                string Source = "";
                string tblSerialNo = "";
                string tblPcbaSerialNo = "";
                string pcbaBoxId = "";
                string Status = "";


                string Counter = objDataRow["Counter"].ToString();
                string PcbaSerialNo = objDataRow["PCBA_Serials"].ToString();
                string SerialNo = objDataRow["Serials"].ToString();

                //For Source value
                sql = @"SELECT psa.Value from pls.partSerialAttribute psa
                        INNER JOIN pls.CodeAttribute ca on ca.ID = psa.AttributeID
                        INNER JOIN pls.PartSerial ps ON psa.PartSerialID = ps.ID
                        WHERE ca.AttributeName = 'SOURCE'
                        AND ps.SerialNo = '<SerialNo>'
                        AND ps.ProgramID = '<programId>'";

                sql = sql.Replace("<SerialNo>", SerialNo);
                sql = sql.Replace("<programId>", programId);

                DataTable dtSource = oDAL.GetData(sql);
                if (dtSource.Rows.Count > 0)
                    Source = Convert.ToString(dtSource.Rows[0][0]);

                //For Serial No Value
                sql = @"SELECT ps.SerialNo from pls.partSerialAttribute psa
                        INNER JOIN pls.CodeAttribute ca on ca.ID = psa.AttributeID
                        INNER JOIN pls.PartSerial ps ON psa.PartSerialID = ps.ID 
                        WHERE Value = '<pcbaSerial>' 
                        AND ps.SerialNo = '<SerialNo>' 
                        AND ca.AttributeName = 'PCBA_SN'
                        AND ps.ProgramID = '<programId>'";

                sql = sql.Replace("<pcbaSerial>", PcbaSerialNo);
                sql = sql.Replace("<SerialNo>", SerialNo);
                sql = sql.Replace("<programId>", programId);

                DataTable dtSerialNo = oDAL.GetData(sql);
                if (dtSerialNo.Rows.Count > 0)
                    tblSerialNo = Convert.ToString(dtSerialNo.Rows[0][0]);


                //For PCBA Serial No Value
                sql = @"SELECT psa.Value from pls.partSerialAttribute psa
                        INNER JOIN pls.CodeAttribute ca on ca.ID = psa.AttributeID
                        INNER JOIN pls.PartSerial ps ON psa.PartSerialID = ps.ID 
                        WHERE Value = '<pcbaSerial>' 
                        AND ps.SerialNo = '<SerialNo>' 
                        AND ca.AttributeName = 'PCBA_SN'
                        AND ps.ProgramID = '<programId>'";

                sql = sql.Replace("<pcbaSerial>", PcbaSerialNo);
                sql = sql.Replace("<SerialNo>", SerialNo);
                sql = sql.Replace("<programId>", programId);

                DataTable dtPCBASerialNo = oDAL.GetData(sql);
                if (dtPCBASerialNo.Rows.Count > 0)
                    tblPcbaSerialNo = Convert.ToString(dtPCBASerialNo.Rows[0][0]);

                //For PCBA box id
                sql = @"Select psa.Value from pls.partSerialAttribute psa
                        INNER JOIN pls.CodeAttribute ca on ca.ID = psa.AttributeID
                        INNER JOIN pls.PartSerial ps ON psa.PartSerialID = ps.ID 
                        where ps.SerialNo = '<pcbaSerial>' 
                        AND ca.AttributeName = 'CartonNo'
                        AND ps.ProgramID = '<programId>'";

                sql = sql.Replace("<pcbaSerial>", PcbaSerialNo);
                sql = sql.Replace("<programId>", programId);

                DataTable dtPCBABoxId = oDAL.GetData(sql);
                if (dtPCBABoxId.Rows.Count > 0)
                    pcbaBoxId = Convert.ToString(dtPCBABoxId.Rows[0][0]);

                if (!string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(tblSerialNo) && !string.IsNullOrEmpty(tblPcbaSerialNo) && !string.IsNullOrEmpty(pcbaBoxId))
                    Status = "Matched";

                else
                    Status = "Not Matched";

                dt.Rows.Add(Counter, Source, SerialNo, PcbaSerialNo, pcbaBoxId, Status);
            }

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDriveInquiry = cCommon.ConvertDtToArrayList(dt);
                return true;
            }

        }
        static DataTable ConvertJsonToDataTable(string jsonString)
        {
            DataTable dataTable = new DataTable();

            try
            {
                // Remove surrounding double quotes
                jsonString = jsonString.Trim('\"');

                // Deserialize JSON string to List of KeyValuePairs
                var list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonString);

                // Extract column names from the first dictionary
                foreach (var item in list.First())
                {
                    dataTable.Columns.Add(item.Key, typeof(string));
                }

                // Add rows to the DataTable
                foreach (var dict in list)
                {
                    var row = dataTable.NewRow();
                    foreach (var item in dict)
                    {
                        row[item.Key] = item.Value;
                    }
                    dataTable.Rows.Add(row);
                }
            }
            catch (JsonReaderException ex)
            {
                // Handle JSON parsing errors
                Console.WriteLine("Error parsing JSON: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return dataTable;
        }
    }
}
public class KeyValue
{
    public int Index { get; set; }
    public string Value { get; set; }
}