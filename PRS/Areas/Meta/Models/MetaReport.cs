using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.Meta.Models
{
    public class MetaReport
    {
        cDAL oDAL;
        [Display(Name = "Program:")]
        public string contract { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstMetaReport { get; set; }

        public DataTable Contract() // Contract
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            string query = string.Empty;
            // query = "SELECT ID, ContractNo as contract FROM ContractList ORDER BY ContractNo ";
            query = "SELECT DISTINCT contract FROM Inmessage_hdr WHERE contract != '' ORDER BY contract";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public DataTable GetSitewiseContract()
        {// Contract
            oDAL = new cDAL("INIT");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select  Site, 
                              Name AS programName,
                              Contract
                             FROM dbo.SitewiseContract
                      WHERE SITE = '<site>'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        static string GetContractValues(string contract)
        {
            switch (contract)
            {
                case "10009":
                    return "10009,21455";
                case "10034":
                    return "10034,12527";
                case "10041":
                    return "10041,12535";
                case "10042":
                    return "10042,12534";
                default:
                    return "";
            }
        }
        public bool GetList(string contract, string programName)
        {

            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (conType == "TRAN")
            {
                oDAL = new cDAL("Z004_INBOUND");
            }
            else if (conType == "PROD")
            {
                oDAL = new cDAL("Z001_INBOUND");
            }
            //oDAL = new cDAL("Z004_INBOUND");
            string query = string.Empty;
            //string _contract = GetInValue(contract);
            string _contract = GetContractValues(contract);

            query = @"
select 
       Contract
      ,hdr.Inmessage_Hdr_Id as MessageId
	  , Processed_Date as RowVersion
	  , Customer_order_No as CustOrderNo
	  , adr.First_Name as ShipTOName
	  , Processed as MessageStatus
	  , hdr.C10
	  , adr.Address1 as ShipToAddress 
	  , adr.City as ShipToCity
	  , adr.State as ShipToState
	  , adr.Zip as ShipToZip
	  , adr.Country as ShipToCountry
	  , Message_Type as MessageType
	  , Customer_Order_type as CustomerOrderType
	  , ShipmentMode as ShipmentMethod
	  , Message as ErrorSummary
	  , 'NotFound' as Info
	  , IFS_order_no as IFSRMAOrderNo
	  , 'NotFound' as IFSRMAComplete
	  , hdr.Notes3 as Notes3
	  , 'NotFound' as IFSRMALineStatus
	  , hdr.Notes1
	  , hdr.Notes2
	  , hdr.C11 
	  , line.Model_Catalog_No as ModelCatalogNo
	  , line.Description as Description
	  , 'NotFound' as InventoryPart
	  , line.hts_code1 as HTSFrom
	  , line.hts_code2 as HTSTo
	  ,  Price as UnitPrice
	  ,  Weight as UnitWeight
	  , 'NotFound' as Measurement
	  ,  Box_Length as UnitLenght
	  ,  Box_Width as UnitWidht
	  ,  Box_Height as UnitHeight
	  , 'NotFound' as DimensionUnit
	  , line.Hazardous 
	  , 'NotFound' as COO
	  , hdr.C01 as C1
	  , hdr.Processed_Date as MessageDate
	  , hdr.Contact 
	  , line.Message_Line_No
from Inmessage_hdr hdr
inner join InMessage_line line on line.Inmessage_Hdr_Id = hdr.Inmessage_Hdr_Id
inner join InMessage_Address adr on adr.Inmessage_hdr_id = hdr.Inmessage_hdr_id and adr.Address_type = 'SHIPTO'
where Message_Type in ('PlusRoOrder', 'PlusSoOrder')
and Processed <> 'T'
";
            query += "AND Contract IN (" + _contract + ")";

            //query = query.Replace("<contract>", contract);

            //if (!string.IsNullOrEmpty(custordertype))
            //    query += "AND Customer_Order_type LIKE '%" + custordertype + "%' ";

            DataTable dt = oDAL.GetData(query);

            //Filterstring
            filterString += "> Program = '" + programName + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("173", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaReport = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        private string GetInValue(string Value)
        {
            string[] arr = Value.Split(',');
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
    } 
}