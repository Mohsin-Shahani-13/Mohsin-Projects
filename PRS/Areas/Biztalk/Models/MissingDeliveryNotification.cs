using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.Biztalk.Models
{
    public class MissingDeliveryNotification
    {
        cDAL oDAL;
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstMissingDeliveryNotification { get; set; }
        #endregion
        #region Methods 
        public bool GetList( string frmDt)
        {
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string programId = HttpContext.Current.Session["ProgramIdBySiteForMeta"].ToString();

            oDAL = new cDAL("Z001_OUTBOUND");
            string query = string.Empty;
            query = @" 
Select h.Contract, h.Message_Type, h.Customer_order_No, Shipment_Date, h.From_Org_Code as Site, h.Carrier_Name, h.Waybill, TH.EventCode, TH.EventDescription,
TH.ErrorMessage	
from [dbo].[Outmessage_hdr] h with (nolock) 
LEFT JOIN PLUS2.pls.TrackingHeader TH ON TH.TrackingNo = H.Waybill AND TH.ProgramID = '<programId>'
where h.contract = '<programId>'
and Message_Type = 'META-ShipConfirmation'
and CONVERT(Date,h.Shipment_Date) < '<frmDt>'
and h.Customer_order_No not in
(
Select h1.Customer_order_No
from [dbo].[Outmessage_hdr] h1 with (nolock) 
where h1.contract = '<programId>'
and h1.Message_Type in  ('META-DeliveryNotification','META-Undeliverable')
--Group By S1.N05
)
order by h.Shipment_Date
 ";
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<programId>", programId);


            DataTable dt = oDAL.GetData(query);

            //Filterstring
            filterString += "> Shipment Date < '" + frmDt + "'";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("154", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMissingDeliveryNotification = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

    }
    #endregion
}