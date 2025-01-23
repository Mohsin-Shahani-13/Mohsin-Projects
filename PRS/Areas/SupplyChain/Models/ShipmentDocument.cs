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
    public class ShipmentDocument
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Customer Ref.:")]
        public string CustRef { get; set; }
        public string CustomerReference { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
         public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string CONSIGNEEName { get; set; }
        public string CONSIGNEEAddress { get; set; }
        public string CONSIGNEECity { get; set; }
        public string ShipmentDate { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstShipmentInfo { get; set; }
        public List<Hashtable> lstShipmentDocument { get; set; }
        public List<object> lstMst = new List<object>();

        #endregion
        #region Methods
       

        public bool GetList(string CustRef)
        {
             oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @" 
SELECT 
       SOH.CustomerReference, 
     CAD.Name,
	  CAD.Address1 + ' ' + CAD.Address2 Address,
	 CAD.City + ' ' + CAD.State + ' ' + CAD.Zip City,
	 CAD.State,
	 CAD.Zip,
	 CAD.Phone,
	 CADs.Name AS CONSIGNEEName,
	 Cads.Address1 + CADs.Address2 AS CONSIGNEEAddress,
	 CADs.City + CADs.State + CADs.Zip AS CONSIGNEECity,
	 FORMAT(SOSI.ShipmentDate, 'dd/MM/yyyy') as ShipmentDate
FROM  pls.SOHeader SOH
LEFT join pls.CodeAddressDetails CAD on CAD.AddressID = SOH.AddressID AND AddressType = 'Site'
LEFT join pls.CodeAddressDetails CADs on CADs.AddressID = SOH.AddressID AND CADs.AddressType = 'ShipTo'
LEFT JOIN pls.SOShipmentInfo SOSI ON SOSI.SOHeaderID = SOH.ID
WHERE (ProgramID = 10063) AND SOH.CustomerReference = '<CustRef>' ;";
            query = query.Replace("<CustRef>", CustRef);
            // oDAL = new cDAL(HttpContext.Current.Request["DB"]);
            DataTable dtHeader = oDAL.GetData(query);

            if (dtHeader.Rows.Count > 0)
            {
                DataRow dr = dtHeader.Rows[0];

                CustomerReference = Convert.ToString(dr["CustomerReference"]);
                Name = Convert.ToString(dr["Name"]);
                Address = Convert.ToString(dr["Address"]);
                City = Convert.ToString(dr["City"]);
                State = Convert.ToString(dr["State"]);
                Zip = Convert.ToString(dr["Zip"]);
                Phone = Convert.ToString(dr["Phone"]);
                CONSIGNEEName = Convert.ToString(dr["CONSIGNEEName"]);
                CONSIGNEEAddress = Convert.ToString(dr["CONSIGNEEAddress"]);
                CONSIGNEECity = Convert.ToString(dr["CONSIGNEECity"]);
                ShipmentDate = Convert.ToString(dr["ShipmentDate"]);
                //ShipmentDate = dr["ShipmentDate"].ToString() == DBNull.Value.ToString() ? "" : Convert.ToDateTime(dr["ShipmentDate"]).ToString("dd/MM/yyyy");

            }

            query +=@"SELECT DISTINCT
    ROW_NUMBER() OVER (ORDER BY RMRModule) AS SeqNo, -- Auto-generated sequence number
    CustomerReference,
    RMRModule,
    PartNo,
    ModuleName,
    SerialNo,
    RCToolNo,
    FRNo,
    RepairResult,
    PO,
    FailDescription,
    Customer,
    CheckList
FROM (
    SELECT DISTINCT
        SOH.CustomerReference,
        ROUA.Value AS RMRModule,
        SOL.PartNo,
        PN.ModelNo AS ModuleName,
        SOU.SerialNo,
        PSA.Value AS RCToolNo,
        ROUA2.Value AS FRNo,
        '' AS RepairResult,
        PSA2.Value AS PO,
        '' AS FailDescription,
        ROUA3.Value AS Customer,
        '' AS CheckList
    FROM pls.PartSerial PS
    INNER JOIN pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID
    INNER JOIN pls.ROLine ROL ON ROL.ROHeaderID = ROH.id AND ROL.PartNo = PS.PartNo
    INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.id AND ROU.SerialNo = PS.SerialNo
    LEFT JOIN pls.CodeAttribute CA ON CA.AttributeName = 'RMR'
    LEFT JOIN pls.ROUnitAttribute ROUA ON ROUA.ROUnitID = ROU.id AND ROUA.AttributeID = CA.ID
    INNER JOIN pls.SOHeader SOH ON SOH.ID = PS.SOHeaderID
    INNER JOIN pls.SOLine SOL ON SOL.SOHeaderID = SOH.ID AND SOL.PartNo = PS.PartNo
    INNER JOIN pls.SOUnit SOU ON SOU.SOLineID = SOL.ID AND SOU.SerialNo = PS.SerialNo
    INNER JOIN pls.partno PN ON PN.PartNo = SOL.PartNo
    LEFT JOIN pls.CodeAttribute CA2 ON CA2.AttributeName = 'RCTNO'
    LEFT JOIN pls.PartSerialAttribute PSA ON PSA.AttributeID = CA2.ID AND PSA.PartSerialID = PS.ID --link with ID
    LEFT JOIN pls.CodeAttribute CA3 ON CA3.AttributeName = 'FR'
    LEFT JOIN pls.ROUnitAttribute ROUA2 ON ROUA2.ROUnitID = ROU.ID AND ROUA2.AttributeID = CA3.ID
    LEFT JOIN pls.CodeAttribute CA4 ON CA4.AttributeName = 'POORSCRAPNUM'
    LEFT JOIN pls.PartSerialAttribute PSA2 ON PSA2.AttributeID = CA4.ID AND PSA2.PartSerialID = PS.ID --link with ID
    LEFT JOIN pls.CodeAttribute CA5 ON CA5.AttributeName = 'CUSTOMER_NAME'
    LEFT JOIN pls.ROUnitAttribute ROUA3 ON ROUA3.ROUnitID = ROU.ID AND ROUA3.AttributeID = CA5.ID
    WHERE PS.ProgramID = 10063 AND SOH.CustomerReference = '<CustRef>'

    UNION ALL

   SELECT DISTINCT
        SOH.CustomerReference,
        ROUA.Value AS RMRModule,
        SOL.PartNo,
        PN.ModelNo AS ModuleName,
        SOU.SerialNo,
        PSA.Value AS RCToolNo,
        ROUA2.Value AS FRNo,
        '' AS RepairResult,
        PSA2.Value AS PO,
        '' AS FailDescription,
        ROUA3.Value AS Customer,
        '' AS CheckList
    FROM pls.PartSerialHistory PSH
    INNER JOIN pls.ROHeader ROH ON ROH.ID = PSH.ROHeaderID
    INNER JOIN pls.ROLine ROL ON ROL.ROHeaderID = ROH.id AND ROL.PartNo = PSH.PartNo
    INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.id AND ROU.SerialNo = PSH.SerialNo
    LEFT JOIN pls.CodeAttribute CA ON CA.AttributeName = 'RMR'
    LEFT JOIN pls.ROUnitAttribute ROUA ON ROUA.ROUnitID = ROU.id AND ROUA.AttributeID = CA.ID
    INNER JOIN pls.SOHeader SOH ON SOH.ID = PSH.SOHeaderID
    INNER JOIN pls.SOLine SOL ON SOL.SOHeaderID = SOH.ID AND SOL.PartNo = PSH.PartNo
    INNER JOIN pls.SOUnit SOU ON SOU.SOLineID = SOL.ID AND SOU.SerialNo = PSH.SerialNo
    INNER JOIN pls.partno PN ON PN.PartNo = SOL.PartNo
    LEFT JOIN pls.CodeAttribute CA2 ON CA2.AttributeName = 'RCTNO'
    LEFT JOIN pls.PartSerialAttributeHistory PSA ON PSA.AttributeID = CA2.ID AND PSA.PartSerialHistoryID = PSH.ID --link with ID
    LEFT JOIN pls.CodeAttribute CA3 ON CA3.AttributeName = 'FR'
    LEFT JOIN pls.ROUnitAttribute ROUA2 ON ROUA2.ROUnitID = ROU.ID AND ROUA2.AttributeID = CA3.ID
    LEFT JOIN pls.CodeAttribute CA4 ON CA4.AttributeName = 'POORSCRAPNUM'
    LEFT JOIN pls.PartSerialAttributeHistory PSA2 ON PSA2.AttributeID = CA4.ID AND PSA2.PartSerialHistoryID = PSH.ID --link with ID
    LEFT JOIN pls.CodeAttribute CA5 ON CA5.AttributeName = 'CUSTOMER_NAME'
    LEFT JOIN pls.ROUnitAttribute ROUA3 ON ROUA3.ROUnitID = ROU.ID AND ROUA3.AttributeID = CA5.ID
    WHERE PSH.ProgramID = 10063 AND SOH.CustomerReference = '<CustRef>'
) a
GROUP BY 
    CustomerReference,
    RMRModule,
    PartNo,
    ModuleName,
    SerialNo,
    RCToolNo,
    FRNo,
    RepairResult,
    PO,
    FailDescription,
    Customer,
    CheckList;


";
            query = query.Replace("<CustRef>", CustRef);

            DataSet DS = oDAL.GetDataSet(query);

            if (!string.IsNullOrEmpty(CustRef))
                filterString += "> Customer Ref. = '" + CustRef + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("242", query, string.Empty, false);

            if (!oDAL.HasErrors)
            {
                List<ArrayList> lstDtl = new List<ArrayList>();

                //lstDtl = cCommon.ConvertDtToArrayList(DS.Tables[0]);
                //lstPartAttribute = cCommon.ConvertDtToHashTable(DS.Tables[0]);


                lstDtl = cCommon.ConvertDtToArrayList(DS.Tables[0]);
                lstShipmentInfo = cCommon.ConvertDtToHashTable(DS.Tables[0]);

                lstDtl = cCommon.ConvertDtToArrayList(DS.Tables[1]);
                lstShipmentDocument = cCommon.ConvertDtToHashTable(DS.Tables[1]);

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