using IP.Classess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

public class cEmployee
{
    string sql = string.Empty;
    cDAL oDal = new cDAL("INIT");

    public int EmpId { get; set; }
    public string EmpName { get; set; }
    public string WinLogin { get; set; }
    public bool HasEmployee { get; set; }
    public string DefaultSite { get; set; }
    public string DefaultProgram { get; set; }
    public int EmpIdApp { get; set; }
    public string SiteIdPhyApp { get; set; }
    public string EmpNameApp { get; set; }
    public string ConType { get; set; }
    public string ProfilePicture { get; set; }

    public static string GetSingleImage(string path, int? width = null, int? height = null, long jpegQuality = 100L)
    {
        try
        {
            return "data:image/png;base64," + path;
        }
        catch (Exception ex)
        {
            // If error, return error image
            return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAABgmlDQ1BzUkdCIElFQzYxOTY2LTIuMQAAKJF1kb9LQlEUxz9qYZRRkENDg4k1VViB1NKglAXVYAb9WvTlj8Afj/eUkNagVSiIWvo11F9Qa9AcBEURREtLc1FLyeu8FJTIczn3fO733nO491ywhlNKWm/wQjqT00JBv2thccllf8FOA07cdEcUXZ2ZmwhT1z7vsZjxtt+sVf/cv9ayGtMVsDQJjymqlhOeFJ5ez6km7wg7lWRkVfhMuE+TCwrfmXq0zK8mJ8r8bbIWDgXA2i7sStRwtIaVpJYWlpfjSafySuU+5kscscz8nES3eBc6IYL4cTHFOAF8DDIqs49+hhiQFXXyvb/5s2QlV5FZpYDGGgmS5OgTNS/VYxLjosdkpCiY/f/bVz0+PFSu7vBD47NhvPeAfRtKRcP4OjKM0jHYnuAyU83PHsLIh+jFquY5gLZNOL+qatFduNiCzkc1okV+JZu4NR6Ht1NoXYSOG2heLvesss/JA4Q35KuuYW8feuV828oPZNBn5Sp0JFQAAAAJcEhZcwAACxMAAAsTAQCanBgAAAKwSURBVFiFzde9jw5RFAbwn1GsbCwqCVqR2ElofBUKGmSJDSVapUo0EgpkK9v5F7b2kbXJamwoCFuIGApKloTCkmxssxT3Tox57/vOePeVeJIp5n6d55577jnPXaMlCoZwGCcxiq3xg4X4vcY9PMxZbrPumhaGt+AqzmKkJd/vmMK1nI99EShYh8u4iOGWhutYwiQmcn60JhB3fRv7+zRcx1OcTnmjg0DBLsxgW8Oii3grxMYo1jaM/4CxnJddCcSdP+9h/Clu4hG+5PyM84axG3twHEe7zH+PvTmfOgjEM5+TdvsKxnG/NNoNRVjzDG5hU5dNHC5jIqt0XO5ivBx3BRt6GYecn3m4AblwlHUciLYQPRBd/05ztD/DkTycfyOiN6YxVutawvacj6UHriaMryTW3IcHBRvbEIjHdR5fa13D0aYsZrhzifnjwo5XS2IBFxJdZwuGMiG9rq91PsF9HBkECSEmZmttIziUCbm9jskYTIuDIBGPIhWQ45mQROp4XJk8EBKYT7TtzPyuaCUW8bnaMCASL3QG9tYUgbepZLNaEnm4ekWKQB1DPRZZrSc6NEImXJMqRoseCalfEjEp7ag1L6QIrBUKS1f0SWKzzlS+kAkyqo49vQj0SeJgYtybTNBwdYwVLeRaWxJxrUuJMXczPBQ0XBXHhJLaiDYkcEJnpf2OuSyq16nE5FtF5xXtl8SdRPtUznJTOZ7BiSYRUiKe+YNotBf+LMdRLE4mBo5hekCeqGKyFKjVRDQhyKUUiVdFKJ+NgYlvuCGtJ0QbE+XP34rSWeFY5vEiptcyyWwWrtol3aVdd1FaIdFWlq+E4ZaFDNekF9/jeE9ZXiHxLx4mp6o7L5EqRmVQHsJ10c19YgnXBBneYZz/+XGaIDIkeGUcO6Wf529wF3Ntn+e/ALav24H68Z8pAAAAAElFTkSuQmCC";
        }

    }


    public void GetEmployeeByLogon(string logonUser, bool isFirst = true)
    {
        sql = "SELECT EmpId, EmpName, WinLogin, DefaultSite,DefaultProgram, ISNULL(EpicorCon,'TEST') AS ConnType, ProfilePicture FROM IP.Employee ";
        sql += "WHERE WinLogin = '" + logonUser + "' AND ";
        sql += "IsActive = 1 ";
        DataTable dt = oDal.GetData(sql);
        if (dt.Rows.Count > 0)
        {
            HasEmployee = true;
            EmpId = Convert.ToInt32(dt.Rows[0]["EmpId"]);
            WinLogin = dt.Rows[0]["WinLogin"].ToString();
            EmpName = dt.Rows[0]["EmpName"].ToString();
            DefaultSite = dt.Rows[0]["DefaultSite"].ToString();
            DefaultProgram = dt.Rows[0]["DefaultProgram"].ToString();
            ConType = dt.Rows[0]["ConnType"].ToString();
            ProfilePicture = dt.Rows[0]["ProfilePicture"].ToString();
            string photo = "";
            if (ProfilePicture != "")
            {
                photo = GetSingleImage(ProfilePicture, 50, 50, 100);
            }
            ProfilePicture = photo;

            if (isFirst == true)
                UpdateEmployee(logonUser);
        }
        else
        {
            SaveEmployee(logonUser);
            GetEmployeeByLogon(logonUser, false);
        }


    }

    public bool HasHelpDoc(string logonUser, string sysValue)
    {
       
        if(sysValue.Contains(logonUser))
        {
            return true;
        }
        else
        {
            return false;
        }
       
    }
    private void SaveEmployee(string logonUser)
    {
        var sql = $@"
INSERT INTO IP.Employee
    (EmpName, WinLogin, IsActive, EpicorCon, PicsCon, DefaultSite)
Values
    ('{logonUser}', '{logonUser}', 1, 'TEST', 'NA', 'DEMO')
";
        oDal.Execute(sql);
    }

    private void UpdateEmployee(string logonUser)
    {
        sql = "UPDATE IP.Employee ";
        sql += "SET LastLoginDate = '" + DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss") + "' ";
        sql += "WHERE WinLogin = '" + logonUser + "'";
        oDal.Execute(sql);
    }

    public string GetConnectionString(string conType, string appType)
    {
        sql = "SELECT ConValue FROM zConStr WHERE ConType = '" + conType + "' AND AppType ='" + appType + "'";
        DataTable dt = oDal.GetData(sql);
        return dt.Rows[0]["ConValue"].ToString();
    }

    public void GetEmployeeConType(string winlogin, ref string epicorCon)
    {
        sql = "SELECT WinLogin, ISNULL(EpicorCon,'') AS EpicorCon FROM IP.Employee WHERE WinLogin = '" + winlogin + "'";
        DataTable dt = oDal.GetData(sql);
        epicorCon = dt.Rows[0]["EpicorCon"].ToString();
    }

    public void UpdateEmployeeConType(string winlogin, string conType)
    {
        sql = "UPDATE IP.Employee SET EpicorCon = '" + conType + "' WHERE WinLogin = '" + winlogin + "'";
        oDal.Execute(sql);
    }

    public static ArrayList GetEmployeeAxs(string report_code)
    {
        ArrayList al = new ArrayList();
        string sql = "SELECT A.AxsCode FROM IP.Axs A " +
                 "INNER JOIN IP.EmpAxsX EA ON EA.AxsId = A.AxsId " +
                 "WHERE A.RptCode = '" + report_code + "'  " +
                 "AND EA.WinLogin = '" + HttpContext.Current.Session["LogonUser"].ToString() + "' ";
        cDAL oDAL = new cDAL("INIT");
        DataTable dt = oDAL.GetData(sql);
        if (dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                al.Add(row["AxsCode"].ToString());
            }
        }
        return al;
    }

}
