using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaTPPLTTest
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstMetaTPPLTTest { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList(string programId, string ProgramName)
        {
            string query = string.Empty;

            query = @"
select ps.ProgramID,cws.Description as  WorkstationDescription, wo.ID, ps.partno, ps.SerialNo, cs.Description as StatusDescription, ps.PalletBoxNo, ppa.[Value] wdr,
replace(pl.Bin,cast(ps.programid as varchar(7)) + '.', '') LocationNo,
    (
    select max(wosa.[Value])
    from pls.WOStationAttribute wosa
    LEFT JOIN pls.CodeAttribute ca on ca.ID = wosa.AttributeID
    where wosa.WOStationHistoryID = 
        (select max(wosh.ID)
        from pls.WOStationHistory wosh
        INNER JOIN pls.CodeWorkStation cws on wosh.WorkStationID = cws.ID
        where wosh.WOHeaderID = wo.Id
        and cws.Description = 'Triage'
        )
    and ca.AttributeName = 'GRADE'
    ) TriageGrade
from pls.PartSerial ps
inner join pls.PartLocation pl on pl.ProgramID = ps.ProgramID and ps.LocationID = pl.ID
left join pls.WOHeader wo on wo.ProgramID = ps.ProgramID and wo.ID = ps.WOHeaderID
LEFT JOIN pls.CodeWorkStation cws on ps.WorkStationID = cws.ID
left join pls.PartPalletBoxNo ppn on ppn.ProgramID = ps.ProgramID and ppn.CustomPalletBoxNo = ps.PalletBoxNo
left join pls.vPartPalletBoxNoAttribute ppa on ppa.PartPalletBoxNoID = ppn.ID and ppa.AttributeName = 'PalletNo'	
INNER JOIN pls.CodeStatus cs ON cs.ID = ps.StatusID 
where cs.Description != 'SHIPPED'
and ps.PalletBoxNo like 'TPPLT%'
";

            if (programId != "0" && programId != null)
            {
                query += " AND ps.ProgramID = '" + programId + "' ";
            }
            else
            {
                query += " AND ps.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("165", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaTPPLTTest = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }

    }
}