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
    public class PackingList
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string menuTitle { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Pallet Box No.:")]
        public string PalletBoxNo { get; set; }
        public List<Hashtable> lstPacketList { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion

        #region Methods 
        public bool GetList(string PalletBoxNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
                    Select SUBSTRING(Pallet, LEN(Pallet) - CHARINDEX('.', REVERSE(Pallet)) + 2, LEN(Pallet) -  CHARINDEX('.', REVERSE(Pallet)) - 1) Pallet, Box_No, ROW_NUMBER() OVER(ORDER BY LastActivityDate ASC)  as 'Seq_No', RFC_NO as 'RFC#', RMA as 'RMA#', Model as 'Model#', TSB_Serial_No as 'TSB Serial No', IIF(MCODE = 'NA', '', MCODE) as 'M-CODE', COO
FROM (
Select  (SELECT max(pl.LocationNo) FROM pls.PartQty pq
		inner join pls.PartLocation pl on pl.ID = pq.LocationID
		WHERE pq.ProgramID = ps.ProgramID AND PalletBoxNo = ps.PalletBoxNo) as 'Pallet',
        b.CustomPalletBoxNo as 'Box_No',
        (Select max(roua.Value) from [pls].[ROUnit] rou
        inner join [pls].[CodeAttribute] ca on ca.AttributeName ='RFC_NO'
        inner join  [pls].[ROUnitAttribute] roua on roua.ROUnitID = rou.ID and roua.AttributeID = ca.ID
        where rou.SerialNo = ps.SerialNo) as RFC_NO,
        (Select max(CustomerReference) from [pls].[ROHeader] where ID = ps.ROHeaderID) as 'RMA',
        (select max(ModelNo) from [pls].[PartNo] where partno = ps.PartNo) as 'Model',
        ps.SerialNo as 'TSB_Serial_No',
        (Select max(roua.Value) from [pls].[ROUnit] rou
        inner join [pls].[CodeAttribute] ca on ca.AttributeName ='MCODE'
        inner join [pls].[ROUnitAttribute] roua on roua.ROUnitID = rou.ID and roua.AttributeID = ca.ID
        where rou.SerialNo = ps.SerialNo) as 'MCODE',
        (Select max(roua.Value) from [pls].[ROUnit] rou
        inner join [pls].[CodeAttribute] ca on ca.AttributeName ='VMI_ACTION'
        inner join [pls].[ROUnitAttribute] roua on roua.ROUnitID = rou.ID and roua.AttributeID = ca.ID
        where rou.SerialNo = ps.SerialNo) as 'VMI_ACTION',
        (Select max(roua.Value) from [pls].[ROUnit] rou
        inner join [pls].[CodeAttribute] ca on ca.AttributeName ='ODM'
        inner join [pls].[ROUnitAttribute] roua on roua.ROUnitID = rou.ID and roua.AttributeID = ca.ID
        where rou.SerialNo = ps.SerialNo) as 'ODM',
        (Select max(roua.Value) from [pls].[ROUnit] rou
        inner join [pls].[CodeAttribute] ca on ca.AttributeName ='COO'
        inner join [pls].[ROUnitAttribute] roua on roua.ROUnitID = rou.ID and roua.AttributeID = ca.ID
        where rou.SerialNo = ps.SerialNo) as 'COO',
        (SELECT TOP 1 WOSA.[Value] FROM pls.WOHeader WOH INNER JOIN pls.WOStationHistory WOSH
        ON WOH.ID = WOSH.WOHeaderID INNER JOIN pls.WOStationAttribute WOSA
        ON WOSH.ID = WOSA.WOStationHistoryID INNER JOIN pls.CodeAttribute CA
        ON CA.ID = WOSA.AttributeID
        WHERE WOH.ProgramID = ps.ProgramID AND CA.AttributeName = 'ACTION' AND WOSH.WOHeaderID = ps.WOHeaderID
        ORDER BY WOSH.ID DESC) as 'WO_ACTION',
        ps.LastActivityDate
from [pls].[PartPalletBoxNo] b
inner join [pls].[PartSerial] ps on ps.ProgramID = b.ProgramID and ps.PalletBoxNo = b.CustomPalletBoxNo

where b.programID = 10028
and b.CustomPalletBoxNo =  '<PalletBoxNo>') as TAB_1
";


            //if (!string.IsNullOrEmpty(PalletBoxNo))
            //    query += "AND SOH.CustomerReference LIKE '%" + PalletBoxNo + "%'";

            query = query.Replace("<PalletBoxNo>", PalletBoxNo);
            query += "Where SUBSTRING(Box_No, 1, 1) = 'G'";
            DataTable dt = oDAL.GetData(query);

           
                filterString += "> Box No. = '" + PalletBoxNo + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("146", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPacketList = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }

        #endregion
    } 
}