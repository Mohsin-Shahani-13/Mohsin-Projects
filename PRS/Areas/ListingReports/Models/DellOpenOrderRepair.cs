using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{
    public class DellOpenOrderRepair
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstDellOpenOrderRepair { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList()
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"
/****** Script for SelectTopNRows command from SSMS  ******/
SELECT 
      rou1.Value 'RTV Order Number', rh.CustomerReference 'RMA Number', '' 'Dell Order Number', ROH1.Value 'SPMOrderID', st.Description 'RTV Order Status', ps.[RODate] 'RTV Order Creation Date',
	  1 'Line Item Number', ps.[PartNo] 'Item Number' , p.Description 'Item Description', ps.[SerialNo] 'Item Revision',
	  c.Description 'Item Family',  rl.QtyToReceive 'Quantity Ordered', rl.QtyReceived 'Quantity Received', 
	   roh2.value 'Inbound Way Bill', roh3.value 'Inbound Carrier Code', '' 'Owner' , SHIP.ShipmentDate, SHIP.TrackingNo 'Outbound Way Bill', SHIP.carrier 'Outbound Carrier', ''  'ETA Closure', '' 'Comments' , '' 'Sub-Comments'
  FROM [pls].[PartSerial] ps
  join pls.roheader rh on rh.id = ps.roheaderid
  join pls.roline rl on rl.ROHeaderID = rh.ID
  join pls.rounit ru on ru.ROLineID = rl.id and ru.SerialNo = ps.SerialNo
  join pls.PartNo p on p.PartNo = ps.PartNo
  join pls.CodeCommodity c on p.PrimaryCommodityID = c.ID
  join pls.codestatus St on st.ID = ps.StatusID
  left join pls.woheader wh on wh.id = ps.woheaderid
  left join pls.soheader sh on sh.id = ps.soheaderid
  LEFT JOIN pls.CodeAttribute CA on CA.AttributeName ='RTV_NO'
  LEFT JOIN pls.ROUnitAttribute ROU1 on ROU1.ROUnitID= ru.ID AND ROU1.AttributeID = CA.ID
  LEFT JOIN pls.CodeAttribute CA1 on CA1.AttributeName ='DELL_PO'
  LEFT JOIN pls.ROHeaderAttribute ROH1 on ROH1.ROHeaderID= rh.ID AND ROH1.AttributeID = CA1.ID
  LEFT JOIN pls.CodeAttribute CA2 on CA2.AttributeName ='TRACKING NUMBER'
  LEFT JOIN pls.ROHeaderAttribute ROH2 on ROH2.ROHeaderID= rh.ID AND ROH2.AttributeID = CA2.ID
  LEFT JOIN pls.CodeAttribute CA3 on CA3.AttributeName ='CARRIER'
  LEFT JOIN pls.ROHeaderAttribute ROH3 on ROH3.ROHeaderID= rh.ID AND ROH3.AttributeID = CA3.ID
  LEFT JOIN pls.SOShipmentInfo SHIP on SHIP.SOHeaderID = sh.ID ";
            if (sites == "BYDGOSZCZ")
            {
                query += "where ps.ProgramID=10064 ";
            }
            else if (sites == "JUAREZ")
            {
                query += "where ps.ProgramID=10061 ";

            }
            else if (sites == "MEXICALI")
            {
                query += "where ps.ProgramID=10055 ";

            }
            else if (sites == "MEMPHIS")
            {
                query += "where ps.ProgramID=10053 ";

            }

            DataTable dt = oDAL.GetData(query);

            filterString += "> Program = DELL";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("212", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDellOpenOrderRepair = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}