using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.Validators
{
    public class CheckAmountValidator : AbstractValidator
    {
        public override void InitializeConfiguration(ValidateContext validateContext, Context ctx)
        {
            base.InitializeConfiguration(validateContext, ctx);
            if (validateContext.BusinessInfo != null)
            {
                EntityKey = validateContext.BusinessInfo.GetEntity(0).Key;
            }
        }

        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            if (dataEntities == null || dataEntities.Length <= 0)
            {
                return;
            }

            DateTime dtNow = TimeServiceHelper.GetSystemDateTime(this.Context);
            DateTime startDate = new DateTime(dtNow.Year, dtNow.Month, 1);
            DateTime endDate = new DateTime(dtNow.Year, dtNow.Month + 1, 1).AddSeconds(-1);

            string strSql = @"SELECT SUM(FALLAMOUNT) FROM T_PUR_POORDERENTRY_F F 
                                INNER JOIN T_PUR_POORDER T ON F.FID = T.FID
                            WHERE T.FDATE >=@StartDate and t.FDATE <=@EndDate ";
            List<SqlParam> sqlParaList = new List<SqlParam>
            {
                new SqlParam("@StartDate",KDDbType.DateTime,startDate),
                new SqlParam("@EndDate",KDDbType.DateTime,endDate)
            };
            var allAmount = DBUtils.ExecuteScalar<decimal>(this.Context, strSql, 0, sqlParaList.ToArray());
          
            foreach (var et in dataEntities)
            {
                var dyEntrys = et.DataEntity["POOrderEntry"] as DynamicObjectCollection;
                if (dyEntrys != null)
                {

                    foreach (var dy in dyEntrys)
                    {
                        allAmount += Convert.ToDecimal(dy["AllAmount"]);
                        if (allAmount > 100000)
                        {
                            validateContext.AddError(et, new ValidationErrorInfo(
                                 "",
                                 Convert.ToString(et.DataEntity[0]),
                                 et.DataEntityIndex,
                                 et.RowIndex,
                                 "E1",
                                string.Format("本月的价税合计已经超过十万，禁止保存，当前价税合计为:{0}", allAmount),
                                  "价税合计超额检查"));
                        }
                    }
                }
            }

            //查询本月的价税合计数

            
        }            
    }
}
