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
    public class RepairReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

        [Display(Name = "Program:")]
        public string program { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("Active");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>' AND Name = 'BOSE'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);


            return dt;
        }
        public List<Hashtable> lstRepairReport { get; set; }
        #endregion
        #region Methods 
        public bool GetList(string programId, string programName)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
	SELECT 
    x.ProgramID,
    x.PartNo, 
    x.Description, 
    x.SerialNo, 
    x.WorkstationDescription, 
    x.CreateDate, 
    x.Aging, 
    x.Status, 
    x.ProcessType, 
    x.CodeName, 
    x.Username, 
    x.ProcessType,
    CASE 
        WHEN x.Aging BETWEEN 0 AND 4 THEN '1-5 days'
        WHEN x.Aging BETWEEN 5 AND 9 THEN '5-10 days'
        WHEN x.Aging BETWEEN 10 AND 14 THEN '10-15 days'
        WHEN x.Aging BETWEEN 15 AND 19 THEN '15-20 days'
        WHEN x.Aging BETWEEN 20 AND 24 THEN '20-25 days'
        WHEN x.Aging BETWEEN 25 AND 30 THEN '25-30 days'
        WHEN x.Aging > 30 THEN '> 30 days'
    END AS 'Aging Group',
    (SELECT cs.[Description]
     FROM pls.CodeStatus cs
     WHERE cs.ID = x.PartTransactionID) AS Shipstatus
FROM(
    SELECT
        WO.ProgramID,
        WO.PartNo,
        PN.Description,
        WO.SerialNo,
        CASE
            WHEN WO.StatusID = 19 THEN-- FOR WIP
                CASE
                    WHEN WSD.Code IS NULL THEN CWS.Description
                    ELSE WSD.Code
                END
            ELSE-- FOR HOLD
                CS.Description
        END AS WorkstationDescription,
        WO.CreateDate,
        CAST(DATEDIFF(DAY, WO.CreateDate, GETDATE()) AS INT) AS Aging, --Corrected the calculation for Aging

      CS.Description AS Status,
      (
          SELECT TOP 1

              MAX(CASE

                  WHEN rha1.Value IN('RETURN', 'EXCHANGE') THEN 'REMAN'

                  ELSE rha1.Value

              END)

          FROM pls.ROHeaderAttribute rha1

          JOIN pls.CodeAttribute ca1 ON ca1.ID = rha1.AttributeID

          JOIN pls.partserial ps ON ps.ROHeaderID = rha1.ROHeaderID-- Added this join

          WHERE ca1.AttributeName = 'PROCESS_TYPE'

          AND ps.WOHeaderID = WO.ID
      ) AS ProcessType,
        (
            SELECT MAX(pna.[Value])

          FROM pls.PartNoAttribute pna

          JOIN pls.CodeAttribute ca1 ON ca1.ID = pna.AttributeID

          WHERE ca1.AttributeName = 'CODE_NAME'

          AND pna.PartNo = WO.PartNo
        ) AS CodeName,
      u.Username,
        pt.PartTransactionID
    FROM pls.WOHeader WO
    INNER JOIN pls.PartNo PN ON PN.PartNo = WO.PartNo
    INNER JOIN pls.CodeStatus CS ON CS.ID = WO.StatusID
    INNER JOIN pls.CodeWorkStation CWS ON CWS.ID = WO.WorkStationId
    LEFT JOIN pls.[User] u ON u.ID = WO.UserID
    LEFT JOIN pls.CodeWorkStationCustomDescription WSD ON WSD.ProgramID = WO.ProgramID
        AND WSD.RepairTypeID = WO.RepairTypeID
        AND WSD.CodeWorkStationID = WO.WorkStationID
    LEFT JOIN pls.PartTransaction pt ON pt.ProgramID = WO.ProgramID
        AND pt.PartNo = WO.PartNo
        AND pt.SerialNo = WO.SerialNo
        AND pt.PartTransactionID = 18
    LEFT JOIN pls.partserial ps ON ps.WOHeaderID = WO.ID-- Added this join to ensure 'ps' is defined
  WHERE WO.PROGRAMID = '<ProgramId>'
    AND WO.StatusID IN(19, 28, 15) --WIP, HOLD, REPAIR
) x
WHERE x.ProcessType = 'REPAIR'; ";


            query = query.Replace("<ProgramId>", programId);

            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            DataTable dt = oDAL.GetDataForGeneric(query);


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("206", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRepairReport = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}