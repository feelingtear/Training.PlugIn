using Kingdee.BOS.App;
using Kingdee.BOS.Core.BusinessFlow.PlugIn;
using Kingdee.BOS.Core.BusinessFlow.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.BusinessFlow
{
    [Description("采购订单反写到采购申请单")]
    public class PurWriteBackPlugIn : AbstractBusinessFlowServicePlugIn
    {
        private bool _thisIsMyRule = false;
        private const string MyRuleId = "9d1a8ba0-a533-4c04-9b19-11cf3c599ac7";

        public override void BeforeWriteBack(BeforeWriteBackEventArgs e)
        {
            // 采购订单反写到采购申请单
            if (e.Rule.Id.EqualsIgnoreCase(MyRuleId))
            {
                _thisIsMyRule = true;
            }
        }


        public override void BeforeCheckHighLimit(BeforeCheckHighLimitEventArgs e)
        {
            if (this._thisIsMyRule == true)
            {
                //采购管理系统参数
                ICommonService service = ServiceHelper.GetService<ICommonService>();
                DynamicObject reqData = e.SourceDataObject;
                DynamicObjectCollection dyEntry = reqData["ReqEntry"] as DynamicObjectCollection;
                long purOrgId = 0;
                if (dyEntry != null && dyEntry.Count > 0)
                {
                    purOrgId = Convert.ToInt64(dyEntry[0]["PurchaseOrgId_Id"]);
                }
                object allowPuroverReq = service.GetSystemProfile(this.Context, purOrgId, "PUR_SystemParameter", "AllowPurOverReq", false);
                if (Convert.ToBoolean(allowPuroverReq))
                {
                    e.Cancel = true;
                }
            }
        }
    }


}