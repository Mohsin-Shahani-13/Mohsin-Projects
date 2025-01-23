using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Models
{
    public class LinkPowerBI
    {
        cDAL oDAL = null;

        [Display(Name = "Workspace:")]
        public string category { get; set; }
        public string designedBy { get; set; }
        public string mailTo { get; set; }
        public List<ArrayList> lstLinkPowerBI { get; set; }
        public char Workspace { get; private set; }

        private DataTable dtResult = null;

        private DataTable dtOriginal = null;

        public string filterString { get; set; }

        public DataTable Category() // Category
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            query = "SELECT distinct Workspace as Category FROM ip.LinksPowerBI ORDER BY Workspace";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }

        public void GetList(string ddCategory)
        {
            designedBy = "Specd by: Abbas Arsiwala @ Teleplan & Designed by: Imdad Ullah @ WinIT";
            mailTo = "mailto:kashif@winit.biz?cc=Tahir@winit.biz&subject=Menu Library";
            string query = string.Empty;

            query = @"SELECT Workspace
,Recnum
,LinkName
,LinkURL
,SeqNo
,CASE WHEN IsActive = 1 THEN 'Yes' ELSE 'No' End
FROM IP.LinksPowerBI
--ORDER BY LinkName
";
            if (ddCategory != "All")
            {
                query += "WHERE Workspace =  '" + ddCategory + "' ";
            }


            if (!string.IsNullOrEmpty(ddCategory))
                filterString += "> Workspace = '" + ddCategory + "' ";

            //filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";
            category = ddCategory;
            dtResult = new DataTable();
            dtOriginal = new DataTable();
            oDAL = new cDAL("INIT");
            dtOriginal = oDAL.GetData(query);
            //dtResult = dtOriginal.Clone();

          
            // PopulateMenu(dtOriginal);
            lstLinkPowerBI = cCommon.ConvertDtToArrayList(dtOriginal);
            cLog oLog = new cLog();
            oLog.AddSqlQuery("IPMENU", query, "Link Power BI");
        }

       
    }
}
    