using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Jobs.Models
{
    public class SSRS
    {
        public string ReportTitle { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        //public string hasValue { get; set; }
        [Display(Name = "Report Name:")]
        public string ReportName { get; set; }
        [Display(Name = "Report Status:")]
        public string statusId { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string Message { get; set; }
        public List<Hashtable> lstSSRS { get; set; }
        public List<ArrayList> lstUsrAxs { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        cDAL oDAL;
        public DataTable Program()
        {
            oDAL = new cDAL("JOB7");
            //string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"
                   
                   use [ReportServer];
select distinct * 
from (
select 
    case when cg.Path like '/Plus%' then REPLACE(SUBSTRING(cg.path, 2, CHARINDEX('/', cg.Path, CHARINDEX('/', cg.Path, CHARINDEX('/', cg.Path) + 1) + 1) - 2), 'Plus/', '')
     else SUBSTRING(cg.path, 2, CHARINDEX('/', cg.Path, CHARINDEX('/', cg.Path) + 1) - 2)
  end as Program
from msdb.dbo.sysjobs j
inner join [ReportServer].dbo.ReportSchedule rs on CAST(rs.ScheduleID AS NVARCHAR(128)) = j.name
inner join [ReportServer].dbo.Subscriptions ss on ss.SubscriptionID = rs.SubscriptionID
inner join [ReportServer].dbo.Catalog cg on cg.ItemID = ss.Report_OID

WHERE cg.Path NOT LIKE '%/Meta-Report-Matrix-Last-60-Days%'
) ssrs

  order by Program ";
            //query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public DataTable Status()
        {
            oDAL = new cDAL("JOB7");
            string query = string.Empty;
            query = @"
                 use  [ReportServer];
select distinct
  case 
       
	   when ss.LastStatus = 'Disabled' then 'Disabled'
	   when ss.LastStatus = 'Running' then 'Running'
	   when ss.LastStatus like 'Mail sent to%' then 'Succeeded'
	   when ss.LastStatus not like '%Disabled%' then 'Enabled'
	   when ss.LastStatus like '%0 errors%' then 'Succeeded'
	   when ss.LastStatus like '%has been saved%' then 'Succeeded' 
	   when ss.LastStatus like 'Fail%' then 'Failed'
	   when ss.LastStatus like 'Error%' then 'Failed' 
	   when ss.LastStatus like '%1 errors%' then 'Failed' 
else ss.LastStatus 
       end as [Status]
from dbo.Subscriptions ss
";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetList(string ReportName, string status, string ProgramId, string ProgramName, string frequency)
        {
            oDAL = new cDAL("JOB7");
            string empName = HttpContext.Current.Session["EmpName"].ToString();
            string query = string.Empty;
            //string _ProgramName = ProgramName.Substring(ProgramName.Length - 2) + "'";

            query = @"
                use [ReportServer];
select * 
from (
select @@ServerName as ServerName
, ss.SubscriptionID
, case when tua.UserAxs like '%<UserName>%' then 'Y' else 'N' end HasAxs
, case when rs.ScheduleID is not null then cg.Name collate SQL_Latin1_General_CP1_CI_AS else j.name end ReportName 
, ss.Description as ReportDesc
, case 
     when j.enabled = 0 then 'N' 
     when j.enabled = 1 then 'Y'
  end JobStatus
, case when cg.Path like '/Plus%' then REPLACE(SUBSTRING(cg.path, 2, CHARINDEX('/', cg.Path, CHARINDEX('/', cg.Path, CHARINDEX('/', cg.Path) + 1) + 1)), 'Plus/', '')
	   else SUBSTRING(cg.path, 2, CHARINDEX('/', cg.Path, CHARINDEX('/', cg.Path) + 1) )
  end as Program
, cg.Path as SSRSPath
, ss.LastRunTime as LastRunOn
, (
	select max(next_scheduled_run_date) 
	from msdb.dbo.sysjobactivity 
	where job_id = j.job_id
	) as NextRunOn
  ,	case sch.RecurrenceType 
		when 1 then 'Once' 
		when 2 then 'Hourly' 
		when 3 then 'Daily' 
		when 4 then 'Weekly' 
		when 5 then 'Monthly' 
	end Frequency
  , (
		select top 1 convert(varchar, TimeEnd - TimeStart, 108) 
		from ExecutionLog3 
		where RequestType = 'Subscription' 
			  and ItemPath = cg.Path
		order by TimeStart desc
	) as last_run_duration
  , ss.LastStatus as statusDesc
  , case 
	   when ss.LastStatus = 'Disabled' then 'Disabled'
	   when ss.LastStatus like 'Mail sent to%' then 'Succeeded'
	   when ss.LastStatus like '%0 errors%' then 'Succeeded'
	   when ss.LastStatus like '%has been saved%' then 'Succeeded' 
	   when ss.LastStatus like 'Fail%' then 'Failed'
	   when ss.LastStatus like 'Error%' then 'Failed' 
	   when ss.LastStatus like '%1 errors%' then 'Failed' 
	   else ss.LastStatus 
    end as [Status]
	, case 
	 when notify_level_email = 0 then 'Never' 
	 when notify_level_email = 1 then 'When the job succeeds'
	 when notify_level_email = 2 then 'When the job fails'
	 when notify_level_email = 3 then 'Whenever the job completes'
  end Notify
, so.email_address as notifyOperator
from msdb.dbo.sysjobs j
left join msdb.dbo.sysjobsteps js on js.job_id = j.job_id   
left join msdb.dbo.sysoperators so on so.id = j.notify_email_operator_id
inner join [ReportServer].dbo.ReportSchedule rs on CAST(rs.ScheduleID AS NVARCHAR(128)) = j.name
inner join [ReportServer].dbo.Subscriptions ss on ss.SubscriptionID = rs.SubscriptionID

inner join [ReportServer].dbo.Catalog cg on cg.ItemID = ss.Report_OID
inner join [ReportServer].dbo.Schedule sch on sch.ScheduleID = rs.ScheduleID
left join  [DC1PRS02].[PLUSRS].dbo.TmpUserAxs tua on tua.SubscriptionId = ss.SubscriptionID
) ssrs
Where 1=1 



 ";
            if (ProgramName != "All")
            {
                query += "AND Program LIKE '%" + ProgramName + "%'";
            }
            //else
            //{
            //    query += "WHERE Program IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            //}
            if (!string.IsNullOrEmpty(ReportName))
            {
                query += "AND ReportName LIKE '%" + ReportName + "%' ";
            }

            if (frequency != "All" && frequency != null)
            {
                query += "AND Frequency = '" + frequency + "'";
            }

            if (status != "All" && status != null && status != "Enabled")
            {
                query += "AND status = '" + status + "'";
            }
            if (status == "Enabled")
            {
                query += "AND status <> 'Disabled'";
            }

            query += " order by LastRunOn desc";

            query = query.Replace("<UserName>", empName);
            ////////FITERSTRINGS////////
            if (ProgramName != "All")
            {
                filterString = "Program = '" + ProgramName + "'";
            }
            else
            {
                filterString = "Program = '" + ProgramName + "'";
            }
            if (!string.IsNullOrEmpty(ReportName))
            {
                filterString += " | Report Name LIKE '" + ReportName + "'";
            }
            if (frequency != "All" && frequency != null)
            {
                filterString += " | Frequency = '" + frequency + "' ";
            }
            else
            {
                filterString += " | Frequency = 'All' ";
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
                oLog.AddSqlQuery("157", query, string.Empty);

                if (!oDAL.HasErrors)
                {

                //lstJob = cCommon.ConvertDtToArrayList(DS.Tables[0]);
                lstSSRS = cCommon.ConvertDtToHashTable(dt);

                }
                else
                {
                    ErrorMessage = oDAL.ErrMessage;
                    return false;
                }

            return true;
        }
        public bool RunSSRS(string subscriptionId)
        {
            oDAL = new cDAL("JOB7");
            string empName = HttpContext.Current.Session["EmpName"].ToString();
            try
            {
                string queryGetData = @"SELECT [UserAxs]
                                         FROM [DC1PRS02].[PLUSRS].[dbo].[TmpUserAxs]
                                         Where SubscriptionId = '<subcriptionId>'";
                queryGetData = queryGetData.Replace("<subcriptionId>", subscriptionId);
                string stringValue = oDAL.GetObject(queryGetData).ToString() ;

                // Split the string by commas and store it in an array
                string[] arrayValue = stringValue.Split(',');
                bool returnValue = false;
                // Output the result
                foreach (string item in arrayValue)
                {
                    if (empName == item)
                    {
                        string query = "exec dbo.AddEvent @EventType='TimedSubscription', @EventData= '@SubscriptionId'";
                        query = query.Replace("@SubscriptionId", subscriptionId);
                        oDAL.Execute(query);
                        returnValue = true ;
                        Message = "Y";
                    }
                    else
                    {

                        returnValue = false;
                        Message = "N";
                    }
                }
                return returnValue;
               
            }
            catch (Exception ex)
            {

                ErrorMessage = ex.Message;
                return false;
            }
           
        }
    }
}