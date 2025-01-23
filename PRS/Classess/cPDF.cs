using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
namespace IP.Classess
{
    public class cPDF : PdfPageEventHelper
    {
        private PdfTemplate total;
        private string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
            set { _imagePath = value; }
        }

        private string _hdr;
        public string DocHdr
        {
            get { return _hdr; }
            set { _hdr = value; }
        }

        public override void OnOpenDocument(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            //MyBase.OnOpenDocument(writer, document)

            PdfPTable tbl = new PdfPTable(1);

            tbl.DefaultCell.FixedHeight = 20;
            tbl.TotalWidth = 522;
            tbl.DefaultCell.Border = 0;

            PdfPCell cell = new PdfPCell();

            cell.AddElement(new Chunk(_hdr, new Font(Font.FontFamily.UNDEFINED, 16)));
            cell.Border = Rectangle.BOTTOM_BORDER;
            cell.HorizontalAlignment = Rectangle.ALIGN_CENTER;
            cell.PaddingBottom = 10;

            tbl.AddCell(cell);
            tbl.WriteSelectedRows(0, -1, 34, 803, writer.DirectContent);

            total = writer.DirectContent.CreateTemplate(30, 10);

        }

        public override void OnStartPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            //MyBase.OnStartPage(writer, document)
        }

        public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            //MyBase.OnEndPage(writer, document)
            Rectangle rect = writer.GetBoxSize("art");

            //Add NEOTech logo
            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(_imagePath + "OctaneWeb.png");
            img.ScalePercent(25);
            img.SetAbsolutePosition(rect.Right - img.ScaledWidth, rect.Top);
            writer.DirectContent.AddImage(img);

            //Add footer
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_CENTER, new Phrase("Page " + writer.CurrentPageNumber), (rect.Left + rect.Right) / 2, rect.Bottom - 18, 0);

            //Add template
            iTextSharp.text.Image imgTotal = iTextSharp.text.Image.GetInstance(total);
            imgTotal.SetAbsolutePosition(((rect.Left + rect.Right) / 1.85f), (rect.Bottom - 18f));
            writer.DirectContent.AddImage(imgTotal);

        }

        public override void OnCloseDocument(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            //MyBase.OnCloseDocument(writer, document)
            ColumnText.ShowTextAligned(total, Element.ALIGN_LEFT, new Phrase("of " + (writer.CurrentPageNumber - 1)), 2, 0, 0);
        }
    }
}