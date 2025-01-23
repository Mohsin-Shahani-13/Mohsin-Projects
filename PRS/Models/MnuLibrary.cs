using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IP.Models
{
    public class MnuLibrary
    {
        cDAL oDAL = null;

        public string designedBy { get; set; }
        public string mailTo { get; set; }
        public List<ArrayList> lstMnuLibrary { get; set; }

        private DataTable dtResult = null;
        private DataTable dtOriginal = null;
        public string ProgramBySite { get; set; }
        public string ProgramNameforSite { get; set; }
        
        // Designed by Muhammad Salman

        public string GetProgramBysite(string site)
        {

            oDAL = new cDAL("ACTIVE");
            string query = "SELECT Name  FROM pls.Program WHERE Site = '<Site>'" ;
            query = query.Replace("<Site>", site);
            DataTable dt = oDAL.GetData(query);
           
                var ProgramNameList = (from p in dt.AsEnumerable()
                             select p.Field<object>("Name")).ToList().Distinct();
                ProgramNameforSite = String.Join("','", ProgramNameList).Insert(0, "'").Insert(String.Join("','", ProgramNameList).Insert(0, "'").Length, "'");
                return ProgramNameforSite;

            
        }
        public void GetList()
        {
            string site = HttpContext.Current.Session["DefaultSite"].ToString();
            int empId = Convert.ToInt32(HttpContext.Current.Session["EmpId"]);
            designedBy = "Specd by: Abbas Arsiwala @ Teleplan & Designed by: Imdad Ullah @ WinIT";
            mailTo = "mailto:kashif@winit.biz?cc=Tahir@winit.biz&subject=Menu Library";
            string query = string.Empty;

            query = @"SELECT  mnu.MnuId, mnu.MnuParent, mnu.MnuHyperlink,ip.MnuTitle,fav.IsFavourite, mnu.MnuTitle, 
CASE WHEN mnu.MnuHyperlink IS NOT NULL THEN 
	CASE WHEN mnu.MnuIsReady = 1 THEN 'Yes' ELSE 'No' END ELSE '' 
END AS IsReady, mnu.IsProd, mnu.IsRept, mnu.IsTran,  mnu.IsTest,
mnu.RPTCode, mnu.MnuTitleShort,mnu.DesignedBy, mnu.MailTo, mnu.MnuTarget,mnu.AuthSite
FROM IP.Mnu mnu
Left join ip.mnu ip on ip.MnuId = mnu.MnuParent
LEFT OUTER JOIN IP.[EmpMnuFav] fav on fav.MnuId = mnu.MnuId and fav.EmpId = '<empId>'";
           query += "ORDER BY mnu.MnuTitle";


            query = query.Replace("<empId>",empId.ToString());
            dtResult = new DataTable();
            dtOriginal = new DataTable();
            oDAL = new cDAL("INIT");
            dtOriginal = oDAL.GetData(query);
            dtResult = dtOriginal.Clone();


            foreach (DataRow row in dtOriginal.Select("MnuParent IS NULL"))
            {
                BuildMenu(row);
            }

           // PopulateMenu(dtOriginal);
            lstMnuLibrary = cCommon.ConvertDtToArrayList(dtResult);
            cLog oLog = new cLog();
            oLog.AddSqlQuery("IPMENU", query, "Menu Library");
        }

        private void BuildMenu(DataRow row) {
            string mnuHl = row["MnuHyperlink"].ToString();
            string mnuId = row["MnuId"].ToString();
            // for parent
            if (mnuHl == "")
            {
                dtResult.ImportRow(row);
                DataRow[] rSub = dtOriginal.Select("MnuParent = " + mnuId);
                if (rSub.Length > 0)
                {
                    foreach (DataRow r in rSub)
                    {
                        BuildMenu(r);
                    }
                }
            }
            else
            { // for child
                dtResult.ImportRow(row);
            }
        }

        public void GetFavorite(string rptCode, string rptTitle, string MnuId)
        {
            string sql = string.Empty;
            oDAL = new cDAL("INIT");
            int empId = Convert.ToInt32(HttpContext.Current.Session["EmpId"]);

            sql = @"INSERT INTO [PlusRS].IP.[EmpMnuFav](MnuId,EmpId, RptCode, RptName, IsFavourite) 
VALUES(@MnuId,
         @EmpId,
        '@RptCode',
        '@RptName',
         @IsFavourite)";

            sql = sql.Replace("@MnuId", MnuId.ToString());
            sql = sql.Replace("@EmpId", empId.ToString());
            sql = sql.Replace("@RptCode", rptCode);
            sql = sql.Replace("@RptName", rptTitle);
            sql = sql.Replace("@IsFavourite","1");

            oDAL.Execute(sql);
     

        }
        public void PopulateMenu(DataTable dt)
        {
            foreach (DataRow _row in dt.Rows)
            {
                //if check parent
                if (_row["MnuHyperLink"].ToString() == "")
                {
                    dtResult.Rows.Add(_row.ItemArray);
                    DataRow[] dRows = dtOriginal.Select("MnuParent = " + _row["MnuId"].ToString());
                    if (dRows.Length > 0)
                    {
                        DataTable dtSub = dRows.CopyToDataTable();
                        DataRow[] subdRows = dtSub.Select("MnuHyperlink IS NULL");
                        if (subdRows.Length > 0)
                        {
                            PopulateMenu(dtSub);
                        }
                        else
                        {
                            foreach (DataRow row in dtSub.Rows)
                            {
                                dtResult.Rows.Add(row.ItemArray);
                            }
                        }
                    }
                }
                else
                {
                    dtResult.Rows.Add(_row.ItemArray);
                }
            }
        }

        public void DeleteFav(string MnuId)
        {
            string sql = string.Empty;
            oDAL = new cDAL("INIT");
            int empId = Convert.ToInt32(HttpContext.Current.Session["EmpId"]);
            sql = @"DELETE FROM IP.[EmpMnuFav] WHERE MnuId = '<mnuId>' AND EmpId = '<empId>'";
            sql = sql.Replace("<mnuId>", MnuId);
            sql = sql.Replace("<empId>", empId.ToString());
     
            oDAL.Execute(sql);
        }
    }
}