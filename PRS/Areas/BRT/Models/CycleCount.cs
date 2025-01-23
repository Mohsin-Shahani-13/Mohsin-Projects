using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Reflection;

namespace IP.Areas.BRT.Models
{
    public class CycleCount
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "Warehouse:")]
        public string Warehouse { get; set; }
        [Display(Name = "Iteration:")]
        public string iteration { get; set; }
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
        public List<Hashtable> lstCycleCount { get; set; }
        public List<Hashtable> lstGetSerials { get; set; }
        public List<ArrayList> lstDetail { get; set; }
        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string partNo, string programId, string ProgramName, string Warehouse, string iteration, string frmDt, string toDt)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"

SELECT CCTL.ProgramID
      , P.Name AS Program
      , CCTL.PartNo
      , PN.Description
      , PL.Warehouse
      , PL.LocationNo
      , CCA.Iteration
      , CCA.AvailableInventoryQty
      , CCA.CycleCountQty
      , (CCA.CycleCountQty - CCA.AvailableInventoryQty) AS Delta
      , ROUND(CCA.CycleCountQty * 100 / NULLIF(CCA.AvailableInventoryQty,0),1) AS 'IRA'
      , CASE WHEN (CCA.CycleCountQty - CCA.AvailableInventoryQty) = 0 THEN 'N' ELSE 'Y' END AS ERROR
      , CCA.ID
      , U.Username
      , CCA.LastActivityDate
From pls.CycleCountTaskList CCTL
INNER JOIN pls.CycleCountActivity CCA ON CCTL.ID = CCA.TaskID
LEFT JOIN pls.PartNo PN ON PN.PartNo = CCTL.PartNo
INNER JOIN pls.Program P ON P.ID = CCTL.ProgramID
INNER JOIN pls.PartLocation PL ON PL.Bin = CCTL.Bin AND PL.ProgramID = P.ID
INNER JOIN pls.[User] U ON CCA.UserID = U.ID
WHERE CONVERT(Date,  CCA.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, CCA.LastActivityDate) <= '<toDt>' AND PL.statusId = 4
 ";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            if (programId != "0")
                query += " AND P.ID = '" + programId + "' ";
            else
                query += " AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";

            if (!string.IsNullOrEmpty(partNo))
                query += "AND CCTL.PartNo LIKE '%" + partNo + "%' ";

            if (!string.IsNullOrEmpty(Warehouse))
                query += "AND PL.Warehouse LIKE '%" + Warehouse + "%' ";

            if (iteration != "All")
            {
                if (iteration == "Above 2")
                    query += "AND CCA.Iteration > 2 ";
                else
                    query += "AND CCA.Iteration = " + iteration;
            }
            // query += " AND CCA.LastActivityDate Between '" +frmDt+ "' AND '" +toDt+ "'";

            query += " Order by CCA.LastActivityDate DESC";

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";
            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. Like '" + partNo + "' ";

            if (!string.IsNullOrEmpty(Warehouse))
                filterString += " | Warehouse Like '" + Warehouse + "' ";

            if (!string.IsNullOrEmpty(iteration))
                filterString += "| Iteration = '" + iteration + "' ";
            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            //if (conType == "PROD")
            //{
            //    query = query.Replace("PalletBoxNo", "PalletNo");
            //    query = query.Replace("LotNo", "CartonNo");
            //}
            DataTable dt = oDAL.GetData(query);



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("107", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstCycleCount = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        public bool GetDetail(string programId, string program, string frmDt, string toDate, string no, string calender)
        {
           
            string query = string.Empty;
            query = @"

SELECT CCTL.ProgramID
      , P.Name AS Program
      , CCTL.PartNo
      , PN.Description
      , PL.Warehouse
      , PL.LocationNo
      , CCA.Iteration
      , CCA.AvailableInventoryQty
      , CCA.CycleCountQty
      , (CCA.CycleCountQty - CCA.AvailableInventoryQty) AS Delta
      , ROUND(CCA.CycleCountQty * 100 / NULLIF(CCA.AvailableInventoryQty,0),1) AS 'IRA'
      , CASE WHEN (CCA.CycleCountQty - CCA.AvailableInventoryQty) = 0 THEN 'N' ELSE 'Y' END AS ERROR
      , CCA.ID
      , U.Username
      , <calender>
From pls.CycleCountTaskList CCTL
INNER JOIN pls.CycleCountActivity CCA ON CCTL.ID = CCA.TaskID
LEFT JOIN pls.PartNo PN ON PN.PartNo = CCTL.PartNo
INNER JOIN pls.Program P ON P.ID = CCTL.ProgramID
INNER JOIN pls.PartLocation PL ON PL.Bin = CCTL.Bin AND PL.ProgramID = P.ID
INNER JOIN pls.[User] U ON CCA.UserID = U.ID
WHERE P.ID = '<programId>' AND PL.statusId = 4 
 ";
           

            if (calender == "Days")
            {
                query += "AND CONVERT(Date, CCA.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, CCA.LastActivityDate) <= '<frmDt>' ";
                query = query.Replace("<calender>", "CCA.LastActivityDate");
            }

            if (calender == "Weeks")
            {
                query += "AND convert(Date,CCA.LastActivityDate) >= '<frmDt>' AND convert(Date,CCA.LastActivityDate) <= DATEADD(DAY, 6, '<frmDt>')  ";
                query = query.Replace("<calender>", "CCA.LastActivityDate");

            }

            if (calender == "Months")
            {
                query += "AND FORMAT(convert(DATETIME, CCA.LastActivityDate), 'yyyy.MM') = '<frmDt>' ";
                query = query.Replace("<calender>", "FORMAT(convert(DATETIME, CCA.LastActivityDate), 'yyyy.MM')");

            }

            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            DataTable dt = oDAL.GetData(query);



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("107", query, "Detail", false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDetail = cCommon.ConvertDtToArrayList(dt);
                return true;

            }
        }
        #endregion
    }
}