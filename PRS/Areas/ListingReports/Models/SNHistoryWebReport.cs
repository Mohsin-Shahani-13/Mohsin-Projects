using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.ListingReports.Models
{
    public class SNHistoryWebReport
    {
		cDAL oDAL = new cDAL("ACTIVE");
		#region Fields

		[Display(Name = "From:")]
		public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
		public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

		[Display(Name = "To:")]
		public string _toDt = DateTime.Now.ToString(Format.DateOnly);
		public string toDt { get { return _toDt; } set { _toDt = value; } }
		[Display(Name = "Serial No.:")]

		public string serialNo { get; set; }

		public string ReportTitle { get; set; }
		public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
		public string filterString { get; set; }
		public string ErrorMessage { get; set; }
		public List<Hashtable> lstSNHistoryWebReport { get; set; }
		#endregion
		#region Methods 

		public bool GetList(string serialNo, string programId, string ProgramName)
		{
			// oDAL = new cDAL("ACTIVE", "ST");
			string query = string.Empty;
			query = @"	   
DECLARE @SerialNo varchar(max) = '<serialNo>'
DECLARE @ProgramID int = <programId>
DECLARE @WOWIP INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WO-WIP')
DECLARE @WOREOPEN INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WO-REOPEN')
DECLARE @WOREPAIR INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WO-REPAIR')
DECLARE @WOSCRAP INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WO-SCRAP')
DECLARE @WOCANCEL INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WO-CANCEL')
DECLARE @WHREIDADDPART INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WH-REIDADDPART')
DECLARE @WHREIDREMOVEPART INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WH-REIDREMOVEPART')
DECLARE @RORECEIVE INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'RO-RECEIVE')
DECLARE @SOSHIP INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'SO-SHIP')
DECLARE @WHADDPART INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WH-ADDPART')
DECLARE @WHREMOVEPART INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WH-REMOVEPART')
DECLARE @WOREOPENCOMPONENTS INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WO-REOPENCOMPONENTS')
DECLARE @WOCONSUMECOMPONENTS INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WO-CONSUMECOMPONENTS')
DECLARE @ROUNRECEIVE INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'RO-UNRECEIVE')
DECLARE @WHDEKITADDPART INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WH-DEKITADDPART')
DECLARE @WHDEKITREMOVEPART INT = (SELECT cpt.ID from pls.CodePartTransaction cpt where cpt.Description = 'WH-DEKITREMOVEPART')
DECLARE @StorageLocationAttributeID int = (SELECT ca.ID FROM PLS.CodeAttribute ca where ca.AttributeName = 'StorageLocation')
DECLARE @CustomerLineNoAttributeID int = (select ID from pls.CodeAttribute where AttributeName = 'CustomerLineNo')
DECLARE @TransactionReport TABLE( 
TransactionDate DATETIME, 
GoProTransactionType VARCHAR(50), 
FromPartNo VARCHAR(100), 
FromLocationNo VARCHAR(100), 
FromGoProLocation VARCHAR(50), 
ToPartNo VARCHAR(100), 
ToLocationNo VARCHAR(100), 
ToGoProLocation VARCHAR(50), 
Quantity INT, 
Reference VARCHAR(100), 
SerialNumber VARCHAR(50), 
TransactionID VARCHAR(50),
Input varchar(20)
) 
CREATE TABLE #myPartTransaction
	(
	 ID INT NOT NULL
	,PartTransactionID INT NOT NULL
	,PartNo VARCHAR(100)
	,ParentSerialNo VARCHAR(50)
	,SerialNo VARCHAR(50)
	,Qty INT
	,Source VARCHAR(100)
	,Location VARCHAR(66)	
	,ToLocation VARCHAR(66)
	,PalletBoxNo VARCHAR(30)
	,ToPalletBoxNo VARCHAR(30)
	,Reason VARCHAR(100)
	,CustomerReference VARCHAR(100)
	,OrderType VARCHAR(10)
	,OrderHeaderID INT
	,OrderLineID INT
	,RODockLogID INT
	,UserID INT
	,CreateDate DATETIME	
	,SourcePartNo VARCHAR(100)
	,SourceSerialNo	VARCHAR(50)
	,PRIMARY KEY (ID)
	) 
CREATE TABLE #myWOStationHistory
	(
		ID INT NOT NULL, 
		WOHeaderID INT NOT NULL, 
		WorkStationID INT, 
		ToWorkStationID INT, 
		StatusID INT NOT NULL, 
		UserID INT NOT NULL, 
		CreateDate DATETIME NOT NULL, 
		LastActivityDate DATETIME NOT NULL, 
		RepairTypeID INT NOT NULL	
	)
CREATE TABLE #myWOHeader
(
	[ID] [int]  NOT NULL,
	[CustomerReference] [varchar](100) NOT NULL,	
	[PartNo] [varchar](100) NOT NULL,
	[SerialNo] [varchar](50) NOT NULL,
	[RepairTypeID] [smallint] NOT NULL,
	[WorkStationIDPrevious] [smallint] NULL,
	[WorkStationID] [smallint] NOT NULL,	
	[StatusID] [tinyint] NOT NULL,	
	[DefaultLocationID] [int] NOT NULL,
	[UserID] [smallint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastActivityDate] [datetime] NOT NULL
 
)
CREATE TABLE #myWOLine
(	
	[ID] [int]  NOT NULL,
	[WOHeaderID] [int] NOT NULL,
	[ComponentPartNo] [varchar](100) NOT NULL,
	[QtyRequested] [int] NOT NULL,
	[QtyConsumed] [int] NOT NULL,
	[StatusID] [tinyint] NOT NULL,
	[UserID] [smallint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastActivityDate] [datetime] NOT NULL 
)
INSERT INTO #myPartTransaction (ID,PartTransactionID,PartNo,ParentSerialNo,SerialNo,Qty,Source,Location,ToLocation,PalletBoxNo,ToPalletBoxNo,Reason,CustomerReference,OrderType,OrderHeaderID,OrderLineID,RODockLogID,UserID,CreateDate,SourcePartNo,SourceSerialNo)
SELECT ID,PartTransactionID,PartNo,ParentSerialNo,SerialNo,Qty,Source,Location,ToLocation,PalletBoxNo,ToPalletBoxNo,Reason,CustomerReference,OrderType,OrderHeaderID,OrderLineID,RODockLogID,UserID,CreateDate,SourcePartNo,SourceSerialNo
FROM pls.PartTransaction pt
WHERE pt.ProgramID = @ProgramID
and pt.SerialNo = @SerialNo
UNION
SELECT ID,PartTransactionID,PartNo,ParentSerialNo,SerialNo,Qty,Source,Location,ToLocation,PalletBoxNo,ToPalletBoxNo,Reason,CustomerReference,OrderType,OrderHeaderID,OrderLineID,RODockLogID,UserID,CreateDate,SourcePartNo,SourceSerialNo
FROM pls.PartTransaction pt
where pt.ProgramID = @ProgramID
and pt.SourceSerialNo = @SerialNo
and pt.SerialNo != @SerialNo
UNION
SELECT pt.ID,pt.PartTransactionID,pt.PartNo,pt.ParentSerialNo,pt.SerialNo,pt.Qty,pt.Source,pt.Location,pt.ToLocation,pt.PalletBoxNo,pt.ToPalletBoxNo,pt.Reason,pt.CustomerReference,pt.OrderType,pt.OrderHeaderID,pt.OrderLineID,pt.RODockLogID,pt.UserID,pt.CreateDate,pt.SourcePartNo,pt.SourceSerialNo
FROM pls.PartTransaction pt 
INNER JOIN pls.WOHeader woh on pt.OrderHeaderID = woh.ID
	and woh.SerialNo = @SerialNo
	and woh.ProgramID = @ProgramID
WHERE pt.ProgramID = @ProgramID
and pt.OrderType = 'WO'
INSERT INTO #myWOHeader (ID,CustomerReference,PartNo,SerialNo,RepairTypeID,WorkStationIDPrevious,WorkStationID,StatusID,DefaultLocationID,UserID,CreateDate,LastActivityDate)
select ID,CustomerReference,PartNo,SerialNo,RepairTypeID,WorkStationIDPrevious,WorkStationID,StatusID,DefaultLocationID,UserID,CreateDate,LastActivityDate
from pls.WOHeader 
where SerialNo = @SerialNo
and ProgramID = @ProgramID
INSERT INTO #myWOLine (ID, WOHeaderID, ComponentPartNo, QtyRequested, QtyConsumed, StatusID, UserID, CreateDate, LastActivityDate)
SELECT wol.ID, wol.WOHeaderID, wol.ComponentPartNo, wol.QtyRequested, wol.QtyConsumed, wol.StatusID, wol.UserID, wol.CreateDate, wol.LastActivityDate
from #myWOHeader woh 
inner join pls.WOLine wol on woh.ID = wol.WOHeaderID
INSERT INTO #myWOStationHistory(ID, WOHeaderID, WorkStationID, ToWorkStationID, StatusID, UserID, CreateDate, LastActivityDate, RepairTypeID)
SELECT wosh.ID, WOHeaderID, wosh.WorkStationID, wosh.ToWorkStationID, wosh.StatusID, wosh.UserID, wosh.CreateDate, wosh.LastActivityDate, wosh.RepairTypeID
FROM #myWOHeader woh
inner join pls.WOStationHistory wosh on woh.ID = wosh.WOHeaderID
--Transfer PartTransfer
INSERT INTO @TransactionReport 
SELECT pt.CreateDate, 
'Transfer', 
pt.PartNo, 
ISNULL(cwsExit.[Description], pt.[Location]) AS FromLocation, 
CASE 
WHEN ISNULL(cwsExit.[Description], pl.Warehouse) = 'RESERVE' THEN (SELECT soha.[value] FROM pls.vSOHeaderAttribute soha WHERE soha.SOHeaderID = pt.OrderHeaderID AND soha.AttributeName = 'StorageLocation') 
WHEN ISNULL(cwsExit.[Description], pl.Warehouse) IN ('gTask0', 'MRB') THEN 'MRB' 
WHEN ISNULL(cwsExit.[Description], pl.Warehouse) IN ('gTest0', 'Cosmetic', 'TDN') THEN 'TDN' 
WHEN ISNULL(cwsExit.[Description], pl.Warehouse) IN ('Scrap', 'SCR', 'SCRAP') THEN 'SCR' 
WHEN ISNULL(cwsExit.[Description], pl.Warehouse) IN ('Inspection', 'Refurbish', 'Audit', 'TST') OR ISNULL(cwsExit.[Description], pl.Warehouse) LIKE 'gTest%' THEN 'TST' 
WHEN ISNULL(cwsExit.[Description], pl.Warehouse) IN ('Kitting', 'Close', 'WIP', 'FLOORSTOCK', 'ISSUE', 'FGI') THEN 'WIP' 
WHEN ISNULL(cwsExit.[Description], pl.Warehouse) = 'RIN' THEN 'RIN' 
WHEN ISNULL(cwsExit.[Description], pl.Warehouse) = 'BFG' THEN 'BFG' 
ELSE '' 
END AS GoProFromLocation, 
pt.PartNo, 
ISNULL(cwsEntry.[Description], pt.ToLocation) AS ToLocation, 
CASE
WHEN ISNULL(cwsEntry.[Description], plTo.Warehouse) = 'RESERVE' THEN (SELECT soha.[value] FROM pls.vSOHeaderAttribute soha WHERE soha.SOHeaderID = pt.OrderHeaderID AND soha.AttributeName = 'StorageLocation') 
WHEN ISNULL(cwsEntry.[Description], plTo.Warehouse) IN ('gTask0', 'MRB') THEN 'MRB' 
WHEN ISNULL(cwsEntry.[Description], plTo.Warehouse) IN ('gTest0', 'Cosmetic', 'TDN') THEN 'TDN' 
WHEN ISNULL(cwsEntry.[Description], plTo.Warehouse) IN ('Scrap', 'SCR', 'SCRAP') THEN 'SCR' 
WHEN ISNULL(cwsEntry.[Description], plTo.Warehouse) IN ('Inspection', 'Refurbish', 'Audit', 'TST') OR ISNULL(cwsExit.[Description], pl.Warehouse) LIKE 'gTest%' THEN 'TST' 
WHEN ISNULL(cwsEntry.[Description], plTo.Warehouse) IN ('Kitting', 'Close', 'WIP', 'FLOORSTOCK', 'ISSUE', 'FGI') THEN 'WIP' 
WHEN ISNULL(cwsEntry.[Description], plTo.Warehouse) = 'RIN' THEN 'RIN' 
WHEN ISNULL(cwsEntry.[Description], plTo.Warehouse) = 'BFG' THEN 'BFG' 
ELSE '' 
END AS GoProToLocation, 
pt.Qty, 
'', 
pt.SerialNo, 
CONVERT(VARCHAR, pt.ID),
pt.SerialNo as Input
FROM #myPartTransaction pt (NOLOCK) 
INNER JOIN pls.PartNo pn ON pn.PartNo = pt.PartNo 
LEFT JOIN pls.PartLocation pl ON pl.ProgramID = @ProgramID
AND pl.LocationNo = pt.[Location] 
LEFT JOIN pls.PartLocation plTo ON plTo.ProgramID = @ProgramID
AND plTo.LocationNo = pt.ToLocation 
LEFT JOIN #myWOStationHistory woshEntry (NOLOCK) ON woshEntry.ID = ( 
SELECT MAX(wosh.ID) 
FROM #myWOStationHistory wosh (NOLOCK) 
WHERE wosh.WOHeaderID = pt.OrderHeaderID 
AND wosh.CreateDate <= pt.CreateDate) 
AND pt.PartTransactionID IN (@WOWIP, @WOREOPEN) 
LEFT JOIN pls.CodeWorkStation cwsEntry ON cwsEntry.ID = woshEntry.WorkStationID 
LEFT JOIN #myWOStationHistory woshExit (NOLOCK) ON woshExit.ID = ( 
SELECT MAX(wosh.ID) 
FROM #myWOStationHistory wosh (NOLOCK) 
WHERE wosh.WOHeaderID = pt.OrderHeaderID 
AND wosh.CreateDate <= pt.CreateDate) 
AND pt.PartTransactionID IN (@WOREPAIR, @WOSCRAP, @WOCANCEL) 
LEFT JOIN pls.CodeWorkStation cwsExit ON cwsExit.ID = woshExit.WorkStationID 
WHERE pt.[Location] != pt.ToLocation 
--Transfer WorkStations
INSERT INTO @TransactionReport 
SELECT wosh.LastActivityDate, 
'Transfer', 
pt.PartNo, 
cws.[Description], 
CASE 
WHEN cws.[Description] = 'gTask0' THEN 'MRB' 
WHEN cws.[Description] IN ('gTest0', 'Cosmetic') THEN 'TDN' 
WHEN cws.[Description] = 'Scrap' THEN 'SCR' 
WHEN cws.[Description] IN ('Inspection', 'Refurbish', 'Audit') OR cws.[Description] LIKE 'gTest%' THEN 'TST' 
WHEN cws.[Description] IN ('Kitting', 'Close') THEN 'WIP' 
ELSE '' END AS GoProFromLocation, 
pt.PartNo, 
tcws.[Description], 
CASE 
WHEN tcws.[Description] = 'gTask0' THEN 'MRB' 
WHEN tcws.[Description] IN ('gTest0', 'Cosmetic') THEN 'TDN' 
WHEN tcws.[Description] = 'Scrap' THEN 'SCR' 
WHEN tcws.[Description] IN ('Inspection', 'Refurbish', 'Audit') OR tcws.[Description] LIKE 'gTest%' THEN 'TST' 
WHEN tcws.[Description] IN ('Kitting', 'Close') THEN 'WIP' 
ELSE '' END AS GoProToLocation, 
1, 
'', 
pt.SerialNo, 
CONVERT(VARCHAR, pt.ID) + '-' + CONVERT(VARCHAR, wosh.ID),
pt.SerialNo as Input
FROM #myWOStationHistory wosh (NOLOCK) 
INNER JOIN #myWOHeader woh (NOLOCK) ON woh.ID = wosh.WOHeaderID 
INNER JOIN #myWOLine wol (NOLOCK) ON woh.ID = wol.WOHeaderID 
AND woh.PartNo = wol.ComponentPartNo 
INNER JOIN pls.CodeWorkStation cws ON cws.ID = wosh.WorkStationID 
INNER JOIN pls.CodeWorkStation tcws ON tcws.ID = wosh.ToWorkStationID 
INNER JOIN #myPartTransaction pt (NOLOCK) ON pt.ID = ( 
SELECT MAX(ppt.ID) 
FROM #myPartTransaction ppt (NOLOCK) 
WHERE ppt.OrderType = 'WO' 
AND ppt.OrderHeaderID = woh.ID 
AND ppt.OrderLineID = wol.ID 
AND ppt.CreateDate <= wosh.LastActivityDate ) 
--Part Flip
INSERT INTO @TransactionReport 
SELECT pt.CreateDate, 
'Part Flip' AS GoProTransaction, 
pt.SourcePartNo AS FromPartNo, 
ISNULL(cws.[Description], pt.ToLocation) AS PlusFromLocation, 
IIF(cws.ID IS NOT NULL, 
CASE 
WHEN cws.[Description] = 'gTask0' THEN 'MRB'
WHEN cws.[Description] IN ('gTest0', 'Cosmetic') THEN 'TDN' 
WHEN cws.[Description] = 'Scrap' THEN 'SCR' 
WHEN cws.[Description] IN ('Inspection', 'Refurbish', 'Audit') OR cws.[Description] LIKE 'gTest%' THEN 'TST' 
WHEN cws.[Description] IN ('Kitting', 'Close') THEN 'WIP' 
ELSE '' END,  
CASE 
WHEN pl.Warehouse IN ('TDN', 'WIP', 'SCR', 'RIN', 'MRB', 'BFG', 'TST') THEN pl.Warehouse 
WHEN pl.Warehouse = 'SCRAP' THEN 'SCR' 
WHEN pl.Warehouse IN ('FLOORSTOCK', 'ISSUE', 'FGI') THEN 'WIP' 
ELSE '' END ) AS FromLocation, 
pt.PartNo AS ToPartNo, 
ISNULL(cws.[Description], pt.ToLocation) AS PlusToLocation, 
IIF(cws.ID IS NOT NULL, 
CASE 
WHEN cws.[Description] = 'gTask0' THEN 'MRB'
WHEN cws.[Description] IN ('gTest0', 'Cosmetic') THEN 'TDN' 
WHEN cws.[Description] = 'Scrap' THEN 'SCR' 
WHEN cws.[Description] IN ('Inspection', 'Refurbish', 'Audit') OR cws.[Description] LIKE 'gTest%' THEN 'TST' 
WHEN cws.[Description] IN ('Kitting', 'Close') THEN 'WIP' 
ELSE '' END,  
CASE 
WHEN pl.Warehouse IN ('TDN', 'WIP', 'SCR', 'RIN', 'MRB', 'BFG', 'TST') THEN pl.Warehouse 
WHEN pl.Warehouse = 'SCRAP' THEN 'SCR' 
WHEN pl.Warehouse IN ('FLOORSTOCK', 'ISSUE', 'FGI') THEN 'WIP' 
ELSE '' END ) AS ToLocation, 
pt.Qty, 
 '', 
pt.SerialNo, 
CONVERT(VARCHAR, pt.ID),
pt.SerialNo as Input
FROM #myPartTransaction pt (NOLOCK) 
INNER JOIN pls.PartLocation pl ON pl.ProgramID = @ProgramID
AND pt.PartTransactionID = @WHREIDADDPART
AND pl.LocationNo = pt.ToLocation 
LEFT JOIN #myWOStationHistory wosh (NOLOCK) ON wosh.ID = ( 
SELECT MAX(wosh2.ID) 
FROM #myWOStationHistory wosh2 (NOLOCK) 
WHERE wosh2.WOHeaderID = pt.OrderHeaderID 
AND wosh2.CreateDate < pt.CreateDate) 
AND pt.OrderType = 'WO' 
LEFT JOIN pls.CodeWorkStation cws ON cws.ID = wosh.WorkStationID 
WHERE pt.[Source] NOT LIKE 'Kitting - %' 
-- RO-RECEIVE, SO-SHIP & Adjustments 
INSERT INTO @TransactionReport 
SELECT pt.CreateDate, 
CASE cpt.[Description] 
WHEN 'RO-RECEIVE' THEN 'Item Receipt' 
WHEN 'SO-SHIP' THEN IIF(soha.[Value] = 'SCR', 'Scrap', 'Item Fulfillment') 
ELSE 'Adjustment' END, 
IIF(cpt.Direction = '+', '', pt.PartNo) AS PartNo, 
IIF(cpt.Direction = '+', '', pl.LocationNo) AS FromPlusLocation, 
IIF(cpt.Direction = '+', '', 
IIF(cpt.[Description] = 'SO-SHIP', 
soha.[Value], 
CASE WHEN pl.Warehouse IN ('FGI', 'FLOORSTOCK', 'ISSUE', 'FGI') THEN 'WIP' 
WHEN pl.Warehouse = 'SCRAP' THEN 'SCR' 
WHEN pl.Warehouse = 'DGI' THEN 'TDN' 
ELSE pl.Warehouse END)) AS FromLocation, 
IIF(cpt.Direction = '+', pt.PartNo, '') AS PartNo, 
IIF(cpt.Direction = '+', pl.LocationNo, '') AS ToPlusLocation, 
IIF(cpt.Direction = '+', 
CASE WHEN pl.Warehouse IN ('FGI', 'FLOORSTOCK', 'ISSUE', 'FGI') THEN 'WIP' 
WHEN pl.Warehouse = 'SCRAP' THEN 'SCR' 
WHEN pl.Warehouse = 'DGI' THEN 'TDN' 
ELSE pl.Warehouse END, '') AS ToLocation, 
pt.Qty, 
IIF(cpt.[Description] IN ('RO-RECEIVE', 'SO-SHIP'), pt.CustomerReference, cpt.[Description]), 
pt.SerialNo, 
CONVERT(VARCHAR, pt.ID),
pt.SerialNo as Input
FROM #myPartTransaction pt (NOLOCK) 
INNER JOIN pls.CodePartTransaction cpt ON cpt.ID = pt.PartTransactionID 
AND cpt.[Description] IN ('RO-RECEIVE', 'SO-SHIP', 'WH-ADDPART', 'WO-REOPENCOMPONENTS', 'WH-REMOVEPART', 'RO-UNRECEIVE') 
INNER JOIN pls.PartNo pn ON pn.PartNo = pt.PartNo 
LEFT JOIN pls.vSOHeaderAttribute soha ON cpt.[Description] = 'SO-SHIP' 
AND soha.SOHeaderID = pt.OrderHeaderID 
AND soha.AttributeName = 'StorageLocation' 
LEFT JOIN pls.PartLocation pl ON pl.ProgramID = @ProgramID
AND pl.LocationNo = IIF(cpt.Direction = '+', pt.ToLocation, pt.[Location]) 
LEFT JOIN pls.ROLineAttribute rola on pt.OrderLineID = rola.ROLineID
	and rola.AttributeID = @CustomerLineNoAttributeID
	and pt.PartTransactionID = 1
--Assembly Unbuild
INSERT INTO @TransactionReport 
SELECT pt.CreateDate, 
'Assembly Unbuild', 
IIF(pt.ParttransactionID = @WHDEKITADDPART, '', pn.PartNo) AS PartNo, 
IIF(pt.ParttransactionID = @WHDEKITADDPART, '', pt.[Location]) AS FromPlusLocation, 
IIF(pt.ParttransactionID = @WHDEKITADDPART, '', CASE WHEN fpl.Warehouse IN ('TDN', 'WIP', 'SCR', 'RIN', 'MRB', 'BFG', 'TST') THEN fpl.Warehouse WHEN fpl.Warehouse = 'SCRAP' THEN 'SCR' WHEN fpl.Warehouse IN ('FLOORSTOCK', 'ISSUE', 'FGI') THEN 'WIP' ELSE '' END) AS FromLocation, 
IIF(pt.ParttransactionID = @WHDEKITADDPART, pn.PartNo, '') AS PartNo, 
IIF(pt.ParttransactionID = @WHDEKITADDPART, pt.ToLocation, '') AS ToPlusLocation, 
IIF(pt.ParttransactionID = @WHDEKITADDPART, CASE WHEN tpl.Warehouse IN ('TDN', 'WIP', 'SCR', 'RIN', 'MRB', 'BFG', 'TST') THEN tpl.Warehouse WHEN tpl.Warehouse = 'SCRAP' THEN 'SCR' WHEN tpl.Warehouse IN ('FLOORSTOCK', 'ISSUE', 'FGI') THEN 'WIP' ELSE '' END, '') AS ToLocation, 
pt.Qty, 
'', 
pt.SerialNo, 
MAX(pt.ID) OVER(PARTITION BY pt.[Source], pt.SourcePartNo, pt.SourceSerialNo),
iif(pt.PartTransactionID = @WHDEKITADDPART,pt.SourceSerialNo,pt.SerialNo)  as Input
FROM #myPartTransaction pt (NOLOCK) 
INNER JOIN pls.PartNo pn ON pn.PartNo = pt.PartNo 
	AND pt.PartTransactionID in (@WHDEKITADDPART,@WHDEKITREMOVEPART) 
LEFT JOIN pls.PartLocation fpl ON fpl.ProgramID = @ProgramID
AND fpl.LocationNo = pt.[Location] 
LEFT JOIN pls.PartLocation tpl ON tpl.ProgramID = @ProgramID 
AND tpl.LocationNo = pt.ToLocation 
-- Assembly Build 
INSERT INTO @TransactionReport 
SELECT pt.CreateDate, 
'Assembly Build', 
IIF(pt.PartTransactionID = @WHREIDADDPART, '', pt.PartNo), 
IIF(pt.PartTransactionID = @WHREIDADDPART, '', pl.LocationNo) AS FromPlusLocation, 
IIF(pt.PartTransactionID = @WHREIDADDPART, '', CASE WHEN pl.Warehouse IN ('TDN', 'WIP', 'SCR', 'RIN', 'MRB', 'BFG', 'TST') THEN pl.Warehouse WHEN pl.Warehouse = 'SCRAP' THEN 'SCR' WHEN pl.Warehouse IN ('FLOORSTOCK', 'ISSUE', 'FGI') THEN 'WIP' ELSE '' END) AS FromLocation, 
IIF(pt.PartTransactionID = @WHREIDADDPART, pt.PartNo, ''), 
IIF(pt.PartTransactionID = @WHREIDADDPART, tpl.LocationNo, ''), 
IIF(pt.PartTransactionID = @WHREIDADDPART, CASE WHEN tpl.Warehouse IN ('TDN', 'WIP', 'SCR', 'RIN', 'MRB', 'BFG', 'TST') THEN tpl.Warehouse WHEN tpl.Warehouse = 'SCRAP' THEN 'SCR' WHEN tpl.Warehouse IN ('FLOORSTOCK', 'ISSUE', 'FGI') THEN 'WIP' ELSE '' END, ''), 
pt.Qty, 
'', 
IIF(pt.PartTransactionID = @WOCONSUMECOMPONENTS, pt.ParentSerialNo, pt.SerialNo), CONVERT(VARCHAR, wopt.ID) 
,pt.SerialNo as Input
FROM #myPartTransaction pt (NOLOCK) 
LEFT JOIN pls.PartLocation pl ON pl.ProgramID = @ProgramID
AND pl.LocationNo = pt.[Location] 
LEFT JOIN pls.PartLocation tpl ON tpl.ProgramID = @ProgramID
AND tpl.LocationNo = pt.ToLocation 
LEFT JOIN #myPartTransaction wopt (NOLOCK) ON wopt.PartTransactionID IN (@WOREPAIR, @WOSCRAP) -- WO-REPAIR, WO-SCRAP 
AND wopt.[Source] = pt.[Source] 
WHERE pt.[Source] LIKE 'Kitting - %' 
and pt.PartTransactionID in (@WHREIDADDPART,@WHREIDREMOVEPART,@WOCONSUMECOMPONENTS)
SELECT 
@ProgramID AS ProgramId,
tr.TransactionDate as [DATE], 
tr.GoProTransactionType as [GOPROMOVETYPE], 
fpn.PartNo as [PARTNO], 
fpn.PartTypeDesc as [PARTTYPEDESC], 
REPLACE(fpn.[Description], ',', '') AS [PARTDESCRIPTION],
tr.FromLocationNo AS [PLUS_LOCATION], 
tr.FromGoProLocation AS [GOPRO_LOCATION], 
tr.Reference as [REFERENCE], 
tr.Quantity * -1 AS [QTY], 
tr.SerialNumber as [SERIALNUMBER], 
tr.TransactionID as [TRANSACTIONID],
tr.Input
FROM @TransactionReport tr 
LEFT JOIN pls.vPartNo fpn ON fpn.PartNo = tr.FromPartNo 
WHERE ISNULL(tr.FromLocationNo, '') != '' 
UNION  ALL
SELECT 
@ProgramID AS ProgramId,
tr.TransactionDate, 
tr.GoProTransactionType, 
tpn.PartNo, 
tpn.PartTypeDesc, 
REPLACE(tpn.[Description], ',', '') AS PartDescription, 
tr.ToLocationNo AS PlusLocation, 
tr.ToGoProLocation AS GoProLocation, 
tr.Reference, 
tr.Quantity, 
tr.SerialNumber, 
tr.TransactionID,
tr.Input
FROM @TransactionReport tr 
LEFT JOIN pls.vPartNo tpn ON tpn.PartNo = tr.ToPartNo 
WHERE ISNULL(tr.ToLocationNo, '') != '' 
order by tr.TransactionDate,tr.TransactionID
drop table #myPartTransaction
drop table #myWOHeader
drop table #myWOStationHistory
drop table #myWOLine
";

			query = query.Replace("<serialNo>", serialNo);
			//query = query.Replace("@toDt", toDt);
			query = query.Replace("<programId>", programId);

			DataTable dt = oDAL.GetData(query);

			if (!string.IsNullOrEmpty(ProgramName))
				filterString += "> Program = '" + ProgramName + "' ";

			//filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
			filterString += "| Serial No. = '" + serialNo + "' ";


			//For SQL Documentation
			cLog oLog = new cLog();
			oLog.AddSqlQuery("226", query, string.Empty, false);

			if (oDAL.HasErrors)
			{
				ErrorMessage = oDAL.ErrMessage;
				return false;
			}
			else
			{
				if (dt.Rows.Count > 0)
					lstSNHistoryWebReport = cCommon.ConvertDtToHashTable(dt);
				return true;

			}
		}
		#endregion
	}
}