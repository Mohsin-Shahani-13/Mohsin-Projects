using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace IP.Areas.SupplyChain.Models
{
    public class PartsBackLog
    {

        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }

        [Display(Name = "Part No.:")]

        public string PartNo { get; set; }
        [Display(Name = "Description:")]
        public string Description { get; set; }
        [Display(Name = "Available Qty:")]
        public string AvailableQty { get; set; }


        [Display(Name = "Serial No.:")]
        public string SerialNo { get; set; }

        [Display(Name = "Location No.:")]
        public string LocationNo { get; set; }
        [Display(Name = "Pallet Box No.:")]
        public string PalletBoxNo { get; set; }
        public object total { get; set; }

        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string Repalevel { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<ArrayList> lstDataColumn { get; set; }
        public List<Hashtable> lstData { get; set; }
        public List<ArrayList> lstDtl { get; set; }
        
       
        public List<Hashtable> lstPartsBackLog { get; set; }
        public List<Hashtable> lstSerial { get; set; }
        #endregion

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

        public bool GetList(string PalletBoxNo, string LocationNo, string PartNo, string programId, string ProgramName)
        {
            
            string query = string.Empty;
            string _PalletBoxNo = GetInValue(PalletBoxNo);
            string _LocationNo = GetInValue(LocationNo);
            string _PartNo = GetInValue(PartNo);
            query = @"
SELECT  PS.ProgramID AS ID
        , P.Name 
        , PS.PartNo
        , CC.Description
        , count(PS.serialNo) Qty
FROM Pls.PartSerial PS
INNER JOIN Pls.CodeConfiguration CC ON CC.ID = PS.ConfigurationID
INNER JOIN Pls.PartLocation PL ON PL.ID = PS.LocationID
INNER JOIN Pls.CodeStatus CS ON CS.ID = PS.StatusID
INNER JOIN pls.Program P ON P.ID = PS.ProgramID
                            AND CS.Description IN ('RECEIVED','REPAIR') 
";
            //query += "WHERE  PS.PalletBoxNo IN ("+ _PalletBoxNo + " ) and PL.LocationNo IN (" + _LocationNo + ") AND PS.PartNo IN (" + _PartNo + " )";

            //query = query.Replace("<frmDt>", fromDt);
            //query = query.Replace("<toDt>", toDt);


            if (programId != "0" && programId != null)
            {
                query += " AND PS.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += " AND PS.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (!string.IsNullOrEmpty(PalletBoxNo))
                query += "AND PS.PalletBoxNo IN (" + _PalletBoxNo + ")";

            if (!string.IsNullOrEmpty(LocationNo))
                query += "AND  PL.LocationNo IN (" + _LocationNo + ")";

            if (!string.IsNullOrEmpty(PartNo))
                query += "AND PS.PartNo IN (" + _PartNo + ")";

            query += @" GROUP BY   PS.PartNo
                                 , CC.Description,PS.ProgramID
                                 , P.Name   ";
           

            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(PalletBoxNo))
                filterString += "| Pallet Box No. = '" + PalletBoxNo + "' ";

            if (!string.IsNullOrEmpty(LocationNo))
                filterString += "| Location No. = '" + LocationNo + "' ";

            if (!string.IsNullOrEmpty(PartNo))
                filterString += "| Part No. = '" + PartNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("138", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstData = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }


        public bool GetDetail(string PalletBoxNo, string LocationNo, string PartNo, string programId, string ProgramName)
        {

            string query = string.Empty;
            string _PalletBoxNo = GetInValue(PalletBoxNo);
            string _LocationNo = GetInValue(LocationNo);
            string _PartNo = GetInValue(PartNo);

            query = @"
SELECT    PS.ProgramID AS ID
        , P.Name
        , PS.SerialNo
        , PS.PartNo
        , PL.LocationNo
        , CC.Description
        , PS.PalletBoxNo
        , CS.Description Code_DESC
 FROM Pls.PartSerial PS
 INNER JOIN Pls.CodeConfiguration CC ON CC.ID = PS.ConfigurationID
 INNER JOIN Pls.PartLocation PL ON PL.ID = PS.LocationID
 INNER JOIN pls.Program P ON P.ID = PS.ProgramID
 INNER JOIN Pls.CodeStatus CS ON CS.ID = PS.StatusID 
                            AND CS.Description IN ('RECEIVED','REPAIR')
   
";
           
            //query = query.Replace("<frmDt>", fromDt);
            //query = query.Replace("<toDt>", toDt);

            if (programId != "0" && programId != null)
            {
                query += "AND P.ID = '" + programId + "' ";
            }
            else
            {
                query += "AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

             if (!string.IsNullOrEmpty(PalletBoxNo))
                query += "AND PS.PalletBoxNo IN (" + _PalletBoxNo + ")";

            if (!string.IsNullOrEmpty(LocationNo))
                query += "AND  PL.LocationNo IN (" + _LocationNo + ")";

            if (!string.IsNullOrEmpty(PartNo))
                query += "AND PS.PartNo IN (" + _PartNo + ")";

           
            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(PalletBoxNo))
                filterString += "| Pallet Box No. = '" + PalletBoxNo + "' ";

            if (!string.IsNullOrEmpty(LocationNo))
                filterString += "| Location No. = '" + LocationNo + "' ";

            if (!string.IsNullOrEmpty(PartNo))
                filterString += "| Part No. = '" + PartNo + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("138", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstData = cCommon.ConvertDtToHashTable(dt);
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