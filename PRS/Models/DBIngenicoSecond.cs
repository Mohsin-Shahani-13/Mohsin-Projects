using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Models
{
    public class DBIngenicoSecond
    {
        cDAL oDAL = new cDAL("dw");
        string query = string.Empty;
        public string RefreshTime { get; set; }
        public List<ArrayList> lstIngenico { get; set; }

        public List<object> lstMst = new List<object>();
        public List<ArrayList> lstMtype1 { get; set; }
        public string ErrorMessage { get; set; }

        public string Shift { get; set; }

        public string WIPStatus { get; set; }


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


        #region Method

        public bool GetDashboard()
        {
            DateTime ClientDate = DateTime.Now;

            ClientDate = ClientDate.ToUniversalTime();
            ClientDate = ClientDate.AddHours(1);

            string s = ClientDate.ToString("HH:mm:ss");
            var now = TimeSpan.Parse(s);

            DataTable dtIngenico = new DataTable();
            DataTable dtMorningRef = new DataTable();
            DataTable dtMtype1 = new DataTable();
            if (now >= MorningStart && now < MorningEnd)
            {

                query = @"-- IGNGENICO WIP Morning Shift start --
select * from [DW].[rc_dw].[dw_ignecio_tech_dim];

select distinct [SHIFT],  Format(REFRESHED, 'yyyy.MM.dd HH:mm') AS REFRESHED
from [DW].[rc_dw].[dw_wip_morning_ignecio_fact]
WHERE REFRESHED is not null;

SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_morning_ignecio_fact] 
WHERE  mtype IN ( 'OUTPUT' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_morning_ignecio_fact] 
WHERE  mtype IN ( 'TO BER' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_morning_ignecio_fact] 
WHERE  mtype IN ( 'TO HOLD' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_morning_ignecio_fact] 
WHERE  mtype IN ( 'FROM HOLD' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_morning_ignecio_fact] 
WHERE  mtype IN ( 'TOTAL PROCESSED' );

select MTYPE, PACKING, REP_LEVEL_2, REP_LEVEL_3, FINAL_TEST, [ACTIVATION], CLEANING, QUALITY
from [DW].[rc_dw].[dw_wip_morning_ignecio_fact]
where MTYPE in ('ACTUAL SHIFT TARGET', 'TO DO');

select MTYPE, PACKING, REP_LEVEL_2, REP_LEVEL_3, FINAL_TEST, [ACTIVATION], CLEANING, QUALITY
from [DW].[rc_dw].[dw_wip_morning_ignecio_fact]
where MTYPE in ('WIP');

select case when FINAL_TEST > 30 or [ACTIVATION] > 20 or CLEANING > 20 or QUALITY > 20 then 'WIP TOO HIGH' else 'WIP UNDER CONTROL' end WIP
from [DW].[rc_dw].[dw_wip_morning_ignecio_fact]
where MTYPE in ('WIP');
-- IGNGENICO WIP Morning Shift end --
";
            }
            else if (now >= AfternoonStart && now < AfternoonEnd)
            {
                query = @"-- IGNGENICO WIP Afternoon Shift start --
select * from [DW].[rc_dw].[dw_ignecio_tech_dim];

select distinct [SHIFT], Format(REFRESHED, 'yyyy.MM.dd HH:mm') AS REFRESHED
from [DW].[rc_dw].[dw_wip_afternoon_ignecio_fact]
WHERE REFRESHED is not null;

SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_afternoon_ignecio_fact] 
WHERE  mtype IN ( 'OUTPUT' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_afternoon_ignecio_fact] 
WHERE  mtype IN ( 'TO BER' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_afternoon_ignecio_fact] 
WHERE  mtype IN ( 'TO HOLD' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_afternoon_ignecio_fact] 
WHERE  mtype IN ( 'FROM HOLD' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_afternoon_ignecio_fact] 
WHERE  mtype IN ( 'TOTAL PROCESSED' ) ;

select MTYPE, PACKING, REP_LEVEL_2, REP_LEVEL_3, FINAL_TEST, [ACTIVATION], CLEANING, QUALITY
from [DW].[rc_dw].[dw_wip_afternoon_ignecio_fact]
where MTYPE in ('ACTUAL SHIFT TARGET', 'TO DO');

select MTYPE, PACKING, REP_LEVEL_2, REP_LEVEL_3, FINAL_TEST, [ACTIVATION], CLEANING, QUALITY
from [DW].[rc_dw].[dw_wip_afternoon_ignecio_fact]
where MTYPE in ('WIP');

select case when FINAL_TEST > 30 or [ACTIVATION] > 20 or CLEANING > 20 or QUALITY > 20 then 'WIP TOO HIGH' else 'WIP UNDER CONTROL' end WIP
from [DW].[rc_dw].[dw_wip_afternoon_ignecio_fact]
where MTYPE in ('WIP');
-- IGNGENICO WIP Afternoon Shift end --";
            }

            else
            {
                query = @"-- IGNGENICO WIP Night Shift start --
select * from [DW].[rc_dw].[dw_ignecio_tech_dim];

select distinct [SHIFT], Format(REFRESHED, 'yyyy.MM.dd HH:mm') AS REFRESHED
from [DW].[rc_dw].[dw_wip_night_ignecio_fact] 
WHERE REFRESHED is not null;

SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_night_ignecio_fact] 
WHERE  mtype IN ( 'OUTPUT' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_night_ignecio_fact] 
WHERE  mtype IN ( 'TO BER' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_night_ignecio_fact] 
WHERE  mtype IN ( 'TO HOLD' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_night_ignecio_fact] 
WHERE  mtype IN ( 'FROM HOLD' ) 
UNION ALL 
SELECT mtype, 
       packing, 
       rep_level_2, 
       rep_level_3, 
       final_test, 
       [activation], 
       cleaning, 
       quality 
FROM   [DW].[rc_dw].[dw_wip_night_ignecio_fact] 
WHERE  mtype IN ( 'TOTAL PROCESSED' ) ;

select MTYPE, PACKING, REP_LEVEL_2, REP_LEVEL_3, FINAL_TEST, [ACTIVATION], CLEANING, QUALITY
from [DW].[rc_dw].[dw_wip_night_ignecio_fact]
where MTYPE in ('ACTUAL SHIFT TARGET', 'TO DO');

select MTYPE, PACKING, REP_LEVEL_2, REP_LEVEL_3, FINAL_TEST, [ACTIVATION], CLEANING, QUALITY
from [DW].[rc_dw].[dw_wip_night_ignecio_fact]
where MTYPE in ('WIP');

select case when FINAL_TEST > 30 or [ACTIVATION] > 20 or CLEANING > 20 or QUALITY > 20 then 'WIP TOO HIGH' else 'WIP UNDER CONTROL' end WIP
from [DW].[rc_dw].[dw_wip_night_ignecio_fact]
where MTYPE in ('WIP');
-- IGNGENICO WIP Night Shift end --";
            }


            oDAL = new cDAL("dw");
            DataSet ds = oDAL.GetDataSet(query);


            if (!oDAL.HasErrors)
            {

                List<ArrayList> lstDtl = new List<ArrayList>();
                lstDtl = cCommon.ConvertDtToArrayListWithZero(ds.Tables[0]);
                lstMst.Add(lstDtl);


                DataTable dtRefresh = new DataTable();
                dtRefresh = ds.Tables[1];
                if (dtRefresh != null)
                    if (dtRefresh.Rows.Count > 0)
                    {
                        RefreshTime = dtRefresh.Rows[0]["REFRESHED"].ToString();
                        Shift = dtRefresh.Rows[0]["SHIFT"].ToString();

                    }

                //DBTitle = "" + shift + " " + refreshTimeMorning + "";

                lstDtl = cCommon.ConvertDtToArrayListWithZero(ds.Tables[2]);
                lstMst.Add(lstDtl);

                lstDtl = new List<ArrayList>();
                lstDtl = cCommon.ConvertDtToArrayListWithZero(ds.Tables[3]);
                lstMst.Add(lstDtl);

                lstDtl = new List<ArrayList>();
                lstDtl = cCommon.ConvertDtToArrayListWithZero(ds.Tables[4]);
                lstMst.Add(lstDtl);


                DataTable dtWipStatus = new DataTable();
                dtWipStatus = ds.Tables[5];
                if (dtWipStatus != null)
                    if (dtWipStatus.Rows.Count > 0)
                    {
                        WIPStatus = dtWipStatus.Rows[0]["WIP"].ToString();

                    }

                return true;
            }
            else
            {
                return false;
            }
        }


        #endregion
    }
}