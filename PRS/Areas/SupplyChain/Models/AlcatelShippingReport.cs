using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class AlcatelShippingReport
    {
        cDAL oDAL = new cDAL("ACTIVE");

        #region Fields
        [Display(Name = "Program:")]
        public string ProgramName { get; set; }
        [Display(Name = "Program:")]
        public string ProgramID { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstAlcatelShipping { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
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
        #endregion
        public bool GetList(string frmDt, string toDt, string ProgramId, string ProgramName)
        {
            string query = string.Empty;
            query = @"SELECT
     SOH.ProgramID
    ,SOH.ID
	,P.Name AS Program
	,CustomerReference
	,SOL.PartNo
	,SOU.SerialNo
    ,(SELECT CASE WHEN COUNT(PS.SerialNo) > 0 THEN 'Y' ELSE 'N' END
       FROM pls.partserial PS
       WHERE PS.SerialNo = SOU.SerialNo AND PS.ProgramID = SOH.ProgramID ) HAS_SN
	,CC.Description AS Configuration
	,ThirdPartyReference
    ,SOSI.TrackingNo
    ,SUM(SOL.QtyToShip) AS QtyToShip
	,SUM(SOL.QtyReserved)AS QtyShipped
    ,CASE 
	      WHEN CustomerReference like '%AVR%' AND ThirdPartyReference LIKE '%ALCATEL RMA%' THEN 'AVR' 
	      WHEN CustomerReference like '%DOA%' AND ThirdPartyReference LIKE '%ALCATEL RMA%' THEN 'DOA'
		  WHEN CustomerReference like '%LLW%' AND ThirdPartyReference LIKE '%ALCATEL RMA%' THEN 'RTF/CSR'
		  WHEN CustomerReference like '%DOA1%' AND ThirdPartyReference LIKE '%ALCATEL RMA%' THEN 'DOA1'
		  WHEN ThirdPartyReference like '%ALCATEL ETOS%' THEN 'ETOS'
		  WHEN ThirdPartyReference like '%ALCATEL FA%' THEN 'FA'
		  WHEN ThirdPartyReference like '%CTO ETOS%' THEN 'CTO ETOS'
	      WHEN ThirdPartyReference like '%ALCATEL CTO%' THEN 'CTO'
		  WHEN ThirdPartyReference like '%ALI%' THEN 'BS60 ALI'
	      WHEN CustomerReference like '%ALI DOA%' AND ThirdPartyReference LIKE '%ALI DOA%' THEN 'INT'
	ELSE 'ETOS' END AS CustOrderType
	,CS.Description AS Status
	,U.Username
	,SOH.CreateDate
	,SOH.LastActivityDate
FROM pls.SOHeader SOH
INNER JOIN PLS.SOLine SOL ON SOL.SOHeaderID = SOH.ID
INNER JOIN pls.SOUnit SOU ON SOU.SOLineID = SOL.ID
INNER JOIN PLS.Program P ON P.ID = SOH.ProgramID
INNER JOIN PLS.CodeAddressDetails CAD ON CAD.AddressID = SOH.AddressID AND CAD.AddressType = 'ShipTo'
INNER JOIN PLS.CodeStatus CS ON CS.ID = SOH.StatusID
INNER JOIN PLS.[User] U ON U.ID = SOH.UserID
LEFT JOIN PLS.CodeConfiguration CC ON CC.ID = SOL.ConfigurationID
LEFT OUTER JOIN pls.[SOShipmentInfo] SOSI ON SOH.ID = SOSI.SOHeaderID
LEFT JOIN pls.CodeAttribute CA on CA.AttributeName ='CUSTORDERTYPE'
LEFT JOIN pls.SOHeaderAttribute SOHA on SOHA.SOHeaderID = SOH.ID AND SOHA.AttributeID = CA.ID
WHERE cs.ID  IN ('18') AND SOH.ProgramID = '<programId>' 
";
            query += " AND CONVERT(Date, SOH.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, SOH.LastActivityDate) <= '<toDt>' ";
            query = query.Replace("<programId>", ProgramId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            query += @" GROUP BY
            SOH.ProgramID
           ,SOH.ID
           ,P.Name
           ,CustomerReference
           ,SOL.PartNo
           ,SOU.SerialNo
           ,CC.Description
           ,ThirdPartyReference
           ,SOSI.TrackingNo
           ,SOHA.Value
           ,CS.Description
           ,U.Username
           ,SOH.CreateDate
           ,SOH.LastActivityDate ";

            query += "ORDER BY CreateDate DESC ";

            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("185", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstAlcatelShipping = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}