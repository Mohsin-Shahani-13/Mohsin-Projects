using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.EMMA;
using System.Web.Mvc;
using System.Net;

namespace IP.Areas.ListingReports.Models
{
    public class RedmineReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Status:")]
        public string status { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string Message { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }

        public List<Hashtable> lstRedmineReport { get; set; }
        public List<Hashtable> lstStatus { get; set; }

        #endregion

        #region ListModelFields
        public string Priority { get; set; }
        public string Jira { get; set; }
        public int Redmine { get; set; }
        public string DevDueDate { get; set; }
        public string PlannedReleaseDate { get; set; }
        public string ClosedDate { get; set; }
        public string CurrentOwner { get; set; }
        public string CurrentStatus { get; set; }
        public string CurrentStatusId { get; set; }


        //public string Remark { get; set; }

        public string Grpvn { get; set; }
        public string Prg { get; set; }
        public string Aus { get; set; }
        public string Jpn { get; set; }
        public int IssueId { get; set; }
        public int StatusId { get; set; }

        #endregion
        #region Methods 
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("INIT");
            //string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select distinct program from rpt.RedmineSource
                      WHERE program IS NOT NULL
                      ORDER BY program ";
            //query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public DataTable GetStatus()
        {
            oDAL = new cDAL("INIT");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @" select distinct 
	                  status 
                FROM rpt.RedmineSource

                ORDER BY status";
            //query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetList(string program, string status)
        {
            oDAL = new cDAL("INIT");
            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            string empName = HttpContext.Current.Session["EmpName"].ToString();

            string query = string.Empty;

            query = @"WITH FilteredSource AS (
    SELECT
        [RequesterSite],
[Program],
        [TicketNo],
        [CustCase],
        [Classification],
        [Subject],
        [CreatedOn],
        [Status],
[StatusId],
[AssignedTo]
    FROM rpt.RedmineSource
where 1=1
";
            if (program != "All")
            {
                query += "AND [Program] = '" + program + "' ";
            }

            if (status != "All")
            {
                query += "AND [Status] = '" + status + "' ";
            }


            query += @"   
)

MERGE INTO [PlusRS].[rpt].[RedmineItems] AS Target
USING FilteredSource AS Source
ON Target.[Redmine] = Source.[TicketNo]
WHEN MATCHED THEN
    UPDATE SET
 Target.[Program] = Source.[Program],
        Target.[Site] = Source.[RequesterSite],
        Target.[Type] = Source.[Classification],
        Target.[Title] = Source.[Subject],
        Target.[CreateDate] = Source.[CreatedOn],
        --Target.[CurrentStatus] = Source.[Status],
 --Target.[CurrentStatusId] = Source.[StatusId],
 Target.[CurrentOwner] = Source.[AssignedTo]

WHEN NOT MATCHED THEN
    INSERT (
        [Site],
[Program],
        [Redmine],
[Jira],
        [Type],
        [Title],
        [CreateDate],
        [CurrentStatus],
[CurrentStatusId],
        [CurrentOwner],
        [InsertedBy],
        [InsertedOn]
    )
    VALUES (
        Source.[RequesterSite],
	Source.[Program],
        Source.[TicketNo],
        Source.[CustCase],
        Source.[Classification],
        Source.[Subject],
        Source.[CreatedOn],
        Source.[Status],
 Source.[StatusId],
            Source.[AssignedTo],
        '<empName>',
        GETDATE()
    );

SELECT Priority, 
       Site, 
       Program,
       Jira, 
       Redmine, 
       Type, 
       Title, 
       Format(CreateDate, 'yyyy.MM.dd') AS CreateDate,
       DevDueDate, 
       PlannedReleaseDate, 
       ClosedDate, 
       CurrentStatus, 
      CurrentStatusId, 
       CurrentOwner, 
       Remarks, 
       Grpvn, 
       Prg, 
       Aus, 
       Jpn, 
       InsertedBy, 
       InsertedOn, 
       ModifiedBy, 
       ModifiedOn, 
       SortOrder
FROM   rpt.RedmineItems rd
WHERE 1 = 1
";
            if (program != "All")
            {
                query += "AND [Program] = '" + program + "' ";
            }

            if (status != "All")
            {
                query += "AND [CurrentStatus] = '" + status + "' ";
            }


            query += @"   
ORDER BY SortOrder
";
            string sql = string.Empty;
            sql = @"
select distinct 
		 CurrentStatus, 
        CurrentStatusId
FROM   rpt.RedmineItems
Order by CurrentStatusId
";


            //query = query.Replace("<program>", program);
            // query = query.Replace("<status>", status);
            query = query.Replace("<empName>", empName);
            //query = query.Replace("<toDt>", tDate);

            DataTable dt = oDAL.GetData(query);
            DataTable dtStatus = oDAL.GetData(sql);

            //if (!string.IsNullOrEmpty(programName))
            filterString += "> Program = '" + program + "' ";
            filterString += " | Status = '" + status + "' ";



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("191", query, string.Empty, false);

            lstStatus = cCommon.ConvertDtToHashTable(dtStatus);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRedmineReport = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        public bool SaveData(List<RedmineReport> dataList)
        {
            string empName = HttpContext.Current.Session["EmpName"].ToString();

            if (dataList == null || dataList.Count == 0)
                throw new ArgumentException("Data list cannot be null or empty");

            var queryBuilder = new StringBuilder();

            foreach (var data in dataList)
            {
                var updateSet = new List<string>();


                updateSet.Add($"Priority = '{data.Priority}'");

                //updateSet.Add($"Jira = '{data.Jira}'");
                if (data.DevDueDate == null)
                {
                    updateSet.Add($"DevDueDate = null");
                }
                else
                {
                    updateSet.Add($"DevDueDate = '{data.DevDueDate}'");
                }
                if (data.PlannedReleaseDate == null)
                {
                    updateSet.Add($"PlannedReleaseDate = null");
                }
                else
                {
                    updateSet.Add($"PlannedReleaseDate = '{data.PlannedReleaseDate}'");
                }
                if (data.ClosedDate == null)
                {
                    updateSet.Add($"ClosedDate = null");
                }
                else
                {
                    updateSet.Add($"ClosedDate = '{data.ClosedDate}'");
                }
                updateSet.Add($"CurrentStatus = '{data.CurrentStatus}'");
                updateSet.Add($"CurrentStatusId = '{data.CurrentStatusId}'");


                updateSet.Add($"CurrentOwner = '{data.CurrentOwner}'");

                updateSet.Add($"Grpvn = '{data.Grpvn}'");

                updateSet.Add($"Prg = '{data.Prg}'");

                updateSet.Add($"Aus = '{data.Aus}'");


                updateSet.Add($"Jpn = '{data.Jpn}'");

                updateSet.Add($"ModifiedBy = '<empName>'");
                updateSet.Add($"ModifiedOn = GETDATE()");

                if (updateSet.Count > 0)
                {
                    string updateQuery = $"UPDATE [PlusRS].[rpt].[RedmineItems] SET {string.Join(", ", updateSet)} WHERE Redmine = {data.Redmine};";
                    queryBuilder.AppendLine(updateQuery);
                }
            }

            string sql = string.Empty;
            queryBuilder = queryBuilder.Replace("<empName>", empName);

            sql = queryBuilder.ToString();
            oDAL.Execute(sql);
            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                Message = "Remarks has been updated";
                return true;
            }
        }

        public bool SaveRemarks(string id, string remarks)
        {
            oDAL = new cDAL("INIT");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"update [PlusRS].[rpt].[RedmineItems] SET Remarks = '<remakrs>'  where  redmine = '<redmineId>' ";
            query = query.Replace("<remakrs>", remarks);
            query = query.Replace("<redmineId>", id);
            oDAL.Execute(query);
            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                Message = "Remarks has been updated";
                return true;
            }
        }

        private readonly string _baseUrl = "https://tracking.reconext.com/";
        private string _apiKey;

        public async Task<Dictionary<int, string>> UpdateMultipleStatuses(List<RedmineReport> issueUpdates)
        {
            var results = new Dictionary<int, string>();
            var obj = GetAPIKey();
            // Set the TLS protocol version
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) })  // Adjust the timeout as needed
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Add("X-Redmine-API-Key", _apiKey);
                client.DefaultRequestHeaders.ConnectionClose = true;  // Disable Keep-Alive

                foreach (var update in issueUpdates)
                {
                    var issueId = update.IssueId;
                    var statusId = update.StatusId;

                    var updateData = new
                    {
                        issue = new
                        {
                            status_id = statusId
                        }
                    };

                    string jsonPayload = JsonConvert.SerializeObject(updateData);
                    var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                    try
                    {
                        content.Headers.ContentLength = jsonPayload.Length;

                        string apiUrl = $"issues/{issueId}.json";
                        HttpResponseMessage response = await client.PutAsync(apiUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            results[issueId] = "Success";
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            results[issueId] = $"Error: {response.StatusCode} - {errorContent}";
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        results[issueId] = $"Request failed: {e.Message}";
                    }
                    catch (TaskCanceledException e)
                    {
                        results[issueId] = $"Timeout occurred: {e.Message}";
                    }
                    catch (Exception e)
                    {
                        results[issueId] = $"Unhandled exception: {e.Message} - {e.InnerException?.Message}";
                    }

                    // Optional: Add delay to prevent overloading the server
                    await Task.Delay(1000);
                }
            }

            return results;
        }
        private string GetAPIKey()
        {
            oDAL = new cDAL("INIT");
            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            string empName = HttpContext.Current.Session["EmpName"].ToString();

            string query = string.Empty;
            query = @"
                    select rd.[Key] from rpt.RedmineAccess rd
                    where rd.UserName = '@username'";
            query = query.Replace("@username", empName);

            string result = oDAL.GetObject(query).ToString();

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return null;
            }
            else
            {
                if (!string.IsNullOrEmpty(result))
                    _apiKey = result;
                return _apiKey;
            }
        }
        #endregion
    }
}