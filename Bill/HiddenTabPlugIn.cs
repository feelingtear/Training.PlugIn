using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using System.ComponentModel;

namespace Witt.Cloud.PlugIn.Bill
{
    [Description("隐藏页签")]
    public class HiddenTabPlugIn : AbstractBillPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            var ctrl = this.View.GetControl("F_PAEZ_Tab_P0");
            if (ctrl != null) ctrl.Visible = false;

            this.View.UpdateView("F_PAEZ_Tab");

        }

        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            var num = this.Model.GetValue("FNumber");
            if (num.Equals("A"))
            {
                var ctrl = this.View.GetControl("F_PAEZ_Tab_P0");
                if (ctrl != null) ctrl.Visible = true;
            }
            else
            {
                var ctrl = this.View.GetControl("F_PAEZ_Tab_P0");
                if (ctrl != null) ctrl.Visible = false;
            }
            this.View.UpdateView("F_PAEZ_Tab");
        }
    }
}
