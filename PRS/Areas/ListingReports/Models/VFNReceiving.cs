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
    public class VFNReceiving
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields

        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstVFNReceiving { get; set; }

        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @" 
---Receiving---
DECLARE @inicio datetime = CAST(convert(varchar,'<frmDt>', 23) + ' 06:59:00'   AS datetime)
DECLARE @fin datetime =  CAST(convert(varchar,'<toDt>', 23) +  ' 07:00:00'   AS datetime)
SELECT P.Model, P.WeekYear, P.PartNo, COUNT(P.WeekYear) as QTTY
from (
    select 
        pns.PartNo,
        pns.SerialNo,
        U.Username as RO_USER,
        vroh.LastActivityDate
        ,(SELECT TOP (1) IIF(LEFT(vROh.Value,6) = 'MEIJER', 'MEIJER', vROh.Value ) 
            FROM pls.vROHeaderAttribute vROh where vROh.ROHeaderID = pnS.ROHeaderID
            and vROh.AttributeName = 'SHIPTOSITENAME') as [Client]
        ,(select pn.Description from pls.PartNo pn where pn.PartNo =  pns.PartNo)  as Model
        ,CAST(datepart(yy,vroh.LastActivityDate) AS varchar(5)) + '-' + CAST(datepart(wk,vroh.LastActivityDate) AS varchar(5)) as [WeekYear]
    from pls.PartTransaction vpt  with (nolock)
    LEFT JOIN pls.PartSerial pnS on vpt.SerialNo = pns.SerialNo and vpt.OrderHeaderID = pns.ROHeaderID and vpt.ProgramID = pns.ProgramID
    LEFT JOIN pls.ROHeader vroh on pns.ROHeaderID = vroh.ID 
	LEFT JOIN pls.[user] U on U.ID = vroh.UserID
    where vpt.CreateDate BETWEEN @inicio and @fin 
    and vpt.PartTransactionID = 1 --'RO-RECEIVE'
    and vpt.ProgramID = 10010
    and vpt.Source = 'DataEntry - PreAlert Receiving Verifone'
) P group by  P.Model,  P.PartNo, P.WeekYear order by P.WeekYear
 ";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);

            filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("232", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstVFNReceiving = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}