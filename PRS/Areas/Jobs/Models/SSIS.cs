using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IP.Areas.Jobs.Models
{
    public class SSIS
    {
        public string ReportTitle { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        public string jobStatus { get; set; }
        [Display(Name = "Package Name:")]
        public string jobName { get; set; }
        [Display(Name = "Status:")]
        public string statusId { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string empName { get; set; }
        
        public List<Hashtable> lstSSIS { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        cDAL oDAL;
        public DataTable Status()
        {
            oDAL = new cDAL("JOB3");
            string query = string.Empty;
            query = @"
                 select distinct
	case 
     when jh.run_status = 0 then 'Failed'
     when jh.run_status = 1 then 'Succeeded'
     when jh.run_status = 2 then 'Retry'
     when jh.run_status = 3 then 'Canceled'
     when jh.run_status = 4 then 'In Progress' 
  end Status
from msdb.dbo.sysJobHistory jh
";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
         
        public bool GetList(string jobName, string status, string ProgramId, string ProgramName, string jobStatus)
        {
            empName = HttpContext.Current.Session["EmpName"].ToString();
            oDAL = new cDAL("JOB3");
            string query = string.Empty;

            query = @" use msdb;
SELECT * FROM (
select @@SERVERNAME as ServerName
, j.name as JobName
, j.description as Description
, case 
     when j.enabled = 0 then 'N' 
     when j.enabled = 1 then 'Y'
  end JobStatus
, case 
     when notify_level_email = 0 then 'Never' 
     when notify_level_email = 1 then 'When the job succeeds'
     when notify_level_email = 2 then 'When the job fails'
     when notify_level_email = 3 then 'Whenever the job completes'
  end Notify
, so.email_address as notifyOperator
, js.retry_attempts
, js.retry_interval
, ja.run_requested_date as LastRunOn
, ja.next_scheduled_run_date as NextRunOn
, js.step_id
, js.step_name
, js.database_name
, case 
     when jh.run_status = 0 then 'Failed'
     when jh.run_status = 1 then 'Succeeded'
     when jh.run_status = 2 then 'Retry'
     when jh.run_status = 3 then 'Canceled'
     when jh.run_status = 4 then 'In Progress' 
  end Status
  , jh.Message 
  --, jh.run_duration
  ,CONVERT(varchar, ja.last_executed_step_date - start_execution_date, 108) as run_duration
  , js.command
from msdb.dbo.sysjobs j
inner join msdb.dbo.sysjobsteps js on js.job_id = j.job_id  
left join msdb.dbo.sysoperators so on so.id = j.notify_email_operator_id
left join msdb.dbo.sysJobHistory jh on jh.instance_id = (select max(instance_id) from msdb.dbo.sysJobHistory where job_id = j.job_id)
left join msdb.dbo.sysjobactivity ja on ja.job_history_id = jh.instance_id
where 
js.subSystem = 'ssis' AND j.name LIKE 'WinIT%' ";


            //if (ProgramName != "0" && ProgramId != null)
            //{
            //    query += "WHERE Program LIKE '%" + ProgramName + "%'";
            //}
            //else
            //{
            //    query += "WHERE Program IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            //}
            if (jobStatus == "Enabled")
            {
                query += " AND j.enabled = '1'";
            }
            else if (jobStatus == "Disabled")
            {
                query += " AND j.enabled = '0'";
            }
            if (status != "All" && status == "Failed")
            {
                query += "AND jh.run_status = 0";
            }
            else if (status != "All" && status == "Succeeded")
            {
                query += "AND jh.run_status = 1";
            }
            else if (status != "All" && status == "Retry")
            {
                query += "AND jh.run_status = 2";
            }
            else if (status != "All" && status == "Canceled")
            {
                query += "AND jh.run_status = 3";
            }
            else if (status != "All" && status == "In Progress")
            {
                query += "AND jh.run_status = 4";
            }
            query += ") AS subquery WHERE 1=1";

            if (!string.IsNullOrEmpty(jobName))
            {
                query += " AND JobName LIKE '%" + jobName + "%' OR step_name LIKE '%" + jobName + "%' ";
            }

            if (status != "All" && status != null)
            {
                query += "AND status = '" + status + "'";
            }

            ////////FITERSTRINGS/////
            //if (ProgramName != "All" && ProgramName != null)
            //{
            //    filterString = "Program = '" + ProgramName + "'";
            //}
            //else
            //{
            //    filterString = "Program = '" + ProgramName + "'";
            //}
            if (!string.IsNullOrEmpty(jobName))
            {
                filterString += "Job Name LIKE '" + jobName + "' | ";
            }
            if (jobStatus == "Enabled")
            {
                filterString += "Job Status = 'Enabled' ";
            }

            else if (jobStatus == "Disabled")
            {
                filterString += " Job Status = 'Disabled' ";
            }
            else
            {
                filterString += "Job Status = 'All' ";
            }
            if (status != "All" && status != null)
            {
                filterString += " | Status = '" + status + "' ";
            }
            else
            {
                filterString += " | Status = 'All'";
            }

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("158", query, string.Empty);

            if (!oDAL.HasErrors)
            {

                //lstJob = cCommon.ConvertDtToArrayList(DS.Tables[0]);
                lstSSIS = cCommon.ConvertDtToHashTable(dt);

            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }

            return true;
        }
        //public bool RunJob(string jobName)
        //{
        //    oDAL = new cDAL("JOB3");
        //    List<DbParameter> parameters = new List<DbParameter>();
        //    parameters.Add(new SqlParameter("@job_name", SqlDbType.VarChar) { Value = jobName }); //{ Value = "WinIT - VF_Orders" });
        //    oDAL.ExecuteProcedure("msdb.dbo.sp_start_job", parameters);

        //    if (oDAL.HasErrors)
        //    {
        //        ErrorMessage = oDAL.ErrMessage;
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }

        //}
    }
}