using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System.Collections.Generic;
using System.ComponentModel;

namespace GalaxyPlugin.Bill
{
    [Description("按照不同编码规则生成编号插件")]
    public class BillCodeRulePlugin : AbstractBillPlugIn
    {

        public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
        {
            base.BeforeSetItemValueByNumber(e);
        }
        public override void BeforeSave(BeforeSaveEventArgs e)
        {
            base.BeforeSave(e);

            //获取物料分组是否是特定编码规则分组
            object groupData = Model.DataObject["MaterialGroup"];
            if (groupData != null && groupData is DynamicObject)
            {
                const string specifiedGroupNum = "SpeCodeRule";
                object groupNum = ((DynamicObject)groupData)["Number"];
                if (groupNum != null && groupNum.ToString().Equals(specifiedGroupNum))
                {
                    //满足特定分组使用自定义编码规则生成单据编号
                    GenerateBillNoById();
                    Test();
                }
            }
        }

        public void Test()
        {
            return;
        }

        public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
        {
            base.BeforeFlexSelect(e);
        }


        private void GenerateBillNoById()
        {
            BusinessDataService dataService = new BusinessDataService();
            Kingdee.BOS.Core.Metadata.BusinessInfo businInfo = View.BillBusinessInfo;
            DynamicObject[] dataObjs = new DynamicObject[] { Model.DataObject };

            string repairBillNo = dataService.GetNextBillNoByRepair(Context, businInfo, dataObjs, string.Empty, null);
            Model.SetValue(businInfo.GetBillNoField().Key, repairBillNo);

            //var businInfo = this.View.BillBusinessInfo;

            List<SelectorItemInfo> selector = new List<SelectorItemInfo>()
            {
                new SelectorItemInfo("Number")
            };
            OQLFilter filter = OQLFilter.CreateHeadEntityFilter(
                string.Format("FUseOrgId = '1' and FForbidStatus = 'A'"));
            var dataCollection =BusinessDataServiceHelper.Load(this.Context, businInfo, selector, filter);

            /*
             * 通过下面语句查询到FRULEID的值，得到 FRULEID=5c48033be79374
             * select * from T_BAS_BILLCODERULE_L t where t.fname ='插件调用编码规则';             
             */

            bool isUpdateMax = true;
            const string specifiedRuleId = "5c48033be79374";
            System.Collections.Generic.List<Kingdee.BOS.Core.Metadata.FormElement.BillNoInfo> billNoList = dataService.GetBillNo(Context, businInfo, dataObjs, isUpdateMax, specifiedRuleId);

            Model.SetValue(businInfo.GetBillNoField().Key, billNoList[0].BillNo);
        }

        public override void EntityRowClick(EntityRowClickEventArgs e)
        {
            base.EntityRowClick(e);
            
        }

        public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
        {
            base.EntityRowDoubleClick(e);
        }


    }
}
