using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace IP.Areas.SupplyChain.Models
{
    public class ReceiptList
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string ReportTitle { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string filterString { get; set; }
        public string ErrorMessage { get; set; }
        [Display(Name = "Pallet Box No.:")]
        public string PalletBoxNo { get; set; }
        public List<Hashtable> lstReceiptList { get; set; }

        public List<object> lstMst = new List<object>();
        #endregion

        #region Methods 
        public bool GetList(string PalletBoxNo, string RptType)
        {
            // oDAL = new cDAL("ACTIVE", "ST");
            string query = string.Empty;

            if (RptType == "Summary")
            {
                query = @"
                    
                        SELECT  pallet,
                                box_no,
                                description,
                                rma      AS 'RMA',
                                model    AS 'Model',
                                Count(1) Qty
                FROM   (SELECT (SELECT MAX(pl.LocationNo)
                FROM   pls.partqty pq
				inner join pls.PartLocation pl on pl.ID = pq.LocationID
                WHERE  pq.programid = ps.programid
                       AND palletboxno = ps.palletboxno
                       AND availableqty > 0)       AS 'Pallet',
               'Toshiba Drives'                    AS Description,
                (SELECT b.CustomPalletBoxNo) AS 'box_no',
               (SELECT Max(customerreference)
                FROM   [pls].[roheader]
                WHERE  id = ps.roheaderid)         AS 'RMA',
               (SELECT Max(modelno)
                FROM   [pls].[partno]
                WHERE  partno = ps.partno)         AS 'Model',
               ps.serialno                         AS 'TSB_Serial_No',
               (SELECT TOP 1 WOSA.[value]
                FROM   pls.woheader WOH
                       INNER JOIN pls.wostationhistory WOSH
                               ON WOH.id = WOSH.woheaderid
                       INNER JOIN pls.wostationattribute WOSA
                               ON WOSH.id = WOSA.wostationhistoryid
                       INNER JOIN pls.codeattribute CA
                               ON CA.id = WOSA.attributeid
                WHERE  WOH.programid = ps.programid
                       AND CA.attributename = 'ACTION'
                       AND WOSH.woheaderid = ps.woheaderid
                ORDER  BY WOSH.id DESC)            AS 'WO_ACTION',
               (SELECT Max(roua.value)
                FROM   [pls].[rounit] rou
                       INNER JOIN [pls].[codeattribute] ca
                               ON ca.attributename = 'VMI_ACTION'
                       INNER JOIN [pls].[rounitattribute] roua
                               ON roua.rounitid = rou.id
                                  AND roua.attributeid = ca.id
                WHERE  rou.serialno = ps.serialno) AS 'VMI_ACTION',
               (SELECT Max(roua.value)
                FROM   [pls].[rounit] rou
                       INNER JOIN [pls].[codeattribute] ca
                               ON ca.attributename = 'ODM'
                       INNER JOIN [pls].[rounitattribute] roua
                               ON roua.rounitid = rou.id
                                  AND roua.attributeid = ca.id
                WHERE  rou.serialno = ps.serialno) AS 'ODM'
        FROM   [pls].[partpalletboxno] b
               INNER JOIN [pls].[partserial] ps
                       ON ps.programid = b.programid
                          AND ps.palletboxno = b.custompalletboxno
        WHERE  b.programid = 10028
               AND b.custompalletboxno = '<PalletBoxNo>'
               ) AS TAB_1

                ";

                query = query.Replace("<PalletBoxNo>", PalletBoxNo);
                //if (!string.IsNullOrEmpty(PalletBoxNo))
                //    query += "AND SOH.CustomerReference LIKE '%" + PalletBoxNo + "%'";

                query += @"WHERE  Ltrim(Rtrim(Concat(Iif(Isnull(wo_action, vmi_action) = 'CID', 'CID',
                          'FALSE'),
                                      Iif(Isnull(wo_action, vmi_action) = 'NTF',
                                      'NTF',
                                      'FALSE')))) <> 'FALSEFALSE'
                        AND odm NOT IN( 'ZIM', 'NTF' )

                        GROUP BY 
                                  pallet,
                                  box_no,
                                  description,
                                  rma,
                                  model";
            }

            else if (RptType == "Detail")
            {
                query = @"SELECT box_no,
       seq_no,
       rma_no,
       model_no,
       serial_no,
       coo,
       Iif(Isnull(chinese, englishremark) IN ( 'PASS', '0000' ), 'OK',
       Isnull(chinese, englishremark)) 'Remark'
FROM   (SELECT box_no,
               Row_number()
                 OVER(
                   ORDER BY lastactivitydate ASC) AS 'Seq_No',
               rma                                AS 'RMA_No',
               model                              AS 'Model_No',
               serial_no,
               coo,
               CASE
                 WHEN ( fd_code = 'X2111' ) THEN Isnull(ndf_result, vmi_result)
                 WHEN ( fd_code = 'X2211' ) THEN
                 Isnull(dnr_result, Isnull(tmdt_result,
                                    vmi_result))
                 WHEN ( fd_code IN ( 'X2511', 'X2411' ) ) THEN
                 Isnull(tmdt_result, vmi_result)
                 ELSE vmi_result
               END                                AS 'EnglishRemark'
        FROM   (SELECT b.custompalletboxno                 AS 'Box_No',
                       (SELECT Max(customerreference)
                        FROM   [pls].[roheader]
                        WHERE  id = ps.roheaderid)         AS 'RMA',
                       (SELECT Max(modelno)
                        FROM   [pls].[partno]
                        WHERE  partno = ps.partno)         AS 'Model',
                       ps.serialno                         AS 'Serial_No',
                       (SELECT Max(qtyreceived)
                        FROM   [pls].[roline]
                        WHERE  roheaderid = ps.roheaderid) AS Qty,
                       (SELECT TOP 1 WOSA.[value]
                        FROM   pls.woheader WOH
                               INNER JOIN pls.wostationhistory WOSH
                                       ON WOH.id = WOSH.woheaderid
                               INNER JOIN pls.wostationattribute WOSA
                                       ON WOSH.id = WOSA.wostationhistoryid
                               INNER JOIN pls.codeattribute CA
                                       ON CA.id = WOSA.attributeid
                        WHERE  WOH.programid = ps.programid
                               AND CA.attributename = 'ACTION'
                               AND WOSH.woheaderid = ps.woheaderid
                        ORDER  BY WOSH.id DESC)            AS 'WO_ACTION',
                       (SELECT Max(roua.value)
                        FROM   [pls].[rounit] rou
                               INNER JOIN [pls].[codeattribute] ca
                                       ON ca.attributename = 'VMI_ACTION'
                               INNER JOIN [pls].[rounitattribute] roua
                                       ON roua.rounitid = rou.id
                                          AND roua.attributeid = ca.id
                        WHERE  rou.serialno = ps.serialno) AS 'VMI_ACTION',
                       (SELECT Max(roua.value)
                        FROM   [pls].[rounit] rou
                               INNER JOIN [pls].[codeattribute] ca
                                       ON ca.attributename = 'ODM'
                               INNER JOIN [pls].[rounitattribute] roua
                                       ON roua.rounitid = rou.id
                                          AND roua.attributeid = ca.id
                        WHERE  rou.serialno = ps.serialno) AS 'ODM',
                       (SELECT Max(roua.value)
                        FROM   [pls].[rounit] rou
                               INNER JOIN [pls].[codeattribute] ca
                                       ON ca.attributename = 'COO'
                               INNER JOIN [pls].[rounitattribute] roua
                                       ON roua.rounitid = rou.id
                                          AND roua.attributeid = ca.id
                        WHERE  rou.serialno = ps.serialno) AS 'COO',
                       (SELECT Max(roua.value)
                        FROM   [pls].[rounit] rou
                               INNER JOIN [pls].[codeattribute] ca
                                       ON ca.attributename = 'FD_CODE'
                               INNER JOIN [pls].[rounitattribute] roua
                                       ON roua.rounitid = rou.id
                                          AND roua.attributeid = ca.id
                        WHERE  rou.serialno = ps.serialno) AS 'FD_CODE',
                       (SELECT Max(roua.value)
                        FROM   [pls].[rounit] rou
                               INNER JOIN [pls].[codeattribute] ca
                                       ON ca.attributename = 'VMI_RESULT'
                               INNER JOIN [pls].[rounitattribute] roua
                                       ON roua.rounitid = rou.id
                                          AND roua.attributeid = ca.id
                        WHERE  rou.serialno = ps.serialno) AS 'VMI_RESULT',
                       (SELECT TOP 1 WOSA.[value]
                        FROM   pls.woheader WOH
                               INNER JOIN pls.wostationhistory WOSH
                                       ON WOH.id = WOSH.woheaderid
                               INNER JOIN pls.wostationattribute WOSA
                                       ON WOSH.id = WOSA.wostationhistoryid
                               INNER JOIN pls.codeattribute CA
                                       ON CA.id = WOSA.attributeid
                        WHERE  WOH.programid = ps.programid
                               AND CA.attributename = 'NDF_RESULT'
                               AND WOSH.woheaderid = ps.woheaderid
                        ORDER  BY WOSH.id DESC)            AS 'NDF_RESULT',
                       (SELECT TOP 1 WOSA.[value]
                        FROM   pls.woheader WOH
                               INNER JOIN pls.wostationhistory WOSH
                                       ON WOH.id = WOSH.woheaderid
                               INNER JOIN pls.wostationattribute WOSA
                                       ON WOSH.id = WOSA.wostationhistoryid
                               INNER JOIN pls.codeattribute CA
                                       ON CA.id = WOSA.attributeid
                        WHERE  WOH.programid = ps.programid
                               AND CA.attributename = 'DNR_RESULT'
                               AND WOSH.woheaderid = ps.woheaderid
                        ORDER  BY WOSH.id DESC)            AS 'DNR_RESULT',
                       (SELECT TOP 1 WOSA.[value]
                        FROM   pls.woheader WOH
                               INNER JOIN pls.wostationhistory WOSH
                                       ON WOH.id = WOSH.woheaderid
                               INNER JOIN pls.wostationattribute WOSA
                                       ON WOSH.id = WOSA.wostationhistoryid
                               INNER JOIN pls.codeattribute CA
                                       ON CA.id = WOSA.attributeid
                        WHERE  WOH.programid = ps.programid
                               AND CA.attributename = 'TMDT_RESULT'
                               AND WOSH.woheaderid = ps.woheaderid
                        ORDER  BY WOSH.id DESC)            AS 'TMDT_RESULT',
                       ps.lastactivitydate
                FROM   [pls].[partpalletboxno] b
                       INNER JOIN [pls].[partserial] ps
                               ON ps.programid = b.programid
                                  AND ps.palletboxno = b.custompalletboxno
                WHERE  b.programid = 10028
                      AND b.custompalletboxno = '<PalletBoxNo>' 
					  ) AS TAB_1";

                query = query.Replace("<PalletBoxNo>", PalletBoxNo);

                query += @" WHERE  Ltrim(Rtrim(Concat(Iif(Isnull(wo_action, vmi_action) = 'CID',
                                  'CID',
                                  'FALSE'),
                                               Iif(Isnull(wo_action, vmi_action)
                                                   =
                                                   'NTF', 'NTF'
                                               ,
                                               'FALSE')))) <> 'FALSEFALSE'
               AND odm NOT IN ( 'ZIM', 'NTF' )) Details
               LEFT JOIN (SELECT cil.programid,
                         cil.[item],
                         Isnull(sa.chinese, Isnull(dt.[chinese], cil.[item]))
                         Chinese
                  FROM   [pls].[codeitemlist] cil
                         LEFT JOIN [pls].[datatranslation] dt
                                ON cil.programid = dt.programid
                                   AND cil.[name] + ' - ' + cil.item =
                                       dt.english
                         LEFT JOIN (SELECT english,
                                           chinese
                                    FROM   [pls].[datatranslation]
                                    WHERE  programid = programid
                                           AND standaloneentry = 1) sa
                                ON cil.[item] = sa.english
                  WHERE  cil.programid = 10028
                         AND cil.[name] IN ( 'DNR_TEST_CODE', 'NDF_ERROR_CODE',
                                             'VMI_RESULT' )
                         AND dt.statusid = 4) standalone
              ON standalone.programid = programid
                 AND standalone.item = englishremark";
            }

            DataTable dt = oDAL.GetData(query);

            filterString += "> Report type = '" + RptType + "' ";

            filterString += "| Pallet Box No. = '" + PalletBoxNo + "' ";


            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("148", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstReceiptList = cCommon.ConvertDtToHashTable(dt);
                return true;

            }
        }

        #endregion
    }
}