using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace IP.Areas.ListingReports.Models
{
    public class VFNERSPickList
    {
        cDAL oDAL = new cDAL("ACTIVE");

        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public List<Hashtable> lstVFNERSPickList { get; set; }

        #endregion

        #region Methods 
        public DataTable GetProgramBySite()
        {
            //oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
 and Name = 'verifone'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);


            return dt;
        }
        public bool GetList(string Type, string TypeText, string programId, string ProgramName)
        {
            string query = @"
                SELECT pt.ProgramID,
                       pt.PartNo,
                       pt.serialno, 
                       pt.CustomerReference,
                       '' AS [IDSStatus],
                       slb.value AS pick, 
                       pt.CreateDate AS 'Pick Transaction Date', 
                       h.processed_date AS 'Pick Message Processed Date',
                       h.message, 
                       h.Outmessage_Hdr_Id, 
                       sla.solineid, 
                       l.soheaderid
                FROM pls.vPartTransaction pt
                JOIN pls.vsolineattribute sla 
                     ON sla.attributename = 'customerlineno' AND sla.value = pt.customerreference
                LEFT JOIN pls.vsolineattribute slb 
                     ON slb.AttributeName = 'PICKSERIAL' AND slb.SOLineID = sla.SOLineID
                JOIN pls.vsoline l 
                     ON l.id = sla.solineid
                LEFT JOIN [Biztalk_Outmessages].dbo.outmessage_hdr h WITH(NOLOCK) 
                     ON h.Customer_Prev_order_No = pt.customerreference 
                     AND h.contract = '<programId>' 
                     AND h.processed_date >= pt.createdate 
                     AND h.message_type = 'RVER-PickSerial'
                LEFT JOIN [Biztalk_Outmessages].dbo.outmessage_serial s WITH(NOLOCK) 
                     ON s.outmessage_hdr_id = h.outmessage_hdr_id AND s.serial_no = pt.serialno
                WHERE pt.programid = '<programId>' AND pt.parttransaction = 'WO-WIP' ";

            if (Type == "LastHour")
            {
                query += "and pt.createdate >= DATEADD(HOUR, -1, GETDATE()) ";
            }
            else
            {
                query += "and pt.createdate >= convert(date,getdate()-1) ";
            }

            query += @"union all
select pt.ProgramID, pt.PartNo, pt.serialno, lb.value as CustomerReference, '' AS [IDSStatus], la.value as pick, pt.createdate as 'Pick Transaction Date', h.processed_date as 'Pick Message Processed Date'
, h.message, h.Outmessage_Hdr_Id, l.id as solineid,pt.orderheaderid as soheaderid
from pls.parttransaction pt
join pls.soline l on l.soheaderid = pt.orderheaderid and l.id = pt.orderlineid
join pls.solineattribute lb on lb.solineid = l.id and lb.attributeid = 48
left join pls.solineattribute la on la.solineid = l.id and la.attributeid = 289
left join pls.solineattribute lc on lc.solineid = l.id and lc.attributeid = 209
left join[Biztalk_Outmessages].dbo.outmessage_hdr h with(NOLOCK) on h.Customer_Prev_order_No = lb.value and h.contract = '<programId>' and h.processed_date >= pt.createdate and h.message_type = 'RVER-PickSerial'
left join[Biztalk_Outmessages].dbo.outmessage_serial s with(NOLOCK) on s.outmessage_hdr_id = h.outmessage_hdr_id and s.serial_no = pt.serialno
where pt.programid = '<programId>'
and pt.parttransactionid = 16
and(pt.customerreference not like '%-cancel' or pt.customerreference not like 'REY%')
and lc.value in ('NS-from-Stock', 'Order-Deployment') ";

            if (Type == "LastHour")
            {
                query += "and pt.createdate >= DATEADD(HOUR, -1, GETDATE()) ";
            }
            else
            {
                query += "and pt.createdate >= getdate()-1  ";
            }

            query += "ORDER BY pt.createdate DESC ";

            query = query.Replace("<programId>", programId);

            DataTable dt = oDAL.GetData(query);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }

            // Process IDS API Call for Each Row
            foreach (DataRow row in dt.Rows)
            {
                string serialno = row["serialno"].ToString();
                if (serialno != "*")
                {
                    string idsStatus = GetIDSStatus(serialno);
                    row["IDSStatus"] = idsStatus;
                }
            }

            if (!string.IsNullOrEmpty(ProgramName))
            {
                filterString += "> Program = '" + ProgramName + "' | Time Frame = '" + TypeText + "' ";
            }

            

            // For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("256", query, string.Empty, false);

            if (dt.Rows.Count > 0)
            {
                lstVFNERSPickList = cCommon.ConvertDtToHashTable(dt);
            }

            return true;
        }

        private string GetIDSStatus(string serialno)
        {
            try
            {
                if (serialno == "*")
                    return "N/A";

                string apiUrl = "https://idsm.verifone.com/idsengine/deployment/orders/terminaldeploymentstatuses";
                string username = "osapiuser";
                string password = "V3r1f0n3_1234";

                var payload = new
                {
                    serialNumberOrderNum = serialno,
                    inputType = "serialNumber",
                    vhqHomeEnabled = "false",
                    site = "SDF",
                    checkTestTransactionStatus = false
                };

                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    // Enable TLS 1.2 explicitly (if needed)
                    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

                    using (HttpClient client = new HttpClient(handler))
                    {
                        // Set up Basic Authentication header
                        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                        // Set request headers
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Serialize the payload to JSON
                        var jsonPayload = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        // Send the POST request
                        HttpResponseMessage response = client.PostAsync(apiUrl, content).GetAwaiter().GetResult();

                        // Handle the response
                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                            return jsonResponse?.response?.jobStatus?[0]?.idsStatus?.ToString() ?? "Unknown";
                        }
                        else
                        {
                            throw new Exception($"API call failed. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Handle specific HTTP request exceptions
                Console.WriteLine($"HTTP Request Error: {httpEx.Message}");
                return "Error";
            }
            catch (Exception ex)
            {
                // General exception handling
                Console.WriteLine($"Error fetching IDS Status for serial number {serialno}: {ex.Message}");
                return "Error";
            }
        }

        #endregion
    }
}
