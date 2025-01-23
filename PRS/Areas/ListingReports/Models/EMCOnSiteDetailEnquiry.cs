using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
namespace IP.Areas.ListingReports.Models
{
    public class EMCOnSiteDetailEnquiry
    {
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = null;
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "Search Date:")]
        public string searchDate { get; set; }
        [Display(Name = "Number of Days:")]
        public string NumberOfDays { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Box No.:")]
        public string BoxNo { get; set; }
        [Display(Name = "Pallet No.:")]
        public string palletNo { get; set; }
        [Display(Name = "PCBA Serial No.:")]
        public string PCBASerial { get; set; }
        [Display(Name = "PCBA Box No.:")]
        public string PCBABox { get; set; }
        [Display(Name = "PCBA Pallet No.:")]
        public string PCBAPallet { get; set; }
        [Display(Name = "Document No.:")]
        public string documentNo { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstEMCOnSiteDetailEnquiry { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        cDAL oDAL = new cDAL("ACTIVE");
        //new cDAL("ACTIVE");


        #endregion
        #region methods
        public bool GetList(string programId, string site, string frmDt, string toDate, string serialNo, string BoxNo, string palletNo, string PCBASerial, string PCBABox, string PCBAPallet, string documentNo, string searchByDate)
        {

            string query = string.Empty;
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();


            query = @" SELECT 
    '302Y9' AS Contract, 
	CASE ps.ProgramID
WHEN '10049' THEN 'FRANKLIN'
WHEN '10050' THEN 'IRELAND'
WHEN '10051' THEN 'THAILAND'
ELSE ''
END As Site,
    P.ID AS Site, 
    P.Name AS programName,
    t1.Value AS Source,  
    ps.PartNo, 
    OEM.Value AS OEM, 
    Interface.Value AS Interface,
    Form_Factor.Value AS Form_Factor, 
    RPM.Value AS RPM, 
    CAPACITY.Value AS Capacity,
    CASE 
        WHEN Interface.Value = 'SSD' THEN 'SSD' 
        ELSE 'HDD' 
    END AS Type, 
    ps.SerialNo AS Serial_No,
    CASE 
        WHEN SUBSTRING(BOX.Value, 2, 3) = 'PRE' THEN 'PRE ERASED' 
        ELSE 'NON ERASED' 
    END AS Erased_Type, 
    CASE 
WHEN SOH.CustomerReference IS NOT NULL THEN 'SHIPPED'
	WHEN ps.PalletBoxNo IS NOT NULL AND ps.PalletBoxNo <> '0' THEN 'PALLETIZED'
	WHEN BOX.Value IS NOT NULL THEN 'BOXED'
	WHEN PCBA.Value IS NOT NULL THEN 'DEMATED'
	ELSE cs.Description
END As Status, 
    ROHA.Value AS Incoming_Pallet,
    ps.CreateDate AS Receiced_Date, 
    Usr_Part_serial.UserName AS Received_UserName,
    PCBA.Value AS PCBA_Serial,
   CASE
WHEN PCBA_Pallet_SOH.CustomerReference IS NOT NULL THEN 'SHIPPED'
WHEN PCBA_Pallet.PalletBoxNo IS NOT NULL AND PCBA_Pallet.PalletBoxNo <> '0' THEN 'PALLETIZED'
WHEN PCBA_BOX.Value IS NOT NULL THEN 'BOXED'
WHEN PCBA.Value IS NOT NULL THEN 'DEMATED'
ELSE null
END As PCBA_STATUS, 
    PCBA.CreateDate AS Demate_Date, 
    PCBA.Username AS Demate_UserName,
    BOX.Value AS Box_ID, 
    BOX.CreateDate AS Boxed_Date, 
    BOX.Username AS Boxed_UserName,
    PCBA_BOX.Value AS PCBA_Box,
    PCBA_BOX.CreateDate AS PCBA_Boxed_Date, 
    PCBA_BOX.Username AS PCBA_Boxed_UserName,
    ps.PalletBoxNo AS Pallet_ID, 
    Usr_PartPalletBoxNo.Username AS Pallet_UserName, 
    Pallet.CreateDate AS Pallet_Date,
    SOH.CustomerReference AS Document_No,
    PCBA_Pallet.PalletBoxNo AS PCBA_Pallet_ID, 
    PCBA_Pallet_Info.CreateDate AS PCBA_Pallet_Date, 
    Usr_PartPalletBoxNo_2.Username AS PCBA_Pallet_UserName, 
    PCBA_Pallet_SOH.CustomerReference AS PCBA_Document_No,
    SOH.CreateDate AS Shipped_Date, 
    Usr_SOH.Username,
    PCBA_Pallet_SOH.CreateDate AS PCBA_Shipped_Date, 
    Usr_PCBA_Pallet_SOH.Username AS PCBA_Shipped_UserName, 
    'N' AS RMA_Status,
	CASE ca.ColName
WHEN 'Demate_Date' THEN PCBA.Username
WHEN 'Receiced_Date' THEN U_Partserial.Username
WHEN 'Boxed_Date' THEN BOX.Username
WHEN 'PCBA_Boxed_Date' THEN PCBA_BOX.Username
WHEN 'Pallet_Date' THEN U_Pallet.Username
WHEN 'PCBA_Pallet_Date' THEN Usr_PartPalletBoxNo_2.Username
WHEN 'Shipped_Date' THEN Usr_SOH.Username
WHEN 'PCBA_Shipped_Date' THEN Usr_PCBA_Pallet_SOH.Username
END As LAST_UPDATED_NAME,
ca.LastDate As ROWVERSION
FROM [pls].[PartSerial] ps

LEFT JOIN [pls].[vPartSerialAttribute] t1 ON ps.serialno = t1.serialno 
											AND t1.AttributeName = 'SOURCE'
LEFT JOIN [pls].[vPartNoAttribute] OEM ON ps.PartNo = OEM.PartNo 
												AND OEM.AttributeName = 'MANUFACTURER' 
												AND ps.ProgramID = OEM.ProgramID
LEFT JOIN [pls].[vPartNoAttribute] Interface ON ps.PartNo = Interface.PartNo
													AND Interface.AttributeName = 'INTERFACE' 
													AND ps.ProgramID = Interface.ProgramID
LEFT JOIN [pls].[vPartNoAttribute] Form_Factor ON ps.PartNo = Form_Factor.PartNo
													AND Form_Factor.AttributeName = 'FORM_FACTOR' 
													AND ps.ProgramID = Form_Factor.ProgramID
LEFT JOIN [pls].[vPartNoAttribute] RPM ON ps.PartNo = RPM.PartNo 
												AND RPM.AttributeName = 'SPEED_RPM' 
												AND ps.ProgramID = RPM.ProgramID
LEFT JOIN [pls].[vPartNoAttribute] CAPACITY ON ps.PartNo = CAPACITY.PartNo 
													AND CAPACITY.AttributeName = 'CAPACITY_GB' 
													AND ps.ProgramID = CAPACITY.ProgramID
LEFT JOIN [pls].[vROHeaderAttribute] ROHA ON ps.ROHeaderID = ROHA.ROHeaderID
											AND ROHA.AttributeName = 'INCOMING_PALLET'
LEFT JOIN [pls].[vPartSerialAttribute] PCBA ON ps.serialno = PCBA.serialno 
													AND PCBA.AttributeName = 'PCBA_SN' 
													AND ps.ProgramID = PCBA.ProgramID
LEFT JOIN [pls].[vPartSerialAttribute] BOX ON ps.serialno = BOX.serialno 
													AND BOX.AttributeName = 'CartonNo' 
													AND ps.ProgramID = BOX.ProgramID
LEFT JOIN [pls].[vPartSerialAttribute] PCBA_BOX ON PCBA.Value = PCBA_BOX.serialno 
														AND PCBA_BOX.AttributeName = 'CartonNo' 
														AND ps.ProgramID = PCBA_BOX.ProgramID
LEFT JOIN [pls].[PartPalletBoxNo] Pallet ON ps.PalletBoxNo = Pallet.CustomPalletBoxNo 
													AND ps.ProgramID = Pallet.ProgramID 
LEFT JOIN pls.SOHeader SOH ON SOH.ID = ps.SOHeaderID
LEFT JOIN [pls].[PartSerial] PCBA_Pallet ON PCBA.Value = PCBA_Pallet.serialno 
													AND ps.ProgramID = PCBA_Pallet.ProgramID
LEFT JOIN [pls].[PartPalletBoxNo] PCBA_Pallet_Info ON PCBA_Pallet.PalletBoxNo = PCBA_Pallet_Info.CustomPalletBoxNo
													AND ps.ProgramID = PCBA_Pallet_Info.ProgramID
LEFT JOIN [pls].[SOHeader] PCBA_Pallet_SOH ON PCBA_Pallet.SOHeaderID = PCBA_Pallet_SOH.ID

INNER JOIN pls.CodeStatus CS ON CS.ID = ps.StatusID 

LEFT JOIN pls.[User] Usr_PartPalletBoxNo ON Usr_PartPalletBoxNo.ID = Pallet.UserID
LEFT JOIN pls.[User] Usr_Part_serial ON Usr_Part_serial.ID = ps.UserID
LEFT JOIN pls.[User] Usr_PCBA_Pallet_SOH ON Usr_PCBA_Pallet_SOH.ID = PCBA_Pallet_SOH.UserID
LEFT JOIN pls.[User] Usr_PartPalletBoxNo_2 ON Usr_PartPalletBoxNo_2.ID = PCBA_Pallet_Info.UserID
LEFT JOIN pls.[User] Usr_SOH ON Usr_SOH.ID = SOH.UserID
LEFT JOIN pls.[User] U_Partserial ON U_Partserial.ID = PS.UserID
LEFT JOIN pls.[User] U_Pallet ON U_Pallet.ID = Pallet.UserID

CROSS APPLY 
		(Select Top 1 * FROM (VALUES 
		('Demate_Date',PCBA.CreateDate),
		('Receiced_Date',ps.CreateDate),
		('Boxed_Date',BOX.CreateDate),
		('PCBA_Boxed_Date',PCBA_BOX.CreateDate),
		('Pallet_Date',Pallet.CreateDate),
		('PCBA_Pallet_Date',PCBA_Pallet_Info.CreateDate),
		('Shipped_Date',SOH.CreateDate),
		('PCBA_Shipped_Date',PCBA_Pallet_SOH.CreateDate)
		) UpdateDate(ColName,LastDate) order by LastDate desc) ca
INNER JOIN pls.Program P ON P.ID = ps.ProgramID
WHERE ps.PartNo <> 'PCBA-DUMMY' ";

            //filterString += "> Program = '" + program + "'";
            if (sites == "PENANG")
            {
                query += "\nAnd  P.ID = '10014'";
            }
            else if (sites == "FRANKLIN")
            {
                query += "\nAnd  P.ID = '10049'";

            }
            else if (sites == "CORK")
            {
                query += "\nAnd  P.ID = '10050'";

            }
            else if (sites == "CHONBURI")
            {
                query += "\nAnd  P.ID = '10051'";

            }
            else if (sites == "PRAGUE")
            {
                query += "\nAnd  P.ID = '10056'";
            }

            //query += " AND CONVERT(Date, t.CreateDate) >= '<frmDt>' AND CONVERT(Date, t.CreateDate) <= '<toDt>' ";
            //filterString += " > Site = '" + site + "' ";
            if (searchByDate != "noDate")
            {
                filterString += " | From = '" + frmDt + "' To = '" + toDate + "' ";
                filterString += " | Search Date = '" + searchByDate + "' ";
            }

            if (!string.IsNullOrEmpty(serialNo))
            {
                query += "AND ps.SerialNo LIKE '%" + serialNo + "%' ";
                filterString += " | Serial No. = '" + serialNo + "' ";
            }
            if (!string.IsNullOrEmpty(BoxNo))
            {
                query += "AND BOX.Value LIKE '%" + BoxNo + "%' ";
                //filterString += " | Box No. = '" + BoxNo + "' ";
            }
            if (!string.IsNullOrEmpty(palletNo))
            {
                query += "AND ps.PalletBoxNo LIKE '%" + palletNo + "%' ";
                //filterString += " | Pallet No. = '" + palletNo + "' ";
            }
            if (!string.IsNullOrEmpty(PCBASerial))
            {
                query += "AND PCBA.Value LIKE '%" + PCBASerial + "%' ";
                //filterString += " | PCBA Serial No. = '" + PCBASerial + "' ";
            }
            if (!string.IsNullOrEmpty(PCBABox))
            {
                query += "AND PCBA_BOX.Value LIKE '%" + PCBABox + "%' ";
                //filterString += " | PCBA Box No. = '" + PCBABox + "' ";
            }
            if (!string.IsNullOrEmpty(PCBAPallet))
            {
                query += "AND PCBA_Pallet.PalletBoxNo LIKE '%" + PCBAPallet + "%' ";
                //filterString += " | PCBA Pallet No. = '" + PCBAPallet + "' ";
            }
            if (!string.IsNullOrEmpty(documentNo))
            {
                query += "AND SOH.CustomerReference LIKE '%" + documentNo + "%' ";
                //filterString += " | Document No. = '" + documentNo + "' ";
            }

            if (searchByDate == "Received Date")
            {
                query += " AND CONVERT(Date, ps.CreateDate) >= '<frmDt>' AND CONVERT(Date, ps.CreateDate) <= '<toDt>' ";
                query += " ORDER BY ps.CreateDate DESC ";
            }
            if (searchByDate == "Demated Date")
            {
                query += " AND CONVERT(Date, PCBA.CreateDate) >= '<frmDt>' AND CONVERT(Date, PCBA.CreateDate) <= '<toDt>' ";
                query += " ORDER BY PCBA.CreateDate DESC ";
            }
            if (searchByDate == "Box Date")
            {
                query += " AND CONVERT(Date, BOX.CreateDate) >= '<frmDt>' AND CONVERT(Date, BOX.CreateDate) <= '<toDt>' ";
                query += " ORDER BY BOX.CreateDate DESC ";
            }
            if (searchByDate == "Pallet Date")
            {
                query += " AND CONVERT(Date, Pallet.CreateDate) >= '<frmDt>' AND CONVERT(Date, Pallet.CreateDate) <= '<toDt>' ";
                query += " ORDER BY Pallet.CreateDate DESC ";
            }
            if (searchByDate == "Shipped Date")
            {
                query += " AND CONVERT(Date, SOH.CreateDate) >= '<frmDt>' AND CONVERT(Date, SOH.CreateDate) <= '<toDt>' ";
                query += " ORDER BY SOH.CreateDate DESC ";
            }
            //query += " ORDER BY t.CreateDate DESC ";

            query = query.Replace("<ProgramId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataTable dt = oDAL.GetData(query);



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("184", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstEMCOnSiteDetailEnquiry = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}