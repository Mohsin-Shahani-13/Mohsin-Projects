using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaReceivingVolume
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }

        [Display(Name = "Program:")]
        public string program_Id { get; set; }

        [Display(Name = "Serial No.:")]
        public string SerialNo { get; set; }

        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstMetaReceivingVolume { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList(string programId, string ProgramName, string serialNo)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"
select   pt.ProgramID
		 ,p.Name
        ,pt.OrderHeaderID
		,pt.Id as TRANSACTION_ID  
       ,'RO-RECEIVE' as [TRANSACTION]  
       ,pt.OrderHeaderId as ORDER_NO  
       ,pt.CustomerReference as RMA_REF  
       ,pt.PartNo as PART_NO  
       ,pn.Description as [DESCRIPTION]  
       ,pt.SerialNo as [SERIAL_NO]  
       ,pt.Qty as QUANTITY  
       ,case when pt.ToLocation = 'SB.SB.0.0.0' then 'GOODSTOCK' 
			 when pt.SerialNo ='*' then 'REC1' 
	         else  'OCR' end as WORK_TYPE_ID 
       ,pn.ModelNo as TYPE_DESIGNATION  
       ,usr.Username as RECEIVED_BY  
       ,pt.CreateDate as DATE_TIME    
	   ,roha.Value as ORDER_TYPE 
	   ,pt_Dekikt.SerialNo as ACC_SERIAL
	   ,pt_Dekikt.PartNo as ACC_PART_NO
	   , CONVERT(varchar(50), NULL) as ACC_SERIAL1
	   , CONVERT(varchar(50), NULL) as ACC_PART_NO1
	   , CONVERT(varchar(50), NULL) as ACC_SERIAL2
	   , CONVERT(varchar(50), NULL) as ACC_PART_NO2
	   , CONVERT(varchar(50), NULL) as ACC_SERIAL3
	   , CONVERT(varchar(50), NULL) as ACC_PART_NO3 
	   , ROW_NUMBER() over(partition by pt_Dekikt.SourceSerialNo order by pt_Dekikt.Id) as RowNumber
into #TmpRcv
from pls.PartTransaction pt   
INNER JOIN Pls.Program P ON P.ID = PT.programId
Left join pls.PartTransaction pt_Dekikt on pt_Dekikt.PartTransactionID = 27  and pt_Dekikt.ProgramID = pt.ProgramID and pt_Dekikt.SourceSerialNo = pt.SerialNo 
inner join pls.PartNo pn on pn.PartNo = pt.PartNo  
inner join pls.[User] usr on usr.Id = pt.UserId  

 

left join pls.CodeAttribute car on car.AttributeName = 'REQUESTTYPE'  
left join pls.ROHeaderAttribute roha on roha.RoHeaderId = pt.OrderHeaderId and roha.AttributeId = car.Id  

 

-- for getting work_type_id column  
left join pls.WOHeader woh ON woh.ProgramId = pt.ProgramId AND woh.SerialNo = pt.SerialNo AND woh.PartNo = pt.PartNo  
left join pls.CodeRepairType crt ON crt.ID = woh.RepairTypeId   
left join pls.WOStationHistory wsh ON wsh.WOHeaderId = woh.Id AND wsh.WorkStationId = woh.WorkStationId AND wsh.Iteration = 1  
left join pls.CodeAttribute cas ON cas.AttributeName = 'FGITYPE'  
left join pls.WOStationAttribute wosa ON wosa.WoStationHistoryId = WSH.Id AND wosa.AttributeId = cas.Id  

 

-- for getting default repair type if order not created  
left join pls.CodeAttribute cap ON cap.AttributeName = 'REPAIRTYPE'  
left join pls.PartNoAttribute pna ON pna.PartNo = pt.PartNo AND pna.AttributeId = cap.Id  

 

where 
   pt.PartTransactionId = 1 -- 1=RO-RECEIVE  
   and pt.UserId <> 52 -- 52=usrJobs  
   and pt.CustomerReference not like 'PL-%'  and pt.CustomerReference not like 'RWK%'  
    
 ";
            if (programId != "0" && programId != null)
            {
                query += " AND pt.ProgramId = '" + programId + "' ";
            }
            else
            {
                query += " AND pt.ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            if (!string.IsNullOrEmpty(serialNo))
                query += " and pt.SerialNo like '%" + serialNo + "%' ";

            query += @" update tr1 set ACC_SERIAL1 = 
    (
     select tr2.ACC_SERIAL

     from #TmpRcv tr2 
	 where tr2.SERIAL_NO = tr1.SERIAL_NO AND tr2.RowNumber = 1
    )

    from #TmpRcv tr1
	where tr1.RowNumber = 1






   update tr1 set ACC_SERIAL2 =
    (
     select tr2.ACC_SERIAL
     from #TmpRcv tr2 
	 where tr2.SERIAL_NO = tr1.SERIAL_NO AND tr2.RowNumber = 2
	)
	from #TmpRcv tr1
	where tr1.RowNumber = 1




     update tr1 set ACC_SERIAL3 =
    (
     select tr2.ACC_SERIAL
     from #TmpRcv tr2 
	 where tr2.SERIAL_NO = tr1.SERIAL_NO AND tr2.RowNumber = 3
	)
	from #TmpRcv tr1
	where tr1.RowNumber = 1





update tr1 set ACC_PART_NO1 =
    (
     select tr2.ACC_PART_NO
     from #TmpRcv tr2 
	 where tr2.SERIAL_NO = tr1.SERIAL_NO AND tr2.RowNumber = 1
	)
	from #TmpRcv tr1
	where tr1.RowNumber = 1






   update tr1 set ACC_PART_NO2 =
    (
     select tr2.ACC_PART_NO
     from #TmpRcv tr2 
	 where tr2.SERIAL_NO = tr1.SERIAL_NO AND tr2.RowNumber = 2
	)
	from #TmpRcv tr1
	where tr1.RowNumber = 1




     update tr1 set ACC_PART_NO3 =
    (
     select tr2.ACC_PART_NO
     from #TmpRcv tr2 
	 where tr2.SERIAL_NO = tr1.SERIAL_NO AND tr2.RowNumber = 3
	)
	from #TmpRcv tr1
	where tr1.RowNumber = 1


    delete from #TmpRcv where RowNumber > 1

  select*
  from #TmpRcv tr

  drop table #TmpRcv ";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(serialNo))
                filterString += "| Serial No. Like '" + serialNo + "' ";

            //if (!string.IsNullOrEmpty(PartNo))
            //    filterString += "> Part No. = '" + PartNo + "' ";
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("122", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaReceivingVolume = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}