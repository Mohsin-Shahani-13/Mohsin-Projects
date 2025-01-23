using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class VALVEComponentUsage
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }
        
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public object total { get; set; }
        [Display(Name = "Status:")]
        public string statusId { get; set; }
        [Display(Name = "Status:")]
        public string description { get; set; }

        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string Repairlevel { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<ArrayList> lstDataColumn { get; set; }
        public List<ArrayList> lstDtl { get; set; }
        public List<Hashtable> lstVALVEComponentUsage { get; set; }

        #endregion
        public DataTable Program() // onHand warehouse method
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
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
        public DataTable Status()
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
                 select distinct 
		WO.StatusID,
		Description AS Status
from pls.WOHeader WO
inner join pls.CodeStatus CS ON CS.ID = WO.StatusID

where CS.ID IN (15,19,28)
";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }

        public bool GetList(string programId, string ProgramName, string fromDt, string toDt, string statusId, string status)
        {
            string query = string.Empty;
            query = @"SELECT WOH.ProgramId AS ID
     , P.Name AS ProgramName
     , FORMAT(WOH.LastActivityDate,'yyyy.MM.dd') AS RepairedDate
     , PSReId.PartNo AS REIDPartnumber
     , WOH.SerialNo AS UnitSerialNumber
     , WOL.ComponentPartno AS ComponentPartNumber
     , CS.Description Status  
     , PN.Description AS ComponentDescription
     , CASE WUA.Value WHEN 'N/A' then '' WHEN 'N?A' then '' WHEN 'N\A' then '' ELSE WUA.Value END AS ComponentNewSerialNumber
	 , CASE WUAT.Value WHEN 'N/A' then '' WHEN 'N?A' then '' WHEN 'N\A' then '' ELSE WUAT.Value END AS ComponentOldSerialNumber
     , FORMAT(WOU.ConsumedDate,'yyyy.MM.dd') AS DateComponentReplaced
     , 'Repair Replacement' AS ComponentState
     , CC.Description AS ComponentConfiguration
     , CR.Description AS OrderType
     , CFT.Code as Faultcode
     , CFt.Description AS FaultDescription
     , CRP.Description AS ComponentRepairDescription
     , ROH.CustomerReference AS ValveReference
     , ROUA.Value AS IncomingPartnumber
FROM pls.WOHeader WOH

INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
INNER JOIN pls.WOLine WOL ON WOH.ID = WOL.WOHeaderID
INNER JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID 
INNER JOIN pls.PartNo PN ON PN.PartNo = WOL.ComponentPartNo  
LEFT JOIN pls.CodeAttribute CA_NEWSERIAL ON  CA_NEWSERIAL.AttributeName = 'NEWSERIAL'
LEFT JOIN pls.WOUnitAttribute WUA ON WUA.WOUnitID = WOU.ID AND WUA.AttributeID = CA_NEWSERIAL.ID 
LEFT JOIN pls.CodeAttribute CA_OLDSERIAL ON  CA_OLDSERIAL.AttributeName = 'OLDSERIAL' 
LEFT JOIN pls.WOUnitAttribute WUAT ON WUAT.WOUnitID = WOU.ID AND WUAT.AttributeID = CA_OLDSERIAL.ID
LEFT JOIN pls.CodeConfiguration CC ON CC.ID = WOU.ConfigurationID
LEFT JOIN pls.CodeRepairType CR ON CR.ID = WOH.RepairTypeID
LEFT JOIN pls.WOUnitCodes WUC ON WUC.WOUnitID = WOU.ID
LEFT JOIN pls.CodeFault CFT ON CFT.ID = WUC.FaultID
LEFT JOIN pls.CodeRepair CRP ON CRP.ID = WUC.RepairID
INNER JOIN pls.PartSerial PS ON PS.WOHeaderID = WOH.ID AND PS.SerialNo = WOH.SerialNo
INNER JOIN pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID
INNER JOIN pls.ROLine ROL ON ROL.ROHeaderId = ROH.Id AND ROL.PartNo = PS.PartNo
INNER JOIN pls.ROUnit ROU ON ROU.ROLineId = ROL.Id AND ROU.SerialNo = WOH.SerialNo
LEFT JOIN pls.CodeAttribute CA_INCOMINGPARTNO ON  CA_INCOMINGPARTNO.AttributeName = 'INCOMINGPARTNO'
LEFT JOIN pls.ROUnitAttribute ROUA ON ROUA.ROUnitID = ROU.ID AND ROUA.AttributeID = CA_INCOMINGPARTNO.ID 
LEFT JOIN pls.PartSerial PSReId ON PSReId.ROHeaderID = ROH.ID AND PSReId.SerialNo = WOH.SerialNo and PSReId.WOHeaderID is null -- ReIdPartNo
INNER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID

WHERE WOH.StatusId = '<statusId>'
      AND WOH.PartNo <> WOL.ComponentPartNo
	  AND WOU.ConsumedDate IS NOT NULL
AND  CONVERT(Date, WOH.LastActivityDate) >= '<fromDt>' AND CONVERT(Date, WOH.LastActivityDate) <= '<toDt>' 

";

           
            query = query.Replace("<fromDt>", fromDt);
            query = query.Replace("<toDt>", toDt);
            query = query.Replace("<statusId>", statusId);
            if (programId != "0" && programId != null)
            {
                query += " AND WOH.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += " AND WOH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            //if (!string.IsNullOrEmpty(statusId))
            //{

            //    query += " AND WOH.StatusId  = '<statusId>'";
            //}
           
            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' | From = '" + fromDt + "' To = '" + toDt + "' | Status = '" + status + "' ";

      
            cLog oLog = new cLog();
            oLog.AddSqlQuery("141", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstVALVEComponentUsage = cCommon.ConvertDtToHashTable(dt);
                return true;

            }


        }
    }
}