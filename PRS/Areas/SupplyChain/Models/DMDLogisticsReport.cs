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
    public class DMDLogisticsReport
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Customer Ref.:")]
        public string custRef { get; set; }
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }

        public List<Hashtable> lstDMDLogisticsReport { get; set; }

        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, string custRef)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            //string programName = HttpContext.Current.Session["Program"].ToString();

            string query = string.Empty;
            //DateTime toDate = Convert.ToDateTime(toDt);
            //toDate.ToString(Format.DateOnly);
            //DateTime aaj = toDate.AddDays(1);

            //string to_date = toDate.ToString(Format.DateOnly);

            query = @"
select 
		 ROH.CustomerReference AS CaseNumber
		,ROHAttr.value AS PickupOption
		,ROHAttr.CreateDate AS RequestDate
		,ROH.ProgramID  
		,CAD.Country
		,CR.TrackingNo
		,CR.LabelType AS trackingType
		,TH.EventDescription AS ShipmentStatus
		,CC.Description AS PrimaryCommodity
		,ROU.SerialNo
        ,ROL.PartNo
		,ROU.Quantity AS Qty

From pls.ROHeader ROH

INNER JOIN pls.ROHeaderAttribute ROHAttr ON ROHAttr.ROHeaderID = ROH.ID
INNER JOIN pls.CodeAddressDetails CAD ON CAD.AddressID = ROH.AddressID
LEFT JOIN pls.CarrierResult CR ON CR.CustomerReference = ROH.CustomerReference
									AND CR.LabelType = 'Customer Return'
									AND CR.LabelType = 'Shipments to Customer'
LEFT JOIN pls.TrackingHeader TH ON TH.TrackingNo = CR.TrackingNo
INNER JOIN pls.ROLine ROL ON ROL.ROHeaderID = ROH.ID
INNER JOIN pls.PartNo PN ON PN.PartNo = ROL.PartNo
INNER JOIN pls.CodeCommodity CC ON CC.ID = PN.PrimaryCommodityID
INNER JOIN pls.ROUnit ROU ON ROU.ROLineID = ROL.ID


WHERE ROH.ProgramID = 10056
 ";


            if (!string.IsNullOrEmpty(custRef))
                query += "AND ROH.CustomerReference LIKE '%" + custRef + "%' ";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            query = query.Replace("<programId>", programId);


            DataTable dt = oDAL.GetData(query);

            //if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = VRS ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            if (!string.IsNullOrEmpty(custRef))
                filterString += " | Customer Ref. Like '" + custRef + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("186", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDMDLogisticsReport = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}