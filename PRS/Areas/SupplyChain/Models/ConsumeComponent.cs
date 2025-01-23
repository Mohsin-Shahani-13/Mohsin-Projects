using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Collections;

namespace IP.Areas.SupplyChain.Models
{
    public class ConsumeComponent
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

        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string Repairlevel { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<ArrayList> lstDataColumn { get; set; }
        public List<ArrayList> lstDtl { get; set; }
        public List<Hashtable> lstConsumeComponent { get; set; }
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

        public bool GetList(string programId, string ProgramName, string fromDt, string toDt)
        {
            string query = string.Empty;
            query = @"SELECT ROH.ID AS RoId
,woh.PartNo
,WOH.ProgramId AS ID
,P.Name AS ProgramName
,WOH.SerialNo
,ROH.CustomerReference
,CASE WHEN wsd.Code IS NULL THEN cws.Description ELSE wsd.Description END As Workstation
,WOL.ComponentPartNo
,PN.Description as Part_Desc
,SUM(WOU.QtyIssued) as Qty_issued
,CC.Description AS  Configuration
,PL.LocationNo AS Location
,CPT.Description as Part_Type
,U.Username AS CreatedBy
,FORMAT(WOU.ConsumedDate, 'yyyy.MM.dd') AS ConsumeDate
FROM pls.WOUnit WOU
INNER JOIN pls.WOLine WOL ON WOL.ID = WOU.WOLineID
INNER JOIN pls.WOHeader WOH ON WOH.ID = WOL.WOHeaderID 
INNER JOIN pls.Program P ON P.ID = WOH.ProgramID
INNER JOIN pls.CodeConfiguration CC ON CC.ID = WOU.ConfigurationID
INNER JOIN pls.PartLocation PL ON PL.ID = WOU.FromLocationID
INNER JOIN pls.PartNo PN ON PN.PartNo = WOl.ComponentPartNo
INNER JOIN pls.PartSerial PS  ON PS.ProgramId = WOH.ProgramId AND PS.PartNo = WOH.PartNo AND PS.SerialNo = WOH.SerialNo
INNER JOIN pls.ROHeader ROH ON ROH.ID = PS.ROHeaderID
INNER JOIN pls.CodePartType CPT ON CPT.ID = PN.PartTypeID
LEFT OUTER JOIN pls.CodeWorkStation CWS ON CWS.ID = WOH.WorkstationID
LEFT JOIN pls.CodeWorkStationCustomDescription wsd ON
                   wsd.ProgramID = WOH.ProgramID 
                   AND wsd.RepairTypeID = WOH.RepairTypeID
                   AND wsd.CodeWorkStationID = WOH.WorkStationID
INNER JOIN pls.[User] U ON U.ID = WOU.UserID

WHERE 
WOL.StatusID = 14 
AND WOH.PartNo <> WOL.ComponentPartNo  
AND WOH.ProgramID = <programId>
AND CONVERT(Date, Wou.ConsumedDate) >= '<fromDt>' AND CONVERT(Date, Wou.ConsumedDate) <= '<toDt>' 

";


            query = query.Replace("<fromDt>", fromDt);
            query = query.Replace("<toDt>", toDt);
            query = query.Replace("<programId>", programId);


            //if (programId != "0" && programId != null)
            //{
            //    query += " AND WOH.ProgramID = '" + programId + "' ";
            //}
            //else
            //{
            //    query += " AND WOH.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            //}

            query += @"GROUP BY 
                     ROH.ID 
                    ,woh.PartNo
                    ,WOH.ProgramId 
                    ,P.Name
                    ,WOH.SerialNo
                    ,ROH.CustomerReference
                    ,WOL.ComponentPartNo
                    ,CPT.Description
                    ,PN.Description
                    ,CC.Description
                    ,PL.LocationNo
                    ,U.Username
                    ,wsd.code
                    ,CWS.Description
                    ,wsd.Description
                    ,FORMAT(WOU.ConsumedDate, 'yyyy.MM.dd')
";

            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' | From = '" + fromDt + "' To = '" + toDt + "' ";


            cLog oLog = new cLog();
            oLog.AddSqlQuery("151", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstConsumeComponent = cCommon.ConvertDtToHashTable(dt);
                return true;

            }


        }
    }
}