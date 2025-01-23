using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Data.SqlClient;

namespace IP.Areas.SupplyChain.Models
{
    public class DellBilling
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Tracking No.:")]
        public string TrackingNo { get; set; }

        public string PO_NUMBER { get; set; }

        public string TERMS { get; set; }

        public string SHIP { get; set; }

        public string TRACKING { get; set; }

        public string CompanyDesc { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyContact { get; set; }
        public string BillToDesc { get; set; }
        public string BillToAddress { get; set; }
        public string BillToContact { get; set; }
        public string ShipToDesc { get; set; }

        public string ShipToAddress { get; set; }
        public string ShipToContact { get; set; }

        public string ReportTitle { get; set; }

        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }

        public List<Hashtable> lstDellBillingReport { get; set; }
        public List<Hashtable> lstInvoice { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public int Qty { get; set; }
        public string Commodity { get; set; }
        public string WarrantyCode { get; set; }
        public string ShipType { get; set; }
        public decimal Cost { get; set; }
        [Display(Name = "Invoice No.:")]
        public string InvNumber { get; set; }

        public string PartNumber { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public decimal extraLineCost { get; set; }
        public decimal extraLineAmount { get; set; }
        public string InvNo { get; set; }
        public string InvDate { get; set; }
        public string TrackingNum { get; set; }

        public List<DellBilling> ExtraLinesData { get; set; }

        #endregion
        #region Methods 
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>' AND NAME = 'DELL'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public DataTable GetStatus()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @" select distinct 
	                  status 
                FROM tpdc01s209.rdw.rpt.RedmineItems

                ORDER BY status";
            //query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetList(string programId, string programName, string InvNumber)
        {
            oDAL = new cDAL("INIT");
            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            //string programName = HttpContext.Current.Session["Program"].ToString();

            string query = string.Empty;

            query = @"
SELECT   distinct
      COALESCE(DB.[InvoiceNo], EL.[InvoiceNo]) AS InvoiceNo,
      DB.[Brand],
      DB.[ChargeType],
      DB.[Reference],
      COALESCE(CAST(DB.[BillDate] AS VARCHAR), CAST(EL.InvoiceDate as varchar))	AS BillDate,
	  DB.Qty,
	  EL.Qty AS ExtraQty,
      COALESCE(EL.[InvoiceDate],db.[InvoiceDate]) AS InvoiceDate
	  
FROM [rpt].[DellBilling] DB
FULL OUTER JOIN [rpt].[DellBillingExtraLine] EL ON DB.[InvoiceNo] = EL.[InvoiceNo]
GROUP BY 
      DB.[InvoiceNo], 
      DB.[Brand],
      DB.[ChargeType],
      DB.[Reference],
      DB.[BillDate],
      DB.[InvoiceDate],
	  El.PartNo,
	  El.Description,
      EL.[InvoiceDate],
	  el.[InvoiceNo]  ,
	  DB.Qty,
	  EL.Qty 
";

            //query = query.Replace("<programId>", programId);

            DataTable dt = oDAL.GetData(query);

            //if (!string.IsNullOrEmpty(programName))
            filterString += "> Program = '" + programName + "' ";

            //if (!string.IsNullOrEmpty(trackingNo))
            //	filterString += "> Tracking No. = '" + _trackingNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("193", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDellBillingReport = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        public bool GetDetail(string programId, string programName, string trackingNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            //string programName = HttpContext.Current.Session["Program"].ToString();
            string _trackingNo = GetInValue(trackingNo);

            string query = string.Empty;

            query = @"
<trackingVar>

declare @ProgramID int = <programId>
declare @GradeAttributeID int = (select ca.ID from pls.CodeAttribute ca where ca.AttributeName = 'GRADE')
declare @StorageLocationAttributeID int = (select ca.ID from pls.CodeAttribute ca where ca.AttributeName = 'StorageLocation')
declare @DellRateCardID int = (select cgtd.ID from pls.CodeGenericTableDefinition cgtd where cgtd.ProgramID = @ProgramID and cgtd.Name= 'DELLRATECARD')
declare @ShippedStatusID int = (select cs.ID from pls.CodeStatus cs where cs.Description = 'SHIPPED')
declare @ActiveStatusID int = (select cs.ID from pls.CodeStatus cs where cs.Description = 'ACTIVE')
select
	P.ID AS ProgramID,
	P.Name AS Program,
	soh.CustomerReference as [Ship Order],
	sosi.TrackingNo as [Tracking Number],
	sosi.ShipmentDate as [ShipmentDate],
	sosi.Carrier as [Carrier],
	cc.Description as [Commodity],	
	DellRateCardcgt.C01 as [Dell Charge Code],
	isnull(sohaStorageLocation.Value,'') as [SHIP TYPE],
	'DELL' AS [Brand],	
	ps.PartNo as [Model],
	roh.CustomerReference as [Receive Order],
	ps.ID as [PartSerialID], 
	ps.SerialNo as [Serial Number],
	'FLAT RATE' as [Charge Type],
	'BILLING SHIPMENT / FLAT RATE' as [Reference],
	'BILLING SHIPMENT - Tracking '+sosi.TrackingNo+' / '+pn.PartNo+' / '+isnull(psaGrade.Value,'')+' / FLAT RATE' as [Bill Description],
	DellRateCardcgt.C05 as [Price],
	'' as [BillDate],
	isnull(psaGrade.Value,'') AS [Wty Code],
	'' as [Inv Number],
	  format(ps.WOEndDate,'MM/dd/yyyy') as [Revenue Date],
    '' AS Invoice
into #Billing
from
	pls.PartSerial ps
	inner join pls.PartSerialAttribute psaGrade on ps.ID = psaGrade.PartSerialID
		and psaGrade.AttributeID = @GradeAttributeID		
		and ps.ProgramID = @ProgramID
		and ps.StatusID = @ShippedStatusID
	inner join pls.SOHeader soh on ps.SOHeaderID = soh.ID

       
	inner join pls.PartNo pn on ps.PartNo = pn.PartNo
	inner join pls.CodeCommodity cc on pn.PrimaryCommodityID = cc.ID
	inner join pls.ROHeader roh on ps.ROHeaderID = roh.ID				
		and roh.ProgramID = @ProgramID
	inner join pls.SOShipmentInfo sosi on ps.SOHeaderID = sosi.SOHeaderID		
	inner join pls.codeGenericTable DellRateCardcgt on DellRateCardcgt.GenericTableDefinitionID = @DellRateCardID
		and DellRateCardcgt.StatusID = @ActiveStatusID
		and DellRateCardcgt.C09 = isnull(psaGrade.Value,'')
		 and (cc.Description = DellRateCardcgt.C08 or (DellRateCardcgt.C08 = 'LCD' AND (cc.Description = 'LCD ASSY MODULE' or cc.Description = 'LCD_SUBASSY' )  ))
	inner join pls.SOHeaderAttribute sohaStorageLocation on sohaStorageLocation.SOHeaderID = soh.ID
		and sohaStorageLocation.AttributeID = @StorageLocationAttributeID
	<trackingJoin>
	INNER JOIN pls.Program P ON P.ID = ps.ProgramID

SELECT * FROM #Billing ";

            if (!string.IsNullOrEmpty(trackingNo))
            {
                query = query.Replace("<trackingVar>", "declare @Trackings varchar(max) =  '<TrackingNo>'");
                query = query.Replace("<trackingJoin>", "inner join string_split(@Trackings,',') as so on sosi.TrackingNo = so.value");
            }
            else
            {
                query = query.Replace("<trackingVar>", "");
                query = query.Replace("<trackingJoin>", "");
            }
            query = query.Replace("<programId>", programId);
            query = query.Replace("<TrackingNo>", _trackingNo);
            //query = query.Replace("<toDt>", tDate);

            DataTable dt = oDAL.GetData(query);


            filterString += "> Program = '" + programName + "'";

            if (!string.IsNullOrEmpty(trackingNo))
                filterString += " | Tracking No. = '" + _trackingNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("193", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDellBillingReport = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        public DataTable BillingInfo()
        {
            oDAL = new cDAL("INIT");

            string query = string.Empty;

            query = @"
SELECT  CompanyDesc
      ,CompanyAddress
      ,CompanyContact
      ,BillToDesc
      ,BillToAddress
      ,BillToContact
      ,ShipToDesc
      ,ShipToAddress
      ,ShipToContact
  FROM PlusRS.rpt.DellBillingInfo ";

            DataTable dt = oDAL.GetData(query);



            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return dt;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    CompanyDesc = dt.Rows[0]["CompanyDesc"].ToString();
                CompanyAddress = dt.Rows.Count > 0 ? dt.Rows[0]["CompanyAddress"].ToString() : string.Empty;
                CompanyContact = dt.Rows.Count > 0 ? dt.Rows[0]["CompanyContact"].ToString() : string.Empty;
                BillToDesc = dt.Rows.Count > 0 ? dt.Rows[0]["BillToDesc"].ToString() : string.Empty;
                BillToAddress = dt.Rows.Count > 0 ? dt.Rows[0]["BillToAddress"].ToString() : string.Empty;
                BillToContact = dt.Rows.Count > 0 ? dt.Rows[0]["BillToContact"].ToString() : string.Empty;
                ShipToDesc = dt.Rows.Count > 0 ? dt.Rows[0]["ShipToDesc"].ToString() : string.Empty;
                ShipToAddress = dt.Rows.Count > 0 ? dt.Rows[0]["ShipToAddress"].ToString() : string.Empty;
                ShipToContact = dt.Rows.Count > 0 ? dt.Rows[0]["ShipToContact"].ToString() : string.Empty;

                return dt;
            }
        }
        public DataTable Invoice(string programId, string trackingNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            //string programName = HttpContext.Current.Session["Program"].ToString();
            string _trackingNo = GetInValue(trackingNo);
            string query = string.Empty;


            query = @"
<trackingVar>
declare @ProgramID int = <programId>
declare @GradeAttributeID int = (select ca.ID from pls.CodeAttribute ca where ca.AttributeName = 'GRADE')
declare @StorageLocationAttributeID int = (select ca.ID from pls.CodeAttribute ca where ca.AttributeName = 'StorageLocation')
declare @DellRateCardID int = (select cgtd.ID from pls.CodeGenericTableDefinition cgtd where cgtd.ProgramID = @ProgramID and cgtd.Name= 'DELLRATECARD')
declare @ShippedStatusID int = (select cs.ID from pls.CodeStatus cs where cs.Description = 'SHIPPED')
declare @ActiveStatusID int = (select cs.ID from pls.CodeStatus cs where cs.Description = 'ACTIVE')
select		
	P.ID AS ProgramID,
	P.Name AS Program,
	soh.CustomerReference as [Ship Order],
	sosi.TrackingNo as [Tracking Number],
	sosi.ShipmentDate as [ShipmentDate],
	sosi.Carrier as [Carrier],
	cc.Description as [Commodity],	
	DellRateCardcgt.C01 as [Dell Charge Code],
	isnull(sohaStorageLocation.Value,'') as [SHIP TYPE],
	'DELL' AS [Brand],	
	ps.PartNo as [Model],
	roh.CustomerReference as [Receive Order],
	ps.ID as [PartSerialID], 
	ps.SerialNo as [Serial Number],
	'FLAT RATE' as [Charge Type],
	'BILLING SHIPMENT / FLAT RATE' as [Reference],
	'BILLING SHIPMENT - Tracking '+sosi.TrackingNo+' / '+pn.PartNo+' / '+isnull(psaGrade.Value,'')+' / FLAT RATE' as [Bill Description],
	DellRateCardcgt.C05 as [Price],
	'' as [BillDate],
	isnull(psaGrade.Value,'') AS [Wty. Code],
	'' as [Inv. Number],
	format(ps.WOEndDate,'MM.dd.yyyy') as [Revenue Date]	
into #Billing
from
	pls.PartSerial ps
	inner join pls.PartSerialAttribute psaGrade on ps.ID = psaGrade.PartSerialID
		and psaGrade.AttributeID = @GradeAttributeID		
		and ps.ProgramID = @ProgramID
		and ps.StatusID = @ShippedStatusID
	inner join pls.SOHeader soh on ps.SOHeaderID = soh.ID
	inner join pls.PartNo pn on ps.PartNo = pn.PartNo
	inner join pls.CodeCommodity cc on pn.PrimaryCommodityID = cc.ID
	inner join pls.ROHeader roh on ps.ROHeaderID = roh.ID				
		and roh.ProgramID = @ProgramID
	inner join pls.SOShipmentInfo sosi on ps.SOHeaderID = sosi.SOHeaderID		
	inner join pls.codeGenericTable DellRateCardcgt on DellRateCardcgt.GenericTableDefinitionID = @DellRateCardID
		and DellRateCardcgt.StatusID = @ActiveStatusID
		and DellRateCardcgt.C09 = isnull(psaGrade.Value,'')
		and (cc.Description = DellRateCardcgt.C08 or (DellRateCardcgt.C08 = 'LCD' AND (cc.Description = 'LCD ASSY MODULE' or cc.Description = 'LCD_SUBASSY' )  ))
	inner join pls.SOHeaderAttribute sohaStorageLocation on sohaStorageLocation.SOHeaderID = soh.ID
		and sohaStorageLocation.AttributeID = @StorageLocationAttributeID
	<trackingJoin>
	INNER JOIN pls.Program P ON P.ID = ps.ProgramID

select	
	'NA' as [PO NUMBER],
	'NET 90' as [TERMS],
	format(b.ShipmentDate,'MM/dd/yyyy') as [SHIP],
	b.[Tracking Number] as [TRACKING],
	count(b.[PartSerialID]) as [QTY],
	b.Commodity as [COMMODITY],
	b.[Wty. Code] as [WARRANTY_CODE],
	b.[SHIP TYPE] as [SHIP_TYPE],
	max((cast(b.Price as money))) AS [COST],
	max((cast(b.Price as money))) * count(b.[PartSerialID]) as [AMOUNT]
from 
	#Billing b
group by
	b.[Dell Charge Code],
	format(b.ShipmentDate,'MM/dd/yyyy'),
	b.[Tracking Number],	
	b.Commodity,
	b.[Wty. Code],
	b.[SHIP TYPE]		
		
drop table #Billing";


            if (!string.IsNullOrEmpty(trackingNo))
            {
                query = query.Replace("<trackingVar>", "declare @Trackings varchar(max) =  '<TrackingNo>'");
                query = query.Replace("<trackingJoin>", "inner join string_split(@Trackings,',') as so on sosi.TrackingNo = so.value");

            }
            else
            {
                query = query.Replace("<trackingVar>", "");
                query = query.Replace("<trackingJoin>", "");
            }
            query = query.Replace("<programId>", programId);
            query = query.Replace("<TrackingNo>", _trackingNo);

            DataTable dt = oDAL.GetData(query);



            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return dt;
            }
            else
            {
                if (dt.Rows.Count > 0)
                {
                    //lstInvoice = cCommon.ConvertDtToHashTable(dt);
                    PO_NUMBER = dt.Rows[0]["PO NUMBER"].ToString();
                    TERMS = dt.Rows.Count > 1 ? dt.Rows[1]["TERMS"].ToString() : string.Empty;
                    SHIP = dt.Rows.Count > 2 ? dt.Rows[2]["SHIP"].ToString() : string.Empty;
                    TRACKING = _trackingNo;

                }

                return (dt);

            }
        }
        public DataTable GeneratedInvoice(string invoiceNo)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            query = @"
select 
	PONumber
	,Terms
	,format(ShipmentDate,'MM/dd/yyyy') AS SHIP
	,TrackingNumber AS 'TRACKING'
	,count(Ticket) AS QTY
	,Commodity
	,WtyCode AS 'Warranty Code'
	,ShipType AS 'Ship Type'
	,max((cast(Price as money))) AS 'COST'
	,max((cast(Price as money))) * count(Ticket) as [AMOUNT]
from rpt.DellBilling
	where InvoiceNo = '32827'

group by
	PONumber
	,Terms
	,format(ShipmentDate,'MM/dd/yyyy'),
	TrackingNumber,	
	Commodity,
	WtyCode,
	ShipType
";

            DataTable dt = oDAL.GetData(query);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return dt;
            }
            else
            {
                if (dt.Rows.Count > 0)
                {
                    //lstInvoice = cCommon.ConvertDtToHashTable(dt);
                    PO_NUMBER = dt.Rows[0]["PO NUMBER"].ToString();
                    TERMS = dt.Rows.Count > 1 ? dt.Rows[1]["TERMS"].ToString() : string.Empty;
                    SHIP = dt.Rows.Count > 2 ? dt.Rows[2]["SHIP"].ToString() : string.Empty;
                    //TRACKING = ;
                }

                return (dt);
            }
        }
        public DataTable InvoiceDt(string programId, string trackingNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            //string programName = HttpContext.Current.Session["Program"].ToString();
            string _trackingNo = GetInValue(trackingNo);
            string query = string.Empty;

            if (trackingNo != "")
            {


                query = @"
<trackingVar>
declare @ProgramID int = <programId>
declare @GradeAttributeID int = (select ca.ID from pls.CodeAttribute ca where ca.AttributeName = 'GRADE')
declare @StorageLocationAttributeID int = (select ca.ID from pls.CodeAttribute ca where ca.AttributeName = 'StorageLocation')
declare @DellRateCardID int = (select cgtd.ID from pls.CodeGenericTableDefinition cgtd where cgtd.ProgramID = @ProgramID and cgtd.Name= 'DELLRATECARD')
declare @ShippedStatusID int = (select cs.ID from pls.CodeStatus cs where cs.Description = 'SHIPPED')
declare @ActiveStatusID int = (select cs.ID from pls.CodeStatus cs where cs.Description = 'ACTIVE')
select		
	P.ID AS ProgramID,
	P.Name AS Program,
	soh.CustomerReference as [Ship Order],
	sosi.TrackingNo as [Tracking Number],
	sosi.ShipmentDate as [ShipmentDate],
	sosi.Carrier as [Carrier],
	cc.Description as [Commodity],	
	DellRateCardcgt.C01 as [Dell Charge Code],
	isnull(sohaStorageLocation.Value,'') as [SHIP TYPE],
	'DELL' AS [Brand],	
	ps.PartNo as [Model],
	roh.CustomerReference as [Receive Order],
	ps.ID as [PartSerialID], 
	ps.SerialNo as [Serial Number],
	'FLAT RATE' as [Charge Type],
	'BILLING SHIPMENT / FLAT RATE' as [Reference],
	'BILLING SHIPMENT - Tracking '+sosi.TrackingNo+' / '+pn.PartNo+' / '+isnull(psaGrade.Value,'')+' / FLAT RATE' as [Bill Description],
	DellRateCardcgt.C05 as [Price],
	'' as [BillDate],
	isnull(psaGrade.Value,'') AS [Wty. Code],
	'' as [Inv. Number],
	format(ps.WOEndDate,'MM.dd.yyyy') as [Revenue Date]	
into #Billing
from
	pls.PartSerial ps
	inner join pls.PartSerialAttribute psaGrade on ps.ID = psaGrade.PartSerialID
		and psaGrade.AttributeID = @GradeAttributeID		
		and ps.ProgramID = @ProgramID
		and ps.StatusID = @ShippedStatusID
	inner join pls.SOHeader soh on ps.SOHeaderID = soh.ID
	inner join pls.PartNo pn on ps.PartNo = pn.PartNo
	inner join pls.CodeCommodity cc on pn.PrimaryCommodityID = cc.ID
	inner join pls.ROHeader roh on ps.ROHeaderID = roh.ID				
		and roh.ProgramID = @ProgramID
	inner join pls.SOShipmentInfo sosi on ps.SOHeaderID = sosi.SOHeaderID		
	inner join pls.codeGenericTable DellRateCardcgt on DellRateCardcgt.GenericTableDefinitionID = @DellRateCardID
		and DellRateCardcgt.StatusID = @ActiveStatusID
		and DellRateCardcgt.C09 = isnull(psaGrade.Value,'')
		and (cc.Description = DellRateCardcgt.C08 or (DellRateCardcgt.C08 = 'LCD' AND (cc.Description = 'LCD ASSY MODULE' or cc.Description = 'LCD_SUBASSY' ) ))
	inner join pls.SOHeaderAttribute sohaStorageLocation on sohaStorageLocation.SOHeaderID = soh.ID
		and sohaStorageLocation.AttributeID = @StorageLocationAttributeID
	<trackingJoin>
	INNER JOIN pls.Program P ON P.ID = ps.ProgramID

select	
	count(b.[PartSerialID]) as [QTY],
	b.Commodity as [COMMODITY],
	b.[Wty. Code] as 'WARRANTY CODE',
	b.[SHIP TYPE] as 'SHIP TYPE',
	max((cast(b.Price as money))) AS [COST],
	max((cast(b.Price as money))) * count(b.[PartSerialID]) as [AMOUNT]
from 
	#Billing b
group by
	b.[Dell Charge Code],
	format(b.ShipmentDate,'MM/dd/yyyy'),
	b.[Tracking Number],	
	b.Commodity,
	b.[Wty. Code],
	b.[SHIP TYPE]		
		
drop table #Billing";


                if (!string.IsNullOrEmpty(trackingNo))
                {
                    query = query.Replace("<trackingVar>", "declare @Trackings varchar(max) =  '<TrackingNo>'");
                    query = query.Replace("<trackingJoin>", "inner join string_split(@Trackings,',') as so on sosi.TrackingNo = so.value");

                }
                else
                {
                    query = query.Replace("<trackingVar>", "");
                    query = query.Replace("<trackingJoin>", "");
                }
                query = query.Replace("<programId>", programId);
                query = query.Replace("<TrackingNo>", _trackingNo);

                DataTable dt = oDAL.GetData(query);



                if (oDAL.HasErrors)
                {
                    ErrorMessage = oDAL.ErrMessage;
                    return new DataTable();
                }
                else
                {

                    return dt;
                }
            }
            else
            {
                return new DataTable();
            }
        }
        public DataTable InvoiceDetail(string programId, string trackingNo)
        {
            oDAL = new cDAL("INIT");
            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            //string programName = HttpContext.Current.Session["Program"].ToString();
            string _trackingNo = GetInValueString(trackingNo);
            string query = string.Empty;
            if (trackingNo != "")
            {
                query = @"
SELECT [Brand]
      ,[Model]
      ,[OrderNo] AS 'Order No.'
      ,[Commodity]
      ,[Ticket]
      ,[SerialNumber] AS 'Serial No.'
      ,[ChargeType] AS 'Charge Type'
      ,[Reference]
      ,[BillDescription] AS 'Bill Description'
      ,[Qty]
      ,[Price]
      ,BillDate AS 'Bill Date'
      ,[WtyCode] AS 'Warranty Code'
      ,[ShipType] AS 'Ship Type'
	  ,FORMAT(RevenueDate, 'MM/dd/yyyy') AS 'Revenue Date'
      ,[InvoiceNo] AS 'Invoice No.'
      ,InvoiceDate AS 'Invoice Date'
  FROM [PlusRS].[rpt].[DellBilling]
WHERE InvoiceNo = <invNo>";


                query = query.Replace("<programId>", programId);
                query = query.Replace("<TrackingNo>", _trackingNo);
                query = query.Replace("<invNo>", InvNo);


                DataTable dt = oDAL.GetData(query);

                if (oDAL.HasErrors)
                {
                    ErrorMessage = oDAL.ErrMessage;
                    return new DataTable();
                }
                else
                {
                    return dt;
                }
            }
            else
            {
                return new DataTable();
            }
        }
        public DataTable GetExtraLineDt()

        {
            string invNo = HttpContext.Current.Session["InvNo"].ToString();
            string invDate = HttpContext.Current.Session["InvDate"].ToString();

            InvNo = invNo;
            InvDate = invDate;

            oDAL = new cDAL("INIT");
            string query = string.Empty;
            query = @"
SELECT 
        [Qty]
	  ,[PartNo] AS 'Part No.' 
      ,[Description]
      ,[Cost]
      ,[Amount]
  FROM [PlusRS].[rpt].[DellBillingExtraLine]
WHERE InvoiceNo = <invNo> ";

            query = query.Replace("<invNo>", invNo);

            DataTable dt = oDAL.GetData(query);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return new DataTable();
            }
            else
            {
                return dt;
            }
        }
        public bool SaveFirstGridData(string trackingNo, string programId, string invNo, string invDate)
        {
            oDAL = new cDAL("ACTIVE");
            string _trackingNo = GetInValueString(trackingNo);
            string _trackingNum = GetInValue(trackingNo);

            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"
<trackingVar>
DECLARE @InvNumber NVARCHAR(50) = '<invNo>';
DECLARE @InvDate varchar(20) = '<invDate>'; 
DECLARE @ProgramID int = <programId>
DECLARE @GradeAttributeID int = (SELECT ca.ID FROM pls.CodeAttribute ca WHERE ca.AttributeName = 'GRADE')
DECLARE @StorageLocationAttributeID int = (SELECT ca.ID FROM pls.CodeAttribute ca WHERE ca.AttributeName = 'StorageLocation')
DECLARE @DellRateCardID int = (SELECT cgtd.ID FROM pls.CodeGenericTableDefinition cgtd WHERE cgtd.ProgramID = @ProgramID AND cgtd.Name = 'DELLRATECARD')
DECLARE @ShippedStatusID int = (SELECT cs.ID FROM pls.CodeStatus cs WHERE cs.Description = 'SHIPPED')
DECLARE @ActiveStatusID int = (SELECT cs.ID FROM pls.CodeStatus cs WHERE cs.Description = 'ACTIVE')

-- Create a temporary table to hold the selected data
SELECT
    P.ID AS ProgramID,
    P.Name AS Program,
    soh.CustomerReference AS [Ship Order],
    sosi.TrackingNo AS [Tracking Number],
    sosi.ShipmentDate AS [ShipmentDate],
    sosi.Carrier AS [Carrier],
    cc.Description AS [Commodity],
    DellRateCardcgt.C01 AS [Dell Charge Code],
    ISNULL(sohaStorageLocation.Value, '') AS [SHIP TYPE],
    'DELL' AS [Brand],
    ps.PartNo AS [Model],
    roh.CustomerReference AS [Receive Order],
    ps.ID AS [PartSerialID], 
    ps.SerialNo AS [Serial Number],
    'FLAT RATE' AS [Charge Type],
    'BILLING SHIPMENT / FLAT RATE' AS [Reference],
    'BILLING SHIPMENT - Tracking ' + sosi.TrackingNo + ' / ' + pn.PartNo + ' / ' + ISNULL(psaGrade.Value, '') + ' / FLAT RATE' AS [Bill Description],
    DellRateCardcgt.C05 AS [Price],
    @InvDate AS [BillDate],
    ISNULL(psaGrade.Value, '') AS [Wty Code],
    @InvNumber AS InvNo,
    FORMAT(su.CreateDate, 'MM/dd/yyyy') AS [Revenue Date],
    @InvDate AS InvDate
INTO #Billing
from
	pls.PartSerial ps
	inner join pls.PartSerialAttribute psaGrade on ps.ID = psaGrade.PartSerialID
		and psaGrade.AttributeID = @GradeAttributeID		
		and ps.ProgramID = @ProgramID
		and ps.StatusID = @ShippedStatusID
	inner join pls.SOHeader soh on ps.SOHeaderID = soh.ID and soh.ProgramID=@ProgramID
	inner join pls.SOLine sl on sl.SOHeaderID=soh.ID and sl.PartNo = ps.PartNo 
	inner join pls.SOUnit su on su.SOLineID=sl.ID and su.SerialNo=ps.SerialNo 
	inner join pls.PartNo pn on ps.PartNo = pn.PartNo
	inner join pls.CodeCommodity cc on pn.PrimaryCommodityID = cc.ID
	inner join pls.ROHeader roh on ps.ROHeaderID = roh.ID				
		and roh.ProgramID = @ProgramID
	inner join pls.SOShipmentInfo sosi on ps.SOHeaderID = sosi.SOHeaderID		
	inner join pls.codeGenericTable DellRateCardcgt on DellRateCardcgt.GenericTableDefinitionID = @DellRateCardID
		and DellRateCardcgt.StatusID = @ActiveStatusID
		and DellRateCardcgt.C09 = isnull(psaGrade.Value,'')
	and (cc.Description = DellRateCardcgt.C08 or (DellRateCardcgt.C08 = 'LCD' AND (cc.Description = 'LCD ASSY MODULE' or cc.Description = 'LCD_SUBASSY' )  ))
	inner join pls.SOHeaderAttribute sohaStorageLocation on sohaStorageLocation.SOHeaderID = soh.ID
		and sohaStorageLocation.AttributeID = @StorageLocationAttributeID
    <trackingJoin>
    INNER JOIN pls.Program P ON P.ID = ps.ProgramID;

-- Insert data into the DellBilling table
INSERT INTO [PlusRS].[rpt].[DellBilling] (Brand, Model, OrderNo, Commodity, Ticket, SerialNumber, ChargeType, Reference, BillDescription, Price, BillDate, WtyCode, ShipType, PONumber, Terms, TrackingNumber, ShipmentDate,  RevenueDate, InvoiceNo, InvoiceDate)

SELECT  
   b.[Brand],
    b.[Model],
    b.[Ship Order] AS [OrderNo],
	b.Commodity,
    b.PartSerialID AS [Ticket],
    b.[Serial Number],
    b.[Charge Type],
    b.[Reference],
    b.[Bill Description],
    CAST(MAX(b.[Price]) AS VARCHAR) AS [Price],  -- Assumes you want to store the maximum price
    b.[BillDate],
    b.[Wty Code],
	b.[SHIP TYPE],
	'NA' AS PONumber,
	'NET 90' AS [Terms],
	b.[Tracking Number] AS TrackingNumber,
	b.[ShipmentDate],
    MAX(b.[Revenue Date]) AS [RevenueDate],
    b.InvNo AS [InvoiceNo],
    b.InvDate AS [InvoiceDate]
FROM 
    #Billing b
<trackingClause>
GROUP BY
    b.ProgramID,
    b.Program,
    b.[Ship Order],
    b.[Tracking Number],
    b.[ShipmentDate],
    b.[Carrier],
    b.[Commodity],    
    b.[SHIP TYPE],
    b.[Brand],
    b.[Model],
    b.[Receive Order],
    b.[Charge Type],
    b.[Reference],
    b.[Bill Description],
    b.[Wty Code],
    b.InvNo,
    b.InvDate,
    b.BillDate,
    b.[PartSerialID],
    b.[Serial Number];

-- Drop the temporary table
DROP TABLE #Billing;
";
            if (!string.IsNullOrEmpty(trackingNo))
            {
                query = query.Replace("<trackingVar>", "declare @Trackings varchar(max) =  '<TrackingNo>'");
                query = query.Replace("<trackingJoin>", "inner join string_split(@Trackings,',') as so on sosi.TrackingNo = so.value");
                query = query.Replace("<trackingClause>", "WHERE b.[Tracking Number] IN (" + _trackingNo + ")");
            }
            else
            {
                query = query.Replace("<trackingVar>", "");
                query = query.Replace("<trackingJoin>", "");
            }
            query = query.Replace("<programId>", programId);
            query = query.Replace("<TrackingNo>", _trackingNum);
            query = query.Replace("<invNo>", invNo);
            query = query.Replace("<invDate>", invDate);


            oDAL.Execute(query);
            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                Message = "Line successfuly added";
                return true;
            }

        }
        public bool SaveExtraLineData(string invNo, string invDate, List<DellBilling> extraLinesData)
        {
            oDAL = new cDAL("INIT");
            string sql = string.Empty;
            sql = @"
            INSERT INTO [PlusRS].[rpt].[DellBillingExtraLine] 
            (PartNo, Qty, Description, Cost, Amount, InvoiceNo, InvoiceDate)
            VALUES ";

            List<string> valueStrings = new List<string>();

            foreach (var line in extraLinesData)
            {
                string valueString = "('" + line.PartNumber + "', " +
                                      line.Quantity + ", '" +
                                      line.Description + "', " +
                                      line.extraLineCost + ", " +
                                      line.extraLineAmount + ", '" +
                                      invNo + "', '" +
                                      invDate + "')";

                valueStrings.Add(valueString);
            }

            // Join all value strings with a comma
            sql += string.Join(", ", valueStrings);

            // Execute the single insert statement with multiple values
            oDAL.Execute(sql);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                Message = "Line successfuly added";
                return true;
            }
        }
        public bool ValidateInvNo(string invNo)
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;

            query = @"SELECT TOP 1 [InvoiceNo] FROM [PlusRS].[rpt].[DellBillingExtraLine]
                    where InvoiceNo = <invNo> ";
            query = query.Replace("<invNo>", invNo);

            DataTable _result = oDAL.GetData(query);
            if (_result.Rows.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private string GetInValueString(string Value)
        {
            string[] arr = Value.Split(',').Select(s => s.Trim()).ToArray();
            string _arr = null;
            foreach (var item in arr)
            {
                if (_arr == null)
                {
                    _arr = "\'" + item + "\'";
                }
                else
                {
                    _arr += "," + "\'" + item + "\'";
                }

            }
            return _arr;
        }
        private string GetInValue(string Value)
        {
            string[] arr = Value.Split(',').Select(s => s.Trim()).ToArray();
            string _arr = null;
            foreach (var item in arr)
            {
                if (_arr == null)
                {
                    _arr = "" + item + "";
                }
                else
                {
                    _arr += "," + "" + item + "";
                }

            }
            return _arr;
        }

        #endregion
    }
}