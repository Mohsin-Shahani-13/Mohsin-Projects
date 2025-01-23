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
    public class PalletwithAttributes
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
        [Display(Name = "Warehouse:")]
        public string Warehouse { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }

        public List<Hashtable> lstPalletwithAttributes { get; set; }

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
        public DataTable GetWarehouse(int ProgramId)
        {
            oDAL = new cDAL("ACTIVE");
            string program = String.Empty;
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            program = HttpContext.Current.Session["ProgramForSite"].ToString();

            string query = string.Empty;
            query = @"SELECT distinct Warehouse
FROM pls.PartSerial ps with (nolock)
INNER JOIN pls.PartLocation pl with (nolock) on pl.ProgramID = ps.ProgramID and ps.LocationID = pl.ID 
inner join pls.program p with (nolock) on p.ID = ps.ProgramID

 ";
            //query += "WHERE ps.ProgramID IN (10058) ";

            query += @"where p.ID = <program>
ORDER BY Warehouse ";
            query = query.Replace("<sites>", sites);
            query = query.Replace("<program>", ProgramId.ToString());
            DataTable dt = oDAL.GetData(query);

            return dt;
        }
        public bool GetList(string programId, string programName, string warehouse)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @" 
SELECT DISTINCT
    ppbna.[ID], 
    ppbn.ProgramID, 
    pl.LocationNo,
    PS.PartNo,
    PQ.AvailableQty AS Qty,
    pl.Warehouse,
    ppbna.[PartPalletBoxNoID], 
    ppbna.[CustomPalletBoxNo], 
    ppbna.[AttributeName],
    ppbna.[Value], 
    ppbna.[Username], 
    MIN(ppbna.[CreateDate]) AS CreateDate,
    MAX(ppbna.[LastActivityDate]) AS LastActivityDate
FROM pls.vPartPalletBoxNoAttribute ppbna 
INNER JOIN pls.PartPalletBoxNo ppbn 
    ON ppbn.CustomPalletBoxNo = ppbna.CustomPalletBoxNo 
    AND ppbn.ID = ppbna.PartPalletBoxNoID 
INNER JOIN pls.PartSerial ps 
    ON ps.PalletBoxNo = ppbn.CustomPalletBoxNo 
    AND ps.ProgramID = ppbn.ProgramID
INNER JOIN pls.PartLocation pl 
    ON pl.ID = ps.LocationID 
    AND pl.ProgramID = ps.ProgramID
LEFT JOIN pls.PartQty PQ WITH (NOLOCK)
    ON PQ.PartNo = PS.PartNo AND PQ.ProgramID = PS.ProgramID AND PQ.LocationID = PL.ID 
	AND  PQ.ConfigurationID = PS.ConfigurationID AND PQ.PalletBoxNo  = ppbn.CustomPalletBoxNo
WHERE ps.ProgramID = <programId> and ps.SODate is null
";

            query = query.Replace("<programId>", programId);
            //query = query.Replace("<toDt>", toDt);

            if (warehouse != "All")
                query += "AND pl.Warehouse = '" + warehouse + "'";

            query += @"GROUP BY 
    ppbna.[ID], 
    ppbn.ProgramID, 
    pl.LocationNo,
    PS.PartNo,
    PQ.AvailableQty,
    pl.Warehouse,
    ppbna.[PartPalletBoxNoID], 
    ppbna.[CustomPalletBoxNo], 
    ppbna.[AttributeName],
    ppbna.[Value], 
    ppbna.[Username]
ORDER BY MIN(ppbna.CreateDate) DESC ";
            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            if (!string.IsNullOrEmpty(warehouse))
                filterString += "| Warehouse = '" + warehouse + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("241", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPalletwithAttributes = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        #endregion
    }
}