using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.ListingReports.Models
{

    public class PCBABoxEnquiry
    {

        //[Display(Name = "Program:")]
        //public string ProgramName { get; set; }
        [Display(Name = "PCBA Box No:")]
        public string PCBA_Box_No { get; set; }

        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<KeyValue> PcbaValues { get; set; }
        public List<KeyValue> SerialValues { get; set; }
        public List<ArrayList> lstPCBABoxEnquiry { get; set; }

        //private readonly string connectionString = "Data Source=10.211.9.71;Initial Catalog=DmtDb;Persist Security Info=True;User ID=usrPRS;Password=mandy;";

        cDAL oDAL = new cDAL("ACTIVE");

        public bool GetList(string pcbaBoxId)
        {
            oDAL = new cDAL("ACTIVE");
            string query = string.Empty;
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string sql = string.Empty;


            //string programId = HttpContext.Current.Session["ProgramForSite"].ToString();
            

            query = @"
        SELECT  Value as Source FROM pls.vPartSerialAttribute WHERE ProgramID = <programId> AND AttributeName = 'SOURCE' 

						AND SerialNo IN (SELECT SerialNo FROM pls.vPartSerialAttribute WHERE AttributeName = 'PCBA_SN' AND ProgramID = <programId>

						AND Value    IN (SELECT SerialNo FROM pls.vPartSerialAttribute WHERE Value = '<pcbaBoxID>' AND AttributeName = 'CartonNo' AND ProgramID = <programId>))
  
        ";
            query = query.Replace("<pcbaBoxID>", pcbaBoxId);
            //query = query.Replace("<programId>", programId);
            if (sites == "PENANG")
            {
                //query += "WHERE  P.ID = '10014'";
                query = query.Replace("<programId>", "10014");
            }
            else if (sites == "FRANKLIN")
            {
                //query += "WHERE  P.ID = '10049'";
                query = query.Replace("<programId>", "10049");


            }
            else if (sites == "CORK")
            {
                //query += "WHERE  P.ID = '10050'";
                query = query.Replace("<programId>", "10050");
            }
            else if (sites == "CHONBURI")
            {
                //query += "WHERE  P.ID = '10051'";
                query = query.Replace("<programId>", "10051");
            }
            else if (sites == "PRAGUE")
            {
                //query += "WHERE  P.ID = '10056'";
                query = query.Replace("<programId>", "10056");
            }
            string Source = Convert.ToString(oDAL.GetObject(query));
            //DataTable dtSource = oDAL.GetData(query);
            


            query = @"
        SELECT Value as [BOX_ID] FROM pls.vPartSerialAttribute WHERE ProgramID = <programId> AND AttributeName = 'CartonNo'


                        AND SerialNo IN (SELECT SerialNo from pls.vPartSerialAttribute WHERE AttributeName = 'PCBA_SN' AND ProgramID = <programId>


                        AND Value IN (SELECT SerialNo FROM pls.vPartSerialAttribute WHERE Value = '<pcbaBoxID>' AND AttributeName = 'CartonNo' AND ProgramID = <programId>))
  
        ";
            query = query.Replace("<pcbaBoxID>", pcbaBoxId);
            //query = query.Replace("<programId>", programId);
            if (sites == "PENANG")
            {
                //query += "WHERE  P.ID = '10014'";
                query = query.Replace("<programId>", "10014");
            }
            else if (sites == "FRANKLIN")
            {
                //query += "WHERE  P.ID = '10049'";
                query = query.Replace("<programId>", "10049");


            }
            else if (sites == "CORK")
            {
                //query += "WHERE  P.ID = '10050'";
                query = query.Replace("<programId>", "10050");
            }
            else if (sites == "CHONBURI")
            {
                //query += "WHERE  P.ID = '10051'";
                query = query.Replace("<programId>", "10051");
            }
            else if (sites == "PRAGUE")
            {
                //query += "WHERE  P.ID = '10056'";
                query = query.Replace("<programId>", "10056");
            }
            //DataTable dtBoxId = oDAL.GetData(query);
            string BoxId = Convert.ToString(oDAL.GetObject(query));
            

            query = @"
        SELECT PalletBoxNo as [PALLET_ID] from pls.PartSerial WHERE ProgramID = <programId> 

						AND SerialNo IN (SELECT SerialNo FROM pls.vPartSerialAttribute WHERE AttributeName = 'PCBA_SN' AND ProgramID = <programId>

						AND Value    IN (SELECT SerialNo FROM pls.vPartSerialAttribute WHERE Value = '<pcbaBoxID>' AND AttributeName = 'CartonNo' AND ProgramID = <programId>))
  
        ";
            query = query.Replace("<pcbaBoxID>", pcbaBoxId);
            //query = query.Replace("<programId>", programId);
            if (sites == "PENANG")
            {
                //query += "WHERE  P.ID = '10014'";
                query = query.Replace("<programId>", "10014");
            }
            else if (sites == "FRANKLIN")
            {
                //query += "WHERE  P.ID = '10049'";
                query = query.Replace("<programId>", "10049");


            }
            else if (sites == "CORK")
            {
                //query += "WHERE  P.ID = '10050'";
                query = query.Replace("<programId>", "10050");
            }
            else if (sites == "CHONBURI")
            {
                //query += "WHERE  P.ID = '10051'";
                query = query.Replace("<programId>", "10051");
            }
            else if (sites == "PRAGUE")
            {
                //query += "WHERE  P.ID = '10056'";
                query = query.Replace("<programId>", "10056");
            }
            //DataTable dtPalletId = oDAL.GetData(query);
            string PalletId = Convert.ToString(oDAL.GetObject(query));

            


            DataTable dt = new DataTable("MergedValue");
            dt.Columns.Add("Source", typeof(string));
            dt.Columns.Add("PCBA_BOX_ID", typeof(string));
            dt.Columns.Add("BOX_ID", typeof(string));
            dt.Columns.Add("PALLET_ID", typeof(string));

            if (!string.IsNullOrEmpty(Source) || !string.IsNullOrEmpty(BoxId) || !string.IsNullOrEmpty(PalletId))
            {
                dt.Rows.Add(Source, pcbaBoxId, BoxId, PalletId);
            }
            



                //DataTable dt = new DataTable("MergedValue");

                //dt.Merge(dtSource);
                //dt.Merge(dtBoxId);
                //dt.Merge(dtPalletId);
                //dt.Columns.Add("Source");
                //dt.Columns.Add("PCBA_BOX_ID");
                //dt.Columns.Add("BOX_ID");
                //dt.Columns.Add("PALLET_ID");


                filterString = "PCBA Box No: " + pcbaBoxId;

            

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstPCBABoxEnquiry = cCommon.ConvertDtToArrayList(dt);
                return true;
            }

        }
        
    }
}
