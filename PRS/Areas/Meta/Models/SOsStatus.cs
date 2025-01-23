using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class SOsStatus
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }

        public string program_Id { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Customer Reference:")]
        public string custRef { get; set; }
        public string _EvDateFrom = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string EvDateFrom { get { return _EvDateFrom; } set { _EvDateFrom = value; } }
        [Display(Name = "To:")]
        public string _EvDateTo = DateTime.Now.ToString(Format.DateOnly);
        public string EvDateTo { get { return _EvDateTo; } set { _EvDateTo = value; } }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstSOsStatus { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        public string ProgramBySite { get; set; }
        public string ProgramNameforSite { get; set; }
        #endregion

        public string GetProgramBysite(string site)
        {

            oDAL = new cDAL("ACTIVE");
            string query = "SELECT Id  FROM pls.Program WHERE  NAME ='META' AND Site = '<Site>'";
            query = query.Replace("<Site>", site);
            DataTable dt = oDAL.GetData(query);

            var ProgramNameList = (from p in dt.AsEnumerable()
                                   select p.Field<object>("Id")).ToList().Distinct();
            ProgramNameforSite = String.Join("','", ProgramNameList).Insert(0, "'").Insert(String.Join("','", ProgramNameList).Insert(0, "'").Length, "'");
            return ProgramNameforSite;


        }


        public bool GetList(string programId, string ProgramName,  string frmDt, string toDt, bool ischecked, string custRef, string EvDateFrom, string EvDateTo)
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string progid = "";
            progid = GetProgramBysite(sites);

            cDAL oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            query = @"
WITH MessageStatus AS (
    SELECT
        sh.ID,
        sh.ProgramId,
        sh.CustomerReference,
        sh.LastActivityDate AS 'shipdate',
        si.Carrier,
        si.servicetypedescription,
        si.TrackingNo,
        cs.Description AS Status,
        th.EventCode,
        th.EventDate,
        th.EventDescription,
        OH.Message_Type,
        CASE 
            WHEN OH.processed = 'N' THEN CONCAT('Pending to be processed | ', CONVERT(VARCHAR, OH.Processed_Date, 120))
            WHEN OH.Processed = 'P' THEN CONCAT('In process | ', CONVERT(VARCHAR, OH.Processed_Date, 120))
            WHEN OH.Processed = 'A' THEN CONCAT('Success | ', CONVERT(VARCHAR, OH.Processed_Date, 120))
            WHEN OH.Processed = 'F' THEN CONCAT('Failure | ', CONVERT(VARCHAR, OH.Processed_Date, 120), '|', OH.Message) 
            ELSE NULL 
        END AS ProcessedStatus
    FROM 
        PLS.SOHEADER SH
    JOIN 
        PLS.SOShipmentInfo SI ON sh.id = si.soheaderid
    JOIN 
        PLS.CodeStatus CS ON CS.ID = si.StatusID
    JOIN 
        PLS.TrackingHeader TH ON si.Carrier = th.Carrier 
                            AND si.TrackingNo = th.TrackingNo 
                            AND si.ServiceTypeDescription = th.ServiceTypeDescription ";
            if (conType == "PROD")
            {
                query += @"	LEFT JOIN 
        Biztalk_Outmessages.dbo.Outmessage_hdr OH WITH (NOLOCK) 
        ON OH.CO_Number = CAST(SH.ID AS VARCHAR) AND CAST(sh.ProgramID AS NVARCHAR) = CAST(OH.Contract AS NVARCHAR) ";
            }
            else
            {
                query += @"LEFT JOIN [TPDC01Z004].Biztalk_Outmessages.dbo.Outmessage_hdr OH WITH (NOLOCK) 
        ON OH.CO_Number = CAST(SH.ID AS VARCHAR)
        AND CAST(sh.ProgramID AS NVARCHAR) = CAST(OH.Contract AS NVARCHAR)";
            }
          

query += @"WHERE 
    sh.programid = '<programId>' 
    AND sh.CustomerReference NOT LIKE 'pl-%'  ";


            filterString += " > Program = '" + ProgramName + "' ";

            //if (ischecked == false)
            //{
            //    query += "AND CONVERT(Date, sh.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, sh.LastActivityDate) <= '<toDt>' ";
            //    filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            //}

            if (ischecked == true)
            {
                query += "AND CONVERT(Date, th.EventDate) >= '<EvDateFrm>' AND CONVERT(Date, th.EventDate) <= '<EvDateTo>' " +
                    "and th.EventDescription = 'Delivered' ";
                
                filterString += " > Event Date From = '" + frmDt + "' To = '" + toDt + "' ";
            }
            else if (ischecked == false)
            {
                //filterString += "> Program = '" + ProgramName + "' ";
                query += "AND CONVERT(Date, sh.LastActivityDate) >= '<frmDt>' AND CONVERT(Date, sh.LastActivityDate) <= '<toDt>' ";
                //query += "AND CONVERT(Date, th.EventDate) >= '<EvDateFrm>' AND CONVERT(Date, th.EventDate) <= '<EvDateTo>' ";
                filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";
                //filterString += " | Event Date From = '" + frmDt + "' To = '" + toDt + "' ";
            }
            

            if (!string.IsNullOrEmpty(custRef))
            {
                query += "AND sh.CustomerReference LIKE '%" + custRef + "%' ";

                filterString += " | Customer Reference Like  '" + custRef + "' ";
            }




            query += @" )

SELECT
    ID,
    ProgramId,
    CustomerReference,
    shipdate,
    Carrier,
    servicetypedescription,
    TrackingNo,
    Status,
    EventCode,
    EventDate,
    EventDescription,
    [META-856] AS 'META-856',
    [META-Shipconfirmation] AS 'META-Shipconfirmation',
    [META-DeliveryNotification] AS 'META-DeliveryNotification',
    [META-ConShipNotification] AS 'META-ConShipNotification',
    [META-ConDeliveryNotification] AS 'META-ConDeliveryNotification',
    [META-Undeliverable] AS 'META-Undeliverable'
FROM 
    MessageStatus
PIVOT 
(
    MAX(ProcessedStatus) 
    FOR Message_Type IN (
        [META-856],
        [META-Shipconfirmation],
        [META-DeliveryNotification],
        [META-ConShipNotification],
        [META-ConDeliveryNotification],
        [META-Undeliverable]
    )
) AS PivotTable
 ";
            query += " ORDER BY shipdate DESC ";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            query = query.Replace("<EvDateFrm>", EvDateFrom);
            query = query.Replace("<EvDateTo>", EvDateTo);

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("208", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstSOsStatus = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}