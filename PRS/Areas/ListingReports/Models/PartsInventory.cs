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
    public class PartsInventory
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Part No.:")]
        public string partNo { get; set; }

        [Display(Name = "Warehouse:")]
        public string Warehouse { get; set; }

        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstPartsInventory { get; set; }
        public List<Hashtable> lstGetSerials { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion
        #region Methods 
      
        public DataTable Getwarehouse(string programId, string ProgramName) // onHand warehouse method
        {
            string query = string.Empty;
            query = @"SELECT DISTINCT  PL.Warehouse
FROM   pls.partqty PQ 
INNER JOIN pls.partno PN ON PQ.partno = PN.partno 
INNER JOIN pls.partlocation PL ON PL.id = PQ.locationid 
INNER JOIN pls.program P ON P.id = PQ.programid  ";
            if (programId != "0" && programId != null)
            {
                query += " WHERE P.ID = '" + programId + "' ";
            }
            else
            {
                query += " WHERE P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            query += "ORDER BY PL.Warehouse ";

            DataTable dt = oDAL.GetData(query);
            return dt;
        }


         

        public bool GetList(string partNo, string programId, string ProgramName, string warehouse)
        {
            string query = string.Empty;
            if (ProgramName == "BOSE")
            {
                query = @"
SELECT P.ID,
       PQ.partno, 
 PnA.Value AS Code_Name,
       PN.description, 
       PQ.LocationId,
	   PL.locationno,
       PL.Bin,
       PL.Warehouse,
       CC.ID AS ConfigId,
	   CC.Description AS Configuration ,
       PQ.PalletBoxNo,
	   PQ.LotNo,
       CASE WHEN PN.SerialFlag = 0 THEN 'N' ELSE 'Y' END AS SerialFlag,
	   --PQ.LotNo,
         PQ.availableqty,
	   --PQ.ReservedQty,
       P.NAME,
	   U.Username,
	   PQ.CreateDate,
       PQ.LastActivityDate
FROM   pls.partqty PQ 
INNER JOIN pls.partno PN ON PQ.partno = PN.partno 
INNER JOIN pls.partlocation PL ON PL.id = PQ.locationid 
INNER JOIN pls.program P ON P.id = PQ.programid 
LEFT JOIN Pls.CodeAttribute CA ON CA.AttributeName = 'CODE_NAME'
LEFT JOIN Pls.PartNoAttribute PnA ON PnA.PartNo = PN.PartNo 
					AND PnA.ProgramID = p.ID 
					AND PnA.AttributeID = CA.ID
INNER JOIN [pls].[CodeConfiguration] CC ON CC.ID = PQ.ConfigurationID
INNER JOIN [pls].[User] U ON U.ID = PQ.UserID  
       WHERE  PQ.availableqty > 0 ";
            }
            else
            {
                query = @"
SELECT P.ID,
       PQ.partno, 
       PN.description, 
       PQ.LocationId,
	   PL.locationno,
       PL.Bin,
       PL.Warehouse,
       CC.ID AS ConfigId,
	   CC.Description AS Configuration ,
       PQ.PalletBoxNo,
	   PQ.LotNo,
       CASE WHEN PN.SerialFlag = 0 THEN 'N' ELSE 'Y' END AS SerialFlag,
	   --PQ.LotNo,
         PQ.availableqty,
	   --PQ.ReservedQty,
       P.NAME,
	   U.Username,
	   PQ.CreateDate,
       PQ.LastActivityDate
FROM   pls.partqty PQ 
INNER JOIN pls.partno PN ON PQ.partno = PN.partno 
INNER JOIN pls.partlocation PL ON PL.id = PQ.locationid 
INNER JOIN pls.program P ON P.id = PQ.programid 
INNER JOIN [pls].[CodeConfiguration] CC ON CC.ID = PQ.ConfigurationID
INNER JOIN [pls].[User] U ON U.ID = PQ.UserID  
       WHERE  PQ.availableqty > 0 ";
            }
           

            if (!string.IsNullOrEmpty(partNo))
                query += "AND PQ.partno LIKE '%" + partNo + "%' ";
            if (!warehouse.Equals("All"))
                query += "AND PL.warehouse ='" + warehouse + "' ";
          
            if (programId != "0")
            {
                query += "AND P.ID = '" + programId + "' ";
            }
            else
            {
                query += "AND P.ID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }


            query += "ORDER BY PQ.LastActivityDate DESC";
            //  query += "ORDER BY PQ.partno";
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(partNo))
                filterString += " | Part No. Like '" + partNo + "' ";

            if (!string.IsNullOrEmpty(warehouse))
                filterString += " | Warehouse = '" + warehouse + "' ";

            //string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            //if (conType == "PROD")
            //{
            //    query = query.Replace("PalletBoxNo", "PalletNo");
            //    query = query.Replace("LotNo", "CartonNo");
            //}
            DataTable dt = oDAL.GetData(query);

            

            //For SQL Documentation
            cLog oLog = new cLog();
             oLog.AddSqlQuery("004", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPartsInventory = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

//        public bool GetSerials(string partNo, string locNo)
//        {
//            string query = string.Empty;
//            query = @"
//SELECT 
//	Ps.SerialNo
//FROM pls.PartSerial PS
//INNER JOIN pls.PartLocation PL ON PL.ID = PS.LocationID
//WHERE PartNo = '<partNo>' 
//AND pl.LocationNo = '<locationno>'
//";


//            //if (programId != "0")
//            //{
//            //    query += "AND PS.ProgramID = '" + programId + "' ";
//            //}
//            //else
//            //{
//            //    query += "AND PS.ProgramID IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
//            //}

//            query = query.Replace("<partNo>", partNo);
//            query = query.Replace("<locationno>", locNo);

//            DataTable dt = oDAL.GetData(query);

//            if (oDAL.HasErrors)
//            {
//                ErrorMessage = oDAL.ErrMessage;
//                return false;
//            }
//            else
//            {
//                if (dt.Rows.Count > 0)
//                    lstGetSerials = cCommon.ConvertDtToHashTable(dt);
//                return true;

//            }
//        }
  
        #endregion
    }
}