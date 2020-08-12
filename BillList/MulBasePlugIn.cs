using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.BillList
{
    [Description("多选基础资料更新到文本示例")]
    public class MulBasePlugIn : AbstractBillPlugIn
    {
        /// <summary>
        /// 多选基础资料的key
        /// </summary>
        private const string mulBaseKey = "F_PAEZ_MulBase";

        /// <summary>
        /// 文本字段key
        /// </summary>
        private const string textKey = "F_PAEZ_Text";
        public override void DataChanged(DataChangedEventArgs e)
        {
            if (e.Field.Key.EqualsIgnoreCase(mulBaseKey))
            {
                var mulDataObjs = this.Model.GetValue(mulBaseKey) as DynamicObjectCollection;
                if (mulDataObjs == null) return;

                StringBuilder sbMulTextVal = new StringBuilder();

                foreach (var obj in mulDataObjs)
                {
                    var baseObj = obj[mulBaseKey] as DynamicObject;
                    if (baseObj != null)
                    {
                        //获取基础资料名称，获取编码一般是Number，实际根据基础资料的编码或者名称的属性名确定
                        sbMulTextVal.AppendFormat("{0};", baseObj["Name"].ToString());
                    }
                }
                this.Model.SetValue(textKey, sbMulTextVal.ToString());
            }
        }

    }
}
