using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
namespace IP.Areas.ListingReports.Models
{
    public class BoseRepair
    {

        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        public int totalIndex { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
		[Display(Name = "Program:")]
		public string program_Id { get; set; }
		[Display(Name = "Program:")]
		public string program { get; set; }
		public String ProgramforSite { get; set; }
        public string ProgramBySite { get; set; }
        public List<ArrayList> lstRepair { get; set; }
        public List<Hashtable> lstWOUnit { get; set; }

        public List<ArrayList> lstData { get; set; }

		#endregion
		#region "Methods"
		public DataTable Program() // onHand warehouse method
		{
			string sites = HttpContext.Current.Session["DefaultSite"].ToString();
			string query = string.Empty;
			query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

			query = query.Replace("<site>", sites);
			DataTable dt = oDAL.GetData(query);
			return dt;
		}
		public bool GetList(string fDate, string tDate, string programId, string programName)
        {
            string query = string.Empty;
            var conType = @HttpContext.Current.Session["CONN_TYPE"].ToString();



            query = @"
-- Step 1: Create the Temporary Table
CREATE TABLE #xx (
    Line NVARCHAR(255),
    partno NVARCHAR(255),
    serialno NVARCHAR(255),
    createdate DATETIME,
    customerreference NVARCHAR(255),
    lastactivitydate DATETIME,
    ship_status NVARCHAR(255),
    WorkStation NVARCHAR(255),
    Position NVARCHAR(255),
    CodeName NVARCHAR(255),
    StatusDesc NVARCHAR(255),
    locationno NVARCHAR(255),
    PROCESS_TYPE NVARCHAR(255),
    RMA NVARCHAR(255),
    MANUAL_DISPO NVARCHAR(255),
    BoxNo NVARCHAR(255),
    Position2 NVARCHAR(255)
);

-- Step 2: Insert Data into the Temporary Table
INSERT INTO #xx (Line, partno, serialno, createdate, customerreference, lastactivitydate, ship_status, 
                 WorkStation, Position, CodeName, StatusDesc, locationno, PROCESS_TYPE, RMA, 
                 MANUAL_DISPO, BoxNo, Position2)
SELECT Concat(CASE
                        WHEN x.codename IN ( 'ARIZONA', /*'EDELMAN',*/ 'LIPTON',
                                             'MINNOW',
                                             'PHELPS',
                                             'SKIPPER', /* 'ZAKIM',*/
                                             'PROFESSOR BB' )
                              THEN
                        'AIO CELL'
                        WHEN x.codename IN ( 'EDDIE', 'LANCOME', 'LANCOME PLUS',
                                             'M3',
                                             'TAYLOR'
                                           ) THEN
                        'BT CELL'
                        WHEN x.codename IN ( 'DURAN', 'GOODYEAR', 'LONE STARR',
                                             'PRINCE'
                                           ) THEN
                        'HEADSET CELL'
                        WHEN x.codename IN ( 'SCOTTY', /* 'SERENA',*/ 'SMALLS' )
                      THEN
                        'INEAR CELL'
                        WHEN x.codename IN ( 'ANGUS', 'BABY YODA',
                                             'BENTO GILLIGAN',
                                             'CHIBI',
                                             'GILLIGAN PREMIUM',
                                             'GINGER CHEEVERS',
                                             'MALCOLM',
                                             'SAN DIEGO',
                                             'STEVIE', 'ZAKIM' ) THEN
                        'SYSTEM CELL'
                        WHEN x.codename IN ( 'EDELMAN' ) THEN 'NPI - AIO CELL'
                        WHEN x.codename IN ( 'SERENA' ) THEN 'NPI - INEAR CELL'
                      END, ' - ', x.codename) Line,
               x.*,
               ''                             Position2
        FROM   (SELECT woh.partno,
                       woh.serialno,
                       woh.createdate,
                       woh.customerreference,
                       woh.lastactivitydate,
                       (SELECT cs.[description]
                        FROM   pls.codestatus cs  with (nolock)
                        WHERE  cs.id =
                       pt.parttransactionid)             ship_status,
                       Isnull(wsd.code, cws.[description])
                       WorkStation,
                       CASE
                         WHEN Upper(Isnull(wsd.code, cws.[description])) =
                              'A1030'
                              AND cs.[description] = 'WIP' THEN 'FAIL'
                         WHEN Upper(Isnull(wsd.code, cws.[description])) IN
                              ( 'A1200', 'CLOSE' ) THEN
                         'FGI'
                         WHEN cs.[description] = 'HOLD' THEN 'HOLD'
                         WHEN Upper(Isnull(wsd.code, cws.[description])) =
                              'WFFA' THEN
                         'FRESH'
                         WHEN Upper(Isnull(wsd.code, cws.[description])) =
                              'Scrap' THEN
                         'SCRAP'
                         WHEN Upper(Isnull(wsd.code, cws.[description])) =
                              'A1090' THEN
                         'PACKING'
                         ELSE 'WIP'
                       END                                               AS
                       'Position',
                       (SELECT Upper(Max(pna.[value]))
                        FROM   pls.partnoattribute pna with (nolock),
                               pls.codeattribute ca1 with (nolock)
                        WHERE  ca1.id = pna.attributeid
                               AND ca1.attributename = 'CODE_NAME'
                               AND pna.partno = woh.partno)              AS
                       CodeName,
                       cs.[description]                                  AS
                       StatusDesc,
                       pl.locationno,
                       (SELECT CASE
                                 WHEN rha1.value IN( 'RETURN', 'EXCHANGE' )
                               THEN
                                 'REMAN'
                                 ELSE rha1.value
                               END
                        FROM pls.roheaderattribute rha1 with (nolock),
                             pls.codeattribute ca1 with (nolock)
                        WHERE ca1.id = rha1.attributeid
                               AND ca1.attributename = 'PROCESS_TYPE'
                               AND rha1.roheaderid = ps.roheaderid)
                       PROCESS_TYPE,
                       (SELECT TOP 1 ro.customerreference
                        FROM   pls.roheader ro
                        WHERE ro.id = ps.roheaderid)                    RMA,
                       Isnull(
               (SELECT TOP 1 psa.value
                FROM   pls.partserialattribute psa with (nolock),
                       pls.codeattribute ca
                WHERE  ca.id = psa.attributeid
                       AND ca.attributename =
                           'RECEIVING_MANUAL_DISPOSITION_OVERRIDE'
                       AND psa.partserialid = ps.id), '0')MANUAL_DISPO,
                       (SELECT ps.palletboxno
                        FROM   pls.partserial ps with (nolock)
                               LEFT JOIN pls.partlocation pl with (nolock)
                                      ON pl.id = ps.locationid
                        WHERE ps.programid = '<programId>'
                               AND ps.partno = WOh.partno
                               AND ps.serialno = WOh.serialno)           BoxNo
                FROM   pls.woheader woh with (nolock)
                       INNER JOIN pls.partserial ps with (nolock)
                               ON ps.woheaderid = woh.id
                                  AND ps.serialno = woh.serialno
                                  AND woh.partno = ps.partno
                       LEFT JOIN pls.codeworkstationcustomdescription WSD  with (nolock)
                              ON WSD.programid = woh.programid
                                 AND WSD.repairtypeid = woh.repairtypeid
                                 AND WSD.codeworkstationid = woh.workstationid
                       LEFT JOIN pls.codestatus cs  with (nolock)
                              ON cs.id = woh.statusid
                       LEFT JOIN pls.codeworkstation cws  with (nolock)
                              ON cws.id = ps.workstationid
                       LEFT JOIN pls.partlocation pl  with (nolock)
                              ON pl.id = ps.locationid
                       LEFT JOIN pls.parttransaction pt  with (nolock)
                              ON(pt.programid = woh.programid
                                   AND pt.partno = woh.partno
                                   AND pt.serialno = woh.serialno
                                   AND pt.parttransactionid = 18)
                WHERE woh.programid = '<programId>'
              --and woh.SerialNo = '073962Z73390206AE'
               ) x
        --where x.ship_status is null
        UNION
        SELECT Concat(CASE
                        WHEN m1.codename IN('ARIZONA', /*'EDELMAN',*/ 'LIPTON'
                                              ,
                                              'MINNOW',
                                              'PHELPS',
                                              'SKIPPER', /* 'ZAKIM',*/
                                              'PROFESSOR BB')
                      THEN
                        'AIO CELL'
                        WHEN m1.codename IN('EDDIE', 'LANCOME', 'LANCOME PLUS'
                                              , 'M3',
                                              'TAYLOR')
                      THEN 'BT CELL'
                        WHEN m1.codename IN('DURAN', 'GOODYEAR', 'LONE STARR',
                                              'PRINCE')
                             THEN
                        'HEADSET CELL'
                        WHEN m1.codename IN('SCOTTY', /* 'SERENA',*/ 'SMALLS'
                                            ) THEN
                        'INEAR CELL'
                        WHEN m1.codename IN('ANGUS', 'BABY YODA',
                                              'BENTO GILLIGAN',
                                              'CHIBI',
                                              'GILLIGAN PREMIUM',
                                              'GINGER CHEEVERS',
                                              'MALCOLM',
                                              'SAN DIEGO',
                                                     'STEVIE') THEN
                        'SYSTEM CELL'
                        WHEN m1.codename IN('ZAKIM', 'EDELMAN') THEN
                        'NPI - AIO CELL'
                        WHEN m1.codename IN('SERENA') THEN 'NPI - INEAR CELL'
                      END, ' - ', m1.codename)       Line,
               m1.partno,
               m1.serialno,
               m1.createdate,
               '',
               m1.lastactivitydate,
               m1.ship_status,
               m1.workstation,
               CASE
                 WHEN m1.manual_dispo IN( 'SCRAP', 'NOSCAN', 'UNWANTED' ) THEN
                 'SCRAP'
                 WHEN m1.manual_dispo IN( 'NEW' ) THEN 'FGI'
                 WHEN m1.manual_dispo IN( 'REPACK' ) THEN 'PACKING'
                 ELSE m1.manual_dispo
               END                                   position,
               m1.codename,
               CASE
                 WHEN m1.manual_dispo IN( 'SCRAP', 'NOSCAN', 'UNWANTED' ) THEN
                 'SCRAP'
                 WHEN m1.manual_dispo IN( 'NEW' ) THEN 'FGI'
                 ELSE m1.manual_dispo
               END                                   statusdesc,
               locationno,
               process_type,
               rma,
               manual_dispo,
               (SELECT ps.palletboxno
                FROM   pls.partserial ps  with (nolock)
                       LEFT JOIN pls.partlocation pl  with (nolock)
                              ON pl.id = ps.locationid
                WHERE ps.programid = '<programId>'
                       AND ps.partno = m1.partno
                       AND ps.serialno = m1.serialno)BoxNo,
               'NO-WO Unit'                          Position2
        FROM(SELECT(SELECT Upper(Max(pna.[value]))
                        FROM   pls.partnoattribute pna  with (nolock),
                               pls.codeattribute ca1  with (nolock)
                        WHERE  ca1.id = pna.attributeid
                               AND ca1.attributename = 'CODE_NAME'
                               AND pna.partno = ps.partno)          AS codename,
                       ps.partno,
                       ps.serialno,
                       ps.createdate,
                       ps.lastactivitydate,
                       (SELECT cs.[description]
                        FROM   pls.codestatus cs  with (nolock)
                        WHERE  cs.id = pt.parttransactionid)        ship_status,
                       NULL workstation,
                       (SELECT TOP 1 psa.value
                        FROM   pls.partserialattribute psa  with (nolock),
                               pls.codeattribute ca
                        WHERE  ca.id = psa.attributeid
                               AND ca.attributename =
                                   'RECEIVING_MANUAL_DISPOSITION_OVERRIDE'
                               AND psa.partserialid = ps.id)        MANUAL_DISPO
                       ,
                       (SELECT CASE
                                 WHEN rha1.value IN( 'RETURN', 'EXCHANGE' )
                               THEN
                                 'REMAN'
                                 ELSE rha1.value
                               END
                        FROM pls.roheaderattribute rha1  with (nolock),
                             pls.codeattribute ca1  with (nolock)
                        WHERE ca1.id = rha1.attributeid
                               AND ca1.attributename = 'PROCESS_TYPE'
                               AND rha1.roheaderid = ps.roheaderid) PROCESS_TYPE
                       ,
                       pl.locationno,
                       (SELECT TOP 1 ro.customerreference
                        FROM   pls.roheader ro
                        WHERE ro.id = ps.roheaderid)               RMA
               -- ps.*
               FROM   pls.partserial ps
                       LEFT JOIN pls.partlocation pl  with (nolock)
                              ON pl.id = ps.locationid
                       LEFT JOIN pls.parttransaction pt  with (nolock)
                              ON(pt.programid = ps.programid
                                   AND pt.partno = ps.partno
                                   AND pt.serialno = ps.serialno
                                   AND pt.parttransactionid = 18)
                WHERE ps.programid = '<programId>'
                      -- and ps.SerialNo = '084152P22673675AE'
                       AND ps.woheaderid IS NULL) m1
        WHERE  Isnull(m1.manual_dispo, '0') != '0'
       --and m1.SerialNo = '073962Z73390206AE'



-- Step 1: Define Start and End Dates
DECLARE @StartDate DATE = '<fDate>';
            DECLARE @EndDate DATE = '<tDate>';
            DECLARE @ProgramId INT = '<programId>';

            --Step 2: Generate a list of dates dynamically
DECLARE @DateList NVARCHAR(MAX) = '';
            DECLARE @DynamicSQL NVARCHAR(MAX);

            WITH DateRange AS(
                SELECT @StartDate AS DateValue
            
                UNION ALL
            
                SELECT DATEADD(DAY, 1, DateValue)
            
                FROM DateRange
            
                WHERE DateValue < @EndDate
            )
SELECT
    @DateList = STRING_AGG(QUOTENAME(CONVERT(VARCHAR, DateValue, 120)), ',')-- Dynamically create column names
FROM DateRange;

            --Step 3: Write the dynamic SQL query
           SET @DynamicSQL = N'
WITH DateRange AS(
    SELECT @StartDate AS DateValue
    UNION ALL
    SELECT DATEADD(DAY, 1, DateValue)
    FROM DateRange
    WHERE DateValue <= @EndDate
),
AggregatedData AS(
    SELECT
        dr.DateValue AS ReportDate,
        ''WIP - [REPAIR]'' AS REPAIR,
        Sum(CASE WHEN xx.process_type = ''REPAIR'' AND xx.statusdesc = ''WIP'' AND CONVERT(DATE, xx.createdate) <= dr.DateValue THEN 1 ELSE 0 END) AS REPAIRValue
    FROM DateRange dr
    LEFT JOIN #xx xx ON CONVERT(DATE, xx.createdate) <= dr.DateValue
    GROUP BY dr.DateValue

    UNION ALL

    SELECT
        dr.DateValue AS ReportDate,
        ''HOLD'' AS REPAIR,
        Sum(CASE WHEN xx.process_type = ''REPAIR'' AND xx.statusdesc = ''HOLD'' AND CONVERT(DATE, xx.createdate) <= dr.DateValue THEN 1 ELSE 0 END) AS REPAIRValue
    FROM DateRange dr
    LEFT JOIN #xx xx ON CONVERT(DATE, xx.createdate) <= dr.DateValue
    GROUP BY dr.DateValue

    UNION ALL

    SELECT
        dr.DateValue AS ReportDate,
        ''InboundPlan'' AS REPAIR,
        (SELECT c03
         FROM pls.vcodegenerictable
         WHERE generictablename = ''FORECAST''
               AND programid = @ProgramId
               --AND CONVERT(DATE, lastactivitydate) <= dr.DateValue
               AND c01 = ''REPAIR'' AND c02 = ''202411'') AS REPAIRValue
    FROM DateRange dr

    UNION ALL

    SELECT
        dr.DateValue AS ReportDate,
        ''InboundActual'' AS REPAIR,
        Sum(CASE WHEN xx.process_type = ''REPAIR'' AND CONVERT(DATE, xx.createdate) = dr.DateValue THEN 1 ELSE 0 END) AS REPAIRValue
    FROM DateRange dr
    LEFT JOIN #xx xx ON CONVERT(DATE, xx.createdate) = dr.DateValue
    GROUP BY dr.DateValue

    UNION ALL

    SELECT
        dr.DateValue AS ReportDate,
        ''ActualvsPlanInbound'' AS REPAIR,
        CASE
            WHEN(SELECT c03
                  FROM pls.vcodegenerictable
                  WHERE generictablename = ''FORECAST''
                        AND programid = @ProgramId
                        --AND CONVERT(DATE, lastactivitydate) <= dr.DateValue
                        AND c01 = ''REPAIR'' AND c02 = ''202411'') = 0 THEN NULL
            ELSE(Sum(CASE WHEN xx.process_type = ''REPAIR'' AND CONVERT(DATE, xx.createdate) = dr.DateValue THEN 1 ELSE 0 END) * 100 /
                  (SELECT c03
                   FROM pls.vcodegenerictable
                   WHERE generictablename = ''FORECAST''
                         AND programid = @ProgramId
                         --AND CONVERT(DATE, lastactivitydate) <= dr.DateValue
                         AND c01 = ''REPAIR'' AND c02 = ''202411''))
        END AS REPAIRValue
    FROM DateRange dr
    LEFT JOIN #xx xx ON CONVERT(DATE, xx.createdate) = dr.DateValue
    GROUP BY dr.DateValue

    UNION ALL

    SELECT
        dr.DateValue AS ReportDate,
        ''OutputPlan'' AS REPAIR,
        (SELECT c04
         FROM pls.vcodegenerictable
         WHERE programid = @ProgramId
               --AND CONVERT(DATE, lastactivitydate) <= dr.DateValue
               AND c01 = ''REPAIR'' AND c02 = ''202411'') AS REPAIRValue
    FROM DateRange dr

    UNION ALL

    SELECT
        dr.DateValue AS ReportDate,
        ''Send to Packout'' AS REPAIR,
        Sum(CASE WHEN xx.workstation = ''CLOSE'' AND xx.process_type = ''REPAIR'' AND xx.ship_status = ''SHIPPED'' AND CONVERT(DATE, xx.createdate) = dr.DateValue THEN 1 ELSE 0 END) AS REPAIRValue
    FROM DateRange dr
    LEFT JOIN #xx xx ON CONVERT(DATE, xx.createdate) = dr.DateValue
    GROUP BY dr.DateValue

    UNION ALL

    SELECT
        dr.DateValue AS ReportDate,
        ''ActualvsPlanOutput'' AS REPAIR,
        CASE
            WHEN(SELECT c04
                  FROM pls.vcodegenerictable
                  WHERE programid = @ProgramId
                        --AND CONVERT(DATE, lastactivitydate) <= dr.DateValue
                        AND c01 = ''REPAIR'' AND c02 = ''202411'') = 0 THEN NULL
            ELSE(Sum(CASE WHEN xx.workstation = ''CLOSE'' AND xx.process_type = ''REPAIR'' AND xx.ship_status = ''SHIPPED'' AND CONVERT(DATE, xx.createdate) = dr.DateValue THEN 1 ELSE 0 END) * 100 /
                  (SELECT c04
                   FROM pls.vcodegenerictable
                   WHERE programid = @ProgramId
                         --AND CONVERT(DATE, lastactivitydate) <= dr.DateValue
                         AND c01 = ''REPAIR'' AND c02 = ''202411''))
        END AS REPAIRValue
    FROM DateRange dr
    LEFT JOIN #xx xx ON CONVERT(DATE, xx.createdate) = dr.DateValue
    GROUP BY dr.DateValue

    UNION ALL

    SELECT
        dr.DateValue AS ReportDate,
        ''Shipped'' AS REPAIR,
        Sum(CASE WHEN xx.ship_status = ''SHIPPED'' AND xx.workstation = ''CLOSE'' AND xx.process_type = ''REPAIR'' AND CONVERT(DATE, xx.createdate) = dr.DateValue THEN 1 ELSE 0 END) AS REPAIRValue
    FROM DateRange dr
    LEFT JOIN #xx xx ON CONVERT(DATE, xx.createdate) = dr.DateValue
    GROUP BY dr.DateValue
)

SELECT REPAIR, ' + @DateList + '
FROM AggregatedData
PIVOT(
    MAX(REPAIRValue)-- Aggregate function
    FOR ReportDate IN(' + @DateList + ')-- Pivot on dynamic dates
) AS PivotTable;
            ';

            -- Step 4: Execute the dynamic SQL
EXEC sp_executesql @DynamicSQL, N'@StartDate DATE, @EndDate DATE, @ProgramId INT', @StartDate, @EndDate, @ProgramId;


            DROP TABLE #xx






            ";
			query = query.Replace("<programId>", programId);
			query = query.Replace("<fDate>", fDate);
            query = query.Replace("<tDate>", tDate);

            DataTable dt = oDAL.GetDataForGeneric(query);

            //dt.Columns.Add("Total");
            //foreach (DataRow row in dt.Rows)
            //{
            //    decimal total = 0;
            //    for (int i = 1; i < dt.Columns.Count - 1; i++)
            //    {
            //        decimal value = 0;
            //        if (row[i] != DBNull.Value)
            //            value = Convert.ToDecimal(row[i]);

            //        total += value;
            //    }


            //    row["TOTAL"] = total;
            //}




            DataTable dtList = dt.Clone();
            DataTable dtColHeader = cCommon.GenerateTransposedTable(dtList); 
            filterString = " Program = BOSE ";
            filterString += " | From = '" + fDate + "' To = '" + tDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("240", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRepair = cCommon.ConvertDtToArrayList(dt);
                lstData = cCommon.ConvertDtToArrayList(dtColHeader);

                return true;

            }
        }
        #endregion
    }
}
