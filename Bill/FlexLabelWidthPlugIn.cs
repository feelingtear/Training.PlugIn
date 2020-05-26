using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System.ComponentModel;

namespace Witt.Cloud.PlugIn.Bill
{
    [Description("弹性域设置标题宽度")]
    public class FlexLabelWidthPlugIn : AbstractBillPlugIn
    {
        public override void OnQueryFlexFieldState(OnQueryFlexFieldStateEventArgs e)
        {
            base.OnQueryFlexFieldState(e);
            e.LabelWidth = 200;
        }
    }
}
