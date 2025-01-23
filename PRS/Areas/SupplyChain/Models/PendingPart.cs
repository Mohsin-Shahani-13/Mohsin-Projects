using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class PendingPart
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields        
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstPendingPart { get; set; }

        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
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
        public bool GetList(string programID, string programName)
        {
            string query = string.Empty;
            query = @"
SELECT 
       WOH.ID, 
       WOH.ProgramID,
       P.Name AS Program,
       WOH.CustomerReference,   
	  pt.reason,
       WOH.PartNo,
       WOH.SerialNo,
       (SELECT CASE WHEN EXISTS (SELECT 1 
                                 FROM pls.PartSerial PS
                                 WHERE PS.SerialNo = WOH.SerialNo 
                                   AND PS.ProgramID = WOH.ProgramID) 
                    THEN 'Y' ELSE 'N' END) AS HAS_SN,
	   --MAX(WOL.ComponentPartNo) AS ComponentPartNo, 
	   WOL.ComponentPartNo AS ComponentPartNo,
       --MAX(CRT.Description) AS RepairType,  
	   CRT.Description as RepairType, 
       --MAX(CS.Description) AS Status,
	   CS.Description AS Status,
	   U.Username as [By], 
	   wol.QtyRequested,
		
	   isnull((SELECT SUM(pq.AvailableQty)
        FROM pls.PartQty PQ
        INNER JOIN pls.PartLocation PL ON PQ.ProgramID = PL.ProgramID
                                       AND PQ.LocationID = PL.ID
                                       AND PL.Warehouse IN ('FGI', 'FLOORSTOCK') 
                                       AND PQ.ProgramID = WOH.ProgramID
                                       AND PQ.PartNo = WOL.ComponentPartNo),0) AS availqty,
	    wol.QtyRequested - isnull((SELECT SUM(pq.AvailableQty)
        FROM pls.PartQty PQ
        INNER JOIN pls.PartLocation PL ON PQ.ProgramID = PL.ProgramID
                                       AND PQ.LocationID = PL.ID
                                       AND PL.Warehouse IN ('FGI', 'FLOORSTOCK') 
                                       AND PQ.ProgramID = WOH.ProgramID
                                       AND PQ.PartNo = WOL.ComponentPartNo),0) as TotalQty,
									    max(pt.CreateDate) as HoldDate, 
  DATEDIFF(DD, max(pt.CreateDate), GETDATE()) as DaysinHold
    
FROM   pls.WOHeader WOH
 INNER JOIN pls.PartTransaction pt ON WOH.SerialNo = pt.SerialNo
             AND pt.OrderType = 'WO'
             AND WOH.ProgramID = pt.ProgramID
             AND WOH.UserID = pt.UserID
             AND CONVERT(DATETIME2(0), pt.CreateDate) = CONVERT(DATETIME2(0), WOH.LastActivityDate)
INNER JOIN pls.[User] U ON U.ID = WOH.UserID 
INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
LEFT JOIN pls.WOLine WOL ON WOL.WOHeaderID = WOH.ID
LEFT JOIN pls.WOUnit WOU ON WOU.WOLineID = WOL.ID
LEFT  JOIN pls.CodeRepairType CRT ON CRT.ID = WOH.RepairTypeID
--LEFT OUTER JOIN pls.CodeWorkStation CWS ON CWS.ID = WOH.WorkstationID
--LEFT JOIN pls.CodeWorkStationCustomDescription WSD ON
--                   WSD.ProgramID = WOH.ProgramID 
--                   AND WSD.RepairTypeID = WOH.RepairTypeID
--                   AND WSD.CodeWorkStationID = WOH.WorkStationID
LEFT OUTER JOIN pls.CodeStatus CS ON CS.ID = WOH.StatusID
WHERE
  WOH.StatusID = 28
  AND (pt.Reason = 'Pendiente de Partes / Pending Parts' 
       OR pt.Reason LIKE 'Awaiting for Parts%' 
       OR pt.Reason LIKE 'Awaiting Parts%')

AND WOL.StatusID = 7 --NEW
 AND WOH.ProgramID = '<programID>'  ";
            if (programName == "GOOGLE")
            {
                query += "\nAND CRT.ID = '214'";
            }
           
            query += "\nGROUP BY WOH.ID, WOH.ProgramID, P.Name, WOH.CustomerReference, pt.reason,  WOH.PartNo, WOH.SerialNo,WOL.ComponentPartNo, wol.QtyRequested, U.Username, pt.CreateDate, CRT.Description, CS.Description  ";
           
            query += "\nORDER BY pt.CreateDate desc; ";

            query = query.Replace("<programID>", programID);

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(programName))
                filterString = "> Program = '" + programName + "' ";


            cLog oLog = new cLog();
            oLog.AddSqlQuery("220", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPendingPart = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}