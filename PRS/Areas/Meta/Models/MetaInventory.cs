using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

//Name change to Month to Date Inventory*@
namespace IP.Areas.Meta.Models
{
    public class MetaInventory
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string ProgramName { get; set; }
        [Display(Name = "Program:")]
        public string program_Id { get; set; }

        [Display(Name = "PartNo.:")]
        public string partno { get; set; }
        [Display(Name = "Unit Of Measure:")]
        public string UnitOfMeasure { get; set; }
        [Display(Name = "Ware House:")]
        public string WareHouse { get; set; }
        [Display(Name = "Location:")]
        public string location { get; set; }
        [Display(Name = "Total:")]
        public string Total { get; set; }
        [Display(Name = "Source:")]
        public string source { get; set; }
        [Display(Name = "TimeStamp:")]
        public string timestamp { get; set; }

        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public string OrderType { get; set; }
        public List<Hashtable> lstMetaInventory { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }

        #endregion
        public bool GetList(string programId, string ProgramName, string partno, string UnitOfMeasure, string WareHouse, string location, string Total, string source, string timestamp)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;
            query = @"SELECT 
             From pls.partqty
             ";



            query += " WHERE CRT.ID  IN ('<Program>') ";


            query = query.Replace("<partno>", partno);
            //query = query.Replace("<OrderType>", OrderType);



            DataTable dt = oDAL.GetData(query);
            if (!string.IsNullOrEmpty(ProgramName))
                filterString += "> Program = '" + ProgramName + "' ";

            if (!string.IsNullOrEmpty(partno))
                filterString += "| partno = '" + partno + "' ";
            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("134", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaInventory = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}