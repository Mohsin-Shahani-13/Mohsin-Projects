using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.Logistics.Models
{
    public class Docklog
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstDocklog { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            if (ProgramName == "DELL")
            {

                query = @"SELECT  DL.ProgramID,
        DL.ROHeaderID,
        ROH.ID,
        P.Name As Program,
        ROH.CustomerReference,
        ROHA.Value AS OrderType,
        DL.TrackingNo,
		DLA1.Value as ReconDelNo,
        DL.CarrierName,
		DL.Type,
		FORMAT(convert(datetime,  DLA.Value,109),'yyyy.MM.dd hh:mm tt') AS DeliveredDate,
		FORMAT(convert(datetime,  DLA.Value,109),'yyyy/MM/dd hh:mm tt') AS DeliveredOn,
        DL.Qty,
        CC.Description,
        U.Username,
		FORMAT(DL.CreateDate, 'yyyy.MM.dd HH:mm') AS CreateDate,
        FORMAT(DL.CreateDate, 'yyyy/MM/dd HH:mm') AS CreateOn,
        FORMAT(DL.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityDate,
        FORMAT(DL.LastActivityDate, 'yyyy/MM/dd HH:mm') AS LastActivityOn
        ,CD.ID AS CrossDockID,
 CD.FromSite,
CD.ToSite,
 CS.Description as CrossDockStatus
FROM pls.RODockLog DL
INNER JOIN pls.ROHeader ROH ON ROH.ID = DL.ROHeaderID 
INNER JOIN pls.Program P ON P.ID= DL.ProgramID
INNER JOIN pls.CodeCondition CC ON CC.ID = DL.ConditionID
INNER JOIN pls.CodeOrderType COT ON COT.ID = ROH.OrderTypeID
INNER JOIN pls.[User] U ON U.ID = DL.UserID
LEFT OUTER JOIN pls.RODockLogAttribute DLA ON DLA.RODockLogID = DL.ID AND DLA.AttributeID = 158
LEFT  OUTER JOIN pls.RODockLogAttribute DLA1 ON DLA1.RODockLogID = DL.ID AND DLA1.AttributeID = 826
LEFT  JOIN pls.ROHeaderAttribute ROHA ON   ROHA.ROHeaderID =ROH.id AND ROHA.AttributeID = 6
LEFT JOIN pls.CrossDock CD ON CD.RODockLogID = DL.ID AND CD.ProgramID = DL.ProgramID 
LEFT JOIN pls.CodeStatus CS ON CS.ID = CD.StatusID
WHERE CONVERT(Date, DL.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, DL.LastActivityDate) <= '<toDt>' 
AND DL.StatusID <> 3  ";

                if (sites == "BYDGOSZCZ")
                {
                    query += "\nAND DL.ProgramID=10064 ";
                }
                else if (sites == "JUAREZ")
                {
                    query += "\nAND DL.ProgramID=10061 ";

                }
                else if (sites == "MEXICALI")
                {
                    query += "\nAND DL.ProgramID=10055 ";

                }
                else if (sites == "MEMPHIS")
                {
                    query += "\nAND DL.ProgramID = 10053 ";
                }
            }
           else if (ProgramName == "BOSE")
            {

                query = @"SELECT  DL.ProgramID,
        DL.ROHeaderID,
        ROH.ID,
        P.Name As Program,
        ROH.CustomerReference,
        ROHA.Value AS OrderType,
        ROHAA.Value AS ProcessType,
        DL.TrackingNo,
		DLA1.Value as ReconDelNo,
        DL.CarrierName,
		DL.Type,
		FORMAT(convert(datetime,  DLA.Value,109),'yyyy.MM.dd hh:mm tt') AS DeliveredDate,
		FORMAT(convert(datetime,  DLA.Value,109),'yyyy/MM/dd hh:mm tt') AS DeliveredOn,
        DL.Qty,
        CC.Description,
        U.Username,
		FORMAT(DL.CreateDate, 'yyyy.MM.dd HH:mm') AS CreateDate,
        FORMAT(DL.CreateDate, 'yyyy/MM/dd HH:mm') AS CreateOn,
        FORMAT(DL.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityDate,
        FORMAT(DL.LastActivityDate, 'yyyy/MM/dd HH:mm') AS LastActivityOn
        ,CD.ID AS CrossDockID,
 CD.FromSite,
CD.ToSite,
 CS.Description as CrossDockStatus
FROM pls.RODockLog DL
INNER JOIN pls.ROHeader ROH ON ROH.ID = DL.ROHeaderID 
INNER JOIN pls.Program P ON P.ID= DL.ProgramID
INNER JOIN pls.CodeCondition CC ON CC.ID = DL.ConditionID
INNER JOIN pls.CodeOrderType COT ON COT.ID = ROH.OrderTypeID
INNER JOIN pls.[User] U ON U.ID = DL.UserID
LEFT OUTER JOIN pls.RODockLogAttribute DLA ON DLA.RODockLogID = DL.ID AND DLA.AttributeID = 158
LEFT  OUTER JOIN pls.RODockLogAttribute DLA1 ON DLA1.RODockLogID = DL.ID AND DLA1.AttributeID = 826
LEFT  JOIN pls.ROHeaderAttribute ROHA ON   ROHA.ROHeaderID =ROH.id AND ROHA.AttributeID = 6
LEFT  JOIN pls.ROHeaderAttribute ROHAA ON ROHAA.ROHeaderID =ROH.id AND ROHAA.AttributeID = 986 AND ROHAA.Value IN('REMAN', 'REPAIR')
LEFT JOIN pls.CrossDock CD ON CD.RODockLogID = DL.ID AND CD.ProgramID = DL.ProgramID 
LEFT JOIN pls.CodeStatus CS ON CS.ID = CD.StatusID
WHERE CONVERT(Date, DL.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, DL.LastActivityDate) <= '<toDt>' 
AND DL.StatusID <> 3  ";

                if (sites == "BYDGOSZCZ")
                {
                    query += "\nAND DL.ProgramID=10058 ";
                }
               
                else if (sites == "MEXICALI")
                {
                    query += "\nAND DL.ProgramID=10059 ";

                }
               
            }
            else
            {

                query = @"
SELECT  DL.ProgramID,
        DL.ROHeaderID,
        ROH.ID,
        P.Name As Program,
        ROH.CustomerReference,
        ROHA.Value AS OrderType,
        DL.TrackingNo,
        DL.CarrierName,
		DL.Type,
		FORMAT(convert(datetime,  DLA.Value,109),'yyyy.MM.dd hh:mm tt') AS DeliveredDate,
		FORMAT(convert(datetime,  DLA.Value,109),'yyyy/MM/dd hh:mm tt') AS DeliveredOn,
        DL.Qty,
        CC.Description,
        U.Username,
		FORMAT(DL.CreateDate, 'yyyy.MM.dd HH:mm') AS CreateDate,
        FORMAT(DL.CreateDate, 'yyyy/MM/dd HH:mm') AS CreateOn,
        FORMAT(DL.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityDate,
        FORMAT(DL.LastActivityDate, 'yyyy/MM/dd HH:mm') AS LastActivityOn
        @additionalCol
FROM pls.RODockLog DL
INNER JOIN pls.ROHeader ROH ON ROH.ID = DL.ROHeaderID 
INNER JOIN pls.Program P ON P.ID= DL.ProgramID
INNER JOIN pls.CodeCondition CC ON CC.ID = DL.ConditionID
INNER JOIN pls.CodeOrderType COT ON COT.ID = ROH.OrderTypeID
INNER JOIN pls.[User] U ON U.ID = DL.UserID
LEFT OUTER JOIN pls.RODockLogAttribute DLA ON DLA.RODockLogID = DL.ID AND DLA.AttributeID = 158
LEFT  JOIN pls.ROHeaderAttribute ROHA ON   ROHA.ROHeaderID =ROH.id AND ROHA.AttributeID = 6
@additionalJoins
WHERE CONVERT(Date, DL.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, DL.LastActivityDate) <= '<toDt>' 
AND DL.StatusID <> 3  
";


                if (programId != "0")
                {
                    query += "AND P.ID = '" + programId + "' ";
                }
                else
                {
                    query += "AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
                }
            }

            if (conType == "TEST" || conType == "TRAN")
            {
                query = query.Replace("@additionalCol", ",CD.ID AS CrossDockID,\n CD.FromSite,\nCD.ToSite,\n CS.Description as CrossDockStatus");
                query = query.Replace
                ("@additionalJoins", "\nLEFT JOIN pls.CrossDock CD ON CD.RODockLogID = DL.ID AND CD.ProgramID = DL.ProgramID \nLEFT JOIN pls.CodeStatus CS ON CS.ID = CD.StatusID");
            }
            else
            {
                query = query.Replace("@additionalCol", "");
                query = query.Replace("@additionalJoins", "");
            }

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            query += "ORDER BY DL.LastActivityDate DESC";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("009", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDocklog = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        #endregion
    }
}