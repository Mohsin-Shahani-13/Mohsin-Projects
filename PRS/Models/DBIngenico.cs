using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Models
{
    public class DBIngenico
    {
        cDAL oDAL = new cDAL("dw");
        #region Properties
        string query = string.Empty;
        public string DBTitle { get; set; }
        public List<ArrayList> lstIngenico { get; set; }

        public string ErrorMessage { get; set; }

        public string shift { get; set; }

        #region Data Fields for Morning
        TimeSpan MorningStart = new TimeSpan(06, 0, 0);
        TimeSpan MorningEnd = new TimeSpan(14, 0, 0);

        string refreshTimeMorning = string.Empty;

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
        #endregion


        #region Method

        public bool GetData()
        {

            DateTime ClientDate = DateTime.Now;

            ClientDate = ClientDate.ToUniversalTime();
            ClientDate = ClientDate.AddHours(1);

            string s = ClientDate.ToString("HH:mm:ss");
            var now = TimeSpan.Parse(s);

            DataTable dtIngenico = new DataTable();
            if (now >= MorningStart && now < MorningEnd)
            {
                oDAL = new cDAL("dw");
                string query = @"
SELECT rlevel, 
       [user_name], 
       targets, 
       [6:00-7:00], 
       [7:00-8:00], 
       [8:00-9:00], 
       [9:00-10:00], 
       [10:00-11:00], 
       [11:00-12:00], 
       [12:00-13:00], 
       [13:00-14:00], 
       total, 
      ( case 
when RLEVEL = '2' then 40 
else 30 
end ) - total TO_DO 
FROM   [DW].[rc_dw].[dw_rep_morning_ignecio_fact]   
ORDER BY rlevel, [user_name] ";

                dtIngenico = oDAL.GetData(query);
                string query2 = @"SELECT DISTINCT Format(refresh_time, 'yyyy.MM.dd HH:mm') AS REFRESH_TIME, 
                [shift] 
FROM   [DW].[rc_dw].[dw_rep_morning_ignecio_fact] ";
                DataTable dtMorningRef = new DataTable();
                dtMorningRef = oDAL.GetData(query2);
                if (dtMorningRef != null)
                    if (dtMorningRef.Rows.Count > 0)
                        refreshTimeMorning = dtMorningRef.Rows[0]["REFRESH_TIME"].ToString();

                DBTitle = "Ingenico - Morning Shift Refresh Time " + refreshTimeMorning + "";
                shift = "Morning";
            }

            else if (now >= AfternoonStart && now < AfternoonEnd)
            {
                oDAL = new cDAL("dw");
                string query = @"SELECT rlevel, 
       [user_name], 
       targets, 
       [14:00-15:00], 
       [15:00-16:00], 
       [16:00-17:00], 
       [17:00-18:00], 
       [18:00-19:00], 
       [19:00-20:00], 
       [20:00-21:00], 
       [21:00-22:00], 
       total, 
       ( CASE 
           WHEN rlevel = '2' THEN 40 
           ELSE 30 
         END ) - total TO_DO 
FROM   [DW].[rc_dw].[dw_rep_afternoon_ignecio_fact]  
ORDER BY rlevel, [user_name] ";
                dtIngenico = new DataTable();
                dtIngenico = oDAL.GetData(query);
                string query2 = @"SELECT DISTINCT Format(refresh_time, 'yyyy.MM.dd HH.mm') AS REFRESH_TIME, 
                [shift] 
FROM   [DW].[rc_dw].[dw_rep_afternoon_ignecio_fact] ";
                DataTable dtAfternoonRef = new DataTable();
                dtAfternoonRef = oDAL.GetData(query2);
                if (dtAfternoonRef != null)
                    if (dtAfternoonRef.Rows.Count > 0)
                        refreshTimeAfternoon = dtAfternoonRef.Rows[0]["REFRESH_TIME"].ToString();

                DBTitle = "Ingenico - Afternoon Shift Refresh Time " + refreshTimeAfternoon + "";
                shift = "Afternoon";
            }

            else
            {
                oDAL = new cDAL("dw");
                string query = @"SELECT rlevel, 
       [user_name], 
       targets, 
       [22:00-23:00], 
       [23:00-24:00], 
       [0:00-1:00], 
       [1:00-2:00], 
       [2:00-3:00], 
       [3:00-4:00], 
       [4:00-5:00], 
       [5:00-6:00], 
       total, 
       ( CASE 
           WHEN rlevel = '2' THEN 40
           ELSE 30 
         END ) - total TO_DO 
FROM   [DW].[rc_dw].[dw_rep_night_ignecio_fact]  
ORDER BY rlevel, [user_name] ";
                dtIngenico = new DataTable();
                dtIngenico = oDAL.GetData(query);
                string query2 = @"SELECT DISTINCT Format(refresh_time, 'yyyy.MM.dd HH.mm') AS REFRESH_TIME, 
                [shift] 
FROM   [DW].[rc_dw].[dw_rep_night_ignecio_fact]  ";
                DataTable dtNightRef = new DataTable();
                dtNightRef = oDAL.GetData(query2);
                if (dtNightRef != null)
                    if (dtNightRef.Rows.Count > 0)

                        refreshTimeNight = dtNightRef.Rows[0]["REFRESH_TIME"].ToString();
                DBTitle = "Ingenico - Night Shift Refresh Time " + refreshTimeNight + "";
                shift = "Night";
            }


            if (!oDAL.HasErrors)
            {
                lstIngenico = cCommon.ConvertDtToArrayListWithZero(dtIngenico);

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
            //            oDAL = new cDAL("REDW");
            //            string query = @"SELECT DISTINCT refresh_time, 
            //                [shift] 
            //FROM   [DW].[rc_dw].[dw_rep_afternoon_ignecio_fact] ";
            //            d = new DataTable();
            //            dtMorningRef = oDAL.GetData(query);
        }
        #endregion
    }

}
