using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaPalletBox
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "Pallet Box No.:")]
        public string PalletNo { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstMetaPalletNoBox { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }

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

        #endregion
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName, string PalletNo)
        {
            
            string query = string.Empty;
            query = @"SELECT 
      distinct P.Name AS Program
      ,P.ID AS ProgramID
      ,PPX.CustomPalletBoxNo
      ,CASE WHEN UPPER(wsd.Code) IS NULL THEN UPPER(cws.Description) ELSE UPPER(wsd.Description) END AS WorkStationDesc 
      ,Pt.ToLocation AS Location
      ,Ps.PartNo 
      ,PS.SerialNo
      ,PPX.CreateDate AS PalletCreatedDate
      ,PS.CreateDate AS UnitPalletdDate 
      ,PT.CreateDate AS CurentLocDate
FROM PLS.PartSerial PS
INNER JOIN pls.PartPalletBoxNo PPX ON PPX.CustomPalletBoxNo = Ps.PalletBoxNo 
AND PPX.ProgramID = Ps.ProgramID
INNER JOIN pls.WOStationHistory WSH ON WSH.ID = (SELECT MAX(ID) FROM pls.WOStationHistory WSH1
WHERE WSH1.WOHeaderID = PS.WOHeaderID)
INNER JOIN pls.CodeWorkStation CWS ON CWS.ID = PS.WorkStationID
LEFT JOIN pls.CodeWorkStationCustomDescription WSD ON WSD.ProgramID = PS.ProgramID 
AND wsd.RepairTypeID = wsh.RepairTypeID AND wsd.CodeWorkStationID = WSH.WorkStationID 
INNER JOIN pls.Program P ON PS.ProgramID = P.ID
INNER JOIN pls.PartTransaction PT ON PT.ID =(SELECT MAX(ID) FROM pls.PartTransaction PT2 
WHERE PT2.SerialNo = PS.SerialNo AND PT2.ProgramID = PS.ProgramID)
LEFT OUTER JOIN pls.PartLocation PL ON PS.LocationID = PL.ID
AND PS.ProgramID = PL.ProgramID
WHERE CONVERT(Date, PPX.CreateDate) >= '<frmDt>' AND CONVERT(Date, PPX.CreateDate) <= '<toDt>' 
 ";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            if (programId != "0" && programId != null)
            {
                query += " AND PS.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += " AND PS.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (!string.IsNullOrEmpty(PalletNo))
                query += " AND  PPX.CustomPalletBoxNo like '%" + PalletNo + "%' ";
           
            query += "ORDER BY PPX.CreateDate DESC";

            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(PalletNo))
                filterString += "| Pallet No. Like '" + PalletNo + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("163", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaPalletNoBox = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}
