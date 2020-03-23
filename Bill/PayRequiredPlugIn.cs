using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Witt.Cloud.PlugIn.Bill
{
    [Description("采购申请单类型测试")]
    public class PayRequiredPlugIn : AbstractBillPlugIn
    {

        private readonly string changedFieldKey = "F_PAEZ_Text";

        private const string contActUnitType = "FCONTACTUNITTYPE";

        private const string StaffIdentity = "1";

        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
        }
        
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (!e.Field.Key.Equals(changedFieldKey, StringComparison.OrdinalIgnoreCase)) return;

            var textVal = this.Model.GetValue(changedFieldKey).ToString();
            this.SetDataSourceType(textVal);
        }

        private void SetDataSourceType(string textValue)
        {
            //这里测试1，仅限选择员工

            var uniType = this.View.GetControl<ComboFieldEditor>(contActUnitType);
            if (uniType == null) return;

            if (textValue == StaffIdentity)
            {

                var supplierItems = GetAllType().Where(f => f.EnumId == "BD_Supplier").ToList();
                uniType.SetComboItems(supplierItems);
            }
            else
            {
                uniType.SetComboItems(GetAllType());
            }
            this.View.UpdateView(contActUnitType);
            this.View.SendAynDynamicFormAction(this.View);
        }


        private List<EnumItem> GetAllType()
        {
            List<EnumItem> items = new List<EnumItem>()
            {
                new EnumItem()
                {
                    EnumId="BD_Supplier",Caption=new LocaleValue("供应商"),Value="BD_Supplier"
                },
                new EnumItem()
                {
                    EnumId="BD_Customer",Caption=new LocaleValue("客户"),Value="BD_Customer"
                },
                new EnumItem()
                {
                    EnumId="BD_Department",Caption=new LocaleValue("部门"),Value="BD_Department"
                },
                new EnumItem()
                {
                    EnumId="BD_Empinfo",Caption=new LocaleValue("员工"),Value="BD_Empinfo"
                },
                new EnumItem()
                {
                    EnumId="FIN_OTHERS",Caption=new LocaleValue("其他往来单位"),Value="FIN_OTHERS"
                },
                new EnumItem()
                {
                    EnumId="BD_BANK",Caption=new LocaleValue("银行"),Value="BD_BANK"
                }
            };

            return items;
        }

    }
}
