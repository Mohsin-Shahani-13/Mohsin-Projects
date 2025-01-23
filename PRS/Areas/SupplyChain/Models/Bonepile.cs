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
    public class Bonepile
    {
        cDAL oDAL = new cDAL("ACTIVE");

        [Display(Name = "Program:")]
        public string ProgramName { get; set; }
        [Display(Name = "Program:")]
        public string ProgramID { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        public List<Hashtable> lstBonepile { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }

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
        public bool GetList(string ProgramId, string ProgramName, string partNo)
        {
            //if (isAllDate == true)
            //    frmDt = "";
            //oDAL = new CDAL("ACTIVE", "ST");
            string _custRef = GetInValue(partNo);
            string query = string.Empty;
            query = @"
            SELECT 
                    P.Name,
                    PS.SerialNo,
                    PN.ModelNo,
                    PN.PartNo,
                    PS.ProgramID,
                    CC.Description AS PrimaryCommodity,
                    ''sim_status

             FROM [pls].[PartSerial] PS
                    
                    INNER JOIN pls.Program P ON P.ID = PS.ProgramID
                    INNER JOIN Pls.WOHeader WO ON PS.WoheaderID = WO.ID
                    INNER JOIN pls.CodeWorkStation CWS ON cws.ID = WO.WorkStationId
                    INNER JOIN [pls].[PartNo] PN
                    ON PS.PartNo = PN.PartNo 
                    INNER JOIN Pls.CodeCommodity CC ON CC.ID = PN.PrimaryCommodityID
            ";

            if (ProgramId != "0")
            {
                query += "WHERE PS.ProgramID = '" + ProgramId + "' ";
            }
            else
            {
                query += "WHERE ROH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += "AND CWS.Description = 'gTask9'";

            if (!string.IsNullOrEmpty(partNo))
                query += "AND PN.PartNo = '" + partNo + "'";

            //query += "GROUP BY ROH.ProgramID,P.Name,serialno";
            //query = query.Replace("<frmDt>", frmDt);
            //query = query.Replace("<toDt>", toDt);
            //query = query.Replace("<custRef>", custRef);
            //query = query.Replace("<OrderType>", OrderType);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(partNo))
                filterString += "| Part No. = '" + partNo + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("149", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstBonepile = cCommon.ConvertDtToHashTable(dt);
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