using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Jobs.Models
{
    public class HighCPUConsumedQueries
    {
        public string ReportTitle { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        [Display(Name = "Servers:")]
        public string ServerNames { get; set; }
        [Display(Name = "Execution Count:")]
        public string Count { get; set; }
        public List<Hashtable> lstJobs1 { get; set; }
        public List<Hashtable> lstServers { get; set; }
        public DataTable listOfServers { get; set; }
        List<cDAL> connectionsList = new List<cDAL>();

        public List<Hashtable> listOfLists = new List<Hashtable>();
        public DataTable GetServerNames(string serverName)
        {
            string query = string.Empty;
            #region connections
            connectionsList.Add(new cDAL("JOB1"));
            connectionsList.Add(new cDAL("JOB2"));
            connectionsList.Add(new cDAL("JOB3"));
            connectionsList.Add(new cDAL("JOB4"));
            connectionsList.Add(new cDAL("JOB5"));
            #endregion
            if (listOfServers == null)
                listOfServers = new DataTable();

            foreach (cDAL oDAL in connectionsList)
            {
                query = @"select distinct @@SERVERNAME as [serverName]
                    from msdb.dbo.sysjobs ";

                query += "ORDER BY @@SERVERNAME ";

                DataTable dt = oDAL.GetData(query);

                if (!oDAL.HasErrors)
                {
                    lstServers = cCommon.ConvertDtToHashTable(dt);
                    listOfServers.Merge(dt);
                }
            }
            return listOfServers;
        }
        public bool GetList(string server, string count)
        {
            string query = string.Empty;
            int firstNumber = 0, secondNumber = 0;
            if (count != "All")
            {
                ExtractNumbers(count, out firstNumber, out secondNumber);
            }
            
            connectionsList.Add(new cDAL("JOB1"));
            connectionsList.Add(new cDAL("JOB2"));
            connectionsList.Add(new cDAL("JOB3"));
            connectionsList.Add(new cDAL("JOB4"));
            connectionsList.Add(new cDAL("JOB5"));
            foreach (cDAL oDAL in connectionsList)
            {
                query = @"
SELECT @@Servername as server_name,
    qs.last_worker_time / (1000000 * 60) AS cpu_time_min,
    qs.last_elapsed_time / (1000000 * 60) AS elapsed_time_min,
	qs.execution_count,
	qs.last_execution_time,
	qs.last_rows as rows_fetched,
	st.text AS batch_text,
    SUBSTRING(st.TEXT, (qs.statement_start_offset / 2) + 1, ((CASE qs.statement_end_offset WHEN - 1 THEN DATALENGTH(st.TEXT) ELSE qs.statement_end_offset END - qs.statement_start_offset) / 2) + 1) AS statement_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(sql_handle) st
where qs.last_worker_time / (1000000 * 60) > 0";

                if (server != "All")
                {
                    query += "AND @@SERVERNAME ='" + server + "'";
                }
                if (count != "All")
                {
                    query += "AND execution_count between '"+ firstNumber + "' AND '"+ secondNumber +"'";
                }
                query += " ORDER BY(qs.total_worker_time / qs.execution_count) DESC";

                //DataTable dt = oDAL.GetData(query);
                DataSet DS = oDAL.GetDataSet(query);

                //For SQL Documentation
                cLog oLog = new cLog();
                oLog.AddSqlQuery("164", query, string.Empty);

                if (!string.IsNullOrEmpty(server))
                {
                    filterString = "Server = '" + server + "'";
                }

                if (!string.IsNullOrEmpty(count))
                {
                    filterString += " | Execution Count = '" + count + "'";
                }

                if (!oDAL.HasErrors)
                {
                    List<ArrayList> lstJob = new List<ArrayList>();

                    lstJobs1 = cCommon.ConvertDtToHashTable(DS.Tables[0]);

                    listOfLists.AddRange(lstJobs1);
                }
                else
                {
                    ErrorMessage = oDAL.ErrMessage;
                    return false;
                }
            }

            return true;
        }

        public static void ExtractNumbers(string value, out int firstNumber, out int secondNumber)
        {
            string[] numbers = value.Split('-');

            if (numbers.Length == 2 && int.TryParse(numbers[0], out firstNumber) && int.TryParse(numbers[1], out secondNumber))
            {
                // Extraction successful
            }
            else
            {
                // Handling case when extraction fails
                firstNumber = 0;
                secondNumber = 0;
            }
        }
    }
} 
    