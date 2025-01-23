using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
namespace IP.Areas.Finance.Models
{
    public class WOComponent
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        [Display(Name = "ID")]
        public string ID { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        [Display(Name = "From Order No.:")]
        public string fromWoId { get; set; }
        [Display(Name = "To Order No.:")]
        public string toWoId { get; set; }


        public List<Hashtable> lstWOComponent { get; set; }
        public string InventorySource { get; private set; }

        public List<object> lstMst = new List<object>();
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

        public bool GetList(string programId, string ProgramName, string partNo, string frmDt, string toDt, string fromWoId, string toWoId, string InventorySource, string rptType)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            if (rptType == "In-Transit")
            {
                query = @"SELECT       
PT.ProgramID,   
P.Name,     
WOH.id AS WOid,      
CS.Description AS Status,       
CASE WHEN WOH.SourcePartNo IS NULL THEN WOH.PartNo ELSE WOH.SourcePartNo END PartNo,  
CASE WHEN WOH.SourceSerialNo IS NULL THEN WOH.SerialNo ELSE WOH.SourceSerialNo END SerialNo,
PT.partno AS ComponentPartNo,    
PT.serialno AS ComponentSerialno, 
CPT.Description AS PartTransaction, 
PT.InventorySource,      
PT.qty,      
PT.CostPerUnit,   
(PT.qty * PT.CostPerUnit ) AS ExtendedCost,
PT.CreateDate
FROM   pls.PartTransaction PT
INNER JOIN pls.Program P ON P.ID = PT.ProgramID
INNER JOIN pls.WOHeader WOH
         ON PT.OrderHeaderID = WOH.ID        
INNER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID
INNER JOIN pls.CodePartTransaction CPT ON  CPT.ID = PT.PartTransactionID 
WHERE WOH.StatusID <> 15   --Status REPAIR       
AND PT.PartTransactionID IN (10)    

    
";
            }

            if (rptType == "WIP")
            {

                query = @"SELECT       
PT.ProgramID,   
P.Name,     
WOH.id AS WOid,      
CS.Description AS Status,       
CASE WHEN WOH.SourcePartNo IS NULL THEN WOH.PartNo ELSE WOH.SourcePartNo END PartNo,  
CASE WHEN WOH.SourceSerialNo IS NULL THEN WOH.SerialNo ELSE WOH.SourceSerialNo END SerialNo,
PT.partno AS ComponentPartNo,    
PT.serialno AS ComponentSerialno, 
CPT.Description AS PartTransaction, 
PT.InventorySource,      
PT.qty,      
PT.CostPerUnit,   
(PT.qty * PT.CostPerUnit ) AS ExtendedCost,
PT.CreateDate
FROM   pls.PartTransaction PT
INNER JOIN pls.Program P ON P.ID = PT.ProgramID
INNER JOIN pls.WOHeader WOH
         ON PT.OrderHeaderID = WOH.ID        
INNER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID
INNER JOIN pls.CodePartTransaction CPT ON  CPT.ID = PT.PartTransactionID 
WHERE WOH.StatusID <> 15   --Status REPAIR       
AND PT.PartTransactionID IN (35) 

    
";
            }

            if (rptType == "COGS")
            {

                query = @"SELECT       
PT.ProgramID,   
P.Name,     
WOH.id AS WOid,      
CS.Description AS Status,       
CASE WHEN WOH.SourcePartNo IS NULL THEN WOH.PartNo ELSE WOH.SourcePartNo END PartNo,  
CASE WHEN WOH.SourceSerialNo IS NULL THEN WOH.SerialNo ELSE WOH.SourceSerialNo END SerialNo,
PT.partno AS ComponentPartNo,    
PT.serialno AS ComponentSerialno, 
CPT.Description AS PartTransaction, 
PT.InventorySource,      
PT.qty,      
PT.CostPerUnit,   
(PT.qty * PT.CostPerUnit ) AS ExtendedCost,
PT.CreateDate
FROM   pls.PartTransaction PT
INNER JOIN pls.Program P ON P.ID = PT.ProgramID
INNER JOIN pls.WOHeader WOH
         ON PT.OrderHeaderID = WOH.ID        
INNER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID
INNER JOIN pls.CodePartTransaction CPT ON  CPT.ID = PT.PartTransactionID 
WHERE WOH.StatusID = 15   --Status REPAIR       
AND PT.PartTransactionID IN (35) 
AND CONVERT(Date, PT.CreateDate) >= '<frmDt>' AND CONVERT(Date, PT.CreateDate) <= '<toDt>'      

    
";
            }
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            if (!string.IsNullOrEmpty(partNo))
                query += "AND((WOH.PartNo = '" + partNo + "' AND WOH.SourcePartNo IS NULL) OR WOH.SourcePartNo = '" + partNo + "') ";


            if (InventorySource == "IFS")
            {
                query += "AND PT.InventorySource LIKE '%IFS%'  ";
            }
            else if (InventorySource == "EPICOR")
            {
                query += "AND UPPER(PT.InventorySource)  LIKE '%EPICOR%' ";
            }
            else if (InventorySource == "PLUS")
            {
                query += "AND ((PT.InventorySource NOT LIKE '%IFS%' AND UPPER(PT.InventorySource) NOT LIKE '%EPICOR%') OR PT.InventorySource is null) ";
            }



            if (!string.IsNullOrEmpty(fromWoId))
                query += "AND WOH.id >= " + fromWoId + " AND WOH.id <= " + toWoId + " ";

            if (programId != "0")
            {
                query += "AND PT.ProgramID = " + programId + " ";
            }
            else
            {
                query += "AND PT.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            query += "ORDER BY woid DESC";
            //query = query.Replace("<programId>",programId);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Report Type = '" + rptType + "' | Program = '" + ProgramName + "' ";


            if ((InventorySource != "All"))
            {
                filterString += "| Data Source Like '" + InventorySource + "' ";
            }

            else
            {
                filterString += "| Data Source = '" + InventorySource + "' ";
            }
            if (rptType == "COGS")
            {
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            }

            if (!string.IsNullOrEmpty(partNo))
                filterString += "| Part No. = '" + partNo + "' ";

            if (!string.IsNullOrEmpty(fromWoId))
                filterString += "| From = '" + fromWoId + "'  To = '" + toWoId + "' ";





            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("195", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWOComponent = cCommon.ConvertDtToHashTableWithZero(dt);
                return true;

            }
        }
        #endregion
    }
}