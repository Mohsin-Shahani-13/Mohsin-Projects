using IP.Classess;
using IP.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace IP.Models
{
    public class Home
    {
        cDAL oDAL;
        string sql = string.Empty;
        public string TotalValue { get; set; }
        public string Menu { get; set; }
        public string MailTo { get; set; }
        [Display(Name = "Program:")]
        public string ProgramName { get; set; }    
        public List<FavMenu> mnuFav { get; set; }
        public List<WebMenus> webMnu { get; set; }
        public List<EPCon> epCon { get; set; }
        public List<PixCon> pixCon { get; set; }
        public IList<ArrayList> lstSqlBlog { get; set; }
        public List<ArrayList> lstlink { get; set; }
        [Display(Name = "Program:")]
      

        public List<Hashtable> lstResult { get; set; }
        public string ProgramID { get; set; }

        public DataTable GetLink()
        {
            //  oDal = new cDAL("INIT");
            oDAL = new cDAL("INIT");
            try
            {
                sql = @"SELECT LinkName, LinkUrl 
                        FROM IP.Links
                        WHERE(IsActive = 1)
                        ORDER BY SeqNo";
                DataTable dt = new DataTable();
                dt = oDAL.GetData(sql);
                // lstlink= cCommon.ConvertDtToArrayList(dt);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public string GetProgramNameBysite(string site)
        {

            oDAL = new cDAL("ACTIVE");
            string query = "SELECT Name  FROM pls.Program WHERE Site = '<Site>'";
            query = query.Replace("<Site>", site);
            DataTable dt = oDAL.GetData(query);

            var ProgramNameList = (from p in dt.AsEnumerable()
                                   select p.Field<object>("Name")).ToList().Distinct();
            ProgramName = String.Join("','", ProgramNameList).Insert(0, "'").Insert(String.Join("','", ProgramNameList).Insert(0, "'").Length, "'");
            return ProgramName;


        }
        public void GetMenus(string programBySite)
        {
            try
            {
                // Designed by Muhammad Salman
               


                string empName = HttpContext.Current.Session["LogonUser"].ToString();                  
                    //string site = HttpContext.Current.Session["SitePrefix"].ToString().Trim();
                    //string region = HttpContext.Current.Session["LinkedSrvr"].ToString();
                    // chages table name change
                    sql = "SELECT * FROM IP.Mnu  WHERE MnuActive = 1 ";
                    sql += "AND (AuthUser = '*' OR AuthUser LIKE '%" + empName + "%') ";

                    sql += "AND (AuthSite = '*' OR AuthSite IN (" + programBySite + ") ) ";
                //}

                oDAL = new cDAL("INIT");
                bool developer = false;
                string sqlForIsDeveloper = "SELECT IsWinIT from IP.Employee where WinLogin = '" + empName + "'";
                DataTable dtIsDeveloper = new DataTable();
                dtIsDeveloper = oDAL.GetData(sqlForIsDeveloper);
                if (dtIsDeveloper.Rows.Count > 0)
                    developer = Convert.ToBoolean(dtIsDeveloper.Rows[0]["IsWinIt"]);

                if (!developer)
                    sql += "AND MnuIsReady = 1 ";

                string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
                if (conType == "PROD")
                    sql += "AND IsProd = 1 ";
                if (conType == "REPT")
                    sql += "AND IsRept = 1 ";
                if (conType == "TRAN")
                    sql += "AND IsTran = 1 ";
                if (conType == "TEST")
                    sql += "AND IsTest = 1 ";


                sql += "ORDER BY MnuParent, MnuType, MnuTitle ";
                DataTable dt = new DataTable();
                oDAL = new cDAL("INIT");
                dt = oDAL.GetData(sql);
                MailTo = dt.Rows[0]["MailTo"].ToString();

                webMnu = dt.AsEnumerable().Select(dataRow => new WebMenus
                {
                    MnuId = Convert.ToInt32(dataRow["MnuId"]),
                    MnuType = dataRow["MnuType"].ToString(),
                    MnuIcon = dataRow["MnuIcon"].ToString(),
                    MnuTitle = dataRow["MnuTitle"].ToString(),
                    MnuTitleShort = dataRow["MnuTitleShort"].ToString(),
                    MnuHyperlink = dataRow["MnuHyperlink"].ToString(),
                    MnuTarget = dataRow["MnuTarget"].ToString(),
                    MnuParent = dataRow["MnuParent"].ToString(),
                    DesignedBy = dataRow["DesignedBy"].ToString(),
                    RptCode = dataRow["RptCode"].ToString(),
                    MailTo = dataRow["MailTo"].ToString()
                }).ToList<WebMenus>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataSet GetConnections()
        {
            try
            {
                var dal = new cDAL("INIT");
                var connections = dal.GetData(@"
                    SELECT ConText
                          ,ConValue
                          ,ConType
                          ,IsDropDown
                    FROM zConStr
                    ")

                    // IsDropDown is Used For Select TEST,TRAN,PRODRPT
                    .ToList<BaseCWConnection>()
                    .Select(c => new BaseCWConnection
                    {
                        ConText = c.ConText,
                        ConType = c.ConType,
                        ConValue = BasicEncrypt.Instance.Encrypt(c.ConValue),
                        IsDropDown = c.IsDropDown
                    });
                DataSet dsconnection = new DataSet();
                DataTable dtConn = connections.ConvertToDataTable();
                dtConn.TableName = "CONN";
                dsconnection.Tables.Add(dtConn);
                return dsconnection;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID
                             ,NAME 
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            

            return dt;
        }

        public string GetConnectionString(string conType, string appType)
        {
            oDAL = new cDAL("ACTIVE");
            sql = "SELECT ConValue FROM zConStr WHERE ConType = '" + conType + "' AND AppType ='" + appType + "'";
            DataTable dt = oDAL.GetData(sql);
            return dt.Rows[0]["ConValue"].ToString();
        }

        public DataTable GetRecord() //Added by Huzaifa
        {
            oDAL = new cDAL("INIT");
            string query = string.Empty;
            query = @"select top 1 RECNUM, RptUrl from zLogQuery 
                        where SigninId = '" + HttpContext.Current.Session["EmpId"].ToString() + "'" +
                        "AND RemoteHost = '" + HttpContext.Current.Session["RemoteAddr"].ToString() + "'";
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
//        public bool GetSyncCount() //Added by Huzaifa
//        {
//            oDAL = new cDAL("INIT");
//            string query = string.Empty;
//                   query = @"select b.rows as count1 
//       from Plus2.sys.objects as a 
//       inner join Plus2.sys.partitions b on b.object_id = a.object_id
//       where a.type = 'U' and a.is_ms_shipped = 0x0 and b.index_id < 2 
//	   and a.name = 'PartTransaction' ;

//                      select b.rows as count2
//from [DC1PLUSREPL].Plus.sys.objects as a 
//inner join [DC1PLUSREPL].Plus.sys.partitions b on b.object_id = a.object_id
//where a.type = 'U' and a.is_ms_shipped = 0x0 and b.index_id < 2 
//	and a.name = 'PartTransaction';";
//            DataSet DS = oDAL.GetDataSet(query);
//            int count1 = Convert.ToInt32(DS.Tables[0].Rows[0]["count1"]);
//            int count2 = Convert.ToInt32(DS.Tables[1].Rows[0]["count2"]);

//            // Compare the counts and determine if they match
//            bool countsMatch = (count1 >= count2);
//            return countsMatch;
//        }
    }
    public class BaseCWConnection
    {
        public string ConText { get; set; }
        public string ConValue { get; set; }
        public string ConType { get; set; }
        public string IsDropDown { get; set; }// added

    }

    public class EPCon
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class PixCon
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class FavMenu
    {
        public int MnuId { get; set; }
        public string MnuTitleShort { get; set; }
        public string MnuHyperlink { get; set; }
    }

    public class WebMenus
    {
        public int MnuId { get; set; }
        public string MnuType { get; set; }
        public string MnuIcon { get; set; }
        public string MnuTitle { get; set; }
        public string MnuTitleShort { get; set; }
        public string MnuHyperlink { get; set; }
        public string MnuTarget { get; set; }
        public string MnuParent { get; set; }
        public string DesignedBy { get; set; }
        public string RptCode { get; set; }
        public string MailTo { get; set; }
    }
}

