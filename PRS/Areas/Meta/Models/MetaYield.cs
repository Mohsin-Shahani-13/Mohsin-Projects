using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.Meta.Models
{
    public class MetaYield
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }


        [Display(Name = "Station Name:")]
        public string Station { get; set; }

        [Display(Name = "Iteration:")]
        public string Iteration { get; set; }

        public string program_Id { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }


        public string filterString { get; set; }
        public string ReportTitle { get; set; }

        public List<Hashtable> lstMetaSO { get; set; }
        //public List<Hashtable> lstROUnitAccessory { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion
        public DataTable TestArea()
        {
            oDAL = new cDAL("INIT");
          

            string query = string.Empty;
            query = @"SELECT  DISTINCT TestArea 
		            FROM [meta].[rptDataWipeResult]
		            ORDER BY Testarea ";
          
            DataTable dt = oDAL.GetData(query);
           return dt;
        }

        public bool GetList(string programId, string TestArea, string frmDt, string toDt, string iteration)
        {
            int prgId = 0;
            if (TestArea =="All")
            {
                TestArea = "";
            }

            string sites = HttpContext.Current.Session["DefaultSite"].ToString();

            if (sites == "GRAPEVINE")
            {
                prgId = 10009;

            }

            else if (sites == "PRAGUE")
            {
                prgId = 10034;

            }

            else if (sites == "SYDNEY")
            {
                prgId = 10041;

            }

            else if (sites == "TOKYO")
            {
                prgId = 10042;

            }

            // oDAL = new cDAL("ACTIVE", "ST");
            cDAL oDAL = new cDAL("INIT");
            string query = string.Empty;
            if (iteration == "All")
            {


                query = @"

SELECT contract,
       testarea,
       iteration,
       total,
       totalpass,
        Cast(( ( Cast(totalpass AS DECIMAL(18, 2)) ) / Cast(total AS DECIMAL(18, 2)) * 100 ) AS DECIMAL(5, 2)) AS YieldCost
FROM   (SELECT contract,
               testarea,
               Cast(iteration AS VARCHAR) AS Iteration,
               Count(result)              AS Total,
               Count(CASE
                       WHEN result = 'PASS' THEN 1
                     END)                 AS TotalPass
        FROM   [meta].[rptDataWipeResult]
        WHERE  iteration > 0
               AND iteration <= 5 AND CONVERT(Date, Fordate) >= '<frmDt>' AND CONVERT(Date,Fordate) <= '<toDt>'";
                query += " AND contract = " + prgId + "";

                if (!string.IsNullOrEmpty(TestArea))
                {
                    query += " AND  testarea = '" + TestArea + "' ";

                }


                query += @" GROUP  BY contract,
                  testarea,
                  iteration
        UNION
        SELECT contract,
               testarea,
               'Above 5'     AS Iteration,
               Count(result) AS Total,
               Count(CASE
                       WHEN result = 'PASS' THEN 1
                     END)    AS TotalPass
        FROM   [meta].[rptDataWipeResult]
        WHERE  iteration > 5   AND CONVERT(Date, Fordate) >= '<frmDt>' AND CONVERT(Date,Fordate) <= '<toDt>'
         ";




                query += "AND contract = " + prgId + "";

                if (!string.IsNullOrEmpty(TestArea))
                {
                    query += " AND testarea = '"+ TestArea + "' ";

                }
        query += @" 
        GROUP  BY contract, testarea
         ) tmp
        ORDER  BY testarea,iteration
                  ";


               

            }

            else if (iteration == "1" || iteration == "2" || iteration == "3" || iteration == "4" || iteration == "5")
            {
                query = @"

SELECT contract,
       testarea,
       iteration,
       total,
       totalpass,
       Cast(( ( Cast(totalpass AS DECIMAL(18, 2)) ) / Cast(total AS DECIMAL(18, 2)) * 100 ) AS DECIMAL(5, 2)) AS YieldCost
FROM   (
        SELECT contract,
               testarea,
               Cast(iteration AS VARCHAR) AS Iteration,
               Count(result)              AS Total,
               Count(CASE
                       WHEN result = 'PASS' THEN 1
                     END)                 AS TotalPass
        FROM   [meta].[rptDataWipeResult]
        WHERE  CONVERT(Date, Fordate) >= '<frmDt>' AND CONVERT(Date,Fordate) <= '<toDt>'
              ";

                query += "AND contract = " + prgId + "";

                if (!string.IsNullOrEmpty(TestArea))
                {
                    query += " AND  testarea = '" + TestArea + "'";
                    query += " AND iteration = '" + iteration + "' ";

                }

                else if (string.IsNullOrEmpty(TestArea))
                {
                   
                        query += " AND iteration = '" + iteration + "' ";

                }



                query += @"
        GROUP  BY contract,
                  testarea,
                  iteration ) tmp ";

                query += "ORDER  BY testarea,iteration ";
            }

            else if (iteration == "Above 5")
            {
                query += @"select 
                                contract,
                                testarea,
                                iteration,
                                total,
                                totalpass,
                                Cast(( ( Cast(totalpass AS DECIMAL(18, 2)) ) / Cast(total AS DECIMAL(18, 2)) * 100 ) AS DECIMAL(5, 2)) AS YieldCost

        from (
                           select contract
                                , TestArea
                                , 'Above 5' as Iteration
                                , count(Result) as Total
                                , count(case when result = 'PASS' then 1 end) as TotalPass

                from [meta].[rptDataWipeResult]

                where Iteration > 5 AND CONVERT(Date, Fordate) >= '<frmDt>' AND CONVERT(Date,Fordate) <= '<toDt>'";

                query += "AND contract = " + prgId + "";

                if (!string.IsNullOrEmpty(TestArea))
                {
                    query += "AND testarea = '" + TestArea + "' ";
                    
                }


                query += @"
        GROUP  BY contract,
                  testarea
                   ) tmp ";

                query += "ORDER  BY testarea,iteration ";

           
            
            }

           
           


            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);

            if (!string.IsNullOrEmpty(TestArea))
            {
               
                filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";
                filterString += "| Station Name = '" + TestArea + "' ";
                filterString += " | Iteration = '" + iteration + "' ";
            }
            else if (string.IsNullOrEmpty(TestArea))
            {
                filterString += " > From = '" + frmDt + "' To = '" + toDt + "' ";
                filterString += " | Iteration = '" + iteration + "' ";
            }

            DataTable dt = oDAL.GetData(query);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("153", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstMetaSO = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }
    }
}