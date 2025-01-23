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
    public class SODetailedForCustoms
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public bool isAllDate { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        [Display(Name = "Serial No.:")]
        public string SerialNo { get; set; }

        [Display(Name = "Order No.:")]
        public string OrderNo { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public List<Hashtable> lstSODetailedForCustoms { get; set; }

        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string SerialNo, string OrderNo)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
SELECT
	P.Name AS Program
	,P.ID
	,SOH.ID as SOHeaderID
	,CustomerReference as [Ship Order Number]
	,SOL.PartNo
	,PNA_FAMILY.Value as [Family/Cell]
	,CONCAT(CAD.Address1, ' ' ,CAD.Address2) as [Ship To Address]
	,CAD.State
	,CAD.Country
	,CAD.Zip
	,SOSI.Carrier
	,SOSI.TrackingNo
	,PSA_CartonNo.Value as [Box ID]
	,SOU.SerialNo
	,SOH.LastActivityDate as [Ship Date]
    ,SOSI.ServiceTypeDescription as [Ship Type]
	,ISNULL(PSA.Value,ROLA.Value) as COO
	,PNA_HTSMX.Value as [HTS MX]
	,PNA_HTSUS.Value as [HTS US]
	,concat(isnull(PNA_CARRIER_WEIGHT.Value, PNA_WEIGHT.Value), ' ', PNA_WEIGHTUNIT.Value) as [weight]
	,SUM(SOL.QtyToShip) AS Quantity
	,SOHA.Value as [Process]
	,PNA_STD_NAME_ID.Value as [STD NAME ID]
	,PNA_STD_NAME.Value as [STD NAME]
	,CS.Description AS Status
	,U.Username
	,SOH.CreateDate
FROM pls.SOHeader SOH
	INNER JOIN PLS.SOLine SOL 
		ON SOL.SOHeaderID = SOH.ID
	INNER JOIN PLS.SOUnit SOU
		ON SOL.ID = SOU.SOLineID
	INNER JOIN PLS.Program P 
		ON P.ID = SOH.ProgramID
	INNER JOIN PLS.CodeAddressDetails CAD 
		ON CAD.AddressID = SOH.AddressID 
		AND CAD.AddressType = 'ShipTo'
	INNER JOIN PLS.CodeStatus CS 
		ON CS.ID = SOH.StatusID
	INNER JOIN PLS.[User] U 
		ON U.ID = SOH.UserID
	LEFT JOIN PLS.CodeConfiguration CC 
		ON CC.ID = SOL.ConfigurationID
	LEFT OUTER JOIN pls.[SOShipmentInfo] SOSI 
		ON SOH.ID = SOSI.SOHeaderID
	LEFT JOIN pls.CodeAttribute CA 
		on CA.AttributeName ='PROCESS_TYPE'
	LEFT JOIN pls.SOHeaderAttribute SOHA 
		on SOHA.SOHeaderID = SOH.ID AND SOHA.AttributeID = CA.ID
	INNER JOIN PLS.PartSerial PS
		ON PS.ProgramID = SOH.ProgramID
		AND PS.PartNo = SOL.PartNo
		AND PS.SerialNo = SOU.SerialNo
	LEFT JOIN PLS.PartSerialAttribute PSA
		ON PSA.PartSerialID = PS.ID
		AND PSA.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'COO')
	LEFT JOIN PLS.PartSerialAttribute PSA_CartonNo
		ON PSA_CartonNo.PartSerialID = PS.ID
		AND PSA_CartonNo.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'CartonNo')
LEFT JOIN PLS.ROUnit ROU
ON ROU.SerialNo = PS.SerialNo
AND ROU.StatusID = 6 -- RECEIVED
	LEFT JOIN PLS.ROLine ROL
		ON ROL.ROHeaderID = PS.ROHeaderID
		AND ROL.ID = ROU.ROLineID
	LEFT JOIN PLS.ROLineAttribute ROLA
		ON ROLA.ROLineID = ROL.ID
		AND ROLA.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'COO') --
	LEFT JOIN PLS.PartNoAttribute PNA_CARRIER_WEIGHT
		ON PNA_CARRIER_WEIGHT.ProgramID = SOH.ProgramID
		AND PNA_CARRIER_WEIGHT.PartNo = SOL.PartNo
		and PNA_CARRIER_WEIGHT.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'CARRIER_WEIGHT')
	LEFT JOIN PLS.PartNoAttribute PNA_WEIGHT
		ON PNA_WEIGHT.ProgramID = SOH.ProgramID
		AND PNA_WEIGHT.PartNo = SOL.PartNo
		and PNA_WEIGHT.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'weight')
	LEFT JOIN PLS.PartNoAttribute PNA_WEIGHTUNIT
		ON PNA_WEIGHTUNIT.ProgramID = SOH.ProgramID
		AND PNA_WEIGHTUNIT.PartNo = SOL.PartNo
		and PNA_WEIGHTUNIT.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'WeightUnit')
	LEFT JOIN PLS.PartNoAttribute PNA_STD_NAME
		ON PNA_STD_NAME.ProgramID = SOH.ProgramID
		AND PNA_STD_NAME.PartNo = SOL.PartNo
		and PNA_STD_NAME.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'STD NAME')
	LEFT JOIN PLS.PartNoAttribute PNA_STD_NAME_ID
		ON PNA_STD_NAME_ID.ProgramID = SOH.ProgramID
		AND PNA_STD_NAME_ID.PartNo = SOL.PartNo
		and PNA_STD_NAME_ID.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'STD NAME ID')
	LEFT JOIN PLS.PartNoAttribute PNA_FAMILY
		ON PNA_FAMILY.ProgramID = SOH.ProgramID
		AND PNA_FAMILY.PartNo = SOL.PartNo
		and PNA_FAMILY.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'FAMILY')
	LEFT JOIN PLS.PartNoAttribute PNA_HTSUS
		ON PNA_HTSUS.ProgramID = SOH.ProgramID
		AND PNA_HTSUS.PartNo = SOL.PartNo
		and PNA_HTSUS.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'HTSUS')
	LEFT JOIN PLS.PartNoAttribute PNA_HTSMX
		ON PNA_HTSMX.ProgramID = SOH.ProgramID
		AND PNA_HTSMX.PartNo = SOL.PartNo
		and PNA_HTSMX.AttributeID = (SELECT ID FROM PLS.CodeAttribute WHERE AttributeName = 'HTSMX')
WHERE  cs.ID  IN ('18') AND P.ID = '10059'
";

            if (!string.IsNullOrEmpty(frmDt) && !string.IsNullOrEmpty(toDt))
                query += "AND CONVERT(Date, SOH.LastActivityDate) >= '" + frmDt + "' AND CONVERT(Date, SOH.LastActivityDate) <= '" + toDt + "' \n";

            if (!string.IsNullOrEmpty(SerialNo))
                query += "AND SOU.SerialNo LIKE '%" + SerialNo + "%'\n";

            if (!string.IsNullOrEmpty(OrderNo))
                query += "AND SOH.CustomerReference LIKE '%" + OrderNo + "%'\n";

            query += @"GROUP BY

			SOH.ProgramID
           ,SOH.ID
           ,P.Name
		   ,P.ID
           ,CustomerReference
           ,SOL.PartNo
		   ,PNA_FAMILY.Value
           ,CC.Description
		   ,SOU.SerialNo
           ,SOSI.TrackingNo
		   ,SOSI.Carrier
           ,CONCAT(CAD.Address1, ' ', CAD.Address2)
           ,CAD.City
           ,CAD.State
           ,CAD.Country
           ,CAD.Zip
           ,SOHA.Value
           ,CS.Description
           ,U.Username
           ,SOH.CreateDate
           ,SOH.LastActivityDate
		   ,SOSI.ServiceTypeDescription
		   ,PSA.Value
		   ,ROLA.Value
		   ,concat(isnull(PNA_CARRIER_WEIGHT.Value, PNA_WEIGHT.Value), ' ', PNA_WEIGHTUNIT.Value)
		   ,PNA_STD_NAME_ID.Value
		   ,PNA_STD_NAME.Value
		   ,PSA_CartonNo.Value
		   ,PNA_HTSMX.Value
		   ,PNA_HTSUS.Value
ORDER BY CreateDate DESC";

            DataTable dt = oDAL.GetData(query);

            filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";

            if (!string.IsNullOrEmpty(SerialNo))
                filterString += " | Serial No. LIKE '" + SerialNo + "' ";

            if (!string.IsNullOrEmpty(OrderNo))
                filterString += " | Order No = '" + OrderNo + "' ";





            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("243", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSODetailedForCustoms = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}