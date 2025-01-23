using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace IP.Models
{
    public class Documents
    {
        cDAL oDAL = null;

        //public string designedBy { get; set; }
        //public string mailTo { get; set; }
        public List<ArrayList> lstDoc { get; set; }
        public string ErrorMessage { get; private set; }

        private DataTable dtResult = null;

        
        public bool GetList()
        {
            string query = string.Empty;

            query = @"SELECT DocId, DocTitle, DocDesc, DocPath, DocActive, UpdatedBy, UpdatedOn
                      FROM   DocLinks WHERE DocActive = 1
                      ORDER BY DocId ";

            //dtResult = new DataTable();
            oDAL = new cDAL("INIT");
            DataTable dt = oDAL.GetData(query);

            cLog oLog = new cLog();
            oLog.AddSqlQuery("IPMENU", query, "Documents");

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstDoc = cCommon.ConvertDtToArrayList(dt);
                return true;

            }


        }
        public byte[] GetFile(string LnkUrl, string LnkName)
        {
            string sql = "SELECT LnkName, LnkUrl, LnkDesc FROM zLkupLnk WHERE LnkName = '" + LnkName + "' ";
            oDAL = new cDAL("INIT");
            DataTable dataTable = oDAL.GetData(sql);
            if (dataTable.Rows.Count > 0)
            {
                string path = dataTable.Rows[0]["LnkUrl"].ToString();
                byte[] _byte = new byte[path.Length];
                //byte[] _byte = Convert.FromBase64String(path);
                return _byte;
            }
            return null;
        }
    }
}