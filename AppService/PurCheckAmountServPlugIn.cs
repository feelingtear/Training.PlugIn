﻿using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Witt.Cloud.PlugIn.Validators;

namespace Witt.Cloud.PlugIn.AppService
{
    [Description("价税合计超额检查")]
    public class PurCheckAmountServPlugIn : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            //添加价税合计
            e.FieldKeys.Add("FAllAmount");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            var ds = DBUtils.ExecuteDataSet(this.Context,"exec p_test 1");

            //DBUtils.ExecuteStoreProcedure()

            base.EndOperationTransaction(e);

            if (e.DataEntitys.Length > 0)
            {
                var dataObj = e.DataEntitys[0];

                dataObj["Date"] = new DateTime(2012, 1, 1);
            }

            this.OperationResult.OperateResult.Add(new Kingdee.BOS.Core.DynamicForm.OperateResult()
            {
                 MessageType = Kingdee.BOS.Core.DynamicForm.MessageType.FatalError
            });
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            var validator = new CheckAmountValidator();
            validator.EntityKey = "FPOOrderEntry";
            e.Validators.Add(validator);
        }
    }
}
