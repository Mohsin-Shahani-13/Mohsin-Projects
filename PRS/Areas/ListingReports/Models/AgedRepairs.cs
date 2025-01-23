using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class AgedRepairs
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }

        public int totalIndex { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Aged:")]
        public string IdleOver { get; set; }
        public string ProgramBySite { get; set; }

        public List<Hashtable> lstAgedRepairs { get; set; }

        #endregion
        #region "Methods"
        public DataTable Program() // onHand warehouse method
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;
            query = @"SELECT DISTINCT Id As ProgramId, Name AS Program  FROM pls.Program where name = 'BOSE' AND site = '<site>'";

            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetList(string aged, string programId, string ProgramName)
        {
            string query = string.Empty;
            var conType = @HttpContext.Current.Session["CONN_TYPE"].ToString();

            query = @"
select 
ps.programid,
ro.ID,
ro.CustomerReference 'RMA', 
ps.SerialNo, 
rd.createdate 'DOCK DATE', 
ps.createdate 'RECEIPT DATE', 
ca.Country, 
ps.PartNo 'SKU', 
pn.DESCRIPTION, 
cs.DESCRIPTION 'STATUS',
datediff(day,ps.createdate,getdate()) 'AGE' 
from pls.PartSerial ps 
inner join pls.roheader ro on ro.id = ps.roheaderid and ro.programid = ps.programid
inner join pls.ROHeaderAttribute roa on roa.ROHeaderID = ro.id and roa.AttributeID = 986 and roa.[Value] = 'REPAIR'
inner join pls.CodeAddress ca on ca.ID = ro.AddressID and ca.ProgramID = ps.ProgramID 
inner join pls.CodeStatus cs on cs.id = ps.StatusID
inner join pls.partno pn on pn.PartNo = ps.PartNo
inner join pls.RODockLog RD on rd.roheaderid = ro.ID
where ps.ProgramID = '<programId>'
and ps.statusid not in (8,18,3,17)
and datediff(day,ps.createdate,getdate()) > '<aging>'
order by 4
			";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<aging>", aged);

            //query = query.Replace("<fDate>", fDate);
            //query = query.Replace("<tDate>", tDate);

            DataTable dt = oDAL.GetData(query);

            filterString = "> Program = BOSE ";
            filterString += " | Aged > '" + aged + "'";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("247", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstAgedRepairs = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}