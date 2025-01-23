using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class InvoiceRepairlevel
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        [Display(Name = "Program:")]
        public string program { get; set; }
        [Display(Name = "From:")]
        public string _fromDt = DateTime.Now.AddDays(-1).ToString(Format.DateOnly);
        public string fromDt { get { return _fromDt; } set { _fromDt = value; } }
        [Display(Name = "To:")]
        public string _toDt = DateTime.Now.ToString(Format.DateOnly);
        public string toDt { get { return _toDt; } set { _toDt = value; } }
        [Display(Name = "Serial No.:")]
        public string SerialNo { get; set; }
        public object total { get; set; }

        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string Repairlevel { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        public string isData { get; set; }
        public List<ArrayList> lstDataColumn { get; set; }
        public List<ArrayList> lstDtl { get; set; }
        public string Ratio1 { get; set; }
        public string Invoicefamily { get; set; }

        public List<Hashtable> lstInvoiceRepairlevelDetail { get; set; }
        public List<Hashtable> lstSerial { get; set; }
        //public List<ArrayList> lstInvoiceRepairlevelDetail { get; set; }
        #endregion
        #region Methods 
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
        public bool GetList(string frmDt, string toDt, string programId, string ProgramName)
        {
            string query = string.Empty;
            query = @"
-- STEP NO. 1 
DELETE TOP(100) PERCENT FROM PlusRS.rpt.RepairLevel

-- STEP NO. 2
INSERT INTO PlusRS.rpt.RepairLevel (WoHeaderId, ProgramID, PartNo, SerialNo, InvoiceFamily, RepairLevel)
SELECT  WOH.ID
      , WOH.ProgramID
	  , WOH.PartNo
	  , WOH.SerialNo
	  , PNA.Value AS InvoiceFamily
	  , CASE WOH.RepairTypeID WHEN 42 THEN '1' 
							  WHEN 71 THEN '3'
							  ELSE MAX(PLVL.Value)
	    END AS RepairLevel		
FROM pls.WOHeader WOH
INNER JOIN pls.WOStationHistory WSH ON WOH.ID = WSH.WOHeaderID
INNER JOIN pls.CodeRepairType CRT ON CRT.ID = WOH.RepairTypeID
INNER JOIN pls.WOLine WOL ON WOL.WOHeaderID = WOH.ID AND WOL.StatusID = 14
LEFT JOIN pls.PartNoAttribute PNA ON PNA.AttributeID = 354 AND PNA.ProgramID = WOH.ProgramID AND PNA.PartNo = WOH.PartNo 
LEFT JOIN pls.PartNoAttribute PLVL ON PLVL.AttributeID = 149 AND PNA.ProgramID = WOH.ProgramID AND PLVL.PartNo = WOL.ComponentPartNo
WHERE WSH.IsPass = 1 AND WSH.WorkStationID = 2  AND WOH.StatusID != 3
AND woh.ProgramID = '<programId>' and CONVERT(Date, WSH.lastactivitydate) >= '<frmDt>' AND CONVERT(Date, WSH.lastactivitydate) <= '<toDt>'
GROUP BY  WOH.ID
         , WOH.ProgramID
		 , WOH.PartNo
		 , WOH.SerialNo
		 , PNA.Value
		 , WOH.RepairTypeId

UPDATE  PlusRS.rpt.RepairLevel SET BCRatio = CASE WHEN RepairLevel = '0' THEN 9 
											      WHEN RepairLevel = '1' THEN 0
												  WHEN RepairLevel = '2' THEN 53
												  WHEN RepairLevel = '2.5' THEN 0
												  WHEN RepairLevel = '3' THEN 38
												  WHEN RepairLevel = 'O' THEN 0 
											  END

Declare @sqlquery nvarchar(max), 
@column nvarchar(max);
SELECT @column = COALESCE(@column + ',', '') + QUOTENAME(InvoiceFamily)
FROM
(
SELECT Distinct InvoiceFamily
FROM PlusRS.rpt.RepairLevel
) AS PIVOT_COLUMNS

SET @sqlquery =
N'select * from 
(
SELECT   SerialNo as Qty
	   , InvoiceFamily
	   , RepairLevel
       , BCRatio
FROM PlusRS.rpt.RepairLevel

)as t
pivot(
count(qty)
for InvoiceFamily In('+@column+')
)As Pvt_table
ORDER BY RepairLevel'
EXEC sp_executesql @sqlquery

";

            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            query = query.Replace("<programId>", programId);

            if (!string.IsNullOrEmpty(ProgramName))
                filterString += " Program = '" + ProgramName + "' ";

            filterString += " | From = '" + frmDt + "' To = '" + toDt + "' ";

            DataTable dt = oDAL.GetData(query);
            if (dt.Rows.Count == 0)
            {
                isData = "false";
                return true;
            }
            else
            {
                isData = "true";
            }

            dt.Columns.Add("Total", typeof(int));
            dt.Columns["BCRatio"].SetOrdinal(dt.Columns.Count - 1);

            foreach (DataRow row in dt.Rows)
            {
                int total = 0;
                for (int i = 1; i < dt.Columns.Count - 1; i++)
                {
                    int value1 = 0;
                    if (row[i] != DBNull.Value)
                        value1 = Convert.ToInt32(row[i]);

                    total += value1;
                }


                row["TOTAL"] = total;
            }

            //total = dt.Compute("SUM(Generic)", "");
            //dt.NewRow();
            string dtcolumn;
            int[] value = new int[dt.Columns.Count];
            for (int i = 1; i <= dt.Columns.Count - 1; i++)
            {
                dtcolumn = dt.Columns[i].ColumnName.ToString();
                // dt.Rows[dt.Rows.Count - 2][i] =Convert.ToInt32(dt.Compute("SUM( " + dtcolumn + " )", "1 > 0"));
                value[i] = Convert.ToInt32(dt.Compute("SUM(" + dtcolumn + ")", "1 > 0"));
                //value[i]= (int)dt.Compute("SUM(" + dtcolumn + ")", "");

            }


            DataColumn column;
            int vr1 = dt.Columns.Count;
            int vr2 = 0;
            int vl = 0;
            int vr3 = dt.Rows.Count;
            for (int i = 0; i < vr1 - 2; i++)
            {
                vr2 += 2;

                column = dt.Columns.Add("Ratio" + i + " %",typeof(double));
                column.SetOrdinal(vr2);
            }
            int _vr1 = dt.Columns.Count;
            int cond1 = 0;

            for (int i = 0; i <= vr3 - 1; i++)
            {
                cond1 = i + 1;
                int l = 1;
                for (int j = 1; j <= _vr1 - 1; j++)
                {

                    // object val = dt.Rows[i][j];
                    if (j % 2 == 0)
                    {

                        int value0 = (int)dt.Rows[i][j - 1];
                        int value1 = value[l];
                        double ratio0 = 0.00;
                        if (value1 == 0)
                        {
                            ratio0 = 0.00;
                        }
                        else
                        {
                            // ratio0 = Convert.ToDecimal((value0 * 100) / value1);
                            ratio0 = (value0   * 100f ) / value1;
                            ratio0 = Math.Round(ratio0, 2);
                        }

                        dt.Rows[i][j] = ratio0;
                        l++;
                    }

                }

            }


            //column.SetOrdinal(2);
            DataSet DS = new DataSet();


            DS.Tables.Add(dt);


            DataTable DSHeader = dt.Clone();
            DataTable dtColHeader = cCommon.GenerateTransposedTable(DSHeader);

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("114", query, "", false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else

            {
                lstDtl = cCommon.ConvertDtToArrayListForInvoiceRepair(DS.Tables[0]);



                lstDataColumn = cCommon.ConvertDtToArrayList(dtColHeader);


                return true;

            }

        }
        public bool GetDetail(string programId, string Repairlevel, string frmDt, string toDate, string Invoicefamily)
        {
            // oDAL = new cDAL("INIT");

            string query = string.Empty;
            query = @"
SELECT WoHeaderId,
       ProgramID,
	   PartNo, 
	   SerialNo
FROM PlusRS.rpt.RepairLevel
WHERE RepairLevel = '<RepLvl>' 
 ";
            if ((Invoicefamily != "Total"))
                query += @" AND InvoiceFamily = '<Invoicefamily>' ";
            query = query.Replace("<programId>", programId);
            query = query.Replace("<frmDt>", frmDt);
            query = query.Replace("<toDt>", toDt);
            query = query.Replace("<RepLvl>", Repairlevel);
            query = query.Replace("<Invoicefamily>", Invoicefamily);
            DataTable dt = oDAL.GetData(query);



            if (!oDAL.HasErrors)
            {
                if (dt.Rows.Count > 0)
                {
                    lstSerial = cCommon.ConvertDtToHashTable(dt);
                }
                return true;
            }
            return false;
        }

        #endregion
    }
}