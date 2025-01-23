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
    public class VFNReceivingDataGrapevine
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
        public List<Hashtable> lstVFNReceivingDataGrapevine { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @" 
						   ---Receiving Data Grapevine----
DECLARE @inicio datetime = CAST(convert(varchar,'<frmDt>', 23) + ' 06:59:00'   AS datetime)
DECLARE @fin datetime =  CAST(convert(varchar,'<toDt>', 23) +  ' 07:00:00'   AS datetime)
    select DISTINCT
        pns.PartNo,
        pns.SerialNo,
        PL.LocationNo,
        U.Username as RO_USER,
        vroh.LastActivityDate
        ,(SELECT TOP (1) IIF(LEFT(vROh.Value,6) = 'MEIJER', 'MEIJER', vROh.Value ) 
            FROM pls.vROHeaderAttribute vROh where vROh.ROHeaderID = pnS.ROHeaderID
            and vROh.AttributeName = 'SHIPTOSITENAME') as [Client]
        ,(select pn.Description from pls.PartNo pn with (nolock) where pn.PartNo =  pns.PartNo)  as Model
        ,CAST(datepart(yy,vroh.LastActivityDate) AS varchar(5)) + '-' + CAST(datepart(wk,vroh.LastActivityDate) AS varchar(5)) as [WeekYear]
    from pls.PartTransaction vpt with (nolock)
    LEFT JOIN pls.PartSerial pnS with (nolock)on vpt.SerialNo = pns.SerialNo and vpt.OrderHeaderID = pns.ROHeaderID
    LEFT JOIN pls.ROHeader vroh with (nolock) on pns.ROHeaderID = vroh.ID 
	LEFT JOIN pls.PartLocation PL with (nolock) on PL.ID = pnS.LocationID 
	LEFT JOIN pls.[User] U with (nolock) ON U.ID = vroh.UserID
	 where vpt.CreateDate BETWEEN @inicio and @fin 
	 and vpt.ProgramID = 10008
     and vpt.PartTransactionID = 1
     and vpt.Source = 'DataEntry - PreAlert Receiving Verifone'
 ";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);

            filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("229", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstVFNReceivingDataGrapevine = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}