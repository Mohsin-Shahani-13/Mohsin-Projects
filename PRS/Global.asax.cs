using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using IP.Classess;
using IP.Models;

namespace IP
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        }

        protected void Session_Start()
        {
            //// Get Connection string

            string fileName = ConfigurationManager.AppSettings["Key"];

            Session["CONN_INIT"] = BasicEncrypt.Instance.Encrypt(System.IO.File.ReadAllLines(fileName)[0].ToString());

            cDAL oDAL = new cDAL("INIT");
            DataTable dt = oDAL.GetData("SELECT * FROM zConStr ");
            if (dt.Rows.Count > 0)
            {
                // dt.DefaultView.RowFilter = "ConType = 'TEST'";
                Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(dt.Rows[0]["ConValue"].ToString());
               



                //foreach (DataRow row in dt.Rows)
                //{
                //    Session["CONN_" + row["ConType"]] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());


                    //        string Apptype = row["AppType"].ToString();
                    //        if (Apptype == "BIZTALK_PROD_INBOUND")
                    //        {
                    //            Session["CONN_Z001_INBOUND"] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());
                    //        }
                    //        else if (Apptype == "BIZTALK_PROD_OUTBOUND")
                    //        {
                    //            Session["CONN_Z001_OUTBOUND"] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());
                    //        }
                    //        else if (Apptype == "BIZTALK_TRAN_INBOUND")
                    //        {
                    //            Session["CONN_Z004_INBOUND"] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());
                    //        }
                    //        else if (Apptype == "BIZTALK_TRAN_OUTBOUND")
                    //        {
                    //            Session["CONN_Z004_OUTBOUND"] = BasicEncrypt.Instance.Encrypt(row["ConValue"].ToString());
                    //        }
                    //    }

                    //}

                    //// End



                    //string winLogin = Request.ServerVariables["LOGON_USER"];
                    //if (string.IsNullOrEmpty(winLogin))
                    //{
                    //    winLogin = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    //}

                    //Session["Domain"] = winLogin.Substring(0, winLogin.IndexOf("\\"));

                    //winLogin = winLogin.Substring(winLogin.IndexOf("\\") + 1);
                    //Session["LogonUser"] = winLogin;

                    string remoteIP = string.Empty;
                    string remoteHost = string.Empty;
                    string regIP = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
                    System.Net.IPHostEntry pcIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

                    remoteHost = pcIP.HostName;
                    foreach (var ipVal in pcIP.AddressList)
                    {
                        if (Regex.IsMatch(ipVal.ToString(), regIP))
                        {
                            remoteIP = ipVal.ToString();
                            break;
                        }
                    }

                    if (Request.IsLocal)
                    {
                        Session["RemoteHost"] = remoteHost;
                        Session["RemoteAddr"] = remoteIP;
                    }
                    else
                    {
                        Session["RemoteHost"] = Request.ServerVariables["Remote_Host"];
                        Session["RemoteAddr"] = Request.ServerVariables["Remote_Addr"];
                    }

                //    cEmployee oEmp = new cEmployee();

                //    oEmp.GetEmployeeByLogon("");

                //    if (oEmp.HasEmployee)
                //    {
                //        Session["EmpId"] = oEmp.EmpId;
                //        Session["EmpName"] = oEmp.EmpName;
                //        Session["DefaultSite"] = oEmp.DefaultSite;
                //        Session["ProgramId"] = oEmp.DefaultProgram;
                //        Session["CONN_TYPE"] = oEmp.ConType;
                //        Session["CONN_ACTIVE"] = BasicEncrypt.Instance.Encrypt(oEmp.GetConnectionString(oEmp.ConType, "PLUS"));

                //    }

                //    // Get Programs for site selected

                //    cSite oSite = new cSite();
                //    oSite.GetProgramBysite(System.Web.HttpContext.Current.Session["DefaultSite"].ToString());
                //    Session["ProgramForSite"] = oSite.ProgramforSite;
                //}

           }
        }
        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }
        protected void Session_End(object sender, EventArgs e)
        
        {

        }

        void Application_Error(object sender, EventArgs e)
        {
            if (Session["EmpName"] == null)
            {
                Response.Redirect("Error.html");
            }
            else
            {
                string path = Request.Url.PathAndQuery;
                Exception ex = Server.GetLastError();
                cLog oLog = new cLog();
                oLog.RecordError(ex.Message, ex.StackTrace, string.Empty);
                Response.Redirect("/Home/Error?page=detailed&message="+ ex.Message);
                Server.ClearError();
            }
        }
    }
}