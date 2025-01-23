using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace IP.Models
{
    public class UserProfile
    {
        cDAL oDAL;
        string empId = HttpContext.Current.Session["EmpId"].ToString();
        public string AddDetails(string EmpName, string image_path)
        {

            oDAL = new cDAL("INIT");
            string sql = "SELECT *FROM IP.EMPLOYEE WHERE EmpId = '" + empId + "'";
            DataTable dtEmpExist = oDAL.GetData(sql);

            if (dtEmpExist.Rows.Count > 0)
                sql = "UPDATE IP.EMPLOYEE SET EmpName = '" + EmpName + "'  WHERE EmpId = '" + empId + "'  ";

            oDAL.Execute(sql);
            if (oDAL.HasErrors == false)
                return "Success";
            else
                return "Fail";
        }

        public string AddPicture(HttpPostedFileBase file)
        {
            cDAL oDAL = new cDAL("INIT");
            string sql = "";
            if (file != null)
            {

                sql = "SELECT *FROM IP.EMPLOYEE WHERE EmpId = '" + empId + "'";
                DataTable dtEmpExist = oDAL.GetData(sql);
                byte[] varBinary_Image;

                try
                {
                    Stream fs = file.InputStream;
                    Image _img = Image.FromStream(fs);
                    Image new_Image = ResizeImage(_img, new Size(180, 180));
                    varBinary_Image = ImageToBinary(new_Image);
                    fs.Dispose();
                }
                catch (FileNotFoundException ex)
                {
                    cLog oLog = new cLog();
                    oLog.RecordError(ex.Message, ex.StackTrace, string.Empty);
                    return "Fail";
                }
                if (dtEmpExist.Rows.Count > 0)
                    sql = "UPDATE IP.EMPLOYEE SET ProfilePicture= '" + Convert.ToBase64String(varBinary_Image) + "'  WHERE EmpId = '" + empId + "'  ";

                oDAL.Execute(sql);
                if (oDAL.HasErrors == false)
                    return "Success";
                else
                    return "Fail";
            }
            else
                return "Fail";
        }
        //convert image to bytearray
        public byte[] ImageToBinary(Image img)
        {
            using (MemoryStream mStream = new MemoryStream())
            {
                img.Save(mStream, ImageFormat.Jpeg);
                return mStream.ToArray();
            }
        }

        public Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }
    }
}