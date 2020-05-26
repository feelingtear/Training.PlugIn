using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.BillList
{
    [Description("列表测试单据体双击")]
    public class EntityRowClickPlugIn: AbstractListPlugIn
    {

        public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
        {
            base.EntityRowDoubleClick(e);
            this.View.ShowMessage("EntityRowDoubleClick");
        }

        public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
        {
            base.ListRowDoubleClick(e);
            this.View.ShowMessage("ListRowDoubleClick");
        }
    }
}
