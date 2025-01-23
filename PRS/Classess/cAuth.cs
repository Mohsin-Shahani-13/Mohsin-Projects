using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.DirectoryServices.AccountManagement;

namespace IP.Models
{
    public class cAuth
    {
        cDAL oDAL = null;
        public string SigninId { get; set; }
        public string Password { get; set; }
        public string ProgramId { get; set; }
        public string Program { get; set; }
        public string FirstName { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string LastSignInDt { get; set; }
        public string Message { get; set; }
        public string EmailMessage { get; set; }
        public string UsrMenu { get; set; }
        public string isAdmin { get; set; }
        public string Autenticated { get; set; }
        public string DefaultSite { get; set; }
        public DataTable VendorList { get; set; }
        public DataTable Connection { get; set; }

        public string ConType { get; set; }
        public string Permission { get; set; }
        public bool CheckUser(string username, string password, string domain)
        {
            bool check = false;
            //Check Existing User
            // int dbChkUsr = Convert.ToInt32(CheckExistingUser(username));
            bool domainChkUsr = ValidateActDirUser(username, password, domain);
            //UserName have  authentication in Domain and Database 
            if (domainChkUsr)
            {

                check = true;
                return check;
            }

            if (domainChkUsr)
            {
                Autenticated = "authenticated";
            }
            return check;
        }
        //Add User Records
        public void AddUser(String empName)
        {
            oDAL = new cDAL("INIT");
            string sql =
                    @"
                        INSERT INTO Employee                                   
                          (
                           
                           EmpName,
                           WinLogin,
                           IsActive  
                           ) 
                           Values 
                            (
                            '@EmpName',
                            '@WinLogin',
                            '@IsActive'                                  
                           ) 
                            ";
            sql = sql.Replace("@EmpName", empName);
            sql = sql.Replace("@WinLogin", empName);
            sql = sql.Replace("@IsActive", "1");

            oDAL.Execute(sql);
        }

        //Check Existing User
        public Object CheckExistingUser(string userName)
        {
            oDAL = new cDAL("INIT");
            string sql = @"
             SELECT COUNT(WinLogin) 
                    FROM Ip.Employee 
                    WHERE WinLogin = '@userName'";
            sql = sql.Replace("@userName", userName);
            return oDAL.GetObject(sql);
        }
        //Get UserName Detail
        public DataTable GetUserDetail(string userName)
        {
            oDAL = new cDAL("INIT");
            string sql = @"
                SELECT EmpId,EmpName,IsAdmin,DBType, DefaultSite 
                FROM Employee 
                WHERE WinLogin = '@userName'";
            sql = sql.Replace("@userName", userName);
            return oDAL.GetData(sql);
        }

        //Get User Permission
        public Object GetUserPermission(string userId)
        {
            string sql = "SELECT COUNT(UserId) FROM UserMnuX WHERE USERID = '@userId'";
            sql = sql.Replace("@userId", userId);
            return oDAL.GetObject(sql);
        }

        //User Last Login
        public void UpdateLastLogin(int userId)
        {
            string sql = @"
                                 UPDATE IP.Employee 
                                 SET LastLoginDate=GETDATE() 
                                  WHERE EmpId='@userId'
                                 ";
            sql = sql.Replace("@userId", userId.ToString());
            oDAL.Execute(sql);
        }
        //Domain check
        public bool ValidateActDirUser(string username, string password, string sDomain)
        {
            bool bResult = false;
            try
            {
                if (sDomain == "teleplan")
                {
                    PrincipalContext domain;
                    try
                    {
                        // Connect to the domain:
                        domain = new PrincipalContext(ContextType.Domain, "tgn.teleplan.com", username, password);
                        bResult = domain.ValidateCredentials(username, password);

                        if (domain.ConnectedServer.Contains("Exception"))
                            bResult = false;

                        if (!bResult)
                            Message = "Authenticated! failed";
                    }
                    catch (Exception ex)
                    {
                        Message = "Authenticated! failed";
                        // Unable to connect to the domain (connection error or bad username/password):
                        return false;
                    }
                    return bResult;
                }
                else if (sDomain == "Valuout")
                {
                    PrincipalContext domain;
                    try
                    {
                        // Connect to the domain:
                        domain = new PrincipalContext(ContextType.Domain, "valuout.com", username, password);
                        bResult = domain.ValidateCredentials(username, password);

                        if (domain.ConnectedServer.Contains("Exception"))
                            bResult = false;

                        if (!bResult)
                            Message = "Authenticated! failed";
                    }
                    catch (Exception ex)
                    {
                        Message = "Authenticated! failed";
                        // Unable to connect to the domain (connection error or bad username/password):
                        return false;
                    }
                    return bResult;
                }
                else if (sDomain == "reconext")
                {
                    PrincipalContext domain;
                    try
                    {
                        // Connect to the domain:
                        domain = new PrincipalContext(ContextType.Domain, "reconext.com", "svc_PlusRS", "4cce55PRS#");
                        bResult = domain.ValidateCredentials(username, password);

                        if (domain.ConnectedServer.Contains("Exception"))
                            bResult = false;

                        if (!bResult)
                            Message = "Authenticated! failed";

                    }
                    catch (Exception ex)
                    {
                        Message = "Authenticated! failed";
                        // Unable to connect to the domain (connection error or bad username/password):
                        return false;
                    }

                    return bResult;
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                bResult = false;
            }
            return bResult;
        }

        public bool UpdateEmpDbType(string conType)
        {
            oDAL = new cDAL("INIT");
            string sql = @"UPDATE Employee 
                           SET DBType = '" + conType + "' " +
                           "WHERE EmpId = '" + HttpContext.Current.Session["SigninId"] + "'";
            oDAL.Execute(sql);
            if (oDAL.HasErrors)
                return false;
            return true;
        }

        public bool CheckUserForRegister(string username, string password, string domain)
        {
            bool check = false;
            //Check Existing User
            int dbChkUsr = Convert.ToInt32(CheckExistingUser(username));
            bool domainChkUsr = ValidateActDirUser(username, password, domain);
            //UserName have  authentication in Domain and Database 
            if (domainChkUsr && dbChkUsr > 0)
            {
                //Get User Name Detail
                DataTable dt = GetUserDetail(username);
                SigninId = dt.Rows[0]["EmpId"].ToString();
                UserName = dt.Rows[0]["EmpName"].ToString();
                ConType = dt.Rows[0]["DBType"].ToString();
                DefaultSite = dt.Rows[0]["DefaultSite"].ToString();
                //Update Last Login
                UpdateLastLogin(Convert.ToInt32(SigninId));
                //Get User Type
                if (dt.Rows.Count > 0)
                    isAdmin = dt.Rows[0]["isAdmin"].ToString();
                if (isAdmin.ToString() == "False")
                {
                    //Get User Permission
                    var dtPermission = GetUserPermission(SigninId);
                    if (dtPermission.Equals(0))
                    {
                        GiveMnuPermission(SigninId, username);
                        // Permission = "0";
                    }
                }
                check = true;
                return check;
            }
            //UserName  authentication in Domain but username did not found in Database 
            if (domainChkUsr && dbChkUsr == 0)
            {
                //AddUserRecords
                AddUser(username);
                //Check Existing User
                int dbChkUsr1 = Convert.ToInt32(CheckExistingUser(username));
                if (dbChkUsr1 > 0)
                {
                    //Get User Detail
                    DataTable dt = GetUserDetail(username);
                    SigninId = dt.Rows[0]["EmpId"].ToString();
                    UserName = dt.Rows[0]["EmpName"].ToString();
                    ConType = dt.Rows[0]["DBType"].ToString();
                    DefaultSite = dt.Rows[0]["DefaultSite"].ToString();
                    //User Last Login 
                    UpdateLastLogin(Convert.ToInt32(SigninId));
                    if (dt.Rows.Count > 0)
                        isAdmin = dt.Rows[0]["isAdmin"].ToString();
                    //Get User Permission
                    if (isAdmin.ToString() == "False")
                    {
                        //Get User Permission
                        var dtPermission = GetUserPermission(SigninId);
                        if (dtPermission.Equals(0))
                        {
                            // Permission = "0";
                            GiveMnuPermission(SigninId, username);
                        }
                    }
                    check = true;
                    return check;
                }
            }
            return check;
        }

        public void GiveMnuPermission(string signId, string userName)
        {

            string queryForMnuX = "";
            string queryForMnu = "";
            DataTable dt = new DataTable();
            queryForMnu = "SELECT * FROM dbo.Mnu Where MnuParent = '1' AND MnuIsReady = '1' ";
            dt = oDAL.GetData(queryForMnu);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string mnuId = dt.Rows[i]["MnuId"].ToString();
                queryForMnuX = "INSERT INTO UserMnuX (UserId, MnuID , CreatedBy) VALUES ('" + signId + "', '" + mnuId + "', '" + userName + "')";
                oDAL.AddQuery(queryForMnuX);
            }

            oDAL.Execute();
        }
    }

    public class LoginStatus
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string TargetURL { get; set; }
    }
}