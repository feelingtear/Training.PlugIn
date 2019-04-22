using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.Validators
{

    /// <summary>
    /// 检查数量是否超额校验器
    /// </summary>
    public class CheckCountValidator : AbstractValidator
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

            foreach (var et in dataEntities)
            {
                var dyEntrys = et.DataEntity["POOrderEntry"] as DynamicObjectCollection;
                if (dyEntrys != null)
                {
                    var count = 0;
                    foreach (var dy in dyEntrys)
                    {
                        count += Convert.ToInt32(dy["Qty"]);
                    }
                    if (count > 100)
                    {
                        validateContext.AddError(et, new ValidationErrorInfo(
                      "",
                      Convert.ToString(et.DataEntity[0]),
                      et.DataEntityIndex,
                      et.RowIndex,
                      "E1",
                     string.Format("数量超额，总数量不能超过100个，当前物料总数量为:{0}", count),
                      "数量超额检查"));
                    }
                }
            }
        }
    }
}
