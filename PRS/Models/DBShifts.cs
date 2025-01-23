using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Web.Script.Serialization;

namespace IP.Models
{
    public class DBShifts
    {
        cDAL oDAL;
        #region Fields

        public List<ArrayList> lstIngenShift1 { get; set; }

        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        #endregion

        #region Methods

        public bool GetShift1()
        {

            oDAL = new cDAL("INIT");
            string query = string.Empty;

            query = @"Execute usp.DashboardShift1";

            DataTable dt = oDAL.GetData(query);

            lstIngenShift1 = cCommon.ConvertDtToArrayListWithZero(dt);

            if (!oDAL.HasErrors)
                return true;
            else
                return false;


        }

        #endregion

    }
}