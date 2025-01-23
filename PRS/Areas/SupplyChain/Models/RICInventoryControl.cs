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
    public class RICInventoryControl
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Location:")]
        public string Location { get; set; }
        [Display(Name = "Customer PO:")]
        public string CustomerPO { get; set; }
        [Display(Name = "RMA:")]
        public string RMA { get; set; }
        [Display(Name = "Standard Name:")]
        public string StandardName { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }

        public List<Hashtable> lstRICInventoryControl { get; set; }

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
                      WHERE SITE = '<site>'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetList(string programId, string programName, string location, string customerPO, string RMA, string standardName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            //string programName = HttpContext.Current.Session["Program"].ToString();

            string query = string.Empty;

            query = @" SELECT 
A.ID,
A.LocationNo,
A.LastActivityDate,
A.FRU,
A.Tracker,
A.DISPOSITION,
A.Current_status,
A.SERIAL,
A.SERIAL_11S,
A.SERIAL_8S,
A.SERIAL_52S,
A.SERIAL_1,
A.SERIAL_2,
A.SERIAL_3,
A.PLUS_RO,
A.RMA,
A.CreateDate,
A.Description,
A.MANUF_PART,
A.Customer_PO,
A.reference,
A.COO,
A.WMS_LOCATION,
A.TAPEINSIDE,
A.FAILURE_MODE,
A.Username,
A.RETCNT,
A.MfgDATE,
A.Pictures,
A.STANDARD_NAME,
 string_agg(A.EXTRA_FIELDS, ', ') EXTRA_FIELDS,
A.HAS_SN
FROM(
select 
P.ID,
pl.LocationNo,
ps.LastActivityDate,
ps.PartNo AS FRU,
ps.SerialNo as Tracker,
PSA4.Value AS DISPOSITION,
PSA.Value as Current_status,
ROUA.Value AS SERIAL,
ROUA2.Value AS SERIAL_11S,
ROUA3.Value AS SERIAL_8S,
ROUA4.Value AS SERIAL_52S,
ROUA5.Value AS SERIAL_1,
ROUA6.Value AS SERIAL_2,
ROUA7.Value AS SERIAL_3,
ROL.ROHeaderID AS PLUS_RO,
PSA3.Value as RMA,
ps.CreateDate,
pn.Description,
PSA5.Value as MANUF_PART,
PSA2.Value as Customer_PO,
ROH.CustomerReference as reference,
ROUA8.Value as COO,
ROUA9.Value AS WMS_LOCATION,
ROUA10.Value as TAPEINSIDE,
ROUA11.Value AS FAILURE_MODE,
U.Username,
(select count(psh.ID)
from pls.PartSerialHistory  PSH
	INNER JOIN pls.vPartSerialAttributeHistory PSAH
		on PSH.ProgramID = PSAH.ProgramID
		and PSAH.ID = PSH.ID
		and PSAH.AttributeName = 'SerialNumber'
		and PSAH.Value = ROUA.Value
		and PSH.ConfigurationID = 2) as RETCNT,
ROUA12.Value AS MfgDATE,
(select (CASE WHEN count(ID) > 0 THEN 'YES' ELSE 'NO' END)
from pls.[Image] I
where ProgramID = <programId> and SerialNo = ps.SerialNo and PartNo = ps.PartNo) AS Pictures,
isnull(ROUA13.Value,pna.Value) as STANDARD_NAME,
(GENERIC.Field + ': ' + GENERIC.VALUE) as EXTRA_FIELDS,
(SELECT CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
   FROM pls.PartSerial
   WHERE SerialNo = ps.SerialNo AND ProgramID = ps.ProgramID) AS HAS_SN --for validating hyperlink
from pls.PartSerial PS
	LEFT JOIN pls.PartSerialAttribute PSA
		on PS.ID = PSA.PartSerialID
		and PSA.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'PROCESS_CODE')
	LEFT JOIN pls.PartSerialAttribute PSA2
		on ps.ID = PSA2.PartSerialID
		and PSA2.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'CARTONNO')	
	LEFT JOIN pls.PartSerialAttribute PSA3
		on ps.ID = PSA3.PartSerialID
		and PSA3.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'RMA')	
	LEFT JOIN pls.PartSerialAttribute PSA4
		on ps.ID = PSA4.PartSerialID
		and PSA4.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'DISPOSITION')
	LEFT JOIN pls.PartSerialAttribute PSA5
		on PSA5.PartSerialID = ps.ID
		and PSA5.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'CUST_PART_NO')
	LEFT JOIN pls.PartSerialAttribute PSA6
		on PSA6.PartSerialID = ps.ID
		and PSA6.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'GENERIC')
	OUTER APPLY OPENJSON(PSA6.[Value])
		WITH (Field varchar(100), [Value] varchar(100)) GENERIC
	INNER JOIN pls.ROLine ROL
		on ROL.ROHeaderID = ps.ROHeaderID
		and ROL.PartNo = PS.PartNo
	INNER JOIN pls.ROHeader ROH	
		on ROH.id = ROL.ROHeaderID
		and ROH.ProgramID = ps.ProgramID
	INNER JOIN pls.ROUnit ROU
		on ROU.ROLineID = ROL.ID
		and ROU.SerialNo = ps.SerialNo
	INNER JOIN pls.ROUnitAttribute ROUA
		on ROUA.ROUnitID = ROU.ID	
		and ROUA.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'SerialNumber')
	LEFT JOIN pls.ROUnitAttribute ROUA2
		on ROUA2.ROUnitID = ROU.ID	
		and ROUA2.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'SERIAL NO 11S')
	LEFT JOIN pls.ROUnitAttribute ROUA3
		on ROUA3.ROUnitID = ROU.ID	
		and ROUA3.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'SERIAL NO 8S')
	LEFT JOIN pls.ROUnitAttribute ROUA4
		on ROUA4.ROUnitID = ROU.ID	
		and ROUA4.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'SERIAL NO 52S')
	LEFT JOIN pls.ROUnitAttribute ROUA5
		on ROUA5.ROUnitID = ROU.ID	
		and ROUA5.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'SERIAL NO 1')
	LEFT JOIN pls.ROUnitAttribute ROUA6
		on ROUA6.ROUnitID = ROU.ID	
		and ROUA6.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'SERIAL NO 2')
	LEFT JOIN pls.ROUnitAttribute ROUA7
		on ROUA7.ROUnitID = ROU.ID	
		and ROUA7.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'SERIAL NO 3')
	LEFT JOIN pls.ROUnitAttribute ROUA8
		on ROUA8.ROUnitID = ROU.ID	
		and ROUA8.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'COO')
	LEFT JOIN pls.ROUnitAttribute ROUA9
		on ROUA9.ROUnitID = ROU.ID	
		and ROUA9.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'WMS LOCATION')
	LEFT JOIN pls.ROUnitAttribute ROUA10
		on ROUA10.ROUnitID = ROU.ID	
		and ROUA10.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'TAPE INSIDE')
	LEFT JOIN pls.ROUnitAttribute ROUA11
		on ROUA11.ROUnitID = ROU.ID	
		and ROUA11.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'FAILURE MODE')
	LEFT JOIN pls.ROUnitAttribute ROUA12
		on ROUA12.ROUnitID = ROU.ID	
		and ROUA12.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'MfgDATE')
	LEFT JOIN pls.ROUnitAttribute ROUA13
		on ROUA13.ROUnitID = ROU.ID	
		and ROUA13.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'STANDARD_NAME')
	INNER JOIN pls.PartLocation	PL
		on ps.LocationID = pl.ID
		and pl.ProgramID = ps.ProgramID	
	INNER JOIN pls.PartNo PN
		on ps.PartNo = pn.PartNo
	LEFT JOIN pls.PartNoAttribute PNA
		on PN.PartNo = PNA.PartNo
		and PNA.AttributeID = ((select id from pls.CodeAttribute where AttributeName = 'STANDARD_NAME'))
	INNER JOIN pls.[User] U
		on ps.UserID = U.ID
	INNER JOIN pls.Program P ON P.ID = PS.ProgramID
where PS.ProgramID = <programId> and PS.StatusID NOT IN (18, 8, 32)
";

            //query += " AND CONVERT(Date, PSA4.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, PSA4.LastActivityDate) <= '<toDt>'";
            if (!string.IsNullOrEmpty(location))
                query += "AND Pl.LocationNo LIKE '%" + location + "%' ";

            if (!string.IsNullOrEmpty(customerPO))
                query += "AND PSA2.Value LIKE '%" + customerPO + "%' ";

            if (!string.IsNullOrEmpty(RMA))
                query += "AND PSA3.Value LIKE '%" + RMA + "%' ";

			if (!string.IsNullOrEmpty(standardName))
				query += "AND pna.Value LIKE '%" + standardName + "%' ";

			query = query.Replace("<programId>", programId);
            //query = query.Replace("<frmDt>", fDate);
            //query = query.Replace("<toDt>", tDate);

            query += @") A

GROUP BY
A.ID,
A.LocationNo,
A.LastActivityDate,
A.FRU,
A.Tracker,
A.DISPOSITION,
A.Current_status,
A.SERIAL,
A.SERIAL_11S,
A.SERIAL_8S,
A.SERIAL_52S,
A.SERIAL_1,
A.SERIAL_2,
A.SERIAL_3,
A.PLUS_RO,
A.RMA,
A.CreateDate,
A.Description,
A.MANUF_PART,
A.Customer_PO,
A.reference,
A.COO,
A.WMS_LOCATION,
A.TAPEINSIDE,
A.FAILURE_MODE,
A.Username,
A.RETCNT,
A.MfgDATE,
A.Pictures,
A.STANDARD_NAME,
A.HAS_SN; ";
            DataTable dt = oDAL.GetData(query);

            //if (!string.IsNullOrEmpty(programName))
            filterString += "> Program = '" + programName + "' ";

            if (!string.IsNullOrEmpty(location))
                filterString += " | Location Like '" + location + "' ";
            if (!string.IsNullOrEmpty(customerPO))
                filterString += " | Customer PO Like '" + customerPO + "' ";

            if (!string.IsNullOrEmpty(RMA))
                filterString += " | RMA Like '" + RMA + "' ";

			if (!string.IsNullOrEmpty(standardName))
				filterString += " | Standard Name Like '" + standardName + "' ";

			//For SQL Documentation
			cLog oLog = new cLog();
            oLog.AddSqlQuery("189", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRICInventoryControl = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        #endregion
    }
}