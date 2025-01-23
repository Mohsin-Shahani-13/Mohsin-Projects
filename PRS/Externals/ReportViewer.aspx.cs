
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace IP.Externals
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string custRef = string.Empty;
            string PalletBoxNo = string.Empty;
            string rptName = Request.QueryString["rptName"].ToString();
            //string PalletBoxNo = Request.QueryString["PalletBoxNo"].ToString();
            string conType = HttpContext.Current.Session["CONN_TYPE"].ToString();
            if (rptName == "SOPickList")
            {
                custRef = Request.QueryString["CustRef"].ToString();
            }

            else if (rptName == "PackingList" )
            {
                PalletBoxNo = Request.QueryString["PalletBoxNo"].ToString();
            }

            else if (rptName == "ReceiptList")
            {
                PalletBoxNo = Request.QueryString["PalletBoxNo"].ToString();
            }
            else if (rptName == "ShipmentDocument")
            {
                custRef = Request.QueryString["CustRef"].ToString();
            }

            string url = string.Empty;

            if (rptName == "PackingList" && conType == "TEST")
            {
                url = "http://dc1prs02/ReportServer?/Plus/Toshiba/Packing_List_Test/&rs:Command=Render"
                                       + "&PalletBoxNo=" + PalletBoxNo + "&rs:Format=EXCEL";
                DownloadExcel(url);
            }

           else if (rptName == "PackingList" && conType == "TRAN")
            {
                url = "http://dc1prs02/ReportServer?/Plus/Toshiba/Packing_List_Tran/&rs:Command=Render"
                                       + "&PalletBoxNo=" + PalletBoxNo + "&rs:Format=EXCEL";
                DownloadExcel(url);
            }

            else if (rptName == "PackingList" && conType == "PROD")
            {
                url = "http://dc1prs02/ReportServer?/Plus/Toshiba/Packing_List_Prod/&rs:Command=Render"
                                       + "&PalletBoxNo=" + PalletBoxNo + "&rs:Format=EXCEL";
                DownloadExcel(url);
            }

            else if (rptName == "ReceiptList" && conType == "TEST")
            {
                url = "http://dc1prs02/ReportServer?/Plus/Toshiba/Test/Receipt_List/&rs:Command=Render"
                                       + "&PalletBoxNo=" + PalletBoxNo + "&rs:Format=EXCEL";
                DownloadExcel(url);
            }
            else if (rptName == "ReceiptList" && conType == "TRAN")
            {
                url = "http://dc1prs02/ReportServer?/Plus/Toshiba/Tran/Receipt_List/&rs:Command=Render"
                                       + "&PalletBoxNo=" + PalletBoxNo + "&rs:Format=EXCEL";
                DownloadExcel(url);
            }
            else if (rptName == "ReceiptList" && conType == "PROD")
            {
                url = "http://dc1prs02/ReportServer?/Plus/Toshiba/Prod/Receipt_List/&rs:Command=Render"
                                       + "&PalletBoxNo=" + PalletBoxNo + "&rs:Format=EXCEL";
                DownloadExcel(url);
            }

            else if (rptName == "SOPickList")
            {
                url = "http://dc1prs02/ReportServer?/PRS/SO_Pick_Ticket_R2/&rs:Command=Render"
                                        + "&CustRef=" + custRef + "&rs:Format=PDF";
                OpenPDF(url);
            }
            else if (rptName == "ShipmentDocument" && conType == "TEST")
            {
                url = "http://dc1prs02/ReportServer?/PRS/BOSE/ShipmentDocumentTEST/&rs:Command=Render"
                                       + "&CustRef=" + custRef + "&rs:Format=EXCEL";
                DownloadExcel(url);
            }

            else if (rptName == "ShipmentDocument" && conType == "TRAN")
            {
                url = "http://dc1prs02/ReportServer?/PRS/BOSE/ShipmentDocumentTRAN/&rs:Command=Render"
                                       + "&CustRef=" + custRef + "&rs:Format=EXCEL";
                DownloadExcel(url);
            }

            else if (rptName == "ShipmentDocument" && conType == "PROD")
            {
                url = "http://dc1prs02/ReportServer?/PRS/BOSE/ShipmentDocumentPROD/&rs:Command=Render"
                                       + "&CustRef=" + custRef + "&rs:Format=EXCEL";
                DownloadExcel(url);
            }

            else if (custRef != null)
            {
                custRef = Request.QueryString["CustRef"].ToString();
                url = "http://dc1prs02/ReportServer?/PRS/ERS_Pick_List_test/&rs:Command=Render"
                                  + "&CustRef=" + custRef + "&rs:Format=PDF";

                url = url.Replace(",", "&CustRef=");

                OpenPDF(url);
            }

        }

        protected void OpenPDF(string url)
        {
            HttpWebRequest loRequest;
            HttpWebResponse loResponse;
            Stream loResponseStream;
            byte[] laBytes = new byte[256];
            int liCount = 1;
            System.IO.FileInfo fileInfo;

            try
            {
                loRequest = (HttpWebRequest)WebRequest.Create(url);
                loRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
                loRequest.Timeout = 600000;
                loRequest.Method = "GET";
                loResponse = (HttpWebResponse)loRequest.GetResponse();
                loResponseStream = loResponse.GetResponseStream();
                //loResponseStream.Write(laBytes, 0, 256);

                byte[] fileBytes = ReadFully(loResponseStream);

                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();
                Response.Buffer = true;
                Response.Charset = "";
                //  Response.AddHeader("Content-Disposition", "inline; filename=" + oFile.Name);

                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Length", fileBytes.Length.ToString());
                Response.AddHeader("Content-disposition", "inline; filename= NombreReporte");
                //Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(fileBytes);
                Response.Flush();
                Response.End();
                // Could save to a database or file here as well.
                //Response.Clear();
                //Response.ContentType = "application/pdf";
                ////Response.AddHeader(
                ////    "content-disposition",
                ////    "attachment; filename=\"Report For " +
                ////        ParamValue + ".pdf\"");

                //Response.BinaryWrite(fileBytes);
                //Response.Flush();
                //Response.End();
            }
            catch (Exception ex)
            {
            }

        }

        protected void DownloadExcel(string url)
        {
            HttpWebRequest loRequest;
            HttpWebResponse loResponse;
            Stream loResponseStream;
            byte[] laBytes = new byte[256];
            int liCount = 1;
            System.IO.FileInfo fileInfo;

            try
            {
                loRequest = (HttpWebRequest)WebRequest.Create(url);
                loRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
                loRequest.Timeout = 600000;
                loRequest.Method = "GET";
                loResponse = (HttpWebResponse)loRequest.GetResponse();
                loResponseStream = loResponse.GetResponseStream();
                //loResponseStream.Write(laBytes, 0, 256);

                byte[] fileBytes = ReadFully(loResponseStream);

                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();
                Response.Buffer = true;
                Response.Charset = "";
                //  Response.AddHeader("Content-Disposition", "inline; filename=" + oFile.Name);

                //Response.ContentType = "application/EXCEL";
                //Response.AddHeader("Content-Length", fileBytes.Length.ToString());
                string rptName = Request.QueryString["rptName"].ToString();
                if (rptName == "PackingList")
                {
                    Response.AddHeader("content-disposition", "attachment; filename=PackingList.xls");
                    Response.ContentType = "application/ms-excel";
                }
                else if (rptName == "ReceiptList")
                {
                    Response.AddHeader("content-disposition", "attachment; filename=ReceiptList.xls");
                    Response.ContentType = "application/ms-excel";
                }

                else if (rptName == "ShipmentDocument")
                {
                    Response.AddHeader("content-disposition", "attachment; filename=ShipmentDocument.xls");
                    Response.ContentType = "application/ms-excel";
                }

                //Response.AddHeader("Content-disposition", "inline; filename= NombreReporte");
                //Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(fileBytes);
                Response.Flush();
                Response.End();
                // Could save to a database or file here as well.
                //Response.Clear();
                //Response.ContentType = "application/pdf";
                ////Response.AddHeader(
                ////    "content-disposition",
                ////    "attachment; filename=\"Report For " +
                ////        ParamValue + ".pdf\"");

                //Response.BinaryWrite(fileBytes);
                //Response.Flush();
                //Response.End();
            }
            catch (Exception ex)
            {
            }

        }

        protected void DownloadPDF(ArrayList lstUrl, string custRef)
        {
            HttpWebRequest loRequest;
            HttpWebResponse loResponse;
            Stream loResponseStream;
            byte[] laBytes = new byte[256];
            int liCount = 1;
            System.IO.FileInfo fileInfo;
            string url = string.Empty;
            string fileName = string.Empty;
            List<string> PDFfiles = new List<string>();

            byte[] fileBytes = null;
            string savePath2 = string.Empty;
            object newPDFFile = null;
            try
            {
                string[] arryCustRef = custRef.Split(',');

                for (int j = 0; j <= arryCustRef.Length - 1; j++)
                {
                    url = lstUrl[j].ToString();// url.Replace("<CusRef>", arryCustRef[j]);

                    loRequest = (HttpWebRequest)WebRequest.Create(url);
                    loRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
                    loRequest.Timeout = 600000;
                    loRequest.Method = "GET";
                    loResponse = (HttpWebResponse)loRequest.GetResponse();
                    loResponseStream = loResponse.GetResponseStream();
                    //  loResponseStream.Write(laBytes, 0, 256);

                    fileName = "ERS_Pick_List" + "_" + Convert.ToString(arryCustRef[j]) + ".PDF";

                    PDFfiles.Add(fileName);



                    fileBytes = ReadFully(loResponseStream);
                    string savePath = "E:\\PDFDoc\\" + "ERS_PICK_List.pdf";
                    savePath2 = savePath.Replace(@"\\", @"\");



                    using (FileStream stream = new FileStream(savePath2, FileMode.Create))
                    {
                        stream.Write(fileBytes, 0, fileBytes.Length);
                    }

                }



                //MergePDFs(savePath2, PDFfiles);
                // MergePDF("", "");
                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();
                Response.Buffer = true;
                Response.Charset = "";
                //  Response.AddHeader("Content-Disposition", "inline; filename=" + oFile.Name);

                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Length", fileBytes.Length.ToString());
                Response.AddHeader("Content-disposition", "inline; filename= NombreReporte");
                //Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(fileBytes);
                Response.Flush();
                //Response.End();
                // Could save to a database or file here as well.
                //Response.Clear();
                //Response.ContentType = "application/pdf";
                ////Response.AddHeader(
                ////    "content-disposition",
                ////    "attachment; filename=\"Report For " +
                ////        ParamValue + ".pdf\"");

                //Response.BinaryWrite(fileBytes);
                //Response.Flush();
                //Response.End();
            }
            catch (Exception ex)
            {
            }

        }
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        //public static void MergePDFs(string targetPath, List<string> pdfs)
        //{
        //    using (PdfSharp.Pdf.PdfDocument targetDoc = new PdfSharp.Pdf.PdfDocument())
        //    {
        //        foreach (string pdf in pdfs)
        //        {
        //            using (PdfSharp.Pdf.PdfDocument pdfDoc = PdfSharp.Pdf.IO.PdfReader.Open(pdf, PdfDocumentOpenMode.Import))
        //            {
        //                for (int i = 0; i < pdfDoc.PageCount; i++)
        //                {
        //                    targetDoc.AddPage(pdfDoc.Pages[i]);
        //                }
        //            }
        //        }
        //        targetDoc.Save(targetPath);
        //    }



        //}
        private static void MergePDF(string File1, string File2)
        {
            File1 = @"E:/PDFDoc/ERS_Pick_List_211212-17.pdf";
            File2 = @"E:/PDFDoc/ERS_Pick_List_211218-87120.pdf";

            string[] fileArray = new string[3];
            fileArray[0] = File1;
            fileArray[1] = File2;

            iTextSharp.text.pdf.PdfReader reader = null;
            Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;
            string outputPdfPath = @"E:/PDFDoc/ERS_PICK_List.pdf";

            sourceDocument = new Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            //output file Open  
            sourceDocument.Open();


            //files list wise Loop  
            for (int f = 0; f < fileArray.Length - 1; f++)
            {
                int pages = TotalPageCount(fileArray[f]);

                reader = new iTextSharp.text.pdf.PdfReader(fileArray[f]);
                //Add pages in new file  
                for (int i = 1; i <= pages; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(importedPage);
                }

                reader.Close();
            }
            //save the output file  
            sourceDocument.Close();
        }

        private static int TotalPageCount(string file)
        {
            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());

                return matches.Count;
            }
        }
    }
}