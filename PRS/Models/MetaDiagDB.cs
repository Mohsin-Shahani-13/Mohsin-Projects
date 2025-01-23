using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;

namespace IP.Models
{
    public class MetaDiagDB
    {
        cDAL oDAL;

        #region Method

        string query = string.Empty;
        public string DBTitle { get; set; }
        public List<ArrayList> lstIngenico { get; set; }

        public string ErrorMessage { get; set; }

        public string shift { get; set; }
        #endregion

        #region Data Fields for Morning
        TimeSpan MorningStart = new TimeSpan(06, 30, 0);
        TimeSpan MorningEnd = new TimeSpan(15, 0, 0);

        string refreshTimeMorning = string.Empty;

        string Operation;
        #endregion

        #region Data Fields for Afternoon
        TimeSpan AfternoonStart = new TimeSpan(14, 0, 0);
        TimeSpan AfternoonEnd = new TimeSpan(22, 0, 0);

        string refreshTimeAfternoon = string.Empty;
        #endregion

        #region Data Fields for Night
        TimeSpan NightStart = new TimeSpan(22, 0, 0);
        TimeSpan NightEnd = new TimeSpan(06, 0, 0);

        string refreshTimeNight = string.Empty;
        #endregion
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        #region Method



        public bool GetData(string selectedVal, string selectedText, string programId, string site, string programName)
        {

            DateTime ClientDate = DateTime.Now;

            //ClientDate = ClientDate.ToUniversalTime();
            //ClientDate = ClientDate.AddHours(2);

            string s = ClientDate.ToString("HH:mm:ss");

            string am = ClientDate.ToString("hh:mm tt");
            var now = TimeSpan.Parse(s);

            DataTable dt = new DataTable();

            var _selectedText = selectedText.Replace("DB", "").Trim();
            oDAL = new cDAL("DWDB");
            string query = string.Empty;
            query = @"
use DW;							 
SELECT username,
[%01],
[01],
[%02],
[02],
[%03],
[03], 
[%04],
[04],
[%05],
[05],
[%06],
[06],
[%07],
[07],
[%08],
[08],
qtotal,    
[%total]
  FROM [DW].[btv].[Productivity]
WHERE programId  = '@programId' AND Operation = '@operation'";

            query = query.Replace("@programId", programId);
            query = query.Replace("@operation", _selectedText);

            dt = oDAL.GetData(query);

            string sql1 = string.Empty;
            sql1 = @"Select distinct MAX(RefreshTime) from btv.Productivity  WHERE programId = '@programId' AND Operation like '%@operation%'";
            sql1 = sql1.Replace("@programId", programId);
            sql1 = sql1.Replace("@operation", _selectedText);

            string refreshTime = oDAL.GetObject(sql1).ToString();

            string sql = string.Empty;
            sql = @"SELECT 
    CASE
        WHEN CONVERT(TIME, SWITCHOFFSET(SYSDATETIMEOFFSET(), 
            CASE 
                WHEN TimeZone = 'Pacific Standard Time' THEN '-08:00'
                WHEN TimeZone = 'Pacific Standard Time (Mexico)' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Singapore Standard Time' THEN '+08:00'
                WHEN TimeZone = 'Central Europe Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Central European Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Eastern Standard Time' THEN '-05:00'
                WHEN TimeZone = 'GMT Standard Time' THEN '+00:00'
                WHEN TimeZone = 'Greenwich Standard Time' THEN '+00:00'
                WHEN TimeZone = 'SE Asia Standard Time' THEN '+07:00'
                WHEN TimeZone = 'Tokyo Standard Time' THEN '+09:00'
                WHEN TimeZone = 'AUS Eastern Standard Time' THEN '+10:00'
                ELSE '00:00' -- Default offset for unknown time zones
            END
        )) BETWEEN '06:00:00' AND '11:59:59' THEN 'Morning'
        WHEN CONVERT(TIME, SWITCHOFFSET(SYSDATETIMEOFFSET(), 
            CASE 
                WHEN TimeZone = 'Pacific Standard Time' THEN '-08:00'
                WHEN TimeZone = 'Pacific Standard Time (Mexico)' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Singapore Standard Time' THEN '+08:00'
                WHEN TimeZone = 'Central Europe Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Central European Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Eastern Standard Time' THEN '-05:00'
                WHEN TimeZone = 'GMT Standard Time' THEN '+00:00'
                WHEN TimeZone = 'Greenwich Standard Time' THEN '+00:00'
                WHEN TimeZone = 'SE Asia Standard Time' THEN '+07:00'
                WHEN TimeZone = 'Tokyo Standard Time' THEN '+09:00'
                WHEN TimeZone = 'AUS Eastern Standard Time' THEN '+10:00'
                ELSE '00:00'
            END
        )) BETWEEN '12:00:00' AND '17:59:59' THEN 'Afternoon'
        WHEN CONVERT(TIME, SWITCHOFFSET(SYSDATETIMEOFFSET(), 
            CASE 
                WHEN TimeZone = 'Pacific Standard Time' THEN '-08:00'
                WHEN TimeZone = 'Pacific Standard Time (Mexico)' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Singapore Standard Time' THEN '+08:00'
                WHEN TimeZone = 'Central Europe Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Central European Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Eastern Standard Time' THEN '-05:00'
                WHEN TimeZone = 'GMT Standard Time' THEN '+00:00'
                WHEN TimeZone = 'Greenwich Standard Time' THEN '+00:00'
                WHEN TimeZone = 'SE Asia Standard Time' THEN '+07:00'
                WHEN TimeZone = 'Tokyo Standard Time' THEN '+09:00'
                WHEN TimeZone = 'AUS Eastern Standard Time' THEN '+10:00'
                ELSE '00:00'
            END
        )) BETWEEN '18:00:00' AND '21:59:59' THEN 'Evening'
        ELSE 'Night'
    END AS TimeOfDay
FROM 
    pls.program

	where id = @programId;
";
            sql = sql.Replace("@programId", programId);
            oDAL = new cDAL("ACTIVE");
            string day = oDAL.GetObject(sql).ToString();

            string program = programName + "-" + site;
            program = program.Replace("'", "");
            DBTitle = program + " - " + day + " Shift - Operation " + _selectedText + " Last Refreshed Time " + refreshTime;
            shift = day;

            if (!oDAL.HasErrors)
            {
                lstIngenico = cCommon.ConvertDtToArrayListWithZero(dt);
                return true;
            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
        }
        public bool GetCounter(out Dictionary<string, Dictionary<string, object>> resultData, string selectedVal, string selectedText, string programId, string site, string programName)
        {
            resultData = new Dictionary<string, Dictionary<string, object>>();

            DateTime ClientDate = DateTime.Now;

            ClientDate = ClientDate.ToUniversalTime();
            ClientDate = ClientDate.AddHours(2);

            string s = ClientDate.ToString("HH:mm:ss");

            string am = ClientDate.ToString("hh:mm tt");
            var now = TimeSpan.Parse(s);
            string sql = string.Empty;
            sql = @"SELECT 
    CASE
        WHEN CONVERT(TIME, SWITCHOFFSET(SYSDATETIMEOFFSET(), 
            CASE 
                WHEN TimeZone = 'Pacific Standard Time' THEN '-08:00'
                WHEN TimeZone = 'Pacific Standard Time (Mexico)' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Singapore Standard Time' THEN '+08:00'
                WHEN TimeZone = 'Central Europe Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Central European Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Eastern Standard Time' THEN '-05:00'
                WHEN TimeZone = 'GMT Standard Time' THEN '+00:00'
                WHEN TimeZone = 'Greenwich Standard Time' THEN '+00:00'
                WHEN TimeZone = 'SE Asia Standard Time' THEN '+07:00'
                WHEN TimeZone = 'Tokyo Standard Time' THEN '+09:00'
                WHEN TimeZone = 'AUS Eastern Standard Time' THEN '+10:00'
                ELSE '00:00' -- Default offset for unknown time zones
            END
        )) BETWEEN '06:00:00' AND '11:59:59' THEN 'Morning'
        WHEN CONVERT(TIME, SWITCHOFFSET(SYSDATETIMEOFFSET(), 
            CASE 
                WHEN TimeZone = 'Pacific Standard Time' THEN '-08:00'
                WHEN TimeZone = 'Pacific Standard Time (Mexico)' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Singapore Standard Time' THEN '+08:00'
                WHEN TimeZone = 'Central Europe Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Central European Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Eastern Standard Time' THEN '-05:00'
                WHEN TimeZone = 'GMT Standard Time' THEN '+00:00'
                WHEN TimeZone = 'Greenwich Standard Time' THEN '+00:00'
                WHEN TimeZone = 'SE Asia Standard Time' THEN '+07:00'
                WHEN TimeZone = 'Tokyo Standard Time' THEN '+09:00'
                WHEN TimeZone = 'AUS Eastern Standard Time' THEN '+10:00'
                ELSE '00:00'
            END
        )) BETWEEN '12:00:00' AND '17:59:59' THEN 'Afternoon'
        WHEN CONVERT(TIME, SWITCHOFFSET(SYSDATETIMEOFFSET(), 
            CASE 
                WHEN TimeZone = 'Pacific Standard Time' THEN '-08:00'
                WHEN TimeZone = 'Pacific Standard Time (Mexico)' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time' THEN '-07:00'
                WHEN TimeZone = 'Mountain Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time' THEN '-06:00'
                WHEN TimeZone = 'Central Standard Time (Mexico)' THEN '-06:00'
                WHEN TimeZone = 'Singapore Standard Time' THEN '+08:00'
                WHEN TimeZone = 'Central Europe Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Central European Standard Time' THEN '+01:00'
                WHEN TimeZone = 'Eastern Standard Time' THEN '-05:00'
                WHEN TimeZone = 'GMT Standard Time' THEN '+00:00'
                WHEN TimeZone = 'Greenwich Standard Time' THEN '+00:00'
                WHEN TimeZone = 'SE Asia Standard Time' THEN '+07:00'
                WHEN TimeZone = 'Tokyo Standard Time' THEN '+09:00'
                WHEN TimeZone = 'AUS Eastern Standard Time' THEN '+10:00'
                ELSE '00:00'
            END
        )) BETWEEN '18:00:00' AND '21:59:59' THEN 'Evening'
        ELSE 'Night'
    END AS TimeOfDay
FROM 
    pls.program

	where id = @programId;
";
            sql = sql.Replace("@programId", programId);

            oDAL = new cDAL("ACTIVE");
            string day = oDAL.GetObject(sql).ToString();

            

            //shift = "Morning";
            oDAL = new cDAL("DWDB");
            string sql1 = string.Empty;
            sql1 = @"Select distinct MAX(RefreshTime) from btv.Productivity  WHERE programId = '@programId'";
            sql1 = sql1.Replace("@programId", programId);
            //sql1 = sql1.Replace("@operation", _selectedText);

            string refreshTime = oDAL.GetObject(sql1).ToString();
            DBTitle = "" + programName + "-" + site + " - " + day + " Shift - Total Shift Last Refreshed Time " + refreshTime;

            string query = string.Empty;

            query = @"
---RECEIVING---
SELECT 
COUNT(*) AS Operators,
isnull(SUM(qTotal),0) AS qTotal,
ISNULL(
    CASE 
        WHEN SUM(qtouch) IS NULL OR COUNT(*) = 0 THEN '0%'
        ELSE CONCAT(CAST(SUM(qtouch) * 100.0 / (COUNT(*) * 450) AS DECIMAL(5, 1)), '%')
    END,
    '0%'
) AS Productivity
FROM 
[DW].[btv].[Productivity] Where ProgramId = '@programId' AND Operation = 'RECEIVING'

---DIAGNOSIS---
SELECT 
COUNT(*) AS Operators,
ISNULL (SUM(qTotal),0) AS qTotal,
ISNULL(
    CASE 
        WHEN SUM(qtouch) IS NULL OR COUNT(*) = 0 THEN '0%'
        ELSE CONCAT(CAST(SUM(qtouch) * 100.0 / (COUNT(*) * 450) AS DECIMAL(5, 1)), '%')
    END,
    '0%'
) AS Productivity
FROM 
[DW].[btv].[Productivity] Where ProgramId = '@programId' AND Operation = 'DIAGNOSIS'

---DATAWIPE---
SELECT 									
COUNT(*) AS Operators,
ISNULL(SUM(qTotal),0) AS qTotal,
ISNULL(
    CASE 
        WHEN SUM(qtouch) IS NULL OR COUNT(*) = 0 THEN '0%'
        ELSE CONCAT(CAST(SUM(qtouch) * 100.0 / (COUNT(*) * 450) AS DECIMAL(5, 1)), '%')
    END,
    '0%'
) AS Productivity
FROM 
[DW].[btv].[Productivity] Where ProgramId = '@programId' AND Operation = 'DATAWIPE'

---FINALTEST---
SELECT 
COUNT(*) AS Operators,
ISNULL(SUM(qTotal),0) AS qTotal,
ISNULL(
    CASE 
        WHEN SUM(qtouch) IS NULL OR COUNT(*) = 0 THEN '0%'
        ELSE CONCAT(CAST(SUM(qtouch) * 100.0 / (COUNT(*) * 450) AS DECIMAL(5, 1)), '%')
    END,
    '0%'
) AS Productivity
FROM 
[DW].[btv].[Productivity] Where ProgramId = '@programId' AND Operation = 'FINALTEST'

---TRIAGE---
SELECT 
COUNT(*) AS Operators,
ISNULL(SUM(qTotal),0) AS qTotal,
ISNULL(CONCAT(CAST(SUM(qtouch) * 100.0 / (COUNT(*) * 450) AS DECIMAL(5, 1)), '%'),'0%')  AS Productivity
FROM 
[DW].[btv].[Productivity] Where ProgramId = '@programId' AND Operation = 'TRIAGE'

---GATEKEEPER---
SELECT 
COUNT(*) AS Operators,
ISNULL(SUM(qTotal),0) AS qTotal,
ISNULL(
    CASE 
        WHEN SUM(qtouch) IS NULL OR COUNT(*) = 0 THEN '0%'
        ELSE CONCAT(CAST(SUM(qtouch) * 100.0 / (COUNT(*) * 450) AS DECIMAL(5, 1)), '%')
    END,
    '0%'
) AS Productivity
FROM 
[DW].[btv].[Productivity] Where ProgramId = '@programId' AND Operation = 'GATEKEEPER'

---FACOS---
SELECT 
COUNT(*) AS Operators,
ISNULL(SUM(qTotal),0) AS qTotal,
ISNULL(
    CASE 
        WHEN SUM(qtouch) IS NULL OR COUNT(*) = 0 THEN '0%'
        ELSE CONCAT(CAST(SUM(qtouch) * 100.0 / (COUNT(*) * 450) AS DECIMAL(5, 1)), '%')
    END,
    '0%'
) AS Productivity
FROM 
[DW].[btv].[Productivity] Where ProgramId = '@programId' AND Operation = 'FACOS'

---OBA---
SELECT 
COUNT(*) AS Operators,
ISNULL(SUM(qTotal), 0)  AS qTotal,
ISNULL(
    CASE 
        WHEN SUM(qtouch) IS NULL OR COUNT(*) = 0 THEN '0%'
        ELSE CONCAT(CAST(SUM(qtouch) * 100.0 / (COUNT(*) * 450) AS DECIMAL(5, 1)), '%')
    END,
    '0%'
) AS Productivity
FROM 
[DW].[btv].[Productivity] Where ProgramId = '@programId' AND Operation = 'OBA'


";
            query = query.Replace("@programId", programId);
            DataSet DS = oDAL.GetDataSet(query);

            if (!oDAL.HasErrors)
            {
                // Process each DataTable in the DataSet and add to resultData
                string[] sectionNames = { "RECEIVING", "DIAGNOSIS", "DATAWIPE", "FINALTEST", "TRIAGE", "GATEKEEPER", "FACOS", "OBA" };

                for (int i = 0; i < DS.Tables.Count; i++)
                {
                    DataTable table = DS.Tables[i];
                    if (table.Rows.Count > 0)
                    {
                        DataRow row = table.Rows[0];

                        var sectionData = new Dictionary<string, object>
                {
                    { "Operators", row["Operators"] },
                    { "qTotal", row["qTotal"] },
                    { "Productivity", row["Productivity"] }
                };

                        resultData[sectionNames[i]] = sectionData;
                    }
                }

                return true;
            }
            else
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
        }
        public void GetMorningRefTime()
        {
            //            oDAL = new cDAL("dw");
            //            query = @"SELECT DISTINCT refresh_time, 
            //                [shift] 
            //FROM   [DW].[rc_dw].[dw_rep_afternoon_ignecio_fact] ";
            //            d = new DataTable();
            //            dtMorningRef = oDAL.GetData(query);
        }
        #endregion
    }
}