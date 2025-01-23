using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;

namespace IP.Classess
{

    public class cSite
    {

        string sql = string.Empty;
        cDAL oDal = new cDAL("INIT");

        public string CompanyCode { get; set; }
        public string Plant { get; set; }
        public int SiteId { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public string SitePrefix { get; set; }
        public string CompanyName { get; set; }
        public string LinkedSrvr { get; set; }
        public string ProgramBySite { get; set; }
        public string TNSName { get; set; }
        public string Contract { get; set; }
        public string sites { get; set; }
        private static string qry = "";
        public string ProgramforSite { get; set; }
        public string ProgramNamesforSite { get; set; }
        private static cDAL oLayer = null;

        public DataTable GetSites()
        {
            try
            {
                sql = "SELECT SiteId, SiteName ";
                sql += "FROM zSites ";
                sql += "WHERE IsActive = 1 ORDER BY SiteName";

                DataTable dt = new DataTable();
                dt = oDal.GetData(sql);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public DataTable GetSite()
        {
            oDal = new cDAL("ACTIVE");
            try
            {
                //sql = @"SELECT ID, (Name + ' (' + Site + ')') AS NAME FROM [pls].[Program]";
                sql = @"SELECT Site AS ID,Site AS NAME FROM [IP].[Program]
                       GROUP BY Site";

                DataTable dt = new DataTable();
                dt = oDal.GetData(sql);
                if (dt.Rows.Count > 0)
                {
                    sites = dt.Rows[0]["ID"].ToString();
                }
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string GetProgramBysite(string site)
        {
            
            cDAL oDAL = new cDAL("ACTIVE");
            string query = "SELECT Id, Name  FROM IP.Program WHERE Site = '<Site>'";
            query = query.Replace("<Site>", site);
            DataTable dt = oDAL.GetData(query);
            HttpContext.Current.Session["ProgramIdBySiteForMeta"] = "";
            foreach (DataRow row in dt.Rows)
            {
                if (row["Name"].ToString() == "META")
                {
                    HttpContext.Current.Session["ProgramIdBySiteForMeta"] = row["ID"];
                }
            }

            ProgramBySite = dt.Rows[0]["Id"].ToString();
            var ProgramList = (from p in dt.AsEnumerable()
                               select p.Field<object>("ID")).ToList().Distinct();
            ProgramforSite = String.Join("','", ProgramList).Insert(0, "'").Insert(String.Join("','", ProgramList).Insert(0, "'").Length, "'");

            var ProgramNameList = (from p in dt.AsEnumerable()
                                   select p.Field<object>("Name")).ToList().Distinct(); // commit: 1
            ProgramNamesforSite = String.Join("','", ProgramNameList).Insert(0, "'").Insert(String.Join("','", ProgramNameList).Insert(0, "'").Length, "'"); // commit: 1
            return ProgramBySite;
        }
        public void GetSiteDetail(string siteName)
        {
            // sql = "SELECT  SiteCode, SiteName, Company, (CompanyName + ' (' + Plant + ')') AS CompanyName, Plant ";
            sql = "SELECT  SiteCode, SiteName, SitePrefix, CompanyCode, CompanyName  AS CompanyName, Plant, LinkedSrvr, TNSName, FitBit ";
            sql += "FROM zSites ";
            sql += "WHERE IsActive = 1 AND SiteName = '" + siteName + "'";

            DataTable dt = oDal.GetData(sql);
            if (dt.Rows.Count > 0)
            {
                UpdateDefaultSite(siteName);
                //SiteId = siteId;
                SiteName = siteName;
                SiteCode = dt.Rows[0]["SiteCode"].ToString();
                SitePrefix = dt.Rows[0]["SitePrefix"].ToString();
                CompanyCode = dt.Rows[0]["CompanyCode"].ToString();
                CompanyName = dt.Rows[0]["CompanyName"].ToString();
                Plant = dt.Rows[0]["Plant"].ToString();
                LinkedSrvr = dt.Rows[0]["LinkedSrvr"].ToString();
                TNSName = dt.Rows[0]["TNSName"].ToString();
                Contract = dt.Rows[0]["FitBit"].ToString();
            }
        }

        public void UpdateDefaultSite(string site)
        {
            int empId = Convert.ToInt32(HttpContext.Current.Session["EmpId"]);
            sql = "UPDATE IP.Employee SET DefaultSite = '" + site + "' WHERE EmpId = " + empId;
            DataTable dt = oDal.GetData(sql);
        }

    }
}