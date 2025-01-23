using IP.Classess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class Image
    {
        #region fields
        cDAL oDAL = new cDAL("ACTIVE");

        [Display(Name = "Program:")]
        public string ProgramName { get; set; }
        [Display(Name = "Program:")]
        public string ProgramID { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }

        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Serial No.:")]
        public string serialNo { get; set; }
        [Display(Name = "Customer Ref.:")]
        public string custRef { get; set; }
        public List<Hashtable> lstImage { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public byte[] imageBytes;
        public string ImageBase64String { get; set; }
        cLog oLog = new cLog();
        #endregion
        #region methods
        public DataTable GetProgramBySite()
        {
            oDAL = new cDAL("ACTIVE");
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            string query = string.Empty;
            query = @"select ID AS programId
                             ,NAME AS programName
                             FROM pls.PROGRAM  
                      WHERE SITE = '<site>'
                      ORDER BY NAME ";
            query = query.Replace("<site>", sites);
            DataTable dt = oDAL.GetData(query);
            return dt;
        }
        public bool GetList(string programId, string programName, string fromDt, string toDt, string serialNo, string custRef, bool checkAllDate)
        {
            string _serialNo = GetInValue(serialNo);
            string query = string.Empty;
            query = @"
             SELECT  A.ID, 
		A.ProgramId,
        P.Name as programName,
		--A.Name,
        CASE 
        WHEN CHARINDEX('DataEntry_', A.[Name]) > 0 THEN 
            B.Name
        ELSE A.TableName
        END AS ScriptName,
		A.Picture, 
		A.OrderNo, 
		A.SerialNo, 
		A.PartNo, 
		A.CustomerReference, 
		A.TableName, 
		A.TableID, 
		A.ImageDataID,  
		A.CreateDate, 
		A.LastActivityDate 
FROM pls.Image A
INNER JOIN pls.Program P ON P.ID = A.ProgramId
LEFT JOIN pls.DataEntryScript B ON A.TableID = B.ID
";

            if (programId != "0")
            {
                query += "WHERE A.[ProgramId] = '" + programId + "' ";
            }
            else
            {
                query += "WHERE A.[ProgramId] IN (" + HttpContext.Current.Session["ProgramForSite"].ToString() + ") ";
            }
            if (!checkAllDate)
            {
                query += @" AND CONVERT(Date, A.CreateDate) >=  '<frmDt>' AND CONVERT(Date, A.CreateDate) <= '<toDt>'";
            }

            if (!string.IsNullOrEmpty(serialNo))
                query += "AND A.[SerialNo] IN (" + _serialNo + ")";
            if (!string.IsNullOrEmpty(custRef))
                query += "AND A.CustomerReference LIKE '%" + custRef + "%'";

            query += @"
            Order By 
            A.CreateDate DESC
            ";
            query = query.Replace("<frmDt>", fromDt);
            query = query.Replace("<toDt>", toDt);
            DataTable dt = oDAL.GetData(query);

            if (!string.IsNullOrEmpty(programName))
                filterString += "> Program = '" + programName + "' ";

            if (!checkAllDate)
            {
                filterString += " | From = '" + fromDt + "' To = '" + toDt + "' ";
            }

            if (!string.IsNullOrEmpty(serialNo))
                filterString += " | Serial No. =  " + _serialNo + "";
            if (!string.IsNullOrEmpty(custRef))
                filterString += " | Cust. Ref =  " + custRef + "";


            //For SQL Documentation

            oLog.AddSqlQuery("180", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstImage = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        public bool GetImage(string imageDataID)
        {
            string query = string.Empty;
            query = @"SELECT 
                Picture, 
                FilePath 
                    FROM 
                [plusimage].plusimage.pls.imagedata 
                    WHERE 
                id = '<imageDataID>'
";
            query = query.Replace("<imageDataID>", imageDataID);

            DataTable dt = oDAL.GetData(query);
            if (dt.Rows.Count == 1)
            {
                byte[] imageBytes = dt.Rows[0]["Picture"] as byte[];
                if (imageBytes != null)
                {
                    ImageBase64String = Convert.ToBase64String(imageBytes);
                }
                else
                {
                    try
                    {
                        string filePath = dt.Rows[0]["FilePath"] as string;
                        using (new cImpersonate())
                        {
                            byte[] imageDataFromFilePath = File.ReadAllBytes(filePath);
                            ImageBase64String = Convert.ToBase64String(imageDataFromFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        oLog.RecordError(ex.Message, ex.StackTrace, "Report: Image - Method: GetImage(string imageDataID)");
                    }

                }

            }

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                //if (dt.Rows.Count > 0)
                //    lstImage = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
        private string GetInValue(string Value)
        {
            string[] arr = Value.Split(',');
            string _arr = null;
            foreach (var item in arr)
            {
                if (_arr == null)
                {
                    _arr = "\'" + item.Trim() + "\'";
                }
                else
                {
                    _arr += "," + "\'" + item.Trim() + "\'";
                }

            }
            return _arr;
        }
        #endregion
    }
}