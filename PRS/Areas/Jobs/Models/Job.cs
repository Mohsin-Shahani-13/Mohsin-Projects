using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using IP.Areas.Jobs.Controllers;

namespace IP.Areas.Jobs.Models
{
    public class Job
    { 
        #region properties
        public string ReportTitle { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        public string servers { get; set; }
        public List<Hashtable> lstJobs1 { get; set; }
        public List<Hashtable> lstServers { get; set; }
        [Display(Name = "Servers:")]
        public string ServerNames { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public DataTable listOfServers { get; set; }

        List<cDAL> connectionsList = new List<cDAL>();

        public List<Hashtable> listOfLists = new List<Hashtable>();

        public List<Hashtable> lstDetail = new List<Hashtable>();
        //public List<Hashtable> listOfServers = new List<Hashtable>();

        //cDAL oDAL = new cDAL("JOB2");
        #endregion
        public DataTable GetServerNames(string serverName) 
        {
            string query = string.Empty;
            #region connections
            connectionsList.Add(new cDAL("JOB1"));
            connectionsList.Add(new cDAL("JOB2"));
            connectionsList.Add(new cDAL("JOB3"));
            connectionsList.Add(new cDAL("JOB4"));
            connectionsList.Add(new cDAL("JOB5"));
            connectionsList.Add(new cDAL("E2DB1"));
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
                    //listOfServers.AddRange(lstServers);
                    listOfServers.Merge(dt);
                }
            }
            return listOfServers;
        }
        public bool GetList(string server)
        {
            string query = string.Empty;
            bool dtConsumed = false;
            connectionsList.Add(new cDAL("JOB1"));
            connectionsList.Add(new cDAL("JOB2"));
            connectionsList.Add(new cDAL("JOB3"));
            connectionsList.Add(new cDAL("JOB4"));
            connectionsList.Add(new cDAL("JOB5"));
            connectionsList.Add(new cDAL("E2DB1"));
            DataTable dt2 = new DataTable();
            foreach (cDAL oDAL in connectionsList)
            {
                
                query = @"
                
                SELECT ServerName, JobName, Notify, notifyOperator, LastRunOn, NextRunOn, JobStartedOn, JobFinishedOn, JobStatus, job_duration, ScheduledOn, retry_attempts, retry_interval
FROM (
select @@SERVERNAME as ServerName
, j.name as JobName
, ISNULL(ja.run_requested_date,  msdb.dbo.Agent_Datetime(jh.run_date, jh.run_time)) as LastRunOn
, ja.next_scheduled_run_date as NextRunOn
, (select case 
	 when ja.start_execution_date is not null and ja.stop_execution_date is null then 'Running'
     when run_status = 0 then 'Failed'
     when run_status = 1 then 'Succeeded'
     when run_status = 2 then 'Retry'
     when run_status = 3 then 'Canceled'
     when run_status = 4 then 'Running' 
 	end 
   from msdb.dbo.sysjobhistory 
   where job_id = j.job_id and step_id = 0 and instance_id = (select max(instance_id) from msdb.dbo.sysjobhistory where job_id = j.job_id) ) as JobStatus
,(select top 1 CASE
       WHEN run_duration > 235959
           THEN CAST((CAST(LEFT(CAST(run_duration AS VARCHAR),LEN(CAST(run_duration AS VARCHAR)) - 4) AS INT) / 24) AS VARCHAR) + '.' + RIGHT('00' + CAST(CAST(LEFT(CAST(run_duration AS VARCHAR), LEN(CAST(run_duration AS VARCHAR)) - 4) AS INT) % 24 AS VARCHAR), 2) + ':' + STUFF(CAST(RIGHT(CAST(run_duration AS VARCHAR), 4) AS VARCHAR(6)), 3, 0, ':')
       ELSE STUFF(STUFF(RIGHT(REPLICATE('0', 6) + CAST(run_duration AS VARCHAR(6)), 6), 3, 0, ':'), 6, 0, ':')
     END 
   from msdb.dbo.sysjobhistory 
   where job_id = j.job_id
   and step_id = 0
   and instance_id > jh.instance_id
   order by instance_id desc
  ) as job_duration
, CAST(CAST(ja.start_execution_date as DATE) AS DATETIME) + CAST(CAST(ja.next_scheduled_run_date as TIME) as DATETIME) as JobStartedOn
, ja.stop_execution_date as JobFinishedOn
, sj.name as ScheduledOn
, js.retry_attempts
, js.retry_interval
, case 
     when notify_level_email = 0 then 'Never' 
     when notify_level_email = 1 then 'When the job succeeds'
     when notify_level_email = 2 then 'When the job fails'
     when notify_level_email = 3 then 'Whenever the job completes'
  end Notify
, so.email_address as notifyOperator
,ROW_NUMBER() OVER (PARTITION BY j.name ORDER BY jh.instance_id DESC) as row_num
from msdb.dbo.sysjobs j
inner join msdb.dbo.sysjobsteps js on js.job_id = j.job_id  
left join msdb.dbo.sysoperators so on so.id = j.notify_email_operator_id
left join msdb.dbo.sysJobHistory jh on jh.job_id = j.job_id and jh.step_id = js.step_id and jh.run_date = (select max(run_date) from msdb.dbo.sysjobhistory where job_id = j.job_id  and step_id = js.step_id ) 
left join msdb.dbo.sysjobactivity ja on ja.job_id = j.job_id and ja.session_id = (select max(session_id) from msdb.dbo.sysjobactivity where job_id = j.job_id)
left join msdb.dbo.sysjobschedules sjs on sjs.job_id = j.job_id
left join msdb.dbo.sysschedules sj on sj.schedule_id = sjs.schedule_id
where j.enabled = 1 AND j.Name LIKE 'WinIT%' ";

                if (server != "All")
                {
                    query += "AND @@SERVERNAME ='" + server + "'";
                }

                query += ") AS subquery WHERE row_num = 1";
                //DataTable dt = oDAL.GetData(query);
                DataSet DS = oDAL.GetDataSet(query);
                
                //For SQL Documentation
                cLog oLog = new cLog();
                oLog.AddSqlQuery("155", query, string.Empty);

                if (!string.IsNullOrEmpty(server))
                {
                    filterString = "Server = '" + server + "'";
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

        public bool GetDetail(string jobName)
        {
            string query = string.Empty;
            connectionsList.Add(new cDAL("JOB1"));
            connectionsList.Add(new cDAL("JOB2"));
            connectionsList.Add(new cDAL("JOB3"));
            connectionsList.Add(new cDAL("JOB4"));
            connectionsList.Add(new cDAL("JOB5"));
            DataTable dt2 = new DataTable();
            foreach (cDAL oDAL in connectionsList)
            {

                query = @"
                 select distinct j.description as Description
, js.step_id
, js.step_name
, js.database_name
, case 
	 
	 when js.step_id = ja.last_executed_step_id + 1 then 'Running'
     when jh.run_status = 0 then 'Failed'
     when jh.run_status = 1 then 'Succeeded'
     when jh.run_status = 2 then 'Retry'
     when jh.run_status = 3 then 'Canceled'
     when jh.run_status = 4 then 'Running' 
  end StepStatus 

, jh.message
, CASE
       WHEN jh.run_duration > 235959
           THEN CAST((CAST(LEFT(CAST(jh.run_duration AS VARCHAR),LEN(CAST(jh.run_duration AS VARCHAR)) - 4) AS INT) / 24) AS VARCHAR) + '.' + RIGHT('00' + CAST(CAST(LEFT(CAST(jh.run_duration AS VARCHAR), LEN(CAST(jh.run_duration AS VARCHAR)) - 4) AS INT) % 24 AS VARCHAR), 2) + ':' + STUFF(CAST(RIGHT(CAST(jh.run_duration AS VARCHAR), 4) AS VARCHAR(6)), 3, 0, ':')
       ELSE STUFF(STUFF(RIGHT(REPLICATE('0', 6) + CAST(jh.run_duration AS VARCHAR(6)), 6), 3, 0, ':'), 6, 0, ':')
  END as Step_duration
  , js.command
, (select convert(varchar, step_id) + '-' + step_name
   from msdb.dbo.sysjobsteps 
   where job_id = j.job_id 
   and step_id =  isnull(ja.last_executed_step_id, 0) + 1 
   ) as crnt_executing_step
, case when ja.stop_execution_date is null then convert(varchar, GETDATE() - isnull(ja.last_executed_step_date, ja.start_execution_date), 108) else NULL end as crnt_step_duration

from msdb.dbo.sysjobs j
inner join msdb.dbo.sysjobsteps js on js.job_id = j.job_id  
left join msdb.dbo.sysoperators so on so.id = j.notify_email_operator_id
left join msdb.dbo.sysJobHistory jh on jh.job_id = j.job_id and jh.step_id = js.step_id and jh.run_date + jh.run_time = (select max(run_date+run_time) from msdb.dbo.sysjobhistory where job_id = j.job_id and step_id = js.step_id)
left join msdb.dbo.sysjobactivity ja on ja.job_id = j.job_id and ja.session_id = (select max(session_id) from msdb.dbo.sysjobactivity where job_id = j.job_id)
left join msdb.dbo.sysjobschedules sjs on sjs.job_id = j.job_id
left join msdb.dbo.sysschedules sj on sj.schedule_id = sjs.schedule_id
where j.enabled = 1
 ";

                if (jobName != null)
                {
                    query += "AND j.name LIKE'" + jobName + "%'";
                }
                //DataTable dt = oDAL.GetData(query);
                DataSet DS = oDAL.GetDataSet(query);

                //For SQL Documentation
                cLog oLog = new cLog();
                oLog.AddSqlQuery("155", query, string.Empty);

                if (!string.IsNullOrEmpty(jobName))
                {
                    filterString = "Server = '" + jobName + "'";
                }

                if (!oDAL.HasErrors)
                {

                    //lstJob = cCommon.ConvertDtToArrayList(DS.Tables[0]);
                    lstJobs1 = cCommon.ConvertDtToHashTable(DS.Tables[0]);

                    lstDetail.AddRange(lstJobs1);
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