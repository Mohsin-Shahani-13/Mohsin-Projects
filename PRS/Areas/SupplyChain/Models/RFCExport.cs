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
    public class RFCExport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string ProgramName { get; set; }
        [Display(Name = "Program:")]
        public string ProgramID { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public bool isAllDate { get; set; }
        [Display(Name = "Customer Ref.:")]
        public string custRef { get; set; }
        [Display(Name = "SN_Status:")]
        public string SNStatus { get; set; }
        [Display(Name = "Remark:")]
        public string Remark { get; set; }
        [Display(Name = "Model_Number:")]
        public string Modelnumber { get; set; }
        //[Display(Name = "COO:")]
        //public string Coo { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public string OrderType { get; set; }
        public List<Hashtable> lstRFCExport { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
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
             public DataTable GetIDBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select distinct top 10 ID
                             FROM pls.WOHeader ";
            DataTable dtId = oDAL.GetData(query);
            return dtId;
        }
        #endregion
        public bool GetList(string frmDt, string toDt, bool isAllDate, string ProgramId, string ProgramName, string custRef)
        {
            //if (isAllDate == true)
            //    frmDt = "";
            //oDAL = new CDAL("ACTIVE", "ST");
            string _custRef = GetInValue(custRef);
            string query = string.Empty;
            query = @"SELECT  
                  wsh.ID
                 ,P.Name AS Program
                 , ROH.ProgramID
                 , ROU.SerialNo
                 ,  (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
                     FROM pls.partserial PS
                     WHERE PS.SerialNo = ROU.SerialNo AND PS.ProgramID = ROH.ProgramID ) HAS_SN
                 , CASE WHEN (WSAT.Value = '' OR WSAT.Value IS NULL) OR WOH.StatusID = 3 THEN ROUA.Value ELSE WSAT.Value END AS SN_Status
                 ,ROUA.Value
                 , NULL AS Remark
                 , CASE WHEN  ROUAT.Value= 'NA' THEN '' ELSE ROUAT.Value END AS Model_Number
                 , ROL.PartNo
from pls.ROUnit ROU
INNER JOIN Pls.ROLine ROL ON ROL.ID = ROU.ROLineID
INNER JOIN Pls.ROHeader ROH ON ROH.ID = ROL.ROHeaderID
INNER JOIN Pls.Program  P ON P.ID = ROH.ProgramID
LEFT JOIN pls.PartSerial PS on PS.ROHeaderID = ROH.ID AND PS.SerialNo = ROU.SerialNo AND PS.ProgramID =ROH.ProgramID AND ps.PartNo = ROL.PartNo
LEFT JOIN pls.WOHeader WOH ON WOH.ID = PS.WOHeaderID
LEFT JOIN pls.WOStationHistory WSH ON WSH.WOHeaderID = WOH.ID 
                                      AND WOH.WorkStationID = WSH.WorkStationID 
                                      AND WSH.Iteration = (SELECT MAX(Iteration)
                                                           FROM pls.WOStationHistory 
                                                           WHERE WOHeaderID = WOH.ID
                                                                 AND WorkStationID = WOH.WorkStationID)
LEFT JOIN pls.CodeAttribute CA ON CA.AttributeName = 'Action'
LEFT JOIN pls.WOStationAttribute WSAT ON WSAT.WOStationHistoryID = WSH.ID AND WSAT.AttributeID = CA.ID
LEFT JOIN pls.CodeAttribute CAT ON CAT.AttributeName = 'VMI_ACTION'
LEFT JOIN pls.ROUnitAttribute ROUA ON ROUA.ROUnitID = ROU.ID AND ROUA.AttributeID = CAT.ID
LEFT JOIN pls.CodeAttribute CATT ON CATT.AttributeName = 'MCODE'
LEFT JOIN pls.ROUnitAttribute ROUAT ON ROUAT.ROUnitID = ROU.ID AND ROUAT.AttributeID = CATT.ID
";

            //query += "AND ROH.ID  IN ('<Program>') ";
            if (ProgramId != "0")
            {
                query += "WHERE ROH.ProgramID = '" + ProgramId + "' ";
            }
            else
            {
                query += "WHERE ROH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (isAllDate != true)
            {
                query += " AND CONVERT(Date, ROH.lastactivitydate) >= '<frmDt>' AND CONVERT(Date, ROH.lastactivitydate) <= '<toDt>'";
            }

            if (!string.IsNullOrEmpty(custRef))
                query += " AND ROH.CustomerReference IN (" + _custRef +")";

            //query += "GROUP BY ROH.ProgramID,P.Name,serialno";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
          //query = query.Replace("<custRef>", custRef);
            //query = query.Replace("<OrderType>", OrderType);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (isAllDate != true)
            {
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            }

            if (isAllDate == true)
                filterString += " | Date Range = All ";

            if (!string.IsNullOrEmpty(custRef))
                filterString += " | Customer Ref. = '" + _custRef + "' ";

           

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("137", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRFCExport = cCommon.ConvertDtToHashTable(dt);
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