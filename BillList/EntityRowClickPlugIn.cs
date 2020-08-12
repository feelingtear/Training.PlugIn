using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.BillList
{
    [Description("列表测试单据体双击")]
    public class EntityRowClickPlugIn: AbstractListPlugIn
    {


        public override void FormatCellValue(FormatCellValueArgs args)
        {
            base.FormatCellValue(args);

            var ds = new DataSet();

            foreach(var row in ds.Tables[0].Rows)
            {
                var val = ((DataRow)row)["colName"].ToString();
            }


            ListShowParameter para = new ListShowParameter();
            para.CustomParams.Add("key", "value");
            this.View.ShowForm(para);
        }

        public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
        {
            base.EntityRowDoubleClick(e);
            this.View.ShowMessage("EntityRowDoubleClick");
        }

        public override void AfterShowForm(AfterShowFormEventArgs e)
        {
            base.AfterShowForm(e);

            var editor =this.View.GetControl<ComboFieldEditor>("key"); 
        }

        public override void ListRowDoubleClick(ListRowDoubleClickArgs e)
        {
            base.ListRowDoubleClick(e);
            this.View.ShowMessage("ListRowDoubleClick");
        }
    }
}
