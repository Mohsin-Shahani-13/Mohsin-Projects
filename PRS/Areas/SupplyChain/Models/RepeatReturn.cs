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
    public class RepeatReturn
    {
        cDAL oDAL = new cDAL("INIT");
        #region Fields
        public bool isAllDate { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "Serial No.:")]
        public string SerialNo { get; set; }
        public string hasRepaired { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("Active");
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
        public List<Hashtable> lstRepeatReturn { get; set; }
        #endregion
        #region Methods 
        public bool GetList(string frmDt, string toDt, bool isAllDate, string programId, string programName, string serialNo, string hasRepaired, string hasRepairedText)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
		SELECT  
                Recnum, 
				TranId, 
				ProgramId, 
				ProgramName, 
				SerialNo,
                (SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
				FROM pls.partserial PS
				WHERE PS.SerialNo = SerialNo AND PS.ProgramID = ProgramID ) HAS_SN,
                PartNo,
                CustomerReference,
                FORMAT(RcvdOn, 'yyyy/MM/dd') AS RcvdOn , 
				FORMAT(ShippedOn, 'yyyy/MM/dd') AS ShippedOn ,
			    Days, 
				CASE WHEN Under30 = 1 THEN 'Y' ELSE 'N' END Under30,
              	CASE WHEN Under60 = 1 THEN 'Y' ELSE 'N' END Under60,
	   			CASE WHEN Under90 = 1 THEN 'Y' ELSE 'N' END Under90,
				PrevFailCode, 
				CrntFailCode,
                PrevRetReason,
				CrnRetReason,
                RepeatReturnCategory,
				SameFailure, 
                ShipTimes,
				RepairedBy, 
				RepairedOn,
				MinOrderId, 
				MaxOrderId,
                PreviousROType,
                LastPrvWorkStation
      ,[PrvRecvWoID]
	FROM        PlusRS.rpt.RepeatReturn RepeatReturn
";
            //query = query.Replace("<frmDt>", frmDt);
            //query = query.Replace("<toDt>", toDt);

            if (programId != "0")
            {
                query += "WHERE ProgramId = '" + programId + "' ";
            }
            else
            {
                query += "WHERE ProgramId IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }

            if (isAllDate != true)
            {
                if (!string.IsNullOrEmpty(frmDt) && !string.IsNullOrEmpty(toDt))
                    // query += "AND Format(Cast(CD.MMDDYYYY as date), 'yyyy.MM.dd') = '" + OrdCreatOnFrm + "'";
                    query += " AND (FORMAT(CAST(RcvdOn as date), 'yyyy.MM.dd')) BETWEEN '" + frmDt + "' " + "AND '" + toDt + "' ";
            }
            

            if (!string.IsNullOrEmpty(serialNo))
                query += "AND SerialNo LIKE '%" + serialNo + "%' ";

            if (hasRepaired == "Y")
            {
                query += "AND RepairedBy Is Not Null ";
            }

            if (hasRepaired == "N")
            {
                query += "AND RepairedBy Is Null ";
            }


            query += " ORDER BY Days ASC";

            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";


            if (isAllDate != true)
            {
                filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            }
            else
            {

            }
            

            if (!string.IsNullOrEmpty(serialNo))
                filterString += " | Serial No. Like '" + serialNo + "' ";

            if (!string.IsNullOrEmpty(hasRepairedText))
                filterString += " | Has Repaired = '" + hasRepairedText + "' ";

            DataTable dt = oDAL.GetData(query);



            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("101", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstRepeatReturn = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}