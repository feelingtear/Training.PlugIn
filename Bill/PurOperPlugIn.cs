using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.Bill
{
    [Description("操作以后更新数据包测试")]
    public class PurOperPlugIn :AbstractBillPlugIn
    {

        public override void AfterDoOperation(AfterDoOperationEventArgs e)
        {
            base.AfterDoOperation(e);

            var date = this.Model.DataObject["Date"].ToString();
            this.View.UpdateView();
            this.View.ShowMessage(date);

            
        }
    }
}
