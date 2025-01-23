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
    public class RequiredIndexes
    {
        cDAL oDAL;
        #region properties
        public string ReportTitle { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        public string servers { get; set; }
        public List<Hashtable> lstJobs1 { get; set; }
        public List<Hashtable> lstServers { get; set; }
        [Display(Name = "Servers:")]
        public string serverName{ get; set; }
        [Display(Name = "Database:")]
        public string database { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public DataTable listOfServers { get; set; }
        public List<ArrayList> lstDatabase { get; set; }

        List<cDAL> connectionsList = new List<cDAL>();

        public List<Hashtable> listOfLists = new List<Hashtable>();

       
        
        #endregion
        public DataTable ServerNames(string serverName)
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
                    //listOfServers.AddRange(lstServers);
                    listOfServers.Merge(dt);
                }
            }
            return listOfServers;
        }

        public bool Database(string serverName)
        {
            
            string query = string.Empty;
            #region connections
            connectionsList.Add(new cDAL("JOB1"));
            connectionsList.Add(new cDAL("JOB2"));
            connectionsList.Add(new cDAL("JOB3"));
            connectionsList.Add(new cDAL("JOB4"));
            connectionsList.Add(new cDAL("JOB5"));
            #endregion
            if (lstDatabase == null)
               

            foreach (cDAL oDAL in connectionsList)
            {
                query =@"SELECT [DATABASE]
                          FROM (
                          SELECT  
                              @@SERVERNAME AS [serverName],
                              DB_NAME() AS [DATABASE]
                          FROM msdb.dbo.sysjobs
                          ) AS DB
                          GROUP BY serverName, [DATABASE]
                ";        
                    
                    query = query.Replace("<serverName>", serverName);

                    DataTable dt = oDAL.GetData(query);
                if (!oDAL.HasErrors)
                {
                        lstDatabase = cCommon.ConvertDtToArrayList(dt);
                   
                      
                }
            }
            return true;
        }
        public bool GetList(string serverName, string database)
        {
            string query = string.Empty;
            bool dtConsumed = false;
            connectionsList.Add(new cDAL("JOB1"));
            connectionsList.Add(new cDAL("JOB2"));
            connectionsList.Add(new cDAL("JOB3"));
            connectionsList.Add(new cDAL("JOB4"));
            connectionsList.Add(new cDAL("JOB5"));
            DataTable dt2 = new DataTable();
            foreach (cDAL oDAL in connectionsList)
            {

                query = @"
               SELECT
@@SERVERNAME as [serverName],
DB_Name() as 'DATABASE',
Schema_Name(SQLOPS_SysObj.schema_id) as 'SCHEMA NAME',
Object_Name(mid.object_id) as 'OBJECT NAME',
migs.user_scans,
migs.user_seeks,
migs.last_user_seek as 'LAST USER SEEK',
Cast(round(migs.avg_total_user_cost,2) as varchar)+'%' as 'ESTIMATED CURRENT COST',
Cast(migs.avg_user_impact as varchar)+'%' as 'CAN BE IMPROVED',
'CREATE INDEX '+DB_Name()+Object_Name(mid.object_id)+'_' +CONVERT (varchar, mig.index_group_handle) + '_' +
CONVERT (varchar, mid.index_handle) + ' ON ' +
mid.statement + '
(' + ISNULL (mid.equality_columns,'')
+ CASE WHEN mid.equality_columns IS NOT NULL
AND mid.inequality_columns IS NOT NULL
THEN ',' ELSE '' END + ISNULL (mid.inequality_columns, '')
+ ')'
+ ISNULL (' INCLUDE (' + mid.included_columns + ');', '') as 'CREATE INDEX COMMAND'
FROM sys.dm_db_missing_index_groups AS mig
INNER JOIN sys.dm_db_missing_index_group_stats AS migs ON migs.group_handle = mig.index_group_handle
INNER JOIN sys.dm_db_missing_index_details AS mid ON mig.index_handle = mid.index_handle
INNER JOIN sys.objects as SQLOPS_SysObj ON mid.object_id = SQLOPS_SysObj.object_id


 ";

                if (serverName != "All")
                {
                    query += "WHERE @@SERVERNAME ='" + serverName + "'";
                }
                
                if (database != "All")
                {
                    query += "AND DB_Name() = '" + database + "' ";
                }
                query += "ORDER BY 'ESTIMATED CURRENT COST' desc";
                //DataTable dt = oDAL.GetData(query);
                DataSet DS = oDAL.GetDataSet(query);
                if (!string.IsNullOrEmpty(serverName))
                    filterString = "> Server = '" + serverName + "' ";
                if (!string.IsNullOrEmpty(database))
                    filterString += "| DataBase = '" + database + "' ";

                //For SQL Documentation
                cLog oLog = new cLog();
                oLog.AddSqlQuery("162", query, string.Empty);

                if (!oDAL.HasErrors)
                {
                    //List<ArrayList> lstJob = new List<ArrayList>();

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