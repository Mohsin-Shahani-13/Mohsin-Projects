using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class MaterialFulfillment
    {
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Reference No.:")]
        public string reference { get; set; }
        [Display(Name = "Order Type:")]
        public string orderType { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public List<Hashtable> lstMaterialFulfillment { get; set; }
        public List<Hashtable> lstReferenceDetail { get; set; }
        public string ErrorMessage { get; set; }


        cDAL oDAL;
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
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
        

        public bool GetList(string programID, string programName, string orderType, string reference)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            string _reference = GetInValue(reference);

            query = @"
                 -- Create a temp table to store the initial data
CREATE TABLE #TempData (
    ProgramID INT,
    CreateDate DATETIME,
    Reference NVARCHAR(100),
    PartNo NVARCHAR(100),
    QtyToShip INT,
    OrderType NVARCHAR(100),
    Fulfilled INT,
    LastActivity DATETIME,
    Priority INT
)

-- Insert data into the temp table
INSERT INTO #TempData (ProgramID, CreateDate, Reference, PartNo, QtyToShip, OrderType, Fulfilled, LastActivity, Priority)
SELECT 
    SOH.ProgramID,
    SOH.CreateDate, 
    SOH.CustomerReference AS Reference, 
    SOL.PartNo, 
    SOL.QtyToShip,
    CASE SOHA2.Value
        WHEN 'OEM' THEN 'OEM'
        WHEN 'VEN MIS-SORT' THEN 'VEN'
        WHEN 'VEN RE-STOCK' THEN 'VEN'
        WHEN 'VEN SCRAP' THEN 'VEN'
        WHEN 'SCRAP ADJ' THEN 'ADJUSTMENT'
        WHEN 'RESORT ADJ' THEN 'ADJUSTMENT'
        ELSE '' 
    END AS OrderType,
    CASE
        WHEN SOHA2.Value IN ('OEM', 'SCRAP ADJ', 'RESORT ADJ') THEN
            (SELECT COUNT(pt.ID)
             FROM pls.PartSerial ps
             INNER JOIN pls.PartSerialAttribute psa ON ps.id = psa.PartSerialID
             AND psa.AttributeID IN (SELECT id FROM pls.CodeAttribute WHERE AttributeName IN ('VEN_NUMBER','CARTONNO','RMA'))
             INNER JOIN pls.PartTransaction pt ON ps.ProgramID = pt.ProgramID    
             AND ps.PartNo = pt.PartNo
             AND ps.SerialNo = pt.SerialNo
             WHERE ps.ProgramID = soh.ProgramID AND pt.ToLocation = 'SHIPPING.R2.0.0.0' AND pt.PartTransactionID = 23 --WH-MOVEPART
             AND psa.Value = SOH.CustomerReference AND pt.PartNo = SOL.PartNo)
        WHEN SOHA2.Value IN ('VEN MIS-SORT', 'VEN RE-STOCK', 'VEN SCRAP') THEN
            (SELECT COUNT(pt.ID)
             FROM pls.PartSerial ps
             INNER JOIN pls.PartSerialAttribute psa ON ps.id = psa.PartSerialID
             AND psa.AttributeID IN (SELECT id FROM pls.CodeAttribute WHERE AttributeName IN ('VEN_NUMBER','CARTONNO','RMA'))
             INNER JOIN pls.PartTransaction pt ON ps.ProgramID = pt.ProgramID    
             AND ps.PartNo = pt.PartNo
             AND ps.SerialNo = pt.SerialNo
             WHERE ps.ProgramID = soh.ProgramID AND pt.ToLocation = 'PACK-VEN.R2.0.0.0' AND pt.PartTransactionID = 23 --WH-MOVEPART
             AND psa.Value = SOH.CustomerReference AND pt.PartNo = SOL.PartNo)
        ELSE 0
    END AS Fulfilled,
    CASE
        WHEN SOHA2.Value IN ('OEM', 'SCRAP ADJ', 'RESORT ADJ') THEN
            (SELECT MAX(pt.CreateDate)
             FROM pls.PartSerial ps
             INNER JOIN pls.PartSerialAttribute psa ON ps.id = psa.PartSerialID
             AND psa.AttributeID IN (SELECT id FROM pls.CodeAttribute WHERE AttributeName IN ('VEN_NUMBER','CARTONNO','RMA'))
             INNER JOIN pls.PartTransaction pt ON ps.ProgramID = pt.ProgramID    
             AND ps.PartNo = pt.PartNo
             AND ps.SerialNo = pt.SerialNo
             WHERE ps.ProgramID = soh.ProgramID AND pt.ToLocation = 'SHIPPING.R2.0.0.0' AND pt.PartTransactionID = 23 --WH-MOVEPART
             AND psa.Value = SOH.CustomerReference AND pt.PartNo = SOL.PartNo)
        WHEN SOHA2.Value IN ('VEN MIS-SORT', 'VEN RE-STOCK', 'VEN SCRAP') THEN
            (SELECT MAX(pt.CreateDate)
             FROM pls.PartSerial ps
             INNER JOIN pls.PartSerialAttribute psa ON ps.id = psa.PartSerialID
             AND psa.AttributeID IN (SELECT id FROM pls.CodeAttribute WHERE AttributeName IN ('VEN_NUMBER','CARTONNO','RMA'))
             INNER JOIN pls.PartTransaction pt ON ps.ProgramID = pt.ProgramID    
             AND ps.PartNo = pt.PartNo
             AND ps.SerialNo = pt.SerialNo
             WHERE ps.ProgramID = soh.ProgramID AND pt.ToLocation = 'PACK-VEN.R2.0.0.0' AND pt.PartTransactionID = 23 --WH-MOVEPART
             AND psa.Value = SOH.CustomerReference AND pt.PartNo = SOL.PartNo)
        ELSE 0
    END AS LastActivity,
    CASE SOHA2.Value
        WHEN 'OEM' THEN 1
        WHEN 'VEN MIS-SORT' THEN 2
        WHEN 'VEN RE-STOCK' THEN 2
        WHEN 'VEN SCRAP' THEN 2
        WHEN 'SCRAP ADJ' THEN 3
        WHEN 'RESORT ADJ' THEN 3
        ELSE '' 
    END AS Priority
FROM 
    pls.SOHeader SOH
INNER JOIN 
    pls.SOLine SOL ON SOH.ID = SOL.SOHeaderID
INNER JOIN 
    pls.SOHeaderAttribute SOHA ON SOH.ID = SOHA.SOHeaderID
    AND SOHA.AttributeID = (SELECT id FROM pls.CodeAttribute WHERE AttributeName = 'PROCESS_CODE')
    AND ISNULL(SOHA.Value, 'X') IN ('SCRAPADJNEW','RESORTADJNEW','PARTIALADJUSTFULFILL','PARTIALRMAFULFILL','OEMNEW','VENNEW','PARTIALVENFULFILL')
INNER JOIN 
    pls.SOHeaderAttribute SOHA2 ON SOH.ID = SOHA2.SOHeaderID
    AND SOHA2.AttributeID = (SELECT id FROM pls.CodeAttribute WHERE AttributeName = 'CUSTORDERTYPE')
    AND SOHA2.Value IN ('OEM','SCRAP ADJ','RESORT ADJ','VEN MIS-SORT','VEN RE-STOCK','VEN SCRAP')
WHERE 
    ProgramID = '<ProgramID>' 
    AND SOH.StatusID IN ('7','13','31','12')
    <Reference>

-- Query the temp table with additional conditions
SELECT *
FROM #TempData
<OrderType>
 ";

            //if (!string.IsNullOrEmpty(partNo))
            //    query += "AND ps.PartNo IN (" + _PartNo + ")";

            //if (!string.IsNullOrEmpty(DISPOSITION))
            //    query += " AND psa3.Value = '" + DISPOSITION + "'";

            query = query.Replace("<ProgramID>", programID);

            if (!string.IsNullOrEmpty(reference)) //--AND Reference = '<Reference>';
                query = query.Replace("<Reference>", "AND SOH.CustomerReference IN (" + _reference + ")"); //query += " AND Reference IN (" + reference + ")"; // query = query.Replace("<Reference>", "AND SOH.CustomerReference = " + reference + ")"; 
            else 
                query = query.Replace("<Reference>", "");

            if (!orderType.Equals("All")) //if (!string.IsNullOrEmpty(orderType)) 
                query = query.Replace("<OrderType>", "WHERE\n    ORDERTYPE = '" + orderType + "'");  //query += " WHERE\n    ORDERTYPE = " + orderType + "";  //query = query.Replace("<OrderType>", orderType);
            else 
                query = query.Replace("<OrderType>", "");


            query += " Order by CreateDate Desc \n DROP TABLE #TempData;";

            DataTable dt = oDAL.GetData(query);

            ///////////FILTER SRINGS/////////

            if (!string.IsNullOrEmpty(programName))
                filterString = "> Program = '" + programName + "' ";

            if (!orderType.Equals("All"))
                filterString += " | Order Type = '" + orderType + "'";

            //if (!string.IsNullOrEmpty(reference))
            //    filterString += " | Reference No. = '" + _reference + "' ";

            //filterString += " | Rec. From = '" + fromDt + "' To = '" + toDt + "'";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("190", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMaterialFulfillment = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        public bool Detail(string reference, string programID)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            
            query = @"
            select SOH.ProgramID, ps.SerialNo, ps.PartNo, pl.LocationNo, psa.Value as Reference
from pls.SOHeader SOH
	INNER JOIN pls.SOLine SOL
		on SOH.ID = SOL.SOHeaderID
	INNER JOIN pls.SOHeaderAttribute SOHA
		on SOH.ID = SOHA.SOHeaderID
		and SOHA.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'PROCESS_CODE')
		and SOHA.Value in ('SCRAPADJNEW','RESORTADJNEW','PARTIALADJUSTFULFILL','PARTIALRMAFULFILL','OEMNEW')
	INNER JOIN pls.SOHeaderAttribute SOHA2
		on SOH.ID = SOHA2.SOHeaderID
		and SOHA2.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'CUSTORDERTYPE')
		and SOHA2.Value in ('OEM','SCRAP ADJ','RESORT ADJ')
	INNER JOIN pls.PartSerial PS
		on ps.ProgramID = soh.ProgramID
		and ps.PartNo = SOL.PartNo
	INNER JOIN pls.PartSerialAttribute PSA
		on psa.PartSerialID = ps.ID
		and psa.AttributeID in (select id from pls.CodeAttribute where AttributeName in ('CARTONNO','RMA'))
		and psa.Value = SOH.CustomerReference
	LEFT JOIN pls.PartSerialAttribute PSA2
		on PSA2.PartSerialID = ps.ID
		and PSA2.AttributeID in (select id from pls.CodeAttribute where AttributeName in ('VEN_NUMBER'))
    LEFT JOIN pls.PartSerialAttribute PSA3
        on PSA3.PartSerialID = ps.ID
        and PSA3.AttributeID in (select id from pls.CodeAttribute where AttributeName in ('PROCESS_CODE'))
	INNER JOIN pls.PartLocation PL
		on pl.ID = ps.LocationID
where SOH.ProgramID = '<ProgramID>' and SOH.StatusID IN ('7','13','31','12') and PSA2.Value is null AND PSA3.Value != 'VENPROCESS' and psa.Value = '<reference>'
UNION
select SOH.ProgramID, ps.SerialNo, ps.PartNo, pl.LocationNo, psa.Value as Reference
from pls.SOHeader SOH
	INNER JOIN pls.SOLine SOL
		on SOH.ID = SOL.SOHeaderID
	INNER JOIN pls.SOHeaderAttribute SOHA
		on SOH.ID = SOHA.SOHeaderID
		and SOHA.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'PROCESS_CODE')
		and SOHA.Value in ('VENNEW','PARTIALVENFULFILL')
	INNER JOIN pls.SOHeaderAttribute SOHA2
		on SOH.ID = SOHA2.SOHeaderID
		and SOHA2.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'CUSTORDERTYPE')
		and SOHA2.Value in ('VEN MIS-SORT','VEN RE-STOCK','VEN SCRAP')
	INNER JOIN pls.PartSerial PS
		on ps.ProgramID = soh.ProgramID
		and ps.PartNo = SOL.PartNo
	INNER JOIN pls.PartSerialAttribute PSA
		on psa.PartSerialID = ps.ID
		and psa.AttributeID in (select id from pls.CodeAttribute where AttributeName in ('VEN_NUMBER'))
		and psa.Value = SOH.CustomerReference
	INNER JOIN pls.PartLocation PL
		on pl.ID = ps.LocationID
where SOH.ProgramID = '<ProgramID>' and SOH.StatusID IN ('7','13','31','12') and psa.Value = '<reference>';";

            query = query.Replace("<ProgramID>", programID);
            query = query.Replace("<reference>", reference);




            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("190-1", query, "Detail Material Fulfillment", false);

            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstReferenceDetail = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
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