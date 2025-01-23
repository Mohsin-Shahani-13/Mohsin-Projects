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
    public class WorkOrderDetail
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
        public string ProgramID { get; set; }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public string OrderType { get; set; }
        public List<Hashtable> lstWorkOrderDetail { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }

        #endregion

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
        public bool GetList(string ProgramId, string ProgramName, string frmDt, string toDt, bool isAllDate, string serialNo, string partNo)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
select woh.ProgramId
, woh.ID as WOHeaderId
, woh.CreateDate
, woh.PartNo
, pn.Description as PartDesc
, woh.SerialNo
,(SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
         FROM pls.partserial PS
         WHERE PS.SerialNo = WOH.SerialNo AND PS.ProgramID = WOH.ProgramID ) HAS_SN
, pn.ModelNo as Family
, cc.Description as SecondaryCommodity
, cpt.Description as PartType
, woh.WorkStationId
, case when wsd.code is null then cws.description else wsd.description end as WorkStation
, crt.Id as RepairTypeId
, crt.Description as RepairType
, case when woh.StatusId = 19 then 'WIP' else 'HOLD' end WOStatus
, wol.ComponentPartNo as CompPartNo
, wou.SerialNo as CompSerialNo
, convert(varchar(50), NULL) as CompPartNo1
, convert(varchar(50), NULL) as CompSerialNo1
, convert(varchar(50), NULL) as Comp1Commodity
, convert(varchar(50), NULL) as CompPartNo2
, convert(varchar(50), NULL) as CompSerialNo2
, convert(varchar(50), NULL) as Comp2Commodity
, convert(varchar(50), NULL) as CompPartNo3
, convert(varchar(50), NULL) as CompSerialNo3
, convert(varchar(50), NULL) as Comp3Commodity
, convert(varchar(50), NULL) as Grade
, convert(varchar(50), NULL) as FGIType
, convert(varchar(50), NULL) as Country
, convert(varchar(50), NULL) as ReidPartNo
, convert(varchar(50), NULL) as ReidSerialNo
, convert(varchar(500), NULL) as FailDesc
, convert(varchar(50), NULL) as HoldLoc
, convert(varchar(50), NULL) as HoldPallet
, convert(varchar(50), NULL) as PalletAttr
, 0 as wshId
into #tmp
from pls.WOHeader woh 
left join pls.CodeWorkStation cws ON cws.ID = woh.WorkstationID
left join pls.CodeWorkStationCustomDescription wsd ON
                   wsd.ProgramID = woh.ProgramID 
                   AND wsd.RepairTypeID = woh.RepairTypeID
                   AND wsd.CodeWorkStationID = woh.WorkStationID
left join pls.WOLine wol on wol.WOHeaderID = woh.ID and wol.StatusID = 14 
left join pls.PartNo pn on pn.PartNo = wol.ComponentPartNo
left join pls.WOUnit wou on wou.WOLineId = wol.Id
left join pls.CodeRepairType crt on crt.Id = woh.RepairTypeId
left join pls.CodeCommodity cc on cc.Id = pn.SecondaryCommodityId
left join pls.CodePartType cpt on cpt.Id = pn.PartTypeId  ";

            if (ProgramId != "0")
            {
                query += "WHERE WOH.ProgramId = '" + ProgramId + "' ";
            }
            else
            {
                query += "WHERE WOH.ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }


            query += @"and woh.StatusID in (19, 28) "; /*-- picked only open orders*/

 if (isAllDate == false)
            {
                query += "AND CONVERT(Date, WOH.CreateDate) >= '<frmDt>' AND CONVERT(Date, WOH.CreateDate) <= '<toDt>' ";
            }

            if (!string.IsNullOrEmpty(serialNo))
                query += "AND WOH.SerialNo LIKE '%" + serialNo + "%' ";
            if (!string.IsNullOrEmpty(partNo))
                query += "AND WOH.PartNo LIKE '%" + partNo + "%'  ";

            query += @"
update t set CompPartNo1 = t1.CompPartNo, CompSerialNo1 = t1.CompSerialNo
from #tmp t 
inner join #tmp t1 on t.SerialNo = t1.SerialNo and t1.SecondaryCommodity = 'LEFTCONTROLLER'
where t.SecondaryCommodity = 'Headset'

update t set CompPartNo2 = t1.CompPartNo, CompSerialNo2 = t1.CompSerialNo
from #tmp t 
inner join #tmp t1 on t.SerialNo = t1.SerialNo and t1.SecondaryCommodity = 'RIGHTCONTROLLER'
where t.SecondaryCommodity = 'Headset'

update t set CompPartNo3 = t1.CompPartNo, CompSerialNo3 = t1.CompSerialNo
from #tmp t 
inner join #tmp t1 on t.SerialNo = t1.SerialNo and t1.SecondaryCommodity = 'CHARGING DOCK'
where t.SecondaryCommodity = 'Headset'


update t set Comp1Commodity = cc.Description 
from #tmp t
inner join pls.partNo pn on pn.PartNo = t.CompPartNo1
inner join pls.CodeCommodity cc on cc.Id = pn.SecondaryCommodityId

update t set Comp2Commodity = cc.Description 
from #tmp t
inner join pls.partNo pn on pn.PartNo = t.CompPartNo2
inner join pls.CodeCommodity cc on cc.Id = pn.SecondaryCommodityId

update t set Comp3Commodity = cc.Description 
from #tmp t
inner join pls.partNo pn on pn.PartNo = t.CompPartNo3
inner join pls.CodeCommodity cc on cc.Id = pn.SecondaryCommodityId

update t set wshId = isnull((select max(id) from pls.WoStationHistory 
                      where WOHeaderId = t.WOHeaderId and WorkStationId = t.WorkStationId and RepairTypeId = t.RepairTypeId
                     ), 0)
from #tmp t

update t set Grade = (select top 1 value from pls.vWOStationAttribute where WOStationHistoryId = t.WshId and Attribute = 'GRADE' order by ID Desc) from #tmp t
update t set FGIType = (select top 1 value from pls.vWOStationAttribute where WOStationHistoryId = t.WshId and Attribute = 'FGITYPE' order by ID Desc) from #tmp t
update t set Country = (select top 1 value from pls.vWOStationAttribute where WOStationHistoryId = t.WshId and Attribute = 'COUNTRY' order by ID Desc) from #tmp t
update t set ReidPartNo = (select top 1 value from pls.vWOStationAttribute where WOStationHistoryId = t.WshId and Attribute = 'ReIDPartNumber' order by ID Desc) from #tmp t
update t set ReidSerialNo = (select top 1 value from pls.vWOStationAttribute where WOStationHistoryId = t.WshId and Attribute = 'ReIDSerialNumber' order by ID Desc) from #tmp t
update t set FailDesc = (select top 1 value from pls.vWOStationAttribute where WOStationHistoryId = t.WshId and Attribute = 'FAILURE DESCRIPTION' order by ID Desc) from #tmp t

update t set HoldLoc = pl.LocationNo
from #tmp t 
inner join pls.PartSerial ps on ps.ProgramId = t.ProgramId and ps.WOHeaderId = t.WOHeaderId
inner join pls.PartLocation pl on pl.Id = ps.LocationId
where t.WOStatus = 'Hold'

update t set HoldPallet = ps.PalletBoxNo
from #tmp t 
inner join pls.PartSerial ps on ps.ProgramId = t.ProgramId and ps.WOHeaderId = t.WOHeaderId
where t.WOStatus = 'Hold'

update t set PalletAttr = pba.Value
from #tmp t 
inner join pls.vPartPalletBoxNoAttribute pba on pba.CustomPalletBoxNo = t.HoldPallet and AttributeName = 'PalletNo'
where WOStatus = 'Hold'


delete from #tmp where SecondaryCommodity <> 'Headset'

 

select *
from #tmp 
order by CreateDate desc, SerialNo, SecondaryCommodity

drop table #tmp;
 ";

          

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

           

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (isAllDate == false)
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            if (!string.IsNullOrEmpty(partNo))
                filterString += "| Part No. Like '" + partNo + "' ";

            if (!string.IsNullOrEmpty(serialNo))
                filterString += "| Serial No. Like '" + serialNo + "' ";

           

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("160", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstWorkOrderDetail = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}