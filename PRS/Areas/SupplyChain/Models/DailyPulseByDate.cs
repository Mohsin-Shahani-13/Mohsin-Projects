using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace IP.Areas.SupplyChain.Models
{
    public class DailyPulseByDate
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstDailyPulseByDate { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList(string frmDt, string toDt)
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"
with DAYTAT as (
select
case when (DATEDIFF(dd, ptr.CreateDate,ptp.CreateDate) + 1)  
      -(DATEDIFF(wk, ptr.CreateDate,ptp.CreateDate) * 2)  
      -(CASE WHEN DATENAME(dw, ptr.CreateDate) = 'Saturday' THEN 1 ELSE 0 END)  
      -(CASE WHEN DATENAME(dw, ptr.CreateDate) = 'Sunday' THEN 1 ELSE 0 END)  
      -(CASE WHEN DATENAME(dw, ptp.CreateDate) = 'Saturday' THEN 1 ELSE 0 END) 
       -(CASE WHEN DATENAME(dw, ptp.CreateDate) = 'Sunday' THEN 1 ELSE 0 END)
        -- Exclude holidays
- (SELECT COUNT(*) 
FROM PlusRS.dbo.holidays hc 
WHERE hc.HolidayDate BETWEEN ptr.CreateDate AND ptp.CreateDate) 
        <=5 
        then 1 else 0 
        end  INTAT
from pls.parttransaction pts
join pls.parttransaction ptr on left(ptr.serialno,20) = left(pts.serialno,20) 
                                 and ptr.programid = pts.programid 
                                 and ptr.parttransactionid = 1
join pls.parttransaction ptp on left(ptr.serialno,20) = left(ptp.serialno,20) 
                                and ptr.programid = ptp.programid 
                                and ptr.parttransactionid = 1 and ptp.parttransactionID  in (7,8)
join pls.PartSerial ps on left(ptr.serialno,20) = left(ps.serialno,20) 
								and ps.ProgramID = ptr.ProgramID 
								and ps.ROHeaderID = ptr.OrderHeaderID 
								and ps.WOHeaderID = ptp.OrderHeaderID 
								and ps.SOHeaderID = pts.OrderHeaderID
where pts.ProgramID = 10064
and pts.PartTransactionID = 18
and CONVERT(CHAR(10), pts.CreateDate, 126) = '<date>')
select CONVERT(CHAR(10), '<date>', 101) 'Date', 
'Docking Station' 'Commodity',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = 10064 AND CONVERT(CHAR(10), WO.CreateDate, 126) <= '<date>' 
and wo.id not in (select wsh.WOHeaderID from pls.WOStationHistory wsh where wsh.WorkStationID in (119,5) and CONVERT(CHAR(10), wsh.LastActivityDate, 126)  <= '<date>' 
and wsh.StatusID=24)) 'Current WIP Units',
'0' 'OCB Qty <= 5 Days',
'0' 'OCB Qty > 5 Days',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = 10064 
and  CONVERT(CHAR(10), WO.CreateDate, 126)>= dateadd(day,-5,'<date>') and CONVERT(CHAR(10), WO.CreateDate, 126) <= '<date>' 
and wo.id not in (select wsh.WOHeaderID from pls.WOStationHistory wsh where wsh.WorkStationID in (119,5) and CONVERT(CHAR(10), wsh.LastActivityDate, 126)  <= '<date>' 
and wsh.StatusID=24)) 'Units <= 5 Days',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = 10064 
and CONVERT(CHAR(10), WO.CreateDate, 126) < dateadd(day,-5,'<date>')
and wo.id not in (select wsh.WOHeaderID from pls.WOStationHistory wsh where wsh.WorkStationID in (119,5) and CONVERT(CHAR(10), wsh.LastActivityDate, 126)  <= '<date>' 
and wsh.StatusID=24)) 'Units > 5 Days',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = 10064 
and CONVERT(CHAR(10), WO.CreateDate, 126) >= dateadd(day,-5,'<date>') and CONVERT(CHAR(10), WO.CreateDate, 126) <= '<date>' 
and wo.id not in (select wsh.WOHeaderID from pls.WOStationHistory wsh where wsh.WorkStationID in (119,5) and CONVERT(CHAR(10), wsh.LastActivityDate, 126)  <= '<date>' and wsh.StatusID=24)
and wo.CustomerReference not in (select pt.SerialNo from pls.PartTransaction pt where pt.PartTransactionID  in (7,12,8) and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' and pt.ProgramID=wo.ProgramID )
and wo.SerialNo not in (select pt.SerialNo from pls.PartTransaction pt where pt.PartTransactionID  in (7,12,8) and CONVERT(CHAR(10),pt.CreateDate,126)< '<date>' and pt.ProgramID=wo.ProgramID )) 'Workable WIP <= 5 Days',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = 10064 
and CONVERT(CHAR(10), WO.CreateDate, 126) < dateadd(day,-5,'<date>')
and wo.id not in (select wsh.WOHeaderID from pls.WOStationHistory wsh where wsh.WorkStationID in (119,5) and CONVERT(CHAR(10), wsh.LastActivityDate, 126)  <= '<date>' and wsh.StatusID=24)
and wo.CustomerReference not in (select pt.SerialNo from pls.PartTransaction pt where pt.PartTransactionID  in (7,12,8) and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' and pt.ProgramID=wo.ProgramID )
and wo.SerialNo not in (select pt.SerialNo from pls.PartTransaction pt where pt.PartTransactionID  in (7,12,8) and CONVERT(CHAR(10),pt.CreateDate,126) < '<date>' and pt.ProgramID=wo.ProgramID )
and wo.SerialNo not in (select pt.SerialNo from pls.PartTransaction pt where pt.PartTransactionID  in (7,12,8) and CONVERT(CHAR(10),pt.CreateDate,126) < '<date>' and pt.ProgramID=wo.ProgramID )) 'Workable WIP > 5 Days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12  and pt.reason = 'NPI Vendor'
and pt.CreateDate >= dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' ) 'NPI Hold TRP <= 5 Days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12  and pt.reason = 'NPI Vendor'
and pt.CreateDate < dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' ) 'NPI Hold TRP > 5 Days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12  and pt.reason = 'NPI DELL' 
and pt.CreateDate >= dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' ) 'NPI Hold Dell <= 5 Days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12  and pt.reason = 'NPI DELL' 
and pt.CreateDate < dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' ) 'NPI Hold Dell > 5 Days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12  and pt.reason = 'Awaiting Parts'
and pt.CreateDate >= dateadd(day,datediff(day,1,'<date>'),0) and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>'  ) 'Material Hold in 1 day',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12  and pt.reason = 'Awaiting Parts'
and pt.CreateDate between dateadd(day,-3,'<date>') and dateadd(day,-2,'<date>')) 'Material Hold in 2-3 days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12  and pt.reason = 'Awaiting Parts'
and pt.CreateDate between dateadd(day,-5,'<date>') and dateadd(day,-4,'<date>')) 'Material Hold in 4-5 days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12  and pt.reason = 'Awaiting Parts'
and pt.CreateDate < dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' 
and not exists (select pt1.serialno from pls.PartTransaction pt1 where pt1.PartTransactionID=13 and CONVERT(CHAR(10),pt1.CreateDate,126) <='<date>' and pt.ProgramID = pt1.programID and pt1.SerialNo=pt.SerialNo )) 'Material Hold > 5 days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12 and pt.reason = 'Engineering Vendor' 
and pt.CreateDate >= dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' ) 'Engineering Hold TRP <= 5 Days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12 and pt.reason = 'Engineering Vendor' 
and  pt.CreateDate < dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' 
and not exists (select pt1.serialno from pls.PartTransaction pt1 where pt1.PartTransactionID=13 and CONVERT(CHAR(10),pt1.CreateDate,126) <='<date>' and pt.ProgramID = pt1.programID and pt1.SerialNo=pt.SerialNo )) 'Engineering Hold TRP > 5 Days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12 and pt.reason = 'Engineering DELL' 
and pt.CreateDate >= dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' ) 'Engineering Hold Dell <= 5 Days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12 and pt.reason = 'Engineering DELL' 
and pt.CreateDate < dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' 
and not exists (select pt1.serialno from pls.PartTransaction pt1 where pt1.PartTransactionID=13 and CONVERT(CHAR(10),pt1.CreateDate,126) <='<date>' and pt.ProgramID = pt1.programID and pt1.SerialNo=pt.SerialNo )) 'Engineering Hold Dell > 5 Days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12 and pt.reason = 'OEM' 
and pt.CreateDate >= dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' ) 'OEM Hold <= 5 Days',
(select count(distinct pt.SerialNo) from pls.PartTransaction pt 
where pt.ProgramID=10064 and pt.PartTransactionID=12 and pt.reason = 'OEM' 
and pt.CreateDate < dateadd(day,-5,'<date>') and CONVERT(CHAR(10),pt.CreateDate,126) <= '<date>' 
and not exists (select pt1.serialno from pls.PartTransaction pt1 where pt1.PartTransactionID=13 and CONVERT(CHAR(10),pt1.CreateDate,126) <='<date>' and pt.ProgramID = pt1.programID and pt1.SerialNo=pt.SerialNo )) 'OEM Hold > 5 Days',
0 'Unit Capacity',
(select count(*) from pls.PartTransaction pt where pt.ProgramID = '10064' and pt.PartTransactionID = '1' 
and CONVERT(CHAR(10), pt.CreateDate, 126) = '<date>')  'Volume Received',
(select count(*) from pls.PartTransaction pt where pt.ProgramID = '10064' 
and pt.PartTransactionID = '18' and CONVERT(CHAR(10), pt.CreateDate, 126) = '<date>') 'Volume Shipped',
(select isnull(SUM(INTAT),0) from DAYTAT) 'TAT Performance to 5 Days Qty',
(select count(*) from pls.woheader wo
join pls.WOStationHistory wsh1 on wsh1.WOHeaderID = wo.id and wsh1.WorkStationID = 21 and wsh1.ToWorkStationID = 22
join pls.WOStationHistory wsh2 on wsh2.WOHeaderID = wo.id and wsh2.WorkStationID = 22 and wsh2.ToWorkStationID = 19
join pls.WOStationHistory wsh3 on wsh3.WOHeaderID = wo.id and wsh3.WorkStationID = 19 and wsh3.ToWorkStationID = 20
WHERE WO.PROGRAMID = 10064 AND WO.StatusID =15
and CONVERT(CHAR(10),wo.LastActivityDate, 126) = '<date>')    'NFF Qty',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = '10064' AND WO.StatusID = '15'
and CONVERT(CHAR(10),wo.LastActivityDate, 126) = '<date>')    'Repaired Qty',
(select count(*) from pls.woheader wo
WHERE WO.PROGRAMID = '10064' AND WO.StatusID = '17'
and CONVERT(CHAR(10),wo.LastActivityDate, 126) = '<date>')     'Scrapped Qty' 
 ";
            query = query.Replace("<date>", frmDt);

            DataTable dt = oDAL.GetData(query);

            filterString += "> Program = DELL";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("244", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDailyPulseByDate = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}