using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
namespace IP.Areas.SupplyChain.Models
{
    public class WOHistoryShippedUnits
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


        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public List<ArrayList> lstWOHistoryShippedUnits { get; set; }

        public List<ArrayList> lstDataColumn { get; set; }

        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable Program() // onHand warehouse method
        {
            cDAL oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        #endregion
        #region Methods 
        public bool GetList(string programId, string ProgramName, string frmDt, string toDate)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
				DECLARE @cols AS NVARCHAR(MAX),
		@query  AS NVARCHAR(MAX),
		@STARTDATE nvarchar(100) = '<frmDt>',
		@ENDDATE nvarchar(100) = '<toDt>'
	
select @cols = STUFF((SELECT ',' + QUOTENAME(Description) 
                    from (
					SELECT cwcd.Description 
                    from PLS.CodeWorkStationRouting cwsr
					JOIN PLS.CodeWorkStation cwd ON cwsr.WorkstationID = cwd.ID
					JOIN PLS.CodeWorkStationCustomDescription cwcd ON cwd.ID = cwcd.CodeWorkStationID AND cwcd.repairtypeid = cwsr.RepairTypeID
AND cwcd.ProgramID = cwsr.ProgramID
					WHERE cwcd.ProgramID = '<programId>'
                    group by cwcd.Description
                    
					UNION
					SELECT cwd.Description 
                    from PLS.CodeWorkStationRouting cwsr
					JOIN PLS.CodeWorkStation cwd ON cwsr.WorkstationID = cwd.ID
					LEFT JOIN PLS.CodeWorkStationCustomDescription cwcd ON cwd.ID = cwcd.CodeWorkStationID AND cwsr.RepairTypeID = cwcd.repairtypeid
AND cwcd.ProgramID = cwsr.ProgramID
					WHERE cwsr.ProgramID = '<programId>' AND cwcd.ID IS NULL
                    group by cwd.Description, cwcd.Description
					) a
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,1,'')
set @query = '
SELECT ProgramID,PartNo,SerialNo,ShipDate, ' + @cols + ' from 
             (
                SELECT woh.ProgramID, woh.Partno, woh.SerialNo, ps.lastActivityDate ShipDate, ISNULL(cwcd.Description,cws.description) Description, wosh.Iteration 
				FROM PLS.WOHeader woh WITH (NoLock)
				JOIN PLS.vPartSerial ps WITH (NoLock) ON woh.ID = ps.WOHeaderID
				JOIN PLS.WOStationHistory wosh WiTH (NOLOCK) ON woh.ID = wosh.WOHeaderId
				JOIN PLS.CodeWorkStation cws WITH (NOLOCK) ON wosh.WorkStationID = cws.ID
				LEFT JOIN PLS.CodeWorkStationCustomDescription cwcd ON cws.ID = cwcd.CodeWorkStationID AND cwcd.RepairTypeID = woh.RepairTypeID AND cwcd.ProgramID = woh.ProgramID
				WHERE woh.ProgramID = ''<programId>'' AND ps.StatusDescription = ''SHIPPED'' AND ps.LastActivityDate  BETWEEN ''' + @STARTDATE+ ''' AND ''' +@ENDDATE+'''
            ) x
            pivot 
            (
                MAX(Iteration)
                for Description in (' + @cols + ')
            ) p '
execute(@query);
";

            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDate);

            DataTable dt = oDAL.GetData(query);


            dt.Columns.Add("Total");
            foreach (DataRow row in dt.Rows)
            {
                decimal total = 0;
                for (int i = 4; i < dt.Columns.Count - 1; i++)
                {
                    decimal value = 0;
                    if (row[i] != DBNull.Value)
                        value = Convert.ToDecimal(row[i]);

                    total += value;
                }


                row["TOTAL"] = total;
            }

            DataTable dtList = dt.Clone();
            DataTable dtColHeader = cCommon.GenerateTransposedTable(dtList);

            //filterString = "> Program = Bose ";

            //filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("214", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWOHistoryShippedUnits = cCommon.ConvertDtToArrayList(dt);
                lstDataColumn = cCommon.ConvertDtToArrayList(dtColHeader);
                return true;

            }
        }

        #endregion
    }
}