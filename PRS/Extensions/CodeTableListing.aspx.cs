using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IP.Extensions
{
    public partial class CodeTableListing : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LoadTables();
        }

        private void LoadTables()
        {
            cDAL cDAL = new cDAL("ACTIVE");
            string sql = "SELECT TABLE_NAME AS CodeTables FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'Code%' ORDER BY TABLE_NAME";
            DataTable dt = cDAL.GetData(sql);

            gvList.DataSource = dt;
            gvList.DataBind();
        }

        protected void gvList_SelectedIndexChanged(object sender, EventArgs e)
        {
            cDAL oDAL = new cDAL("ACTIVE");
            string sql = "SELECT * FROM pls.<table_name>";
            sql = sql.Replace("<table_name>", gvList.SelectedRow.Cells[1].Text);
            DataTable dt = oDAL.GetData(sql);

            gvData.DataSource = dt;
            gvData.DataBind();
        }
    }
}