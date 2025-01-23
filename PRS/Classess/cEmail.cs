using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace IP.Classess
{
    public class cEmail
    {
        Dictionary<string, object> tableColumns;
        public Dictionary<string, object> SubjectTags { get; set; }
        public Dictionary<string, object> BodyTags { get; set; }
        public string LoadTagsFrom { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string MailTo = "";
        public string Subject { get; set; }
        public string Body { get; set; }
        public string CcTo = "";
        public string BCcTo = "";
        public string Attachment { get; set; }
        public bool sentEmail { get; set; }    // if email has sent then true.

        public cEmail()
        {
            tableColumns = new Dictionary<string, object>();
            SubjectTags = new Dictionary<string, object>();
            BodyTags = new Dictionary<string, object>();

            LoadTagsFrom = string.Empty;
            SenderName = string.Empty;
            SenderEmail = string.Empty;
            MailTo = string.Empty;
            CcTo = string.Empty;
            BCcTo = string.Empty;
            Attachment = string.Empty;
            sentEmail = false;
        }

        /// <summary>
        /// A generic method to sends an email.
        /// </summary>
        public void LogMail(cDAL oDataLayer, string cnStr, string isLive)
        {
            MailTo = MailTo.Replace(",", ";");
            CcTo = CcTo.Replace(",", ";");
            BCcTo = BCcTo.Replace(",", ";");
            if (string.IsNullOrEmpty(MailTo))
                return;

            if (string.IsNullOrEmpty(SenderName) && string.IsNullOrEmpty(SenderEmail))
            {
                SenderName = cCommon.GetSysValue("DEFAULT_EMAIL_NAME");
                SenderEmail = cCommon.GetSysValue("DEFAULT_EMAIL_FROM");
            }

            // removes duplicate email ids.
            List<string> emailAddress = new List<string>();
            emailAddress.AddRange(MailTo.Split(';'));
            MailTo = string.Empty;
            foreach (string id in emailAddress.Distinct())
            {
                if (!CcTo.Contains(id) && !BCcTo.Contains(id))
                    MailTo += id + ";";
            }

            if (MailTo.LastIndexOf(';') > 0)
                MailTo = MailTo.Remove(MailTo.LastIndexOf(';'), 1);

            // Load values from sys ini.
            Subject = cCommon.GetSysValue(LoadTagsFrom + "_SUBJECT");
            Body = cCommon.GetSysValue(LoadTagsFrom + "_BODY");

            if (isLive == "N")
                Body += cCommon.GetSysValue("DEMO_MESSAGE");

            // Replace Subject tags with values..
            foreach (var itm in SubjectTags)
                Subject = Subject.Replace(itm.Key, itm.Value.ToString());

            // Replace Subject tags with values..
            foreach (var itm in BodyTags)
                Body = Body.Replace(itm.Key, itm.Value.ToString());

            // Insert data into Log Email table.
            tableColumns.Add("SenderName", SenderName);
            tableColumns.Add("SenderEmail", SenderEmail);
            tableColumns.Add("SendTo", MailTo);

            if (!string.IsNullOrEmpty(CcTo))
                tableColumns.Add("Cc", CcTo);

            if (!string.IsNullOrEmpty(BCcTo))
                tableColumns.Add("Bcc", BCcTo);

            tableColumns.Add("Subject", Subject);
            tableColumns.Add("BODY", Body);
            tableColumns.Add("HTML", "Y");

            oDataLayer.AddQuery(cDAL.QueryType.Insert, "CWDW.DBO.zLogEmail", tableColumns);
        }

        public static void SendEmail(cDAL oDAL, string syscode, string subject, string body)
        {
            string senderEmail = "";
            string sendTo = "";
            string cc = "";
            string bcc = "";
            string query = " SELECT SysValue, SysDesc FROM zSysIni WHERE syscode = '" + syscode + "'";

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


        public static string GetTableLine(int columns, string[] values)
        {
            string line = "<tr>";
            for (int i = 0; i < columns; i++)
            {
                line += "<td>" + values[i] + "</td>";
            }
            line += "</tr>";
            return line;
        }
    }
}