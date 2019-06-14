using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Transactions;

namespace Witt.Cloud.PlugIn.AppService
{
    [Description("采购订单事务测试")]
    public class PurOrderServTest : AbstractOperationServicePlugIn
    {

        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
            string strSql = "INSERT INTO t_demo_purServTest(FID,FNAME,FCREATEDATE) VALUES(@ID,@NAME,@CREATEDATE)";
            List<SqlParam> paras = new List<SqlParam> {
                new SqlParam("@ID", KDDbType.Int32,DateTime.Now.Second),
                new SqlParam("@Name",KDDbType.String,"BeforeExecuteOperationTransaction"),
                new SqlParam("@CREATEDATE",KDDbType.DateTime,DateTime.Now)
            };

            DBUtils.Execute(this.Context, strSql, paras);
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            //using (KDTransactionScope scope = new KDTransactionScope(TransactionScopeOption.Required))
            //{
            string strSql = "INSERT INTO t_demo_purServTest(FID,FNAME,FCREATEDATE) VALUES(@ID,@NAME,@CREATEDATE)";
            List<SqlParam> paras = new List<SqlParam> {
                new SqlParam("@ID", KDDbType.Int32,DateTime.Now.Second),
                new SqlParam("@Name",KDDbType.String,"BeginOperationTransaction"),
                new SqlParam("@CREATEDATE",KDDbType.DateTime,DateTime.Now)
                };

            DBUtils.Execute(this.Context, strSql, paras);
            //    scope.Complete();
            //    throw new Exception("新建事务抛出异常");
            //}
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            string strSql = "INSERT INTO t_demo_purServTest(FID,FNAME,FCREATEDATE) VALUES(@ID,@NAME,@CREATEDATE)";
            List<SqlParam> paras = new List<SqlParam> {
                new SqlParam("@ID", KDDbType.Int32,DateTime.Now.Second),
                new SqlParam("@Name",KDDbType.String,"AfterExecuteOperationTransaction"),
                new SqlParam("@CREATEDATE",KDDbType.DateTime,DateTime.Now)
            };
            DBUtils.ExecuteStoreProcedure
            DBUtils.Execute(this.Context, strSql, paras);
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            string strSql = "INSERT INTO t_demo_purServTest(FID,FNAME,FCREATEDATE) VALUES(@ID,@NAME,@CREATEDATE)";
            List<SqlParam> paras = new List<SqlParam> {
                new SqlParam("@ID", KDDbType.Int32,DateTime.Now.Second),
                new SqlParam("@Name",KDDbType.String,"EndOperationTransaction"),
                new SqlParam("@CREATEDATE",KDDbType.DateTime,DateTime.Now)
            };

            DBUtils.Execute(this.Context, strSql, paras);

            strSql = "TRUNCATE table T_DEMO_PURDDLTest";
            DBUtils.Execute(this.Context, strSql);
            throw new Exception("测试服务插件抛出异常");
        }

        public override void RollbackData(OperationRollbackDataArgs e)
        {
            base.RollbackData(e);
        }
    }
}
