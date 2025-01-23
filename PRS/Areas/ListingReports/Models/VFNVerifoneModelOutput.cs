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
    public class VFNVerifoneModelOutput
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
        public List<Hashtable> lstVFNVerifoneModelOutput { get; set; }

        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @" 
---Verifone Model Output---
DECLARE @inicio datetime = CAST(convert(varchar,'<frmDt>', 23) + ' 06:59:00'   AS datetime)
DECLARE @fin datetime =  CAST(convert(varchar,'<toDt>', 23) +  ' 07:00:00'   AS datetime)
SELECT 
      vWH.PartNo
      , vWH.SerialNo
      , CRT.Description RepairTypeDescription
      , CS.Description
      , (select pn.Description from pls.PartNo pn where pn.PartNo =  vWH.PartNo)  as Model
      , CWS.Description	AS CWS_Description
      , CWSD.Description AS CWSD_Description
      , IIF(vWOH.IsPass is null, 'N/A', IIF(vWOH.IsPass =1 , 'PASS', 'FAIL')) as Result
      , DATEPART(HOUR, vWOH.CreateDate) as HOUR
  FROM pls.WOStationHistory vWOH WITH (NOLOCK)
  LEFT JOIN pls.WOHeader vWH on vWH.ID = vWOH.WOHeaderID
  LEFT JOIN pls.CodeWorkStationCustomDescription vWC on vWC.ProgramID = vWH.ProgramID and vWC.RepairTypeID = vWH.RepairTypeID and  vWC.CodeWorkStationID = vWOH.WorkStationID
  LEFT JOIN pls.CodeWorkStation CWS on CWS.ID = vWOH.WorkStationID
  LEFT JOIN pls.CodeWorkStation CWSD on CWSD.ID = vWOH.ToWorkStationID
  left join pls.CodeRepairType CRT on CRT.ID = vWH.RepairTypeID
  LEFT JOIN pls.CodeStatus CS WITH (NOLOCK) on CS.ID = vWOH.StatusID
  where vWH.ProgramID = 10010
  and vWOH.LastActivityDate BETWEEN @inicio and  @fin
  and CWS.Description = 'gtest4' --'gtest4'-- 'Close'
  and CS.Description = 'CLOSED' --in ('Closed', 'WIP')
  and RIGHT(vWH.PartNo,3) <> '.UI'
  and vWOH.IsPass = 1
 ";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            DataTable dt = oDAL.GetData(query);

            filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("233", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstVFNVerifoneModelOutput = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
        #endregion
    }
}