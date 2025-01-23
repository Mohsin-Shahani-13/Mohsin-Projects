using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
namespace IP.Areas.ListingReports.Models
{
    public class WorkOrderRepair
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "From:")]
        public string _frmDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string frmDt { get { return _frmDt; } set { _frmDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Customer Ref.:")]
        public string custRef { get; set; }
        
        public string OrderType { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstWorkOrderRepair { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS ProgramID
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        #endregion
        #region Methods
        public bool GetList(string frmDt, string toDt, string ProgramID, string ProgramName, string custRef)
        {

            string query = string.Empty;
            query = @"
IF OBJECT_ID('tempdb.dbo.#WOREPAIR') IS NULL
BEGIN
CREATE TABLE #WOREPAIR
(
 ID INT,
 ProgramID SMALLINT,
 Program VARCHAR(50),
 CustomerReference VARCHAR(100),
 ROCustomerReference VARCHAR(100),
 PartNo VARCHAR(50),
 SerialNo VARCHAR(50),
 HAS_SN VARCHAR(20),
 InCommingPartNo VARCHAR(50),
 OutgoingPartNo  VARCHAR(50),
 StatusDESC VARCHAR(50),
 RepairType VARCHAR(100),
 FamilyAttribute VARCHAR(250),
 WorkstationID INT,
 Workstation VARCHAR(50),
 Status VARCHAR(50),
 CO5_RightAlign BIGINT,
 CO6_RightAlign BIGINT,
 CreatedBy VARCHAR(50),
 CreatedOn SMALLDATETIME,
 LastActivityOn SMALLDATETIME,
 ROHeaderID INT
)
END
INSERT INTO #WOREPAIR (ID, ProgramID, Program, CustomerReference, ROCustomerReference, PartNo, SerialNo, StatusDESC, RepairType, WorkstationID,Workstation
                        ,CreatedBy,CreatedOn, LastActivityOn, ROHeaderID )
SELECT WOH.ID,
       WOH.ProgramID,
       P.Name AS Program,
       WOH.CustomerReference,
	   ROH.CustomerReference,
       WOH.PartNo,
       WOH.SerialNo,
	   CS.Description,
       CRT.Description As RepairType,
       CASE WHEN wsd.Code IS NULL THEN cws.ID ELSE wsd.ID END AS WorkstationID,
       CASE WHEN wsd.Code IS NULL THEN cws.Description ELSE wsd.Description END As Workstation,
       U.Username AS CreatedBy,
       FORMAT(WOH.CreateDate, 'yyyy.MM.dd HH:mm') AS CreatedOn,
       FORMAT(WOH.LastActivityDate, 'yyyy.MM.dd HH:mm') AS LastActivityOn,
	   ROH.ID
FROM   pls.WOHeader WOH
INNER JOIN pls.[User] U ON U.ID = WOH.UserID 
INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
LEFT OUTER JOIN pls.CodeRepairType CRT ON CRT.ID = WOH.RepairTypeID
LEFT OUTER JOIN pls.CodeWorkStation CWS ON CWS.ID = WOH.WorkstationID
LEFT JOIN pls.CodeWorkStationCustomDescription wsd ON
                   wsd.ProgramID = WOH.ProgramID 
                   AND wsd.RepairTypeID = WOH.RepairTypeID
                   AND wsd.CodeWorkStationID = WOH.WorkStationID
LEFT OUTER JOIN pls.PartSerial PS ON PS.ProgramID = WOH.ProgramID AND PS.PartNo = WOH.PartNo ANd PS.SerialNo = WOH.SerialNo
LEFT OUTER JOIN pls.CodeStatus CS ON CS.ID = PS.StatusID
LEFT OUTER JOIN pls.ROHeader ROH ON PS.ROHeaderID = ROH.ID

WHERE WOH.StatusID = 15 --Repair

AND  CONVERT(Date,WOH.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, WOH.LastActivityDate) <= '<toDt>'


";
            if (ProgramID != "0")
                query += "AND WOH.ProgramID = '" + ProgramID + "' ";
            if (!string.IsNullOrEmpty(custRef))
                query += "AND WOH.CustomerReference LIKE '%" + custRef + "%' ";


            query += "order by WOH.id ";
            query += @"

         Update WOR SET WOR.HAS_SN =
            (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
                FROM pls.partserial PS
                WHERE PS.SerialNo = WOR.SerialNo AND PS.ProgramID = WOR.ProgramID )
                FROM  #WOREPAIR WOR
         --FOR UPDATE HAS_SN
                Update WOR SET WOR.FamilyAttribute =
                (SELECT VALUE
                    FROM pls.PartNoAttribute PNA
                    WHERE pna.ProgramID = WOR.ProgramID and PNA.PartNo = WOR.PartNo and pna.AttributeID = 278)
                    FROM  #WOREPAIR WOR
          
           --FOR UPDATE QTY_Requested
            Update WOR SET WOR.CO5_RightAlign =
          ( SELECT SUM(CAST(QtyRequested AS bigint))
               FROM pls.WOLine
               WHERE WOHeaderID = WOR.ID AND WOR.PartNo <> ComponentPartNo )
               FROM  #WOREPAIR WOR
          --FOR UPDATE QTY_Consumed
            Update WOR SET WOR.CO6_RightAlign =
          ( SELECT SUM(CAST(QtyConsumed AS bigint))
               FROM pls.WOLine
               WHERE WOHeaderID = WOR.ID AND WOR.PartNo <> ComponentPartNo )
               FROM  #WOREPAIR WOR
                -- FOR UPDATE ROUNIT ATTRIBUTE InCommingPartNo
                UPDATE WOR SET WOR.InCommingPartNo = ROUA.Value
                FROM #WOREPAIR WOR
                --INNER JOIN pls.PartSerial PS ON PS.ProgramID = WOR.ProgramID AND PS.PartNo = WOR.PartNo AND PS.SerialNo = WOR.SerialNo
                INNER JOIN pls.ROLine ROL ON ROL.ROHeaderID = WOR.ROHeaderID
                INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID
                LEFT JOIN pls.CodeAttribute CA ON CA.AttributeName = 'INCOMINGPARTNO'
                LEFT JOIN pls.ROUnitAttribute ROUA ON ROUA.ROUnitID = ROU.ID AND ROUA.AttributeID = CA.ID
                -- FOR UPDATE ATTRIBUTE OUT GOING PartNo
                UPDATE WOR SET WOR.OutgoingPartNo = WOHA.Value
                FROM #WOREPAIR WOR
                LEFT JOIN pls.CodeAttribute CA ON CA.AttributeName = 'ReIDPartNumber'
                LEFT JOIN pls.WOHeaderAttribute WOHA ON WOHA.AttributeID = CA.ID AND WOHA.WOHeaderID = WOR.ID
               
    --         -- Updates the latest status of Serial from pls.PartSerial
				--UPDATE WOR SET WOR.StatusDesc = 
				--(
				--	SELECT TOP 1 CS.Description
				--	FROM pls.PartSerial PS 
				--	INNER JOIN pls.CodeStatus CS ON CS.ID = PS.StatusId
				--	WHERE PS.ProgramId = WOR.ProgramId AND PS.SerialNo = WOR.SerialNo
				--	ORDER BY PS.ID DESC
				--)
				--FROM #WOREPAIR WOR 
SELECT * FROM #WOREPAIR
ORDER BY CreatedOn DESC
DROP TABLE #WOREPAIR
";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

          
          



            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";


            if (!string.IsNullOrEmpty(custRef))
                filterString += " | Customer Ref. = '" + custRef + "' ";



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("147", query, string.Empty, true);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWorkOrderRepair = cCommon.ConvertDtToHashTable(dt);
                return true;

            }

        }
        #endregion
    }
}