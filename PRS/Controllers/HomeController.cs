using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Data;
using IP.Models;
using IP.Classess;
using System.Configuration;
using IP.ActionFilters;
using System.Threading;

[OutputCache(Duration = 0)]

public class HomeController : Controller
{
    string sql = string.Empty;
    Home oHome = new Home();
    cEmployee oEmp = new cEmployee();
    cAuth oAuth;

    [OutputCache(Duration = 0)]
    public ActionResult Login()
    {
        oAuth = new cAuth();
        if (cCommon.IsSessionExpired())
            return View(oAuth);
        else
            return RedirectToAction("Base");
    }

    [OutputCache(Duration = 0)]
    public ActionResult Base()
    {
        if (cCommon.IsSessionExpired())
            return RedirectToAction("Login");
        else
        {
            //ViewBag.sysCode = Session["sysCode"].ToString();
            // Temporary comment by Kashif Maqsood (It is irritating on every new landing page need to be fixed)
            //if (cCommon.IsSessionExpired())
            //     return RedirectToAction("Index");

            //// Set connection strings
            // Get Connection string

            string fileName = ConfigurationManager.AppSettings["Key"];
            Session["CONN_INIT"] = BasicEncrypt.Instance.Encrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());
            cDAL oDAL = new cDAL("INIT");
            DataTable dt = oDAL.GetData("SELECT * FROM zConStr ");
            if (dt.Rows.Count > 0)
            {

                dt.DefaultView.RowFilter = "ConType = 'TEST'";
                Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(dt.Rows[0]["ConValue"].ToString());
                Session["CONN_TYPE"] = "TEST";



                foreach (DataRow row in dt.Rows)
                {
                    Session["CONN_" + row["ConType"]] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());


                    //string Apptype = row["AppType"].ToString();
                    //if (Apptype == "BIZTALK_PROD_INBOUND")
                    //{
                    //    Session["CONN_Z001_INBOUND"] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());
                    //}
                    //else if (Apptype == "BIZTALK_PROD_OUTBOUND")
                    //{
                    //    Session["CONN_Z001_OUTBOUND"] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());
                    //}
                    //else if (Apptype == "BIZTALK_TRAN_INBOUND")
                    //{
                    //    Session["CONN_Z004_INBOUND"] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());
                    //}
                    //else if (Apptype == "BIZTALK_TRAN_OUTBOUND")
                    //{
                    //    Session["CONN_Z004_OUTBOUND"] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());
                    //}
                }

            }

            // End

            // DataSet dsCon = oHome.GetConnections();
            oEmp = new cEmployee();

            //// Set user sessions
            oEmp.GetEmployeeByLogon(Session["LogonUser"].ToString());
            string sysValue = cCommon.GetSysValue("showDocs");
            ViewBag.HasHelpDoc = oEmp.HasHelpDoc(Session["LogonUser"].ToString(), sysValue);
            cSite oSite = new cSite();
            if (oEmp.HasEmployee)
            {
                //Session["SessionExp"] = oEmp.EmpName; ;
                Session["EmpId"] = oEmp.EmpId;
                Session["EmpName"] = oEmp.EmpName;
                Session["DefaultSite"] = oEmp.DefaultSite; // it will never be null. commit: 1
                Session["CONN_TYPE"] = oEmp.ConType;
                Session["ProfilePicture"] = oEmp.ProfilePicture;
                //Session["ProgramId"] = oEmp.DefaultProgram; //no use anywhere. commit: 1
                Session["CONN_TYPE"] = oEmp.ConType;
                Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(oEmp.ConType, "PLUS"));
            }

            // DataTable dt = dsCon.Tables["CONN"];
            if (dt.Rows.Count > 0)
            {
                dt.DefaultView.RowFilter = "IsDropDown = true";
                ViewBag.Connections = cCommon.ToDropDownList(dt.DefaultView.ToTable(), "ConType", "ConText", oEmp.ConType, "ConType");
                Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(oEmp.ConType, "PLUS"));
                //if (oEmp.ConType == "PROD")
                //{
                //    Session["CONN_Z001_INBOUND"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(oEmp.ConType, "BIZTALK_PROD_INBOUND"));
                //    Session["CONN_Z001_OUTBOUND"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(oEmp.ConType, "BIZTALK_PROD_OUTBOUND"));

                //}
                //else if (oEmp.ConType == "TRAN")
                //{
                //    Session["CONN_Z004_INBOUND"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(oEmp.ConType, "BIZTALK_TRAN_INBOUND"));
                //    Session["CONN_Z004_OUTBOUND"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(oEmp.ConType, "BIZTALK_TRAN_OUTBOUND"));
                //}

                Session["CONN_TYPE"] = oEmp.ConType;


                foreach (DataRow row in dt.Rows)
                    Session["CONN_" + row["ConType"]] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());
            }
            
            DataTable dtSites = oSite.GetSite();  //Get Sites for Dropdown
            Session["Sites"] = oSite.sites; // Save site in session
            string ProgramBySite = oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["DefaultSite"].ToString()); // Get Programs for site selected
            //Session["ProgramId"] = ProgramBySite; // Save Program in session  commit: 1
            ViewBag.ddlProgram = cCommon.ToDropDownList(dtSites, "ID", "NAME", oEmp.DefaultSite, "ID");
            Session["ProgramForSite"] = oSite.ProgramforSite; //It is containing Program IDS against a site like '123','456'. and it is set by This: oSite.GetProgramBysite  commit: 1
            Session["ProgramNamesforSite"] = oSite.ProgramNamesforSite; //It is containing Program Names against a site like 'Meta','valve'. and it is set by This: oSite.GetProgramBysite   commit: 1

            DataTable lnk = oHome.GetLink();
            ViewBag.ddlLinks = cCommon.ToDropDown(lnk, "LinkUrl", "LinkName", "");
            LinkPowerBI oLinkPowerBI = new LinkPowerBI();
            DataTable dtcategory = oLinkPowerBI.Category();
            ViewBag.ddCategory = cCommon.ToDropDown(dtcategory, "Category", "Category", "Power BI Reports");
            return View();
        }
    }

    [OutputCache(Duration = 0)]
    public ActionResult Index()
    {
        if (cCommon.IsSessionExpired())
        {
            return RedirectToAction("Login");
        }

        else return View();
    }

    [OutputCache(Duration = 0)]
    [HttpPost]
    public JsonResult ValidateUser(string username, string password)
    {
        LoginStatus status = new LoginStatus();
        cAuth oAuth = new cAuth();


        bool isValidUser = oAuth.CheckUser(username, password);

        if (isValidUser)
        {
            status.Success = true;
            Session["LogonUser"] = username;
            Session["SessionID"] = Session.SessionID;
        }
        else
        {
            status.Success = false;
            status.Message = oAuth.Message;
        }

        return Json(status);
    }


    public string GetSiteDtl(string siteName)
    {

        cSite oSite = new cSite();
        oSite.GetSiteDetail(siteName);

        Session["CompanyName"] = oSite.CompanyName;
        Session["CompanyCode"] = oSite.CompanyCode;
        Session["CompanyPlant"] = oSite.Plant;
        Session["SiteId"] = oSite.SiteId;
        Session["SiteCode"] = oSite.SiteCode;
        Session["SiteName"] = oSite.SiteName;
        Session["SitePrefix"] = oSite.SitePrefix;
        Session["LinkedSrvr"] = oSite.LinkedSrvr;

        string tnsName = oSite.TNSName;
        tnsName = tnsName.Replace("@DBType", Session["CONN_TYPE"].ToString());
        Session["TNSName"] = tnsName;
        Session["Contract"] = oSite.Contract;
        Session["SiteCode"] = oSite.SiteCode;

        return oSite.CompanyName;
    }

    public ActionResult Logout()
    {
        cCommon.SessionExpired();
        //this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //this.Response.Cache.SetNoStore();
        //return RedirectToAction("Login", "Home");
        return View();
    }

    public ActionResult Error(string message)
    {
        if (System.Web.HttpContext.Current.Session["ErrorMsgMain"].ToString() != null)
        {
            message = System.Web.HttpContext.Current.Session["ErrorMsgMain"].ToString();
        }

        ViewBag.Message = message;
        return View();
    }

    [SessionTimeout]
    public JsonResult GetMenus(string programBySite)

    {
        //programBySite = oHome.GetProgramNameBysite(Session["DefaultSite"].ToString()); commit: 1 // Get Programs for site selected
        //Session["ProgramsBySite"] = programBySite;

        //oHome.GetMenus(programBySite); commit: 1
        oHome.GetMenus(Session["ProgramNamesforSite"].ToString());
         
        //ViewBag.EmailTo = oHome.MailTo;
        return Json(oHome.webMnu, JsonRequestBehavior.AllowGet);
    }

    [SessionTimeout]
    public void ChangeCon(string conType)
    { 
      
        oEmp = new cEmployee();
        oEmp.UpdateEmployeeConType(Session["LogonUser"].ToString(), conType);
        Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(conType, "PLUS"));
        //Session["CONN_Fimer"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(conType, "Fimer"));


        //if (conType == "PROD")
        //{
        //    Session["CONN_Z001_INBOUND"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(conType, "BIZTALK_PROD_INBOUND"));
        //    Session["CONN_Z001_OUTBOUND"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(conType, "BIZTALK_PROD_OUTBOUND"));

        //}
        //else if (conType == "TRAN")
        //{
        //    Session["CONN_Z004_INBOUND"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(conType, "BIZTALK_TRAN_INBOUND"));
        //    Session["CONN_Z004_OUTBOUND"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(conType, "BIZTALK_TRAN_OUTBOUND"));
        //}


        Session["CONN_TYPE"] = conType;

    }

    public JsonResult AddEmployeeDetail(string EmpName)
    {
        UserProfile oreg = new UserProfile();
        string Check = oreg.AddDetails(EmpName, "");
        return Json(Check, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult AddEmployeePicture(HttpPostedFileBase file)
    {
        if (file != null)
        {
            UserProfile oreg = new UserProfile();
            string Check = oreg.AddPicture(file);
            return Json(Check, JsonRequestBehavior.AllowGet);
        }
        return Json("Fail", JsonRequestBehavior.AllowGet);
    }
    [OutputCache(Duration = 0)]
    [SessionTimeout]
    public void ChangeSite(string site)
    {
        cSite oSite = new cSite();
        oSite.UpdateDefaultSite(site);
        //  string ProgramBySite = oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["Sites"].ToString());

        string ProgramBySite = oSite.GetProgramBysite(site);
        Session["DefaultSite"] = site;
        //Session["ProgramId"] = ProgramBySite; //No Use any where. commit: 1
        Session["ProgramForSite"] = oSite.ProgramforSite;
        Session["ProgramNamesforSite"] = oSite.ProgramNamesforSite; //commit: 1
        //Session["ProgramId"] = program;

    }
    [OutputCache(NoStore = true, Location = System.Web.UI.OutputCacheLocation.Client, Duration = 2)] //addded by tahir to fix this issue "Duration must be a positive number in mvc 5"
    [HttpGet]
    [ChildActionOnly]
    public ActionResult GetProgramBySite()
    {
        oHome = new Home();
        DataTable dtProgram = oHome.GetProgramBySite();
        ViewBag.ddProgram = cCommon.ToDropDown(dtProgram, "ID", "NAME", "");
        return PartialView("GetProgramBySite");

    }
    [HttpPost]
    public ActionResult SaveLog(double gridExecutionTime) //Added by Huzaifa
    {
        cLog oLog = new cLog();
        Home oHome = new Home();
        DataTable dt = oHome.GetRecord();
        string recNo = dt.Rows[0][0].ToString();
        string rptUrl = dt.Rows[0][1].ToString();
        TimeSpan duration = TimeSpan.FromSeconds(gridExecutionTime);
        string gridExecTime = $"{(int)duration.Minutes:00}:{duration.Seconds:00}";
        oLog.UpdateLog(gridExecTime, recNo, rptUrl);

        return new EmptyResult();
    }
    //public JsonResult GetSyncCount()//Added by Huzaifa
    //{
    //    oHome = new Home();

    //    bool Check = oHome.GetSyncCount();

    //    return Json(Check, JsonRequestBehavior.AllowGet);
    //}
}
