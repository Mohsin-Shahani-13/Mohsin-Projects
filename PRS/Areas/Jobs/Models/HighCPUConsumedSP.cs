using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Jobs.Models
{
    public class HighCPUConsumedSP
    {
        public string ReportTitle { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        [Display(Name = "Servers:")]
        public string ServerNames { get; set; }
        [Display(Name = "SP Name:")]
        public string ProcName { get; set; }
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
        public bool GetList(string server, string ProcName)
        {
            string query = string.Empty;

            connectionsList.Add(new cDAL("JOB1"));
            connectionsList.Add(new cDAL("JOB2"));
            connectionsList.Add(new cDAL("JOB3"));
            connectionsList.Add(new cDAL("JOB4"));
            connectionsList.Add(new cDAL("JOB5"));
            foreach (cDAL oDAL in connectionsList)
            {
                query = @"
use msdb;
SELECT @@SERVERNAME as server_name, db_name(d.database_id) as 'db_name', 
	OBJECT_NAME(object_id, database_id) sp_name,
	s.text as sp_script,
    d.last_execution_time, 
    d.Last_worker_time / (1000000 * 60) as 'last_cpu_time(min)',   
    d.last_elapsed_time / (1000000 * 60) as 'last_elapsed_time(min)', 
    d.execution_count,
    p.query_plan
FROM msdb.sys.dm_exec_procedure_stats AS d  
cross apply msdb.sys.dm_exec_query_plan(d.plan_handle) p
cross apply msdb.sys.dm_exec_sql_text(d.sql_handle) s
where d.Last_worker_time / (1000000 * 60) > 0";

                if (server != "All")
                {
                    query += "AND @@SERVERNAME ='" + server + "'";
                }
                if (!string.IsNullOrEmpty(ProcName))
                {
                    query += "AND OBJECT_NAME(object_id, database_id) LIKE '" + ProcName + "' ";
                }

                query += "ORDER BY 'last_cpu_time(min)' DESC";

                //DataTable dt = oDAL.GetData(query);
                DataSet DS = oDAL.GetDataSet(query);

                //For SQL Documentation
                cLog oLog = new cLog();
                oLog.AddSqlQuery("159", query, string.Empty);

                if (!string.IsNullOrEmpty(server))
                {
                    filterString = "Server = '" + server + "'";
                }
                if (!string.IsNullOrEmpty(ProcName))
                {
                    filterString += " | SP Name Like '" + ProcName + "'";
                }

                if (!oDAL.HasErrors)
                {
                    List<ArrayList> lstJob = new List<ArrayList>();

                    //lstJob = cCommon.ConvertDtToArrayList(DS.Tables[0]);
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
    }
} 