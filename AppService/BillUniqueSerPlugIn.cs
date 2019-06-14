using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.AppService
{
    [Description("单据编号唯一服务插件")]
    public class BillUniqueSerPlugIn : AbstractOperationServicePlugIn
    {
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            if (e.DataEntitys.Length <= 0) return;

            var headDataObj = e.DataEntitys[0];

            if (headDataObj == null) return;

            if (headDataObj["BillNo"] == null) return;
            var billNo = headDataObj["BillNo"].ToString();
            var id = int.Parse(headDataObj["Id"].ToString());

            string strSql = "SELECT 1 FROM T_PUR_POORDER WHERE FBILLNO=@BILLNO AND FID <>@ID";
            SqlParam[] sqlParams =
            {
                new SqlParam("@BILLNO",KDDbType.AnsiString,billNo),
                new SqlParam("@ID",KDDbType.Int32,id),
            };


            var count = DBUtils.ExecuteEnumerable(this.Context, strSql, System.Data.CommandType.Text, sqlParams).Count();


            if (count > 0)
            {
                throw new Exception("单据编号存在重复！");
            }
        }
    }
}
