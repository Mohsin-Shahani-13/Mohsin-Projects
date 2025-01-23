using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.SupplyChain.Models
{
    public class Data2LogisticsReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Invoice Date:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "Invoice No.:")]
        public string invNo { get; set; }
        [Display(Name = "Customer Reference:")]
        public string custRef { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("Active");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);


            return dt;
        }
        public List<Hashtable> lstData2LogisticsReport { get; set; }
        #endregion
        #region Methods 
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
        public DataTable GetList(string programId, string programName, string invDate, string invNo, string custRef)
        {
            oDAL = new cDAL("ACTIVE");
            string _CustRef = GetInValue(custRef);
            string query = string.Empty;
            query = @"
	DECLARE @invoice_number VARCHAR(50) = '<InvNo>' 
--DECLARE @invoice_date VARCHAR(50) = '<InvDate>' 
DECLARE @CustomerReferences VARCHAR(1000) = '<_CustRef>'
DECLARE @invoiceDate DATE = CONVERT(DATE, '<InvDate>') 
DECLARE @SOHeaderIDs table (SOHeaderID int)
INSERT INTO @SOHeaderIDs(SOHeaderID)
SELECT soh.ID 
FROM STRING_SPLIT(@CustomerReferences,',') AS A 
INNER JOIN pls.SOHeader soh on a.value = soh.CustomerReference
where soh.ProgramID = '<ProgramId>'
declare @DellRateCardID int = (select ID from pls.CodeGenericTableDefinition where Name = 'DellRateCard' and ProgramID = '<ProgramId>')
declare @GradeAttributeID int = (select ID from pls.CodeAttribute  where AttributeName = 'GRADE')
	SELECT 		
		'VAU51'																			[Carrier ID], 
		'DELL04'																		[Client ID], 
		'D'																				[Invoice Type], 
		@invoice_number																	[Invoice Number], 
		REPLACE(
			FORMAT(SUM(CONVERT(DECIMAL(10,2), cgt.C05) * count(ps.ID)) 
					OVER (PARTITION BY @invoice_number), 'N2'), 
			',', 
			'')																			[Invoice total], 											
		FORMAT(@invoiceDate, 'yyyyMMdd')												[Invoice Date], 
		CONVERT(VARCHAR, @invoice_number) 
			+ RIGHT('000'+ISNULL(CONVERT(VARCHAR, 
					ROW_NUMBER() 
						OVER (PARTITION BY @invoice_number 
								ORDER BY @invoice_number)),''), 3)						[Shipment Number],
		FORMAT(@invoiceDate, 'yyyyMMdd')												[Ship Date], 
		'DELL'																			[Bill-to Account],
		''																				[Weight Unit],
		''																				[Billed Weight],
		''																				[Actual Weight],
		''																				[Dim Weight],
		''																				[Volume],
		count(ps.ID)																	[Pieces],
		ps.partno																		[Package Type],
		0																				[Loading Meters],
		cgt.C07																			[Service Type],
		''																				[Service Zone],
		'O'																				[Direction],
		cgt.C06																			[Mode Code],
		''																				[Payment Terms],
		''																				[Inco Terms],
		''																				[Distance Qualifier],
		''																				[Distance],
		''																				[Bill of Lading],
		''																				[MAWB],
		''																				[HAWB],
		''																				[PO Number],
		cgt.C04																			[Reference Number],
		''																				[Delivery Date],
		''																				[Delivery Time],
		''																				[POD Name],
		''																				[Notes],
		''																				[Origin (Air)Port],
		''																				[Destination (Air)Port],
		''																				[Airline],
		''																				[Shipper Location],
		''																				[Shipper Name],
		'VALUTECH OUTSOURCING, LLC'														[Shipper Company],
		'122 W. MADISON ST / 2ND FLOOR'													[Shipper Address1],
		''																				[Shipper Address2],
		''																				[Shipper Address3],
		'OTTAWA'																		[Shipper City],
		'IL'																			[Shipper State],
		'61350'																			[Shipper Postcode],
		'US'																			[Shipper Country],
		''																				[Consignee Location],
		''																				[Consignee Name],
		'Dell Computer'																	[Consignee Company],
		'P. O. Box 149257'																[Consignee Address1],
		''																				[Consignee Address2],
		''																				[Consignee Address3],
		'Austin'																		[Consignee City],
		'TX'																			[Consignee State],
		'78714'																			[Consignee Postcode],
		'US'																			[Consignee Country],
		''																				[Bill-To Location],
		'Dell Computer'																	[Bill-To Company],
		'P. O. Box 149257'																[Bill-To Address1],
		''																				[Bill-To Address2],
		'Austin'																		[Bill-To City],
		'TX'																			[Bill-To State],
		'78714'																			[Bill-To Postcode],
		'US'																			[Bill-To Country],
		''																				[Equipment Type],
		''																				[Container Number],
		''																				[Trailer/Car ID],
		''																				[Carrier VAT Number],
		''																				[Client VAT Number],
		''																				[VAT Percentage],
		''																				[Currency],
		''																				[Currency Exchange Rate],
		''																				[Spot Quote],
		REPLACE(
			FORMAT(CONVERT(DECIMAL(10,2), count(ps.ID) * cast(cgt.C05 as money)), 'N2'),
			',',
			'')																			[Amount Billed],
		''																				[Freight Amount],
		''																				[Freight VAT Code],
		''																				[Discount Amount],
		''																				[Discount VAT Code],
		''																				[Total Shipment VAT Amount],
		cgt.C01																			[Accessorial Charge Code 1],
		REPLACE(
			FORMAT(CONVERT(DECIMAL(10,2), count(ps.ID) * cast(cgt.C05 as money)), 'N2'),
			',',
			'')																			[Accessorial Charge Amount 1],
		''																				[VAT Liable Flag 1],
		''																				[Accessorial Extra 1],
		''																				[Accessorial Charge Code 2],
		''																				[Accessorial Charge Amount 2],
		''																				[VAT Liable Flag 2],
		''																				[Accessorial Extra 2],
		''																				[Accessorial Charge Code 3],
		''																				[Accessorial Charge Amount 3],
		''																				[VAT Liable Flag 3],
		''																				[Accessorial Extra 3],
		''																				[Accessorial Charge Code 4],
		''																				[Accessorial Charge Amount 4],
		''																				[VAT Liable Flag 4],
		''																				[Accessorial Extra 4],
		''																				[Accessorial Charge Code 5],
		''																				[Accessorial Charge Amount 5],
		''																				[VAT Liable Flag 5],
		''																				[Accessorial Extra 5],
		''																				[Accessorial Charge Code 6],
		''																				[Accessorial Charge Amount 6],
		''																				[VAT Liable Flag 6],
		''																				[Accessorial Extra 6],
		''																				[Accessorial Charge Code 7],
		''																				[Accessorial Charge Amount 7],
		''																				[VAT Liable Flag 7],
		''																				[Accessorial Extra 7],
		''																				[Accessorial Charge Code 8],
		''																				[Accessorial Charge Amount 8],
		''																				[VAT Liable Flag 8],
		''																				[Accessorial Extra 8],
		''																				[Accessorial Charge Code 9],
		''																				[Accessorial Charge Amount 9],
		''																				[VAT Liable Flag 9],
		''																				[Accessorial Extra 9],
		''																				[Accessorial Charge Code 10],
		''																				[Accessorial Charge Amount 10],
		''																				[VAT Liable Flag 10],
		''																				[Accessorial Extra 10],
		''																				[Accessorial Charge Code 11],
		''																				[Accessorial Charge Amount 11],
		''																				[VAT Liable Flag 11],
		''																				[Accessorial Extra 11],
		''																				[Accessorial Charge Code 12],
		''																				[Accessorial Charge Amount 12],
		''																				[VAT Liable Flag 12],
		''																				[Accessorial Extra 12],
		''																				[Accessorial Charge Code 13],
		''																				[Accessorial Charge Amount 13],
		''																				[VAT Liable Flag 13],
		''																				[Accessorial Extra 13],
		''																				[Accessorial Charge Code 14],
		''																				[Accessorial Charge Amount 14],
		''																				[VAT Liable Flag 14],
		''																				[Accessorial Extra 14],
		''																				[Accessorial Charge Code 15],
		''																				[Accessorial Charge Amount 15],
		''																				[VAT Liable Flag 15],
		''																				[Accessorial Extra 15],
		''																				[Accessorial Charge Code 16],
		''																				[Accessorial Charge Amount 16],
		''																				[VAT Liable Flag 16],
		''																				[Accessorial Extra 16],
		''																				[Accessorial Charge Code 17],
		''																				[Accessorial Charge Amount 17],
		''																				[VAT Liable Flag 17],
		''																				[Accessorial Extra 17],
		''																				[Accessorial Charge Code 18],
		''																				[Accessorial Charge Amount 18],
		''																				[VAT Liable Flag 18],
		''																				[Accessorial Extra 18],
		''																				[Accessorial Charge Code 19],
		''																				[Accessorial Charge Amount 19],
		''																				[VAT Liable Flag 19],
		''																				[Accessorial Extra 19],
		''																				[Accessorial Charge Code 20],
		''																				[Accessorial Charge Amount 20],
		''																				[VAT Liable Flag 20],
		''																				[Accessorial Extra 20],
		''																				[Accessorial Charge Code 21],
		''																				[Accessorial Charge Amount 21],
		''																				[VAT Liable Flag 21],
		''																				[Accessorial Extra 21],
		''																				[Accessorial Charge Code 22],
		''																				[Accessorial Charge Amount 22],
		''																				[VAT Liable Flag 22],
		''																				[Accessorial Extra 22],
		''																				[Accessorial Charge Code 23],
		''																				[Accessorial Charge Amount 23],
		''																				[VAT Liable Flag 23],
		''																				[Accessorial Extra 23],
		''																				[Accessorial Charge Code 24],
		''																				[Accessorial Charge Amount 24],
		''																				[VAT Liable Flag 24],
		''																				[Accessorial Extra 24],
		''																				[Accessorial Charge Code 25],
		''																				[Accessorial Charge Amount 25],
		''																				[VAT Liable Flag 25],
		''																				[Accessorial Extra 25]
	FROM 			 
		pls.PartSerial ps 		
		inner join pls.CodeGenericTable cgt on cgt.GenericTableDefinitionID = @DellRateCardID 
			and cgt.StatusID = 4
			and ps.ProgramID = '<ProgramId>'
			and ps.StatusID = 18
		inner join pls.PartNo pn on ps.PartNo = pn.PartNo
		inner join pls.CodeCommodity cc on pn.PrimaryCommodityID = cc.ID
		and IIF(cc.Description LIKE '%LCD%', 'LCD',cc.Description) = cgt.C08			
		inner join pls.PartSerialAttribute psa on ps.ID = psa.PartSerialID	
			and psa.AttributeID = @GradeAttributeID
			and psa.Value = cgt.C09
		inner join @SOHeaderIDs s on ps.SOHeaderID = s.SOHeaderID		
	GROUP BY
		cgt.C07,
		cgt.C06,
		cgt.C01,
		cgt.C05,
		cgt.C04,
		ps.PartNo
 ";


            query = query.Replace("<ProgramId>", programId);
            query = query.Replace("<_CustRef>", _CustRef);
            query = query.Replace("<InvNo>", invNo);
            query = query.Replace("<InvDate>", invDate);


            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("209", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return new DataTable();
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstData2LogisticsReport = cCommon.ConvertDtToHashTable(dt);
                return dt;
            }
        }
        #endregion
    }
}