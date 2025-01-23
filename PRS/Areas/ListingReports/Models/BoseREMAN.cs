using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class BoseREMAN
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public List<ArrayList> lstRemanDetail { get; set; }
        public List<ArrayList> lstDtlColHeader { get; set; }

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
        public List<ArrayList> lstBoseREMAN { get; set; }
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
DECLARE @fromDate DATE = '<fDate>';
DECLARE @toDate DATE = '<tDate>';
DECLARE @ProgramId int = '<programId>';

DECLARE @sql NVARCHAR(MAX);
DECLARE @currentDate DATE = @fromDate;

SET @sql = '
SELECT 
    CASE 
        WHEN GROUPING(REMAN_GROUP) = 1 THEN ''WIP-REMAN'' 
        ELSE REMAN_GROUP
    END AS REMAN,';

WHILE @currentDate <= @toDate
BEGIN
    SET @sql = @sql + '
    SUM(CASE WHEN CAST(xx.LastActivityDate AS DATE) = ''' + CONVERT(VARCHAR(10), @currentDate, 23) + ''' THEN 1 ELSE 0 END) AS [' + CONVERT(VARCHAR(10), @currentDate, 23) + '],';

    SET @currentDate = DATEADD(DAY, 1, @currentDate);
END;


SET @sql = LEFT(@sql, LEN(@sql) - 1);

SET @sql = @sql + '
FROM (
    select concat(case when x.CodeName in ( ''ARIZONA'', ''LIPTON'', ''MINNOW'', ''PHELPS'', ''SKIPPER'', ''PROFESSOR BB'' ) then ''AIO CELL''
           when x.CodeName in ( ''EDDIE'', ''LANCOME'', ''LANCOME PLUS'', ''M3'', ''TAYLOR'' ) then ''BT CELL''
           when x.CodeName in ( ''DURAN'', ''GOODYEAR'', ''LONE STARR'', ''PRINCE'' ) then ''HEADSET CELL''
           when x.CodeName in ( ''SCOTTY'', ''SMALLS'' ) then ''INEAR CELL''
           when x.CodeName in ( ''ANGUS'', ''BABY YODA'', ''BENTO GILLIGAN'', ''CHIBI'', ''GILLIGAN PREMIUM'', ''GINGER CHEEVERS'',''MALCOLM'', ''SAN DIEGO'', ''STEVIE'', ''ZAKIM'') then ''SYSTEM CELL''
           when x.CodeName in ( ''EDELMAN'') then ''NPI - AIO CELL''
           when x.CodeName in ( ''SERENA'') then ''NPI - INEAR CELL'' end,'' - '', x.codename) Line,
          x.*,
       '''' Position2
    from
    (
        select woh.PartNo,
               woh.SerialNo,
               woh.createdate,
               woh.CustomerReference,
               woh.LastActivityDate,
               (select cs.[Description]
                  from pls.CodeStatus cs
                 where cs.ID =  pt.PartTransactionID) ship_status,
               isnull(wsd.code, cws.[Description]) WorkStation,
               
               (
                   SELECT upper(max(pna.[Value]))
                   FROM pls.PartNoAttribute pna,
                        pls.CodeAttribute ca1
                   WHERE ca1.id = pna.AttributeID
                         AND ca1.AttributeName = ''CODE_NAME''
                         AND pna.PartNo = woh.PartNo
               ) AS CodeName,
               cs.[Description] as StatusDesc,
               pl.LocationNo,
               (
                   SELECT CASE
                              WHEN rha1.Value IN ( ''RETURN'', ''EXCHANGE'' ) THEN
                                  ''REMAN''
                              ELSE
                                  rha1.Value
                          END
                   FROM pls.ROHeaderAttribute rha1,
                        pls.CodeAttribute ca1
                   WHERE ca1.id = rha1.AttributeID
                         AND ca1.AttributeName = ''PROCESS_TYPE''
                         AND rha1.ROHeaderID = ps.ROHeaderID
               ) PROCESS_TYPE,
               (select top 1 ro.CustomerReference from pls.ROHeader ro where ro.id =  ps.ROHeaderID) RMA,
               isnull((select top 1 psa.Value from pls.PartSerialAttribute psa, pls.CodeAttribute ca where ca.id = psa.AttributeID and ca.AttributeName = ''RECEIVING_MANUAL_DISPOSITION_OVERRIDE'' and psa.PartSerialID  = ps.id), ''0'') MANUAL_DISPO,
           (SELECT ps.PalletBoxNo from pls.PartSerial ps 
            left join pls.PartLocation pl with (nolock) on pl.ID = ps.LocationID where ps.ProgramID = @ProgramId and ps.PartNo = WOh.PartNo and ps.SerialNo = WOh.SerialNo) BoxNo from pls.WOHeader woh
            inner join pls.PartSerial ps with (nolock) on ps.WOHeaderID = woh.id and ps.SerialNo = woh.SerialNo and woh.PartNo = ps.PartNo
            LEFT JOIN pls.CodeWorkStationCustomDescription WSD with (nolock) ON WSD.ProgramID = woh.ProgramID AND WSD.RepairTypeID = woh.RepairTypeID AND WSD.CodeWorkStationID = woh.WorkStationID
            LEFT JOIN pls.CodeStatus cs with (nolock) on cs.id = woh.StatusID
            LEFT JOIN pls.CodeWorkStation cws with (nolock) on cws.id = ps.WorkStationID
            left join pls.partlocation pl with (nolock) on pl.id = ps.LocationID
            left join pls.PartTransaction pt  on ( pt.ProgramID = woh.ProgramID and pt.partno = woh.PartNo and pt.SerialNo = woh.SerialNo and pt.PartTransactionID = 18) where woh.ProgramID = @ProgramId) x) xx
CROSS APPLY (VALUES (CASE WHEN xx.CodeName IN (''LONE STAR'', ''DURAN'', ''PRINCE'', ''MALCOLM'', ''STEVIE'', ''SMALLS'', ''SCOTTY'') THEN xx.CodeName ELSE ''Other'' END)) AS GroupingAlias(REMAN_GROUP)
WHERE xx.StatusDesc <> ''SCRAP'' AND convert(date,xx.LastActivityDate) >=  @fromDate AND convert(date,xx.LastActivityDate) <= @toDate GROUP BY REMAN_GROUP WITH ROLLUP ORDER BY CASE WHEN REMAN_GROUP = ''Other'' THEN 1 ELSE 0 END, REMAN_GROUP '

 
 SET @sql = @sql + '
SELECT ''SCRAP Inventory'' AS REMAN,' + REPLICATE('NULL, ', DATEDIFF(DAY, @fromDate, @toDate) - 1) + 'COUNT(*) AS [' + CONVERT(VARCHAR(10), @toDate, 23) + ']
FROM (
    select concat(case when x.CodeName in ( ''ARIZONA'', ''LIPTON'', ''MINNOW'', ''PHELPS'', ''SKIPPER'', ''PROFESSOR BB'' ) then ''AIO CELL''
           when x.CodeName in ( ''EDDIE'', ''LANCOME'', ''LANCOME PLUS'', ''M3'', ''TAYLOR'' ) then ''BT CELL''
           when x.CodeName in ( ''DURAN'', ''GOODYEAR'', ''LONE STARR'', ''PRINCE'' ) then ''HEADSET CELL''
           when x.CodeName in ( ''SCOTTY'', ''SMALLS'' ) then ''INEAR CELL''
           when x.CodeName in ( ''ANGUS'', ''BABY YODA'', ''BENTO GILLIGAN'', ''CHIBI'', ''GILLIGAN PREMIUM'', ''GINGER CHEEVERS'',''MALCOLM'', ''SAN DIEGO'', ''STEVIE'', ''ZAKIM'') then ''SYSTEM CELL''
           when x.CodeName in ( ''EDELMAN'') then ''NPI - AIO CELL''
           when x.CodeName in ( ''SERENA'') then ''NPI - INEAR CELL'' end,'' - '', x.codename) Line,
          x.*,
       '''' Position2
    from
    (
        select woh.PartNo,
               woh.SerialNo,
               woh.createdate,
               woh.CustomerReference,
               woh.LastActivityDate,
               (select cs.[Description]
                  from pls.CodeStatus cs with (nolock)
                 where cs.ID =  pt.PartTransactionID) ship_status,
               isnull(wsd.code, cws.[Description]) WorkStation,
               
               (
                   SELECT upper(max(pna.[Value]))
                   FROM pls.PartNoAttribute pna,
                        pls.CodeAttribute ca1
                   WHERE ca1.id = pna.AttributeID
                         AND ca1.AttributeName = ''CODE_NAME''
                         AND pna.PartNo = woh.PartNo
               ) AS CodeName,
               cs.[Description] as StatusDesc,
               pl.LocationNo,
               (
                   SELECT CASE
                              WHEN rha1.Value IN ( ''RETURN'', ''EXCHANGE'' ) THEN
                                  ''REMAN''
                              ELSE
                                  rha1.Value
                          END
                   FROM pls.ROHeaderAttribute rha1,
                        pls.CodeAttribute ca1
                   WHERE ca1.id = rha1.AttributeID
                         AND ca1.AttributeName = ''PROCESS_TYPE''
                         AND rha1.ROHeaderID = ps.ROHeaderID
               ) PROCESS_TYPE,
               (select top 1 ro.CustomerReference from pls.ROHeader ro where ro.id =  ps.ROHeaderID) RMA,
               isnull((select top 1 psa.Value from pls.PartSerialAttribute psa, pls.CodeAttribute ca where ca.id = psa.AttributeID and ca.AttributeName = ''RECEIVING_MANUAL_DISPOSITION_OVERRIDE'' and psa.PartSerialID  = ps.id), ''0'') MANUAL_DISPO,
           (SELECT ps.PalletBoxNo from pls.PartSerial ps with (nolock)
            left join pls.PartLocation pl with (nolock) on pl.ID = ps.LocationID where ps.ProgramID = @ProgramId and ps.PartNo = WOh.PartNo and ps.SerialNo = WOh.SerialNo) BoxNo from pls.WOHeader woh
            inner join pls.PartSerial ps with (nolock) on ps.WOHeaderID = woh.id and ps.SerialNo = woh.SerialNo and woh.PartNo = ps.PartNo
            LEFT JOIN pls.CodeWorkStationCustomDescription WSD with (nolock) ON WSD.ProgramID = woh.ProgramID AND WSD.RepairTypeID = woh.RepairTypeID AND WSD.CodeWorkStationID = woh.WorkStationID
            LEFT JOIN pls.CodeStatus cs with (nolock) on cs.id = woh.StatusID
            LEFT JOIN pls.CodeWorkStation cws with (nolock) on cws.id = ps.WorkStationID
            left join pls.partlocation pl with (nolock)  on pl.id = ps.LocationID
            left join pls.PartTransaction pt with (nolock)  on ( pt.ProgramID = woh.ProgramID and pt.partno = woh.PartNo and pt.SerialNo = woh.SerialNo and pt.PartTransactionID = 18) where woh.ProgramID = @ProgramId) x
) xx_scrap WHERE xx_scrap.StatusDesc = ''SCRAP'' AND convert(date,xx_scrap.LastActivityDate) >=  @fromDate AND convert(date,xx_scrap.LastActivityDate) <=@toDate;'

EXEC sp_executesql @sql, N'@fromDate DATE, @toDate DATE, @ProgramId INT ', @fromDate, @toDate, @ProgramId;


			";
            query = query.Replace("'<programId>'", programId);
            query = query.Replace("<fDate>", fDate);
            query = query.Replace("<tDate>", tDate);

            DataSet DS = oDAL.GetDataSet(query);
            DataTable dt1 = DS.Tables[0];
            DataTable dt2 = DS.Tables[1];
            dt1.Merge(dt2);

            // Create a list to hold columns to be removed
            List<DataColumn> columnsToRemove = new List<DataColumn>();

            // Loop through each column and add to the list if the name starts with "column"
            foreach (DataColumn col in dt1.Columns)
            {
                if (col.ColumnName.StartsWith("column", StringComparison.OrdinalIgnoreCase))
                {
                    columnsToRemove.Add(col);
                }
            }
            // Remove columns in the list
            foreach (DataColumn col in columnsToRemove)
            {
                dt1.Columns.Remove(col);
            }

            //second grid query//
            string sql = string.Empty;
            sql += @"
-- Define the start and end dates
DECLARE @fromDt DATE = '<fDate>';
DECLARE @toDt DATE = '<tDate>';
DECLARE @Program int = '<programId>';

DECLARE @Columns NVARCHAR(MAX);
DECLARE @query NVARCHAR(MAX);

-- Generate dynamic column list based on the date range using a recursive CTE
WITH DateRange AS (
SELECT @fromDt AS DateValue
UNION ALL
SELECT DATEADD(DAY, 1, DateValue)
FROM DateRange
WHERE DATEADD(DAY, 1, DateValue) <= @toDt
)
SELECT @Columns = STUFF(
(SELECT DISTINCT ',[' + CONVERT(VARCHAR(10), DateValue, 120) + ']'
FROM DateRange
FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''
);

-- Construct the dynamic SQL query
SET @query = '
WITH WIP_HOLD_CTE AS (
SELECT 
CAST(xx.LastActivityDate AS DATE) AS LastActivityDate,
SUM(CASE WHEN xx.Position = ''PACKING'' THEN 1 ELSE 0 END) AS ReadyToPack,
(SELECT c03 FROM pls.vCodeGenericTable where GenericTableName = ''FORECAST'' and ProgramID = @Program AND C02 = ''202411'' AND c01 = ''REMAN'') AS InboundPlan,
SUM(CASE WHEN xx.PROCESS_TYPE = ''REMAN'' THEN 1 ELSE 0 END) AS INBOUND_ACTUAL, 
(SELECT c04 FROM pls.vCodeGenericTable where GenericTableName = ''FORECAST'' and ProgramID = @Program AND C02 = ''202411'' AND c01 = ''REMAN'') AS OUTPUT_PLAN,
SUM(CASE WHEN xx.position = ''PACKING'' THEN 1 ELSE 0 END) AS OUTPUT_ACTUAL,
CASE 
WHEN (SELECT C03 
FROM pls.vCodeGenericTable 
WHERE GenericTableName = ''FORECAST''
AND ProgramID = @Program 
AND C02 = ''202411''
AND c01 = ''REMAN'') = 0 THEN NULL
ELSE 
(SUM(CASE WHEN xx.PROCESS_TYPE = ''RETURN'' OR xx.PROCESS_TYPE = ''EXCHANGE'' THEN 1 ELSE 0 END) * 100 / 
(SELECT c03
FROM pls.vCodeGenericTable 
WHERE GenericTableName = ''FORECAST'' 
AND ProgramID = @Program 
AND C02 = ''202411''
AND c01 = ''REMAN''))
END AS  ''ActualvsPlan'',
CASE 
WHEN (SELECT C04 
FROM pls.vCodeGenericTable 
WHERE GenericTableName = ''FORECAST'' 
AND ProgramID = @Program 
AND C02 = ''202411''
AND c01 = ''REMAN'') = 0 
THEN 0 
ELSE 
(SUM(CASE WHEN xx.position = ''PACKING'' THEN 1 ELSE 0 END) * 100 / 
(SELECT C04 
FROM pls.vCodeGenericTable 
WHERE GenericTableName = ''FORECAST''
AND ProgramID = @Program 
AND C02 = ''202411''
AND c01 = ''REMAN'')) 
END AS ''ActualvsPlanOutput'',
SUM(CASE WHEN xx.WorkStation = ''Scrap'' THEN 1 ELSE 0 END) AS SCRAP,
SUM(CASE WHEN xx.position = ''PACKING'' THEN 1 ELSE 0 END) AS PACKING
FROM (
-- Inner query generating data
select concat(case
when x.CodeName in ( ''ARIZONA'', /*''EDELMAN'',*/ ''LIPTON'', ''MINNOW'', ''PHELPS'', ''SKIPPER'',/* ''ZAKIM'',*/''PROFESSOR BB'' ) then
''AIO CELL''
when x.CodeName in ( ''EDDIE'', ''LANCOME'', ''LANCOME PLUS'', ''M3'', ''TAYLOR'' ) then
''BT CELL''
when x.CodeName in ( ''DURAN'', ''GOODYEAR'', ''LONE STARR'', ''PRINCE'' ) then
''HEADSET CELL''
when x.CodeName in ( ''SCOTTY'',/* ''SERENA'',*/ ''SMALLS'' ) then
''INEAR CELL''
when x.CodeName in ( ''ANGUS'', ''BABY YODA'', ''BENTO GILLIGAN'', ''CHIBI'', ''GILLIGAN PREMIUM'', ''GINGER CHEEVERS'',
''MALCOLM'', ''SAN DIEGO'', ''STEVIE'',''ZAKIM''
) then
''SYSTEM CELL''
when x.CodeName in ( ''EDELMAN'') then
''NPI - AIO CELL''
when x.CodeName in ( ''SERENA'') then
''NPI - INEAR CELL''
end,'' - '', x.codename) Line,
x.*,
'''' Position2
from
(
select woh.PartNo,
woh.SerialNo,
woh.createdate,
woh.CustomerReference,
woh.LastActivityDate,
(select cs.[Description]
from pls.CodeStatus cs with(nolock)
where cs.ID =  pt.PartTransactionID)ship_status,
isnull(wsd.code, cws.[Description]) WorkStation,
case
when upper(isnull(wsd.code, cws.[Description])) = ''A1030''
AND cs.[Description] = ''WIP'' then
''FAIL''
when upper(isnull(wsd.code, cws.[Description])) In ( ''A1200'', ''CLOSE'' ) then
''FGI''
when cs.[Description] = ''HOLD'' then
''HOLD''
when upper(isnull(wsd.code, cws.[Description])) = ''WFFA'' then
''FRESH''
when upper(isnull(wsd.code, cws.[Description])) = ''Scrap'' then
''SCRAP''
when upper(isnull(wsd.code, cws.[Description])) = ''A1090'' then
''PACKING''
else
''WIP''
END AS ''Position'',
(
SELECT upper(max(pna.[Value]))
FROM pls.PartNoAttribute pna With (nolock),
pls.CodeAttribute ca1 With (nolock)
WHERE ca1.id = pna.AttributeID
AND ca1.AttributeName = ''CODE_NAME''
AND pna.PartNo = woh.PartNo
) AS CodeName,
cs.[Description] as StatusDesc,
pl.LocationNo,
(
SELECT CASE
WHEN rha1.Value IN( ''RETURN'', ''EXCHANGE'' ) THEN

''REMAN''

ELSE

rha1.Value
END
FROM pls.ROHeaderAttribute rha1 With (nolock),
pls.CodeAttribute ca1 With (nolock)
WHERE ca1.id = rha1.AttributeID
AND ca1.AttributeName = ''PROCESS_TYPE''
AND rha1.ROHeaderID = ps.ROHeaderID
) PROCESS_TYPE,
(select top 1 ro.CustomerReference from pls.ROHeader ro where ro.id = ps.ROHeaderID)RMA,
isnull((select top 1
psa.Value
from pls.PartSerialAttribute psa With (nolock), pls.CodeAttribute ca With (nolock)
where ca.id = psa.AttributeID
and ca.AttributeName = ''RECEIVING_MANUAL_DISPOSITION_OVERRIDE''
and psa.PartSerialID = ps.id),''0'')MANUAL_DISPO,
(SELECT ps.PalletBoxNo
from pls.PartSerial ps with (nolock)
left join pls.PartLocation pl with (nolock) on pl.ID = ps.LocationID
where ps.ProgramID = @Program
and ps.PartNo = WOh.PartNo
and ps.SerialNo = WOh.SerialNo)BoxNo
from pls.WOHeader woh with (nolock)
inner join pls.PartSerial ps with (nolock)
on ps.WOHeaderID = woh.id
and ps.SerialNo = woh.SerialNo
and woh.PartNo = ps.PartNo
LEFT JOIN pls.CodeWorkStationCustomDescription WSD with (nolock)
ON WSD.ProgramID = woh.ProgramID
AND WSD.RepairTypeID = woh.RepairTypeID
AND WSD.CodeWorkStationID = woh.WorkStationID
LEFT JOIN pls.CodeStatus cs with (nolock)
on cs.id = woh.StatusID
LEFT JOIN pls.CodeWorkStation cws with (nolock)
on cws.id = ps.WorkStationID
left join pls.partlocation pl with (nolock)
on pl.id = ps.LocationID
left join pls.PartTransaction pt with (nolock)
on(pt.ProgramID = woh.ProgramID
and pt.partno = woh.PartNo
and pt.SerialNo = woh.SerialNo
and pt.PartTransactionID = 18)
where woh.ProgramID = @Program
--and woh.SerialNo = ''073962Z73390206AE''
) x
--where x.ship_status is null
UNION
select concat(case
when m1.CodeName in (''ARIZONA'', /*''EDELMAN'',*/ ''LIPTON'', ''MINNOW'', ''PHELPS'', ''SKIPPER'',/* ''ZAKIM'',*/''PROFESSOR BB'') then
''AIO CELL''
when m1.CodeName in (''EDDIE'', ''LANCOME'', ''LANCOME PLUS'', ''M3'', ''TAYLOR'' ) then
''BT CELL''
when m1.CodeName in (''DURAN'', ''GOODYEAR'', ''LONE STARR'', ''PRINCE'' ) then
''HEADSET CELL''
when m1.CodeName in (''SCOTTY'',/* ''SERENA'',*/ ''SMALLS'' ) then
''INEAR CELL''
when m1.CodeName in (''ANGUS'', ''BABY YODA'', ''BENTO GILLIGAN'', ''CHIBI'', ''GILLIGAN PREMIUM'', ''GINGER CHEEVERS'',
''MALCOLM'', ''SAN DIEGO'', ''STEVIE''
) then
''SYSTEM CELL''
when m1.CodeName in (''ZAKIM'', ''EDELMAN'') then
''NPI - AIO CELL''
when m1.CodeName in (''SERENA'') then
''NPI - INEAR CELL''
end,'' - '', m1.codename) Line,
m1.PartNo,
m1.SerialNo,
m1.CreateDate,
'''',
m1.LastActivityDate,
m1.ship_status,
m1.workstation,
case
when m1.MANUAL_DISPO in (''SCRAP'', ''NOSCAN'',''UNWANTED'') then
''SCRAP''
when m1.MANUAL_DISPO in (''NEW'' ) then
''FGI''
when m1.MANUAL_DISPO in (''REPACK'' ) then
''PACKING''
else
m1.MANUAL_DISPO
end position,
m1.codename,
case
when m1.MANUAL_DISPO in (''SCRAP'', ''NOSCAN'',''UNWANTED'' ) then
''SCRAP''
when m1.MANUAL_DISPO in (''NEW'' ) then
''FGI''
else
m1.MANUAL_DISPO
end statusdesc,
LocationNo,
PROCESS_TYPE,
RMA,
MANUAL_DISPO,
(SELECT ps.PalletBoxNo
from pls.PartSerial ps
left join pls.PartLocation pl on pl.ID = ps.LocationID
where ps.ProgramID = @Program
and ps.PartNo = m1.PartNo
and ps.SerialNo = m1.SerialNo)BoxNo,
''NO - WO Unit'' Position2
from
(
select
(
SELECT upper(max(pna.[Value]))
FROM pls.PartNoAttribute pna,
pls.CodeAttribute ca1
WHERE ca1.id = pna.AttributeID
AND ca1.AttributeName = ''CODE_NAME''
AND pna.PartNo = ps.PartNo
) AS codename,
ps.PartNo,
ps.SerialNo,
ps.createdate,
ps.LastActivityDate,
(select cs.[Description]
from pls.CodeStatus cs with (nolock)
where cs.ID = pt.PartTransactionID)ship_status,
null workstation,
(
select top 1
psa.Value
from pls.PartSerialAttribute psa,
pls.CodeAttribute ca
where ca.id = psa.AttributeID
and ca.AttributeName = ''RECEIVING_MANUAL_DISPOSITION_OVERRIDE''
and psa.PartSerialID = ps.id
)MANUAL_DISPO,
cs.[Description] as StatusDesc,
pl.LocationNo,
(
SELECT CASE
WHEN rha1.Value IN( ''RETURN'', ''EXCHANGE'' ) THEN

''REMAN''
ELSE

rha1.Value
END
FROM pls.ROHeaderAttribute rha1,
pls.CodeAttribute ca1
WHERE ca1.id = rha1.AttributeID
AND ca1.AttributeName = ''PROCESS_TYPE''
AND rha1.ROHeaderID = ps.ROHeaderID
) PROCESS_TYPE,
(select top 1 ro.CustomerReference from pls.ROHeader ro where ro.id = ps.ROHeaderID)RMA
from pls.PartSerial ps with (nolock)
left join pls.partlocation pl with (nolock)
on pl.id = ps.LocationID
LEFT JOIN pls.CodeStatus cs with (nolock)
on cs.id = ps.StatusID
left join pls.PartTransaction pt with (nolock)
on(pt.ProgramID = ps.ProgramID
and pt.partno = ps.PartNo
and pt.SerialNo = ps.SerialNo
and pt.PartTransactionID = 18)
where ps.ProgramID = @Program
and ps.WOHeaderID is null
-- and ps.SerialNo = ''073962Z73390206AE''
) m1
where isnull(m1.MANUAL_DISPO, ''0'') != ''0''
) AS xx
WHERE CONVERT(date, xx.LastActivityDate) >= @fromDt AND CONVERT(date, xx.LastActivityDate) <= @toDt
GROUP BY CAST(xx.LastActivityDate AS DATE)
)

SELECT
REMAN, ' + @Columns + '
FROM(
SELECT
''READY TO PACK'' AS REMAN,
LastActivityDate,
ReadyToPack AS Value
FROM WIP_HOLD_CTE
UNION ALL
SELECT
''INBOUND PLAN'' AS REMAN,
LastActivityDate,
InboundPlan AS Value
FROM WIP_HOLD_CTE
UNION ALL
SELECT
''INBOUND_ACTUAL'' AS REMAN,
LastActivityDate,
INBOUND_ACTUAL AS Value
FROM WIP_HOLD_CTE
UNION ALL
SELECT
''ACTUAL VS PLAN (INBOUND) %'' AS REMAN,
LastActivityDate,
ActualvsPlan AS Value
FROM WIP_HOLD_CTE
UNION ALL
SELECT
''OUTPUT_PLAN'' AS REMAN,
LastActivityDate,
OUTPUT_PLAN AS Value
FROM WIP_HOLD_CTE
UNION ALL
SELECT
''OUTPUT ACTUAL[packing]'' AS REMAN,
LastActivityDate,
OUTPUT_ACTUAL AS Value
FROM WIP_HOLD_CTE
UNION ALL
SELECT
''ACTUAL VS PLAN (OUTPUT) %'' AS REMAN,
LastActivityDate,
ActualvsPlanOutput AS Value
FROM WIP_HOLD_CTE
UNION ALL
SELECT
''SCRAP Actual'' AS REMAN,
LastActivityDate,
SCRAP AS Value
FROM WIP_HOLD_CTE
UNION ALL
SELECT
''PACKING'' AS REMAN,
LastActivityDate,
PACKING AS Value
FROM WIP_HOLD_CTE
) AS SourceTable
PIVOT(
SUM(Value)
FOR LastActivityDate IN(' + @Columns + ')
) AS PivotTable
ORDER BY
CASE
WHEN REMAN = ''READY TO PACK'' THEN 1
WHEN REMAN = ''INBOUND PLAN'' THEN 2
WHEN REMAN = ''INBOUND_ACTUAL'' THEN 3
WHEN REMAN = ''ACTUAL VS PLAN (INBOUND) %'' THEN 4
WHEN REMAN = ''OUTPUT_PLAN'' THEN 5
WHEN REMAN = ''OUTPUT ACTUAL[packing]'' THEN 6
WHEN REMAN = ''ACTUAL VS PLAN (OUTPUT) %'' THEN 7
WHEN REMAN = ''SCRAP Actual'' THEN 8
WHEN REMAN = ''PACKING'' THEN 9
ELSE 10
END, 
REMAN; ';

EXEC sp_executesql @query, N'@fromDt DATE, @toDt DATE, @Program INT', @fromDt, @toDt, @Program  
 ";

            sql = sql.Replace("'<programId>'", programId);
            sql = sql.Replace("<fDate>", fDate);
            sql = sql.Replace("<tDate>", tDate);

            DataTable dt3 = oDAL.GetDataForGeneric(sql);

            DataTable finalDt = dt1.Clone();

            foreach (DataRow row in dt1.Rows)
            {
                finalDt.ImportRow(row);
            }

            int gapRows = 4;

            // Add empty rows with an invisible character as a gap
            for (int i = 0; i < gapRows; i++)
            {
                DataRow emptyRow = finalDt.NewRow();

                // Add the invisible character only for these empty gap rows
                foreach (DataColumn column in finalDt.Columns)
                {
                    emptyRow[column] = DBNull.Value; // zero-width space only in gap rows
                }

                finalDt.Rows.Add(emptyRow);
            }
            // Add rows from dt3
            foreach (DataRow row in dt3.Rows)
            {
                finalDt.ImportRow(row);
            }

            DataTable dtList = finalDt.Clone();
            DataTable dtColHeader = cCommon.GenerateTransposedTable(dtList);

            filterString = " Program = BOSE ";
            filterString += " | From = '" + fDate + "' To = '" + tDate + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("239", query + " " + sql, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt1.Rows.Count > 0)
                    lstBoseREMAN = cCommon.ConvertDtToArrayList(finalDt);
                lstData = cCommon.ConvertDtToArrayList(dtColHeader);

                return true;
            }
        }
        public bool GetDetail(string programId, string RemanValue, string frmDt, string toDt)
        {


            string query = string.Empty;
            var conType = @HttpContext.Current.Session["CONN_TYPE"].ToString();

            query = @"	
DECLARE @fromDate DATE = '<frmDt>';  -- Start date
DECLARE @toDate DATE = '<toDt>';    -- End date
DECLARE @ProgramId INT = <programId>;         -- Program ID
 
DECLARE @sql NVARCHAR(MAX);
DECLARE @currentDate DATE = @fromDate;
 
SET @sql = '
SELECT 
    PartNo,'; -- Add PartNo to the query
 
-- Dynamically create columns for each date
WHILE @currentDate <= @toDate
BEGIN
    SET @sql = @sql + '
    SUM(CASE WHEN CAST(LastActivityDate AS DATE) = ''' + CONVERT(VARCHAR(10), @currentDate, 23) + ''' THEN 1 ELSE 0 END) AS [' + CONVERT(VARCHAR(10), @currentDate, 23) + '],';
 
    SET @currentDate = DATEADD(DAY, 1, @currentDate);
END;
 
-- Remove the trailing comma
SET @sql = LEFT(@sql, LEN(@sql) - 1);
 
-- Add the FROM clause
SET @sql = @sql + '
FROM (
    SELECT 
        woh.PartNo,
        woh.SerialNo,
        woh.LastActivityDate
    FROM pls.WOHeader woh with (nolock)
    INNER JOIN pls.PartSerial ps with (nolock) ON woh.ID = ps.WOHeaderID AND woh.SerialNo = ps.SerialNo
    LEFT JOIN pls.PartNoAttribute pna ON pna.PartNo = woh.PartNo
    WHERE woh.ProgramID = @ProgramId
      AND pna.Value = ''<PNvalue>''  
      AND CAST(woh.LastActivityDate AS DATE) BETWEEN @fromDate AND @toDate
) AS Data
GROUP BY PartNo
ORDER BY PartNo;
';
 
-- Execute the dynamic SQL
EXEC sp_executesql @sql, N'@fromDate DATE, @toDate DATE, @ProgramId INT', @fromDate, @toDate, @ProgramId;
 ";




            query = query.Replace("<programId>", programId);
            query = query.Replace("<PNvalue>", RemanValue);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);


            DataTable dt = oDAL.GetData(query);
            DataTable dtList = dt.Clone();
            DataTable dtColHeader = cCommon.GenerateTransposedTable(dtList);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("239-1", query, "Detail", false);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstRemanDetail = cCommon.ConvertDtToArrayList(dt);

                    lstDtlColHeader = cCommon.ConvertDtToArrayList(dtColHeader);
                }
                else
                {
                    //lstRemanDetail = new List<ArrayList>();
                    lstDtlColHeader = cCommon.ConvertDtToArrayList(dtColHeader);
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}