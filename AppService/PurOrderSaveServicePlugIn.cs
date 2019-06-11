using Kingdee.BOS.App;
using Kingdee.BOS.App.Core.Validation;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Witt.Cloud.PlugIn.Validators;

namespace Witt.Cloud.PlugIn.AppService
{
    [HotUpdate]

    [Description("采购订单保存服务插件demo")]
    public class PurOrderSaveServicePlugIn : AbstractOperationServicePlugIn
    {
        
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FQty");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            if (e.DataEntitys.Length == 0) return;

            var dataObj = e.DataEntitys[0];
            var strDesc = string.Format("保存插件测试，单据编号为:{0},采购日期为：{1}的采购订单保存成功！"
                , dataObj["BillNo"], dataObj["Date"]);
            LogObject logObj = new LogObject()
            {
                pkValue = dataObj["Id"].ToString(),
                Description = strDesc,
                OperateName = this.FormOperation.OperationName,
                ObjectTypeId = this.BusinessInfo.GetForm().Id,
                SubSystemId = this.BusinessInfo.GetForm().SubsysId,
                Environment = OperatingEnvironment.BizOperate
            };

            ILogService logService = ServiceHelper.GetService<ILogService>();
            logService.WriteLog(this.Context, logObj);
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            var validator = new CheckCountValidator();
            validator.EntityKey = "FPOOrderEntry";
            e.Validators.Add(validator);
        }
    }
}
