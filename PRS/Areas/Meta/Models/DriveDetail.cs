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
    public class DriveDetail
    {
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Receive From:")]
        public string _RecfromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string RecfromDt { get { return _RecfromDt; } set { _RecfromDt = value; } }
        [Display(Name = "Receive To:")]
        public string _RectoDt = DateTime.Now.ToString(Format.DateOnly);
        public string RectoDt { get { return _RectoDt; } set { _RectoDt = value; } }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "RO Reference:")]
        public string referNo { get; set; }
        [Display(Name = "RFC No.:")]
        public string RFCNo { get; set; }
        [Display(Name = "Box Id:")]
        public string boxId { get; set; }
        [Display(Name = "Current Location:")]
        public string currInventoryLocation { get; set; }
        [Display(Name = "Dock Log Receiving Date From:")]
        public string _dLogfromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string dLogfromDt { get { return _dLogfromDt; } set { _dLogfromDt = value; } }
        [Display(Name = "Dock Log Receiving Date To:")]
        public string _dLogToDt = DateTime.Now.ToString(Format.DateOnly);
        public string dLogtoDt { get { return _dLogToDt; } set { _dLogToDt = value; } }
        public bool RecAllDate { get; set; }
        public bool dLogAllDate { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public List<Hashtable> lstDriveDetail { get; set; }
        public string ErrorMessage { get; set; }

        cDAL oDAL = new cDAL("ACTIVE");
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName 
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>' AND NAME ='TOSHIBA' 
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);

            return dt;
        }

        public bool GetList(string recFrmDt, string recToDt, string dockLogFrmDt, string dockLogToDt, bool recAllDate, bool dLogAllDate, string serialNo, string ProgramID, string ProgramName, string referenceNo, string RfcNo, string boxID, string currentInventoryLocation)
        {

            string query = string.Empty;
            string _SerialNo = GetInValue(serialNo);
            string _ReferenceNo = GetInValue(referenceNo);
            string _RfcNo = GetInValue(RfcNo);
            string _box_id = GetInValue(boxID);
            string _currentInventoryLocation = GetInValue(currentInventoryLocation);

            query = @"
        Select
ROH.ID AS ID,
ROH.ProgramID,
P.Name AS ProgramName,
ROU.SerialNo,
ROL.PartNo,
ROUATTT.Value AS Part_Change,
ROH.CustomerReference AS custRef,
CASE WHEN CS.Description = 'SHIPPED' THEN  '' ELSE PL.LocationNo END AS  LocationNo,
Case when ROU.SerialNo != (case when PS.SerialNo is null then PSH.SerialNo else PS.SerialNo end) THEN 'RO' ELSE CS.Description END AS Unit_Status,
ROUA.Value AS FD_Code,
CASE WHEN SUBSTRING(ROH.CustomerReference, 1,3) = 'RMC' AND ROUATTT.Value IS NOT NULL THEN ROUATTT.Value ELSE PN.ModelNo END AS Model,
ROUAT.Value AS MCode,
ROUATT.Value AS MNA,
ROUATTR.Value AS ODM,
ROUATTRI.Value AS Credit_To,
ROUATTRIB.Value AS Drive_Type,
ROUATTRIBU.Value AS StartDate,
Format(CAST(ROH.CreateDate AS datetime2), 'yyyy.MM.dd HH:mm')AS RO_Create_On,
Format(CAST(RODL.CreateDate AS datetime2), 'yyyy.MM.dd HH:mm') AS DockLog_ReceiveDate,
Format(CAST((CASE WHEN PS.CreateDate IS NULL THEN PSH.CreateDate ELSE PS.CreateDate END) AS datetime2), 'yyyy.MM.dd HH:mm') AS Received_Date,
U.username,
CASE WHEN PS.ID IS NULL THEN PSH.ID ELSE PS.ID END AS PS_ID,
Case when ROUAVMI.Value IS NULL THEN '' WHEN ROUAVMI.Value = 'PASS'  THEN 'PASS' ELSE 'FAIL' END AS VMI_Result,
Case when ROUAVMI.Value <> 'PASS'  THEN ROUAVMI.Value END AS VMI_FailReason,
ROUATTRIBUT.Value AS VMI_Action,
ROUATTRIBUTE.Value AS VMI_Res,
ROUACOO.Value AS COO,
ROUAShipto.Value AS Ship_To,
ROUAFreight.Value AS Freight,
ROUARFC.Value AS RFC_No,
CASE WHEN PS.PalletBoxNo IS NULL THEN PSH.PalletBoxNo ELSE PS.PalletBoxNo END  AS Pallet_Box_ID,
SOH.CustomerReference AS SO_Reference_No
from pls.ROUnit ROU
INNER JOIN Pls.ROLine ROL ON ROL.ID = ROU.ROLineID
INNER JOIN Pls.ROHeader ROH ON ROH.ID = ROL.ROHeaderID
LEFT JOIN pls.CodeAttribute CA ON CA.AttributeName = 'FD_CODE'
LEFT JOIN pls.ROUnitAttribute ROUA ON ROUA.ROUnitID = ROU.ID 
AND ROUA.AttributeID = CA.ID
LEFT JOIN pls.PartSerial PS ON PS.SerialNo = ROU.SerialNo 
				AND PS.PartNo = ROL.PartNo 
				AND PS.ProgramID = roh.ProgramID 
				AND PS.ROHeaderID = ROH.ID
LEFT JOIN pls.PartSerialHistory PSH on PSH.SerialNo = ROU.SerialNo
					 AND PSH.PartNo = ROL.PartNo
					 AND PSH.ProgramID = ROH.ProgramID
					 AND PSH.ROHeaderID = ROH.ID
LEFT JOIN pls.CodeStatus CS ON CS.ID = CASE WHEN PS.StatusID IS NULL THEN PSH.StatusID ELSE PS.StatusID END
LEFT JOIN pls.PartLocation PL ON PL.id = CASE WHEN PS.LocationID IS NULL THEN PSH.LocationID ELSE PS.LocationID END
INNER JOIN pls.PartNo PN ON  PN.PartNo = ROL.PartNo
LEFT JOIN pls.SOHeader SOH ON SOH.ID = CASE WHEN PS.SOHeaderID IS NULL THEN PSH.SOHeaderID ELSE PS.SOHeaderID END
LEFT JOIN Pls.RODockLog RODL ON RODL.ROHeaderID = ROH.ID
INNER JOIN Pls.Program P ON P.ID = ROH.ProgramID
LEFT JOIN pls.[User] U ON U.ID = CASE WHEN Ps.UserID IS NULL THEN PSH.UserID ELSE Ps.UserID END
LEFT JOIN pls.CodeAttribute CAT ON  CAT.AttributeName = 'MCODE'
LEFT JOIN pls.ROUnitAttribute ROUAT ON CAT.ID = ROUAT.AttributeID 
AND ROUAT.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CATT ON  CATT.AttributeName = 'MNA'
LEFT JOIN pls.ROUnitAttribute ROUATT ON CATT.ID = ROUATT.AttributeID 
AND ROUATT.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CATTR ON  CATTR.AttributeName = 'ODM'
LEFT JOIN pls.ROUnitAttribute ROUATTR ON CATTR.ID = ROUATTR.AttributeID AND ROUATTR.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CATTRI ON  CATTRI.AttributeName = 'CREDIT_TO'
LEFT JOIN pls.ROUnitAttribute ROUATTRI ON CATTRI.ID = ROUATTRI.AttributeID AND ROUATTRI.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CATTRIB ON CATTRIB.AttributeName = 'DRIVE_TYPE'
LEFT JOIN pls.ROUnitAttribute ROUATTRIB ON CATTRIB.ID = ROUATTRIB.AttributeID AND ROUATTRIB.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CATTRIBU ON  CATTRIBU.AttributeName = 'START_DATE'
LEFT JOIN pls.ROUnitAttribute ROUATTRIBU ON CATTRIBU.ID = ROUATTRIBU.AttributeID AND ROUATTRIBU.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CATTRIBUT ON  CATTRIBUT.AttributeName = 'VMI_ACTION'
LEFT JOIN pls.ROUnitAttribute ROUATTRIBUT ON CATTRIBUT.ID = ROUATTRIBUT.AttributeID AND ROUATTRIBUT.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CATTRIBUTE ON CATTRIBUTE.AttributeName = 'VMI_RES'
LEFT JOIN pls.ROUnitAttribute ROUATTRIBUTE ON CATTRIBUTE.ID = ROUATTRIBUTE.AttributeID AND ROUATTRIBUTE.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CACOO ON CACOO.AttributeName = 'COO'
LEFT JOIN pls.ROUnitAttribute ROUACOO ON CACOO.ID = ROUACOO.AttributeID AND ROUACOO.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CAShipto ON CAShipto.AttributeName = 'SHIP_TO'
LEFT JOIN pls.ROUnitAttribute ROUAShipto ON CAShipto.ID = ROUAShipto.AttributeID AND ROUAShipto.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CAFreight ON CAFreight.AttributeName = 'Freight'
LEFT JOIN pls.ROUnitAttribute ROUAFreight ON CAFreight.ID = ROUAFreight.AttributeID AND ROUAFreight.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CARFC ON CARFC.AttributeName = 'RFC_NO'
LEFT JOIN pls.ROUnitAttribute ROUARFC ON CARFC.ID = ROUARFC.AttributeID AND ROUARFC.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CAVMI ON CAVMI.AttributeName = 'VMI_RESULT'
LEFT JOIN pls.ROUnitAttribute ROUAVMI ON CAVMI.ID = ROUAVMI.AttributeID AND ROUAVMI.ROUnitID = ROU.ID
LEFT JOIN pls.CodeAttribute CDAT ON  CDAT.AttributeName = 'PART_CHANGE'
LEFT JOIN pls.ROUnitAttribute ROUATTT ON CDAT.ID = ROUATTT.AttributeID AND ROUATTT.ROUnitID = ROU.ID

WHERE ROU.StatusID <> 3

 ";
            if (ProgramID != "0")
            {
                query += " AND ROH.ProgramID = '" + ProgramID + "' ";
            }
            else
            {
                query += " AND ROH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            if (!string.IsNullOrEmpty(serialNo))
                query += " AND ROU.SerialNo IN (" +_SerialNo + ") ";

            if (!string.IsNullOrEmpty(referenceNo))
                query += " AND ROH.CustomerReference IN (" +_ReferenceNo + ")";

            if (!string.IsNullOrEmpty(RfcNo))
                query += " AND ROUARFC.Value IN (" +_RfcNo + ")";

            if (!string.IsNullOrEmpty(boxID))
                query += " AND PS.PalletBoxNo IN (" +_box_id + ") ";

            if (!string.IsNullOrEmpty(currentInventoryLocation))
                query += " AND PL.LocationNo IN (" +_currentInventoryLocation + ") ";

            if (recAllDate != true)
            {
                //query += "AND CONVERT(Date, PS.CreateDate) >= '<recFrmDt>' AND CONVERT(Date, PS.CreateDate) <= '<recToDt>'";
                query += @"AND (
        (PS.CreateDate IS NULL AND CONVERT(Date, PSH.CreateDate) >= '<recFrmDt>' AND CONVERT(Date, PSH.CreateDate) <= '<recToDt>')
        OR
        (PS.CreateDate IS NOT NULL AND CONVERT(Date, PS.CreateDate) >= '<recFrmDt>' AND CONVERT(Date, PS.CreateDate) <= '<recToDt>')
    ) ";


            }
            if (dLogAllDate != true)
            {
                query += " AND CONVERT(Date, RODL.CreateDate) >= '<dockLogFrmDt>' AND CONVERT(Date, RODL.CreateDate) <= '<dockLogToDt>'";
            }

            query = query.Replace("<recFrmDt>", recFrmDt);
            query = query.Replace("<recToDt>", recToDt);
            query = query.Replace("<dockLogFrmDt>", dockLogFrmDt);
            query = query.Replace("<dockLogToDt>", dockLogToDt);

            //query += "ORDER BY WOSH.lastactivitydate";

            DataTable dt = oDAL.GetData(query);

            ///////////FILTER SRINGS/////////

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "'";

            if (!string.IsNullOrEmpty(serialNo))
                filterString += " | Serial No. = '" + _SerialNo + "'";

            //if (!string.IsNullOrEmpty(referenceNo))
            //    filterString += " | RO Reference = '" + _ReferenceNo + "'";

            if (!string.IsNullOrEmpty(RfcNo))
                filterString += " | RFC No. = '" + _RfcNo + "'";

            if (!string.IsNullOrEmpty(boxID))
                filterString += " | Box Id Like '" + boxID + "'";

            if (!string.IsNullOrEmpty(currentInventoryLocation))
                filterString += " | Current Location Like '" + currentInventoryLocation + "'";

            //filterString += " | Rec. From = '" + recFrmDt + "' To = '" + recToDt + "' ";

            //filterString += " | Dock Log Rec. From = '" + dockLogFrmDt + "' To = '" + dockLogToDt + "'";





            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("139", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDriveDetail = cCommon.ConvertDtToHashTable(dt);
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