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
    public class AuditDefectsReport
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
		public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstAuditDefectsReport { get; set; }

        public List<object> lstMst = new List<object>();
		#endregion
		#region Methods 
		public DataTable GetProgramBySite()
		{
			oDAL = new cDAL("ACTIVE");
			string sites = HttpContext.Current.Session["DefaultSite"].ToString();

			string query = string.Empty;
			query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>' AND Name = 'GOPRO'
                      ORDER BY NAME ";
			query = query.Replace("<site>", sites);
			DataTable dt = oDAL.GetData(query);
			return dt;
		}

		public bool GetList(string frmDt, string toDt, string programId, string programName)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @" 
declare @datefrom datetime = '@DateStart'
declare @dateto datetime = '@DateEnd'
	
declare @Report table (
[Lineno] int,
[Material Number] varchar(100),
[Serial Number] varchar(100),
[RMA/To Number] varchar(200),
[Audit Date] datetime,
[Def Code] varchar(100)
)
insert into @Report
select
	row_number() over (order by woh.SerialNo) as 'LineNo',
	pt3.PartNo as 'Material Number', 
	woh.SerialNo as 'Serial Number',
	'' as 'RMA/To Number',	
	wosh.LastActivityDate as 'Audit Date',
	replace(cf.Code,'GOPRO_','') as 'Def Code'
from 
	pls.WOHeader woh with (nolock)	
	inner join pls.WOLine wol with (nolock) on woh.ID = wol.WOHeaderID
	inner join pls.WOUnit wou with (nolock) on wol.ID = wou.WOLineID		
	inner join pls.WOUnitCodes wouc with (nolock) on wou.ID = wouc.WOUnitID				
		and wouc.FaultIDWorkStationID = 9
	inner join pls.CodeFault cf with (nolock) on wouc.FaultID = cf.ID	
	inner join pls.WOStationHistory wosh with (nolock) on wosh.ID =
	(				
			select max(ID)
			from pls.WOStationHistory wosh1 with (nolock)
			where wosh1.WOHeaderID = woh.ID
			and wosh1.CreateDate <= wouc.CreateDate	
			and wosh1.WorkStationID = 9
	)	
	inner join pls.PartTransaction pt3 with (nolock) on pt3.ID = 
	(
		SELECT MAX(pt4.ID)
        FROM pls.PartTransaction pt4  with (nolock)
        WHERE pt4.ProgramID = @programID            
		AND pt4.SerialNo = woh.SerialNo            
		AND pt4.CreateDate <= wosh.LastActivityDate
		and pt4.OrderType = 'WO'					
		and pt4.PartTransactionID != 36 
		and pt4.OrderHeaderID = woh.ID
	) 
	inner join pls.WOStationAttribute wosha with (nolock) on wosh.ID = wosha.WOStationHistoryID 
		and wosha.AttributeID = 316	
	and wouc.CreateDate <= wosh.LastActivityDate	
where
	woh.ProgramID = @programID
	and wosh.LastActivityDate between @datefrom and @dateto	
	and (cf.Code like 'GOPRO_4%') 
insert into @Report
select
	row_number() over (order by woh.SerialNo) as 'LineNo',
	pt3.PartNo as 'Material Number', 
	woh.SerialNo as 'Serial Number',
	'' as 'RMA/To Number',	
	wosh.LastActivityDate as 'Audit Date',
	'PASS' as 'Def Code'
from 
	pls.WOHeader woh with (nolock)	
	inner join pls.WOLine wol with (nolock) on woh.ID = wol.WOHeaderID
	inner join pls.WOUnit wou with (nolock) on wol.ID = wou.WOLineID				
	inner join pls.WOStationHistory wosh with (nolock) on woh.ID = wosh.WOHeaderID
		and wosh.WorkStationID = 9	
		and wosh.StatusID = 24
	inner join pls.CodeStatus cs on wosh.StatusID = cs.ID	
		and cs.Description = 'CLOSED'
	inner join pls.PartTransaction pt3 with (nolock) on pt3.ID = 
	(
		SELECT MAX(pt4.ID)
        FROM pls.PartTransaction pt4  with (nolock)
        WHERE pt4.ProgramID = @programID            
		AND pt4.SerialNo = woh.SerialNo            
		AND pt4.CreateDate <= wosh.LastActivityDate
		and pt4.OrderType = 'WO'					
		and pt4.PartTransactionID != 36 
		and pt4.OrderHeaderID = woh.ID
	) 		
	left join pls.WOStationHistoryFailReasons woshfr on wosh.ID = woshfr.WOStationHistoryID
where
	woh.ProgramID = @programID
	and wosh.LastActivityDate between @datefrom and @dateto		
	and woshfr.ID is null
			
declare @i int = 0
declare @j int = 0
declare @CurrentWOHeaderID int = 0
declare @CurrentRma varchar(100) = ''
declare @CurrentSerialNo varchar(100) = ''
declare @CurrentEntryTransactionID int = 0 
declare @CurrentEntryTransactionTypeID int = 0 
declare @CurrentMovementDate datetime
declare @SourceSerialNumber varchar(100) = ''
declare @CurrentReceiptDate varchar(100) = ''
select @j = count(1) from @Report
while (@i < @j)
begin
	set @i = @i + 1
	select 	
	@CurrentSerialNo = t.[Serial Number],				
	@CurrentMovementDate = t.[Audit Date]
	from 
	@Report t
	where
	t.[Lineno] = @i
	set @CurrentEntryTransactionID = 0
	set @CurrentEntryTransactionTypeID = 0
	set @CurrentRma  = ''	
	set @SourceSerialNumber = ''
	set @CurrentReceiptDate = ''
	
	
	
	select top 1 
		@CurrentEntryTransactionID = pt.ID, 
		@CurrentEntryTransactionTypeID = pt.PartTransactionID, 
		@SourceSerialNumber = pt.SourceSerialNo,		
		@CurrentRma = pt.CustomerReference		
	from pls.PartTransaction pt with (nolock)
	where pt.SerialNo = @CurrentSerialNo 
	and pt.CreateDate < @CurrentMovementDate
	and pt.ProgramID = @programID
	and pt.PartTransactionID in (1,21,27)
	order by pt.CreateDate desc
	
	if @CurrentEntryTransactionTypeID = 21
	begin
		select @CurrentRma = 'NA'			
		select top 1  @CurrentRma = pt.CustomerReference
		from pls.PartTransaction pt with (nolock)
		where pt.SerialNo = @CurrentSerialNo
		and pt.CreateDate < @CurrentMovementDate
		and pt.ProgramID = @programID
		and pt.PartTransactionID = 1
		order by pt.PartTransactionID desc
		
		if @CurrentRma = 'NA'
		BEGIN
			select top 1  @CurrentRma = woha.Value
			from pls.WOHeader woh  (NOLOCK)
			inner join pls.WOHeaderAttribute woha (NOLOCK) on woh.ID = woha.WOHeaderID
			and woha.AttributeID = 91			
			where
			woh.ProgramID = @programID
			and woh.CreateDate <= @CurrentMovementDate
			and woh.SerialNo = @CurrentSerialNo				
			order by woh.CreateDate desc
			if @CurrentRma = 'NA'
			begin
				select top 1  @CurrentRma = woha.Value
				from pls.WOHeader woh  (NOLOCK)
				inner join pls.WOHeaderAttribute woha (NOLOCK) on woh.ID = woha.WOHeaderID
				and woha.AttributeID = 91			
				where
				woh.ProgramID = @programID				
				and woh.SerialNo = @CurrentSerialNo				
				order by woh.CreateDate asc
			end
		END
	end
	if @CurrentEntryTransactionTypeID = 27
	begin		
		select top 1 
			@CurrentEntryTransactionID = pt.ID,			
			@CurrentRma = pt.CustomerReference		
		from pls.PartTransaction pt with (nolock)
		where pt.SerialNo = @SourceSerialNumber 
		and pt.CreateDate < @CurrentMovementDate
		and pt.ProgramID = @programID
		and pt.PartTransactionID = 1
		order by pt.CreateDate desc				
	end
	
	update @Report set [RMA/To Number] = @CurrentRma where [Lineno] = @i
end
select 
r.[Lineno],
r.[Material Number],
r.[Serial Number],
r.[RMA/To Number],
format(r.[Audit Date],'yyyy-MM-dd HH:mm:ss.fff') as [Audit Date],
r.[Def Code]
from @Report r order by r.[Lineno] asc
 ";

            string programID = HttpContext.Current.Session["ProgramForSite"].ToString();

            query = query.Replace("@DateStart", frmDt);
            query = query.Replace("@DateEnd", toDt);
            query = query.Replace("@programID", programId);


            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("223", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstAuditDefectsReport = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}