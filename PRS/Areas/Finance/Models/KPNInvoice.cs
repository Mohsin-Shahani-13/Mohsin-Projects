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
    public class KPNInvoice
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
        public List<Hashtable> lstKPNInvoice { get; set; }

       
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
SELECT 
WO.ProgramID,
    wo.ID WoId,
rh.ID RoId,
    MAX(CASE 
        WHEN rh.CustomerReference IS NOT NULL THEN rh.CustomerReference 
        ELSE rh1.CustomerReference 
    END) AS RO_Reference,
    wo.CustomerReference AS WO_Reference,
    wo.PartNo,
    pnn.Description,
    wo.SerialNo,
    wo.SourceSerialNo,
    wo.CreateDate AS ReceiveOrder_Date,
    wo.LastActivityDate AS WorkOrder_ClosedDate,
    MAX(CASE 
        WHEN wo.SourceSerialNo IS NOT NULL THEN 'REID' 
        WHEN ps.SerialNo IS NULL THEN 'CREDIT' 
        ELSE cs.Description
    END) AS final_status,
    MAX(CASE 
        WHEN wh.WorkStationID = 31 AND wh.ToWorkStationID = 11 THEN wh.EndDate 
        ELSE NULL 
    END) AS wipeDate,
    
    MAX(CASE 
        WHEN wh.WorkStationID = 16 AND wh.ToWorkStationID = 17 THEN wh.EndDate 
        ELSE NULL 
    END) AS UnlockDate,
      MAX(CASE 
        WHEN CA.AttributeName = 'UNLOCK_VENDOR' THEN wsa.Value 
        ELSE NULL 
    END) AS Unlock_Vendor,
    
    MAX(CASE 
        WHEN wh.WorkStationID = 13 AND wh.ToWorkStationID = 15 THEN wh.EndDate 
        ELSE NULL 
    END) AS RepairDate,
    MAX(CASE 
        WHEN CA.AttributeName = 'REPAIR_VENDOR' THEN wsa.Value 
        ELSE NULL 
    END) AS Repair_Vendor

FROM
    [pls].[WOHeader] wo
LEFT JOIN  
    [pls].[PartSerial] ps ON ps.SerialNo = wo.SerialNo
LEFT JOIN  
    [pls].[CodeStatus] cs1 ON cs1.ID = ps.StatusID
LEFT JOIN  
    [pls].[ROUnit] ru ON ru.SerialNo = wo.SerialNo
LEFT JOIN
    [pls].[ROLine] rl ON ru.ROLineID = rl.id
LEFT JOIN  
    [pls].[ROHeader] rh ON rh.ID = rl.ROHeaderID
LEFT JOIN  
    [pls].[ROHeader] rh1 ON ps.ROHeaderID = rh1.ID
LEFT JOIN  
    [pls].[CodeStatus] cs ON cs.ID = wo.StatusID
LEFT JOIN  
    [pls].[PartNo] pnn ON pnn.PartNo = wo.PartNo
LEFT JOIN  
    [pls].[WOStationHistory] wh ON wh.WOHeaderID = wo.ID
LEFT JOIN  
    [pls].[WOStationAttribute] wsa ON wsa.WOStationHistoryID = wh.ID   
LEFT JOIN 
    [pls].[CodeAttribute] CA ON wsa.AttributeID = CA.ID 
    AND CA.AttributeName IN ('UNLOCK_VENDOR', 'REPAIR_VENDOR')

WHERE 
    wo.ProgramID = '<programId>' 
    AND wo.StatusID IN (15, 17) 
   AND CONVERT(Date, wo.LastActivityDate) >= '<frmDt>' 
    AND CONVERT(Date, wo.LastActivityDate) < ='<toDt>'
GROUP BY  
    WO.ProgramID,
    wo.ID,
    rh.ID,
    wo.CustomerReference,
    wo.PartNo,
    pnn.Description,
    wo.SerialNo,
    wo.SourceSerialNo,
    wo.CreateDate,
    wo.LastActivityDate,
    cs.Description
ORDER BY 
    wo.CreateDate DESC;



 ";


            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            query = query.Replace("<programId>", programId);
           

            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("229", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstKPNInvoice = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}