using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

public class cLog
{
    cDAL oDal = new cDAL("INIT");
    string sql = string.Empty;

    private void SaveMru(string rptCode)
    {
        int accessCount = 1;
        int empId = Convert.ToInt32(HttpContext.Current.Session["EmpId"]);
        string empName = HttpContext.Current.Session["EmpName"].ToString();

        sql = "SELECT AccessCount FROM zLogMru WHERE EmpId = " + empId + " AND RptCode = '" + rptCode + "' AND Origin = 'IP'";
        object obj = oDal.GetObject(sql);

        if (obj == null)
        {
            sql = "INSERT INTO zLogMru (EmpId, EmpName, RptCode, AccessCount, Origin) ";
            sql += "VALUES (" + empId + ", '" + empName + "', '" + rptCode + "', " + accessCount + ", 'IP')";
        }
        else
        {
            accessCount = Convert.ToInt32(obj) + 1;
            sql = "UPDATE zLogMru Set LogDt = GETDATE(), AccessCount = " + accessCount + " ";
            if (empId != 0)
                sql += "WHERE Origin = 'IP' AND EmpId = " + empId + " AND RptCode = '" + rptCode + "'";
            else
                sql += "WHERE Origin = 'IP' AND EmpName = '" + empName + "' AND RptCode = '" + rptCode + "'";
        }

        oDal.Execute(sql);
    }


    public void SaveLog(string rptName, string rptUrl, string rptCode)
    {

        rptUrl = rptUrl.Replace("'", "''");
        SaveMru(rptCode);

        sql = "INSERT INTO zLogQuery (SigninId, SigninName, RemoteHost, RptCode, RptName, RptUrl, InsertOn, Origin, QueryExecTime, RowsFetched) ";
        sql += "VALUES (";
        sql += "'" + HttpContext.Current.Session["EmpId"].ToString() + "', ";
        sql += "'" + HttpContext.Current.Session["EmpName"].ToString() + "', ";
        sql += "'" + HttpContext.Current.Session["RemoteAddr"].ToString() + "', ";
        sql += "'" + rptCode + "', ";
        sql += "'" + rptName + "', ";
        sql += "'" + rptUrl + "', ";
        sql += "Getdate(), ";
        sql += "'IP',";
        sql += "'" + cDAL.QueryExecTime + "', "; //Added by Huzaifa
        sql += "'" + cDAL.RowsFetched + "') "; //Added by Huzaifa

        oDal.Execute(sql);
    }

    public void UpdateLog(string gridExecTime, string recNo, string rptUrl) //Added by Huzaifa
    {
        cDAL oDal = new cDAL("INIT");
        string sql = string.Empty;

        sql = "UPDATE zLogQuery SET GridExecTime = '" + gridExecTime + "' " +
            "WHERE RECNUM = '" + recNo + "' " +
            "AND SigninId = '" + HttpContext.Current.Session["EmpID"].ToString() + "'" +
            "AND SigninName ='" + HttpContext.Current.Session["EmpName"].ToString() + "'" +
            "AND RptUrl = '" + rptUrl + "' ";

        oDal.Execute(sql);
    }


    public void RecordError(string errorMsg, string errorStack, string errorQry)
    {
        errorMsg = errorMsg.Replace("'", "");
        errorQry = errorQry.Replace("'", "");

        object empName = HttpContext.Current.Session["EmpName"];

        sql = "INSERT INTO zLogError (ErrMsg, ErrStack, ErrBy, ErrOn, ErrFrom, ErrQry) ";
        sql += "VALUES (";
        sql += "'" + errorMsg + "', ";
        sql += "'" + errorStack + "', ";
        sql += "'" + ((empName == null) ? "unknown" : empName) + "', ";
        sql += "GetDate(), ";
        sql += "'IP', ";
        sql += "\'" + errorQry + "\') ";
        oDal.Execute(sql);
    }

    public void AddSqlQuery(string rptCode, string query, string headerText = "", bool isDetail = false)
    {
        query = query.Replace("'", "`");
        int empId = Convert.ToInt32(HttpContext.Current.Session["EmpId"]);
        if (isDetail == true)
        {
            sql = "DELETE FROM IP.SQLQuery WHERE EmpId = " + empId + " AND RptCode LIKE '" + rptCode + "%'";
            oDal.Execute(sql);
        }

        sql = "SELECT COUNT(*) FROM IP.SQLQuery WHERE EmpId = " + empId + " AND RptCode = '" + rptCode + "'";
        object obj = oDal.GetObject(sql);
        int count = Convert.ToInt32(obj);
        if (count == 0)
        {
            sql = "INSERT INTO IP.SQLQuery (RptCode, EmpId, HeaderText, Query, QueryOn) ";
            sql += "VALUES ('" + rptCode + "', " + empId + ", '" + (headerText == "" ? "" : headerText)
            + "', '" + query + "', GETDATE())";
        }
        else
        {
            sql = "UPDATE IP.SQLQuery Set HeaderText = '" + headerText + "', Query = '" + query + "', QueryOn = GETDATE() ";
            sql += "WHERE EmpId = " + empId + " AND RptCode = '" + rptCode + "'";
        }
        oDal.Execute(sql);
    }
    public static void SendEmail(string syscode, string subject, string body)
    {
        string senderEmail = "";
        string sendTo = "";
        string cc = "";
        string bcc = "";
        string query = " SELECT SysValue, SysDesc FROM zSysIni WHERE syscode = '" + syscode + "'";

        cDAL oDAL = new cDAL("INIT");
        DataTable dt = oDAL.GetData(query);

        if (dt.Rows.Count > 0)
        {
            string[] str = dt.Rows[0]["SysDesc"].ToString().Split('|');
            senderEmail = dt.Rows[0]["SysValue"].ToString();
            sendTo = str[0];
            cc = str[1];
            bcc = str[2];
        }

        query = "INSERT INTO zLogEmail(SenderName, SenderEmail, SendTo, Cc, Bcc, Subject, Body, SentOn) ";
        query += "VALUES('<SenderName>', '<SenderEmail>', '<SendTo>', '<Cc>', '<Bcc>', '<Subject>', '<Body>', <SentOn>)";
        query = query.Replace("<SenderName>", "OctelIP");
        query = query.Replace("<SenderEmail>", senderEmail);
        query = query.Replace("<SendTo>", sendTo);
        query = query.Replace("<Cc>", cc);
        query = query.Replace("<Bcc>", bcc);
        query = query.Replace("<Subject>", subject);
        query = query.Replace("<Body>", body);
        query = query.Replace("<SentOn>", "NULL");
        oDAL.Execute(query);
    }


    public static string GetEmailBody(List<string> lst)
    {
        StringBuilder sb = new StringBuilder();
        foreach (string item in lst)
        {
            sb.AppendLine(item);
        }
        return sb.ToString();
    }
}
