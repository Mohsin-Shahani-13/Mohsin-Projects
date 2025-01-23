using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace IP.Areas.SupplyChain.Models
{
    public class ShippingError
    {
        cDAL oDAL = new cDAL("ACTIVE");
        #region Fields
        public string filterString { get; set; }
        public string ReportTitle { get; set; }
        public List<Hashtable> lstShippingError { get; set; }
        public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
        public string ErrorMessage { get; set; }
        #endregion

        public bool GetList()
        {
            string sites = HttpContext.Current.Session["DefaultSite"].ToString();
            string query = string.Empty;

            query = @"

-- Step 1: Create the temporary table
CREATE TABLE #tempTable (
    ProgramID INT,
    PartNo VARCHAR(255),
	 Status VARCHAR(255),
    BATTERY_AGE_RULE VARCHAR(255),
    CARRIER_COUNTRYOFMANUFACTURE VARCHAR(255),
    CARRIER_CURRENCY VARCHAR(255),
    CARRIER_ECCN VARCHAR(255),
    CARRIER_PRICE VARCHAR(255),
    CARRIER_USTHS VARCHAR(255),
    CARRIER_WEIGHT VARCHAR(255),
    CODE_NAME VARCHAR(255),
    LABEL_SIZE VARCHAR(255),
    ReIDPartNumber VARCHAR(255),
    SKUD_1 VARCHAR(255),
    SKUD_2 VARCHAR(255),
    SKUD_3 VARCHAR(255),
    STRIKE_RULE VARCHAR(255),
    WeightUnit VARCHAR(255)
);

-- Step 2: Insert data from Query 1
INSERT INTO #tempTable
SELECT sh.ProgramID, sl.PartNo, sh.StatusDescription ,
     MAX(CASE WHEN ca.AttributeName = 'BATTERY_AGE_RULE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS BATTERY_AGE_RULE,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_COUNTRYOFMANUFACTURE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_COUNTRYOFMANUFACTURE,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_CURRENCY' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_CURRENCY,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_ECCN' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_ECCN,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_PRICE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_PRICE,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_USTHS' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_USTHS,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_WEIGHT' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_WEIGHT,
    MAX(CASE WHEN ca.AttributeName = 'CODE_NAME' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CODE_NAME,
    MAX(CASE WHEN ca.AttributeName = 'LABEL_SIZE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS LABEL_SIZE,
    MAX(CASE WHEN ca.AttributeName = 'ReIDPartNumber' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS ReIDPartNumber,
    MAX(CASE WHEN ca.AttributeName = 'SKUD_1' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS SKUD_1,
    MAX(CASE WHEN ca.AttributeName = 'SKUD_2' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS SKUD_2,
   MAX(CASE WHEN ca.AttributeName = 'SKUD_3' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL 
                       ELSE pna.[Value] 
                  END 
        END) AS SKUD_3,
     MAX(CASE WHEN ca.AttributeName = 'STRIKE_RULE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS STRIKE_RULE,
    MAX(CASE WHEN ca.AttributeName = 'WeightUnit' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS WeightUnit
FROM pls.vSOHeader sh
JOIN pls.vSOLine sl ON sl.SOHeaderID = sh.id
JOIN pls.vSOHeaderAttribute sha ON sha.SOHeaderID = sh.id
LEFT JOIN pls.CodeAttribute ca ON ca.AttributeName IN (
    'BATTERY_AGE_RULE',
    'CARRIER_COUNTRYOFMANUFACTURE',
    'CARRIER_CURRENCY',
    'CARRIER_ECCN',
    'CARRIER_PRICE',
    'CARRIER_USTHS',
    'CARRIER_WEIGHT',
    'CODE_NAME',
    'LABEL_SIZE',
    'ReIDPartNumber',
    'SKUD_1',
    'SKUD_2',
    'SKUD_3',
    'STRIKE_RULE',
    'WeightUnit'
)
LEFT JOIN pls.PartNoAttribute pna ON sh.ProgramID = pna.ProgramID 
    AND pna.PartNo = sl.PartNo 
    AND pna.AttributeID = ca.ID
  WHERE sha.AttributeName = 'PROCESS_TYPE'
  AND sha.Value = 'repair'
AND sh.StatusDescription IN ('RESERVED')

 AND sh.ProgramID = '10058' GROUP BY sh.ProgramID, sl.PartNo, sh.StatusDescription 
 HAVING MAX(CASE WHEN ca.AttributeName = 'BATTERY_AGE_RULE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_COUNTRYOFMANUFACTURE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_CURRENCY' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_ECCN' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_PRICE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_USTHS' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_WEIGHT' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CODE_NAME' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'LABEL_SIZE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'ReIDPartNumber' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'SKUD_1' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'SKUD_2' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'SKUD_3' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'STRIKE_RULE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'WeightUnit' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL ORDER BY sl.PartNo 

-- Step 3: Insert data from Query 2 (modify as needed)
INSERT INTO #tempTable



SELECT rh.ProgramID, rl.PartNo,  rh.Status,
     MAX(CASE WHEN ca.AttributeName = 'BATTERY_AGE_RULE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS BATTERY_AGE_RULE,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_COUNTRYOFMANUFACTURE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_COUNTRYOFMANUFACTURE,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_CURRENCY' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_CURRENCY,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_ECCN' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_ECCN,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_PRICE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_PRICE,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_USTHS' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_USTHS,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_WEIGHT' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_WEIGHT,
    MAX(CASE WHEN ca.AttributeName = 'CODE_NAME' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CODE_NAME,
    MAX(CASE WHEN ca.AttributeName = 'LABEL_SIZE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS LABEL_SIZE,
    MAX(CASE WHEN ca.AttributeName = 'ReIDPartNumber' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS ReIDPartNumber,
    MAX(CASE WHEN ca.AttributeName = 'SKUD_1' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS SKUD_1,
    MAX(CASE WHEN ca.AttributeName = 'SKUD_2' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS SKUD_2,
   MAX(CASE WHEN ca.AttributeName = 'SKUD_3' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL 
                       ELSE pna.[Value] 
                  END 
        END) AS SKUD_3,
     MAX(CASE WHEN ca.AttributeName = 'STRIKE_RULE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS STRIKE_RULE,
    MAX(CASE WHEN ca.AttributeName = 'WeightUnit' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS WeightUnit
FROM pls.vROHeader rh
JOIN pls.vROLine rl ON rl.ROHeaderID = rh.id
JOIN pls.vROHeaderAttribute rha ON rha.ROHeaderID = rh.id
LEFT JOIN pls.CodeAttribute ca ON ca.AttributeName IN (
    'BATTERY_AGE_RULE',
    'CARRIER_COUNTRYOFMANUFACTURE',
    'CARRIER_CURRENCY',
    'CARRIER_ECCN',
    'CARRIER_PRICE',
    'CARRIER_USTHS',
    'CARRIER_WEIGHT',
    'CODE_NAME',
    'LABEL_SIZE',
    'ReIDPartNumber',
    'SKUD_1',
    'SKUD_2',
    'SKUD_3',
    'STRIKE_RULE',
    'WeightUnit'
)
LEFT JOIN pls.PartNoAttribute pna ON rh.ProgramID = pna.ProgramID 
    AND pna.PartNo = rl.PartNo 
    AND pna.AttributeID = ca.ID
  WHERE rha.AttributeName = 'PROCESS_TYPE'
  AND rha.Value = 'repair'
AND rh.Status IN ('RECEIVED')

 AND rh.ProgramID = '10058' GROUP BY rh.ProgramID, rl.PartNo, rh.Status
 HAVING MAX(CASE WHEN ca.AttributeName = 'BATTERY_AGE_RULE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_COUNTRYOFMANUFACTURE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_CURRENCY' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_ECCN' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_PRICE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_USTHS' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_WEIGHT' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CODE_NAME' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'LABEL_SIZE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'ReIDPartNumber' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'SKUD_1' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'SKUD_2' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'SKUD_3' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'STRIKE_RULE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'WeightUnit' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL 
	ORDER BY rl.PartNo 

	INSERT INTO #tempTable
	

SELECT wh.ProgramID, wh.PartNo, wh.StatusDescription,
     MAX(CASE WHEN ca.AttributeName = 'BATTERY_AGE_RULE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS BATTERY_AGE_RULE,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_COUNTRYOFMANUFACTURE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_COUNTRYOFMANUFACTURE,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_CURRENCY' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_CURRENCY,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_ECCN' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_ECCN,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_PRICE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_PRICE,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_USTHS' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_USTHS,
    MAX(CASE WHEN ca.AttributeName = 'CARRIER_WEIGHT' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CARRIER_WEIGHT,
    MAX(CASE WHEN ca.AttributeName = 'CODE_NAME' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS CODE_NAME,
    MAX(CASE WHEN ca.AttributeName = 'LABEL_SIZE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS LABEL_SIZE,
    MAX(CASE WHEN ca.AttributeName = 'ReIDPartNumber' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS ReIDPartNumber,
    MAX(CASE WHEN ca.AttributeName = 'SKUD_1' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS SKUD_1,
    MAX(CASE WHEN ca.AttributeName = 'SKUD_2' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS SKUD_2,
   MAX(CASE WHEN ca.AttributeName = 'SKUD_3' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL 
                       ELSE pna.[Value] 
                  END 
        END) AS SKUD_3,
     MAX(CASE WHEN ca.AttributeName = 'STRIKE_RULE' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS STRIKE_RULE,
    MAX(CASE WHEN ca.AttributeName = 'WeightUnit' 
             THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') 
                       THEN NULL ELSE pna.[Value] END 
        END) AS WeightUnit
FROM pls.vWOHeader wh
--JOIN pls.vWOLine sl ON sl.WOHeaderID = sh.id
JOIN pls.vWOHeaderAttribute wha ON wha.WOHeaderID = wh.id
LEFT JOIN pls.CodeAttribute ca ON ca.AttributeName IN (
    'BATTERY_AGE_RULE',
    'CARRIER_COUNTRYOFMANUFACTURE',
    'CARRIER_CURRENCY',
    'CARRIER_ECCN',
    'CARRIER_PRICE',
    'CARRIER_USTHS',
    'CARRIER_WEIGHT',
    'CODE_NAME',
    'LABEL_SIZE',
    'ReIDPartNumber',
    'SKUD_1',
    'SKUD_2',
    'SKUD_3',
    'STRIKE_RULE',
    'WeightUnit'
)
LEFT JOIN pls.PartNoAttribute pna ON wh.ProgramID = pna.ProgramID 
    AND pna.PartNo = wh.PartNo 
    AND pna.AttributeID = ca.ID
  --WHERE sha.AttributeName = 'PROCESS_TYPE'
  --AND sha.Value = 'repair'


 WHERE wh.ProgramID = '10058' 
 AND wh.StatusDescription IN ('WIP')
 GROUP BY wh.ProgramID, wh.PartNo,wh.StatusDescription
 HAVING MAX(CASE WHEN ca.AttributeName = 'BATTERY_AGE_RULE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_COUNTRYOFMANUFACTURE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_CURRENCY' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_ECCN' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_PRICE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_USTHS' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CARRIER_WEIGHT' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'CODE_NAME' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'LABEL_SIZE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'ReIDPartNumber' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'SKUD_1' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'SKUD_2' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'SKUD_3' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'STRIKE_RULE' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
    OR MAX(CASE WHEN ca.AttributeName = 'WeightUnit' THEN CASE WHEN pna.[Value] IN ('NA', 'N/A', '#N/A') THEN NULL ELSE pna.[Value] END END) IS NULL
	ORDER BY wh.PartNo 


	SELECT * FROM #tempTable;
	drop table #tempTable;



 ";

            DataTable dt = oDAL.GetData(query);

            filterString += "> Program = Bose";

            //For SQL Documentation
            cLog oLog = new cLog();
            oLog.AddSqlQuery("218", query, string.Empty, false);

            if (oDAL.HasErrors)
            {
                ErrorMessage = oDAL.ErrMessage;
                return false;
            }
            else
            {
                if (dt.Rows.Count > 0)
                    lstShippingError = cCommon.ConvertDtToHashTable(dt);
                return true;
            }
        }
    }
}