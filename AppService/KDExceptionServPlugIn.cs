using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.AppService
{
    /// <summary>
    /// 服务插件异常测试（测试使用）
    /// </summary>
    public class KDExceptionServPlugIn : AbstractOperationServicePlugIn
    {

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            throw new KDException("Code","测试抛出一个异常，后面的异常不应该执行");
        }


        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            //string strSql = "INSERT INTO t_demo_purServTest(FID,FNAME,FCREATEDATE) VALUES(@ID,@NAME,@CREATEDATE)";
            //List<SqlParam> paras = new List<SqlParam> {
            //    new SqlParam("@ID", KDDbType.Int32,DateTime.Now.Second),
            //    new SqlParam("@Name",KDDbType.String,"AfterExecuteOperationTransaction"),
            //    new SqlParam("@CREATEDATE",KDDbType.DateTime,DateTime.Now)
            //};
            //DBUtils.Execute(this.Context, strSql, paras);

            //throw new Exception("这个后面的异常不应该执行");
        }
    }
}
