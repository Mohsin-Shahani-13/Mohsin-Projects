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
    public class ManifestReport
    {
            cDAL oDAL = new cDAL("ACTIVE");
            #region Fields

            [Display(Name = "Customer Ref.:")]
            public string custRef { get; set; }
            [Display(Name = "Manifest:")]
		public string manifest { get; set; }
		public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

            public string filterString { get; set; }
            public string ErrorMessage { get; set; }
            public List<Hashtable> lstManifest { get; set; }
            public List<Hashtable> lstDetail { get; set; }
            #endregion

            #region Methods 
            public bool GetList(string programId, string ProgramName, string custRef, string manifest)
            {
			cDAL oDAL = new cDAL("ACTIVE");
			string query = string.Empty;

			query = @"
select distinct A.CustomerReference, A.PO, SUM(A.QTY) AS QTY, A.PartNo, A.DESCRIPTION, A.COUNTRY_ORIGIN,  
CASE WHEN row_number() OVER (PARTITION BY A.PALLET order BY A.MANIFEST asc) > 1 THEN 'SAME' ELSE '1P' END AS BULTOS,
(CAST(DENSE_RANK() OVER (PARTITION BY A.MANIFEST ORDER BY A.MANIFEST, A.PALLET) AS varchar(4)) + ' OF ' + CAST(a.totalBULTOS AS varchar(2))) as TOTAL_BULTOS, SUM(A.QTY) AS CANT_BULTO,
A.TTLWeight, A.TTL_DIM, A.ADDRESS, 
A.LastActivityDate, A.MANIFEST, A.PALLET
from(
select
	PSA.Value AS PO, SOU.QtyReserved AS QTY, SOL.PartNo, 
	isnull(ROUA2.Value,pna.Value) as [DESCRIPTION], ROUA.Value AS COUNTRY_ORIGIN, ISNULL(PPBA.Value, SOU.FromPalletBoxNo) AS PALLET,
	SOH.CustomerReference, 
	isnull(PPBA2.Value, PPBA3.Value) as TTLWeight, 
	isnull(PPBA6.Value,PPBA9.Value) + 'X' + isnull(PPBA5.Value,PPBA8.Value) + 'X' + isnull(PPBA4.Value,PPBA7.Value) as TTL_DIM, 
	(CAD.Name + ' ' + CAD.Address1 + ' ' + CAD.Address2 + ' ' + CAD.Zip + ' ' + CAD.City + ' ' + CAD.State + ' ' + CAD.Country) as [ADDRESS],
	FORMAT(SOU.LastActivityDate, 'yyyy.MM.dd HH:mm')  as LastActivityDate, 
	SOSI.ShipmentGroupNo as MANIFEST,
	(select count(distinct ISNULL(PPBA.Value, frompalletboxno)) 
		from pls.SOUnit SOU
		INNER JOIN PLS.PartPalletBoxNo PPB
			ON SOU.FromPalletBoxNo = PPB.CustomPalletBoxNo
			AND PPB.ProgramID = SOH.ProgramID
		LEFT JOIN pls.PartPalletBoxNoAttribute PPBA
			ON PPB.ID = PPBA.PartPalletBoxNoID
			AND PPBA.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'OldPalletBoxNo')
		where SOU.SOShipmentInfoID in (select id from pls.SOShipmentInfo where ShipmentGroupNo = SOSI.ShipmentGroupNo) 
		and SOU.FromPalletBoxNo != '0' ) as totalBULTOS
from pls.SOHeader SOH
	inner join pls.SOLine SOL
		on SOH.ID = SOL.SOHeaderID
	inner join pls.SOUnit SOU
		on sol.ID = SOU.SOLineID
		and SOU.FromPalletBoxNo != '0' 
	INNER JOIN PLS.PartPalletBoxNo PPB
		ON SOU.FromPalletBoxNo = PPB.CustomPalletBoxNo
		AND PPB.ProgramID = SOH.ProgramID
	LEFT JOIN pls.PartPalletBoxNoAttribute PPBA
		ON PPB.ID = PPBA.PartPalletBoxNoID
		AND PPBA.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'OldPalletBoxNo')
	LEFT JOIN pls.PartPalletBoxNoAttribute PPBA2
		ON PPB.ID = PPBA2.PartPalletBoxNoID
		AND PPBA2.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'Weight')
	LEFT JOIN pls.PartPalletBoxNoAttribute PPBA4
		ON PPB.ID = PPBA4.PartPalletBoxNoID
		AND PPBA4.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'HEIGHT')
	LEFT JOIN pls.PartPalletBoxNoAttribute PPBA5
		ON PPB.ID = PPBA5.PartPalletBoxNoID
		AND PPBA5.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'LENGTH')
	LEFT JOIN pls.PartPalletBoxNoAttribute PPBA6
		ON PPB.ID = PPBA6.PartPalletBoxNoID
		AND PPBA6.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'WIDTH')
	LEFT JOIN pls.PartPalletBoxNo PPB2
		ON PPB2.CustomPalletBoxNo = PPBA.Value
	LEFT JOIN pls.PartPalletBoxNoAttribute PPBA3
		ON PPB2.ID = PPBA3.PartPalletBoxNoID
		AND PPBA3.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'Weight')
	LEFT JOIN pls.PartPalletBoxNoAttribute PPBA7
		ON PPB2.ID = PPBA7.PartPalletBoxNoID
		AND PPBA7.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'HEIGHT')
	LEFT JOIN pls.PartPalletBoxNoAttribute PPBA8
		ON PPB2.ID = PPBA8.PartPalletBoxNoID
		AND PPBA8.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'LENGTH')
	LEFT JOIN pls.PartPalletBoxNoAttribute PPBA9
		ON PPB2.ID = PPBA9.PartPalletBoxNoID
		AND PPBA9.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'WIDTH')
	inner join pls.PartSerial PS
		on ps.SOHeaderID = soh.ID
		and ps.PartNo = sol.PartNo
		and ps.SerialNo = SOU.SerialNo
		and ps.ConfigurationID = sol.ConfigurationID
		AND PS.LocationID NOT IN (SELECT ID FROM PLS.PartLocation WHERE LocationNo = 'CRERETURNS.R2.0.0.0')
	LEFT JOIN pls.PartSerialAttribute PSA
		on ps.ID = PSA.PartSerialID
		and PSA.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'CARTONNO')	
	inner join pls.ROLine ROL
		on ps.ROHeaderID = ROL.ROHeaderID
		and (ROL.PartNo = PS.PartNo
			 OR ROL.PartNo  = (select PT.SourcePartNo from pls.PartTransaction PT where PT.ProgramID = ps.ProgramID and PT.PartNo = PS.PartNo and PT.SerialNo = ps.SerialNo and PT.SourcePartNo is not null))
		and rol.ConfigurationID = ps.ConfigurationID
	inner join pls.ROUnit ROU
		on ROL.ID = ROU.ROLineID
		and ROU.SerialNo = ps.SerialNo
	LEFT JOIN pls.ROUnitAttribute ROUA
		on ROUA.ROUnitID = ROU.ID	
		and ROUA.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'COO')
	LEFT JOIN pls.ROUnitAttribute ROUA2
		on ROUA2.ROUnitID = ROU.ID	
		and ROUA2.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'STANDARD_NAME')
	INNER JOIN pls.PartNo PN
		on ps.PartNo = pn.PartNo
	INNER JOIN pls.PartNoAttribute PNA
		on PN.PartNo = PNA.PartNo
		and PNA.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'STANDARD_NAME')
	LEFT JOIN pls.SOShipmentInfo SOSI
		ON SOH.ID = SOSI.SOHeaderID
		AND SOU.SOShipmentInfoID = SOSI.ID
	INNER JOIN PLS.CodeAddress CA
		on SOH.AddressID = CA.ID
		and soh.ProgramID = CA.ProgramID
	INNER JOIN pls.CodeAddressDetails CAD
		on CA.ID = CAD.AddressID
		and cad.AddressType = 'ShipTo'
where SOH.StatusID in (18,31,12,13)  ";

			

                    if (programId != "0")
                    {
                        query += "AND soh.ProgramID = '" + programId + "' ";
                    }
                    else
                    {
                        query += "AND soh.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                    }

                    if (!string.IsNullOrEmpty(custRef))
                        query += "AND SOH.CustomerReference LIKE '%" + custRef + "%' ";

			if (!string.IsNullOrEmpty(manifest))
				query += "AND SOSI.ShipmentGroupNo = '" + manifest + "' ";


			query += @"
) A
GROUP BY A.PO, A.PartNo, A.DESCRIPTION, A.COUNTRY_ORIGIN, A.PALLET, A.CustomerReference, A.TTLWeight, A.TTL_DIM, A.ADDRESS,
A.LastActivityDate,A.MANIFEST,A.totalBULTOS
order by A.LastActivityDate, A.MANIFEST, A.PALLET
 ";

			


                if (!string.IsNullOrEmpty(ProgramName))
                    filterString = " Program = '" + ProgramName + "' ";

              
                if (!string.IsNullOrEmpty(custRef))
                    filterString += "| Customer Ref. Like '" + custRef + "' ";

            if (!string.IsNullOrEmpty(manifest))
                filterString += "| Manifest = '" + manifest + "' ";


                DataTable dt = oDAL.GetData(query);

                //For SQL Documentation
                cLog oLog = new cLog();
                oLog.AddSqlQuery("192", query, "", false);

                if (oDAL.HasErrors)
                {
                    ErrorMessage = oDAL.ErrMessage;
                    return false;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                        lstManifest = cCommon.ConvertDtToHashTable(dt);
                    return true;

                }

            }
            public bool GetDetail(string programId, string manifest)
            {
                oDAL = new cDAL("ACTIVE");

                string query = string.Empty;
			query = @"  
select
SOH.ProgramID,
	PS.SerialNo as TRACKER, SOL.PartNo, 
	 ROUA.Value AS COUNTRY_ORIGIN, 
	SOH.CustomerReference, 
	CAD.Name as [OEM],
	SOSI.ShipmentGroupNo as MANIFEST
from pls.SOHeader SOH
	inner join pls.SOLine SOL
		on SOH.ID = SOL.SOHeaderID
	inner join pls.SOUnit SOU
		on sol.ID = SOU.SOLineID
		and SOU.FromPalletBoxNo != '0' 
	inner join pls.PartSerial PS
		on ps.SOHeaderID = soh.ID
		and ps.PartNo = sol.PartNo
		and ps.SerialNo = SOU.SerialNo
		and ps.ConfigurationID = sol.ConfigurationID
		AND PS.LocationID NOT IN (SELECT ID FROM PLS.PartLocation WHERE LocationNo = 'CRERETURNS.R2.0.0.0')
	inner join pls.ROLine ROL
		on ps.ROHeaderID = ROL.ROHeaderID
		and (ROL.PartNo = PS.PartNo
			 OR ROL.PartNo  = (select PT.SourcePartNo from pls.PartTransaction PT where PT.ProgramID = ps.ProgramID and PT.PartNo = PS.PartNo and PT.SerialNo = ps.SerialNo and PT.SourcePartNo is not null))
		and rol.ConfigurationID = ps.ConfigurationID
	inner join pls.ROUnit ROU
		on ROL.ID = ROU.ROLineID
		and ROU.SerialNo = ps.SerialNo
	LEFT JOIN pls.ROUnitAttribute ROUA
		on ROUA.ROUnitID = ROU.ID	
		and ROUA.AttributeID = (select id from pls.CodeAttribute where AttributeName = 'COO')
	LEFT JOIN pls.SOShipmentInfo SOSI
		ON SOH.ID = SOSI.SOHeaderID
		AND SOU.SOShipmentInfoID = SOSI.ID
	INNER JOIN PLS.CodeAddress CA
		on SOH.AddressID = CA.ID
		and soh.ProgramID = CA.ProgramID
	INNER JOIN pls.CodeAddressDetails CAD
		on CA.ID = CAD.AddressID
		and cad.AddressType = 'ShipTo'
where SOH.StatusID in (18,31,12,13)  and SOH.ProgramID = '<programId>' ";
			query += "AND SOSI.ShipmentGroupNo = " + manifest + " ";

			query += @" order by SOU.LastActivityDate, SOSI.ShipmentGroupNo ";

			query = query.Replace("<programId>", programId);


                DataTable dt = oDAL.GetData(query);

			//For SQL Documentation
			cLog oLog = new cLog();
			oLog.AddSqlQuery("192-1", query, "Detail", false);

			if (!oDAL.HasErrors)
                {
                    if (dt.Rows.Count > 0)
                    {
                        lstDetail = cCommon.ConvertDtToHashTable(dt);
                    }
                    return true;
                }
                return false;
            }

            #endregion
        }
    }