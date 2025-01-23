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
    public class KPNInventorySnapshot
    {
		cDAL oDAL = new cDAL("ACTIVE");
		#region Fields
		public string ReportTitle { get; set; }
		public System.Web.Script.Serialization.JavaScriptSerializer serializer { get; set; }
		public List<Hashtable> lstKPNInventorySnapshot { get; set; }

		public string filterString { get; set; }
		public string ErrorMessage { get; set; }
		#endregion
		#region Methods 
		
		public bool GetList()
		{
			oDAL = new cDAL("ACTIVE");
			string query = string.Empty;
			query = @"
SELECT 
ps.ProgramID,
    1 AS organization,
    psa.[Value] AS subinventory,
    ps.PartNo,
    pn.[Description],
    '' AS dummy,
    'WIP' AS status,
    COUNT(ps.PartNo) AS quantity
FROM 
    pls.partserial ps
JOIN 
    pls.PartSerialAttribute psa 
    ON psa.PartSerialID = ps.ID
JOIN 
    pls.PartNo pn 
    ON pn.PartNo = ps.PartNo
JOIN 
    pls.CodeStatus cs 
    ON cs.ID = ps.StatusID
JOIN 
    pls.CodeAttribute ca 
    ON ca.ID = psa.AttributeID 
    AND ca.AttributeName = 'SUB_INV'
WHERE 
    ps.ProgramID = '10045' 
    AND cs.[Description] NOT LIKE 'REID'
GROUP BY 
ps.ProgramID,
    ps.PartNo, 
    psa.[Value], 
    pn.[Description];
";

			DataTable dt = oDAL.GetData(query);

			filterString += " > Program = KPN Mobile ";

			//For SQL Documentation
			cLog oLog = new cLog();
			oLog.AddSqlQuery("254", query, string.Empty, false);

			if (oDAL.HasErrors)
			{
				ErrorMessage = oDAL.ErrMessage;
				return false;
			}
			else
			{
				if (dt.Rows.Count > 0)
					lstKPNInventorySnapshot = cCommon.ConvertDtToHashTable(dt);
				return true;

			}
		}

		#endregion
	}
}