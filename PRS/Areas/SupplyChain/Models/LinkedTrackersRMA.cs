using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class LinkedTrackersRMA
    {
        
            [Display(Name = "Program:")]
            public string program { get; set; }
            [Display(Name = "Part No.:")]
            public string PartNo { get; set; }
            [Display(Name = "Disposition:")]
            public string DISPOSITION { get; set; }
            public string ReportTitle { get; set; }
            public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
            public string filterString { get; set; }
            public List<Hashtable> lstLinkedTrackersRMA { get; set; }
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
//        public DataTable GetDisposition()
//        {
//            oDAL = new cDAL("ACTIVE");

//            string query = string.Empty;
//            query = @"SELECT DISTINCT 
//    PSA3.Value AS DispositionValue,  
//    PSA3.Value AS DispositionText  
//FROM 
//    pls.PartSerial PS 
//INNER JOIN 
//    pls.PartSerialAttribute PSA3 ON ps.ID = PSA3.PartSerialID
//INNER JOIN 
//    pls.CodeAttribute CA_DISPOSITION ON PSA3.AttributeID = CA_DISPOSITION.ID 
//                                    AND CA_DISPOSITION.AttributeName = 'DISPOSITION'
//where PS.ProgramID = ";
          
//            query = query.Replace("<ProgramID>", programID);
//            DataTable dt = oDAL.GetData(query);
//            return dt;
//        }

        public bool GetList(string programID, string programName, string DISPOSITION, string partNo)
            {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
                string _PartNo = GetInValue(partNo);
               
                query = @"
                 select 
ps.ProgramID,
ps.CreateDate,
ps.SerialNo as Tracker,
psa3.Value as DISPOSITION,
ROD.TrackingNo,
ps.PartNo AS FRU,
(SELECT CASE WHEN COUNT(SerialNo) > 0 THEN 'Y' ELSE 'N' END
   FROM pls.PartSerial
   WHERE SerialNo = ROUA.Value AND ProgramID = ps.ProgramID) AS HAS_SN,
ROUA.Value as serial,
ps.ROHeaderID as PLUS_ORDER,
PSA4.Value as Customer_PO,
(select PNA.Value + ' | ' + CA.Name 
from PLS.CodeAttribute CA_TCODE
	LEFT JOIN pls.PartNoAttribute PNA
		ON pna.AttributeID = CA_TCODE.ID
		AND CA_TCODE.AttributeName = 'TCODE'
	LEFT JOIN pls.CodeAddressDetailsAttribute CADA
		ON CADA.AttributeID = CA_TCODE.ID
		AND CA_TCODE.AttributeName = 'TCODE' 
		AND CADA.Value = PNA.Value
	LEFT JOIN pls.CodeAddressDetails CAD
		ON CAD.ID = CADA.AddressDetailID
	LEFT JOIN pls.CodeAddress CA
		ON CA.ID = CAD.AddressID
where ca.ProgramID = ps.ProgramID and PNA.PartNo = ps.partNO) as [TCODE OEM NAME],
(select LocationNo
from pls.PartLocation 
where ProgramID = ps.ProgramID and ID = ps.LocationID) as LocationNo,
psa2.Value as INWARRANTY,
(select count(psh.ID)
from pls.PartSerialHistory  PSH
	INNER JOIN pls.vPartSerialAttributeHistory PSAH
		on PSH.ProgramID = PSAH.ProgramID
		and PSAH.ID = PSH.ID
		and PSAH.AttributeName = 'SerialNumber'
		and PSAH.Value = ROUA.Value) as RETCNT,
PN.Description,
--PSA3.Value as DISPOSITION,
ROUA2.Value as MFGDATE,
PSA5.Value as MANUF_PART,
ROUA3.Value as INT_SERIAL,
ROUA4.Value as COO
from pls.PartSerial PS
	INNER JOIN pls.PartSerialAttribute PSA
		on PS.ID = PSA.PartSerialID
	INNER JOIN pls.CodeAttribute CA_PROCESS_CODE 
		on PSA.AttributeID = CA_PROCESS_CODE.ID
		and CA_PROCESS_CODE.AttributeName = 'PROCESS_CODE'
	INNER JOIN pls.PartSerialAttribute PSA2
		on ps.ID = PSA2.PartSerialID
	INNER JOIN pls.CodeAttribute CA_INWARRANTY 
		on PSA2.AttributeID = CA_INWARRANTY.ID
		and CA_INWARRANTY.AttributeName = 'INWARRANTY'
	INNER JOIN pls.PartSerialAttribute PSA3
		on ps.ID = PSA3.PartSerialID
	INNER JOIN pls.CodeAttribute CA_DISPOSITION 
		on PSA3.AttributeID = CA_DISPOSITION.ID
		and CA_DISPOSITION.AttributeName = 'DISPOSITION'
	INNER JOIN pls.PartSerialAttribute PSA4
		on ps.ID = PSA4.PartSerialID
	INNER JOIN pls.CodeAttribute CA_CARTONNO 
		on PSA4.AttributeID = CA_CARTONNO.ID
		and CA_CARTONNO.AttributeName = 'CARTONNO'
	INNER JOIN pls.PartSerialAttribute PSA5
		on ps.ID = PSA5.PartSerialID
	INNER JOIN pls.CodeAttribute CA_CUST_PART_NO 
		on PSA5.AttributeID = CA_CUST_PART_NO.ID
		and CA_CUST_PART_NO.AttributeName = 'CUST_PART_NO'

	INNER JOIN pls.ROLine ROL
		on ROL.ROHeaderID = ps.ROHeaderID
		and ROL.PartNo = PS.PartNo
	INNER JOIN pls.ROUnit ROU
		on ROU.ROLineID = ROL.ID
		and ROU.SerialNo = ps.SerialNo
	INNER JOIN pls.RODockLog ROD
		on ROD.ROHeaderID = PS.ROHeaderID
		and rod.ProgramID = ps.ProgramID

	INNER JOIN pls.ROUnitAttribute ROUA
		on ROUA.ROUnitID = ROU.ID	
	INNER JOIN pls.CodeAttribute CA_SerialNumber 
		on ROUA.AttributeID = CA_SerialNumber.ID
		and CA_SerialNumber.AttributeName = 'SerialNumber'
	INNER JOIN pls.ROUnitAttribute ROUA2
		on ROUA2.ROUnitID = ROU.ID	
	INNER JOIN pls.CodeAttribute CA_MfgDATE 
		on ROUA2.AttributeID = CA_MfgDATE.ID
		and CA_MfgDATE.AttributeName = 'MfgDATE'
	INNER JOIN pls.ROUnitAttribute ROUA3
		on ROUA3.ROUnitID = ROU.ID	
	INNER JOIN pls.CodeAttribute CA_NO11S 
		on ROUA3.AttributeID = CA_NO11S.ID
		and CA_NO11S.AttributeName = 'SERIAL NO 11S'
	INNER JOIN pls.ROUnitAttribute ROUA4
		on ROUA4.ROUnitID = ROU.ID	
	INNER JOIN pls.CodeAttribute CA_COO 
		on ROUA4.AttributeID = CA_COO.ID
		and CA_COO.AttributeName = 'COO'
	INNER JOIN pls.PartNo PN
		on PN.PartNo = PS.PartNo
where PS.ProgramID = '<ProgramID>' and PS.StatusID NOT IN (18, 8, 32) and PSA.Value = 'ALLOCATED' 
	
 ";

                if (!string.IsNullOrEmpty(partNo))
                    query += "AND ps.PartNo IN (" + _PartNo + ")";

                if (!string.IsNullOrEmpty(DISPOSITION))
                query += " AND psa3.Value = '" + DISPOSITION + "'";

            query += " ORDER BY PS.CreateDate;";
            query = query.Replace("<ProgramID>", programID);

            DataTable dt = oDAL.GetData(query);

                ///////////FILTER SRINGS/////////

                if (!string.IsNullOrEmpty(programName))
                    filterString = "> Program = '" + programName + "' ";

            if (!string.IsNullOrEmpty(DISPOSITION))
                filterString += " | Disposition = '" + DISPOSITION + "' ";

            if (!string.IsNullOrEmpty(partNo))
                    filterString += " | Part No. = '" + _PartNo + "' ";

            

                //filterString += " | Rec. From = '" + fromDt + "' To = '" + toDt + "'";

                //For SQL Documentation
                cLog oLog = new cLog();
                oLog.AddSqlQuery("187", query, string.Empty, false);

                if (oDAL.HasErrors)
                {
                    ErrorMessage = oDAL.ErrMessage;
                    return false;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                        lstLinkedTrackersRMA = cCommon.ConvertDtToHashTable(dt);
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